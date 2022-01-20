using System;
using System.Runtime.InteropServices;

namespace Yetibyte.Twitch.Bobota
{
    class Program
    {
        private static ILogger _logger = new ConsoleLogger();

        static void Main()
        {
            BobotaConfig config = LoadConfig();

            if (config is null)
            {
                _logger.LogError("Config file could not be loaded. Possible reason may be a formatting issue.");

                Console.ReadKey(true);

                return;
            }

            _logger.LogInformation("Validating config file...");

            if (!config.Validate(_logger))
            {
                _logger.LogError("Config file invalid. Cannot continue.");

                Console.ReadKey(true);

                return;
            }

            _logger.LogInformation("Success.");

            Console.WriteLine();

            Console.WriteLine("Hi, welcome to the Bobota Twitch bot!");
            Console.WriteLine("=============================================");
            Console.WriteLine();
            Console.WriteLine("The following configuration will be used:");
            Console.WriteLine(config);
            Console.WriteLine();

            Console.WriteLine("Hit any key to start the bot. It will disconnect automatically when the app is closed.");

            Console.ReadKey(true);

            Console.WriteLine();

            BobotaClient bot = new BobotaClient(config, _logger);

            bot.Run();

            Console.WriteLine();

            AppDomain.CurrentDomain.ProcessExit += (o, e) => bot.Stop();

            while (bot.IsRunning)
            {
                System.Threading.Thread.Sleep(1);
            }

            bot.Stop();

        }

        private static BobotaConfig LoadConfig()
        {
            if (!BobotaConfig.FileExists)
            {
                BobotaConfig.Empty.Save(_logger);

                if (!BobotaConfig.FileExists)
                {
                    _logger.LogError("Could not create config file. Cannot continue.");

                    Console.ReadKey(true);
                }
            }

            BobotaConfig config = BobotaConfig.Load(_logger);
            return config;
        }
    }
}
