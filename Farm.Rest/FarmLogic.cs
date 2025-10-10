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
			//sleep a while
			Thread.Sleep(500 + rnd.Next(1500));
			mLog.Info($"Server is running at {DateTime.Now}.");
		}
	}
}