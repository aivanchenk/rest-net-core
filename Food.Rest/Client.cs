namespace Clients;

using System.Net.Http;

using NLog;


/// <summary>
/// Client example.
/// </summary>
class Client
{
	        /// <summary>
        /// Logger for this class.
        /// </summary>
        private readonly Logger mLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Configures logging subsystem.
        /// </summary>
        private static void ConfigureLogging()
        {
                var config = new NLog.Config.LoggingConfiguration();

                var console = new NLog.Targets.ConsoleTarget("console")
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
        private async Task RunAsync(CancellationToken cancellationToken)
        {
                ConfigureLogging();

                mLog.Info("Food client started. Press Ctrl+C to exit.");

                while (!cancellationToken.IsCancellationRequested)
                {
                        mLog.Info($"Current time: {DateTime.Now:O}");

                        try
                        {
                                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                                break;
                        }
                }

                mLog.Info("Food client is stopping.");
        }

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static async Task Main(string[] args)
        {
                using var cts = new CancellationTokenSource();

                Console.CancelKeyPress += (_, eventArgs) =>
                {
                        eventArgs.Cancel = true;
                        cts.Cancel();
                };

                var self = new Client();
                await self.RunAsync(cts.Token);
        }

}