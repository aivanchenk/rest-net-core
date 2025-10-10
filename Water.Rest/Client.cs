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
	private void Run() {
		//configure logging
		ConfigureLogging();

		//initialize random number generator
		var rnd = new Random();

		//run everythin in a loop to recover from connection errors
		while( true )
		{
			try {
				//connect to the server, get service client proxy
				var Farm = new FarmClient("http://127.0.0.1:5100", new HttpClient());

				//log identity data
				mLog.Info($"I am a water client.");

				//do the water stuff
				while (true)
                                {
                                // prepare an amount to send
                                var amount = Math.Round(rnd.NextDouble() * 10.0, 2);

                                mLog.Info($"Sending {amount} units of food to Farm...");

                                try
                                {
                                        // the generated client has a synchronous wrapper SubmitWater(...)
                                        var result = Farm.SubmitWater(amount);

                                        // log the server response
                                        mLog.Info($"SubmitWater -> IsAccepted={result.IsAccepted}, FailReason='{result.FailReason ?? string.Empty}'");
                                }
                                catch (Exception ex)
                                {
                                        // ApiException or other exceptions will be caught here
                                        mLog.Warn(ex, "Failed to submit water");
                                }

                                // wait between attempts
                                Thread.Sleep(2000);
                                }			
			}
			catch( Exception e )
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