namespace Clients;

using System.Net.Http;

using NLog;

using Services;


/// <summary>
/// Client example.
/// </summary>
class Client
{
	/// <summary>
	/// Logger for this class.
	/// </summary>
	Logger mLog = LogManager.GetCurrentClassLogger();

	private double pendingWater = 0.0;

	/// <summary>
	/// Configures logging subsystem.
	/// </summary>
	private void ConfigureLogging()
	{
		var config = new NLog.Config.LoggingConfiguration();

		var console =
			new NLog.Targets.ConsoleTarget("console")
			{
				Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
			};
		config.AddTarget(console);
		config.AddRuleForAllLevels(console);

		LogManager.Configuration = config;
	}

	/// <summary>
	/// Program body.
	/// </summary>
	private void Run()
	{
		//configure logging
		ConfigureLogging();

		//initialize random number generator
		var rnd = new Random();

		//run everythin in a loop to recover from connection errors
		while (true)
		{
			try
			{
				//connect to the server, get service client proxy
				var Farm = new FarmClient("http://127.0.0.1:5100", new HttpClient());

				//do the water stuff
				while (true)
				{
					// produce between -1.0 and 1.0 water
					double producedWater = Math.Round(rnd.NextDouble() * 2.0 - 1.0, 1);
					pendingWater += producedWater;

					var result = Farm.SubmitWater(pendingWater);

					if (result.IsAccepted)
					{
						mLog.Info($"Submitted {pendingWater} water.");
						pendingWater = 0;
					}
					else if (result.FailReason == "FarmSelling")
					{
						mLog.Info("Farm is selling; will retry with accumulated water.");
					}
					else
					{
						mLog.Warn($"Submission failed: {result.FailReason}. Keeping {pendingWater} to retry.");
					}

					// wait between sending attempts
					Thread.Sleep(2000);
				}
			}
			catch (Exception e)
			{
				//log whatever exception to console
				mLog.Warn(e, "Unhandled exception caught. Will restart main loop.");

				//prevent console spamming
				Thread.Sleep(2000);
			}
		}
	}

	/// <summary>
	/// Program entry point.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	static void Main(string[] args)
	{
		var self = new Client();
		self.Run();
	}
}