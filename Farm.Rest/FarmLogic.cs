namespace Servers;

using NLog;

public class SubmissionResult
{
    /// <summary>
    /// Indicates if submission attempt has accepted.
    /// </summary>
    public bool IsAccepted { get; set; }

    /// <summary>
    /// If pass submission has failed, indicates fail reason.
    /// </summary>
    public string FailReason { get; set; }
}

public class FarmState
{
    /// <summary>
    /// Access lock.
    /// </summary>
    public readonly object AccessLock = new object();

    /// <summary>
    /// Total accumulated food.
    /// </summary>
    public double AccumulatedFood = 0;

    /// <summary>
    /// Total accumulated water.
    /// </summary>
    public double AccumulatedWater = 0;

    public double farmSize = 0.0;

    public double consumptionCoef = 0.01;

    public double totalConsumedResources = 0.0;

    public int thirstRounds = 0;

    public int starveRounds = 0;

    /// <summary>
    /// Timestamp of the last consumption event.
    /// </summary>
    public DateTime? LastConsumptionTimestamp;

    public bool IsSelling = false;
    public DateTime? SellingUntil = null;
}

public class FarmLogic
{
    /// <summary>
    /// Logger for this class.
    /// </summary>
    private Logger mLog = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Background task thread.
    /// </summary>
    private Thread mBgTaskThread;

    /// <summary>
    /// State descriptor.
    /// </summary>
    private FarmState mState = new FarmState();

    private readonly Random mRandom = new Random();


    public double baseRate = 0.05;

    public double growthRate = 0.1;

    public int maxFailRounds = 2;

    public double maxFarmSize = 1.5;


    public FarmLogic()
    {
        //start the background task
        mBgTaskThread = new Thread(BackgroundTask);
        mBgTaskThread.Start();
    }

    public SubmissionResult SubmitFood(double amount)
    {
        lock (mState.AccessLock)
        {
            if (mState.IsSelling)
            {
                return new SubmissionResult { IsAccepted = false, FailReason = "FarmSelling" };
            }
            mState.AccumulatedFood += amount;
            return new SubmissionResult { IsAccepted = true, FailReason = string.Empty };
        }
    }

    public SubmissionResult SubmitWater(double amount)
    {
        lock (mState.AccessLock)
        {
            if (mState.IsSelling)
            {
                return new SubmissionResult { IsAccepted = false, FailReason = "FarmSelling" };
            }

            mState.AccumulatedWater += amount;

            return new SubmissionResult { IsAccepted = true, FailReason = string.Empty };
        }
    }

 private double GetRandomFoodConsumption()
    {
        var consumption = mRandom.Next(0, 100) * mState.consumptionCoef;

        if (consumption >= mState.AccumulatedFood)
        {
            mState.starveRounds++;
            mLog.Warn($"Was unable to consume {consumption} of food.");
            if (mState.starveRounds >= maxFailRounds)
            {
                HandleFarmFailure("food");
            }
            return 0;
        }
        
        mLog.Info($"Consumed {consumption} of food. Food left: {mState.AccumulatedFood - consumption}");

        mState.starveRounds = 0;
        return consumption;
    }

    private double GetRandomWaterConsumption()
    {

        var consumption = mRandom.Next(0, 100) * mState.consumptionCoef;

        if (consumption >= mState.AccumulatedWater)
        {
            mState.thirstRounds++;
            mLog.Warn($"Was unable to consume {consumption} of water.");

            if (mState.thirstRounds >= maxFailRounds)
            {
                HandleFarmFailure("water");
            }
            return 0;
        }

        mLog.Info($"Consumed {consumption} of water. Water left: {mState.AccumulatedWater - consumption}.");

        mState.thirstRounds = 0;
        return consumption;
    }
    
    private double ComputeConsumptionCoefficient()
    {
        return Math.Clamp(baseRate + growthRate * mState.farmSize, baseRate, 2.0);
    }

    private double ComputeFarmSize(double total)
    {
        return Math.Log10(total + 1);
    }

    private void ResetFarmState()
    {
        mState.AccumulatedFood = 0;
        mState.AccumulatedWater = 0;
        mState.totalConsumedResources = 0;
        mState.starveRounds = 0;
        mState.thirstRounds = 0;
        mState.farmSize = 0;
        mState.consumptionCoef = 0.01;
        mState.LastConsumptionTimestamp = null;
    }

    private void HandleFarmFailure(string failedResource)
    {
        mLog.Warn($"Farm has been without {failedResource} for {maxFailRounds} consecutive rounds. Farm has failed.");
        ResetFarmState();
        mLog.Info("Farm state has been reset. Background processing will continue with a fresh farm.");
    }

    private void HandleFarmSelling()
    {
        if (mState.IsSelling && mState.SellingUntil.HasValue && DateTime.UtcNow >= mState.SellingUntil.Value)
        {
            ResetFarmState();
            mLog.Info("Farm selling period has ended. Farm is no longer selling.");
            mState.IsSelling = false;
            mState.SellingUntil = null;
        }
    }
    /// <summary>
    /// Background task for the traffic light.
    /// </summary>
    public void BackgroundTask()
    {
        //intialize random number generator
        var rnd = new Random();

        //
        while (true)
        {
            Thread.Sleep(2000);

            double consumedFood = 0;
            double consumedWater = 0;

            lock (mState.AccessLock)
            {
                if (!mState.IsSelling)
                {
                    consumedFood = GetRandomFoodConsumption();
                    consumedWater = GetRandomWaterConsumption();

                    mState.AccumulatedFood -= consumedFood;
                    mState.AccumulatedWater -= consumedWater;
                    mState.LastConsumptionTimestamp = DateTime.UtcNow;

                    mState.totalConsumedResources += consumedFood + consumedWater;

                    mState.farmSize = ComputeFarmSize(mState.totalConsumedResources);
                    mState.consumptionCoef = ComputeConsumptionCoefficient();

                    mLog.Info($"Farm size after consumption {mState.farmSize:F1}, consumption coefficient has been updated to {mState.consumptionCoef:F1}.");

                    if (mState.farmSize >= maxFarmSize && !mState.IsSelling)
                    {
                        mState.IsSelling = true;
                        mState.SellingUntil = DateTime.UtcNow.AddSeconds(5); //selling period lasts for 5 seconds
                        mLog.Info($"Farm has reached maximum size of {maxFarmSize}. Farm is now selling for the next 5 seconds.");
                    }
                }
                else
                {
                    mLog.Info($"Farm is currently selling. Selling will end at {mState.SellingUntil}.");
                }
                
                HandleFarmSelling();
            }
        }
    }
}