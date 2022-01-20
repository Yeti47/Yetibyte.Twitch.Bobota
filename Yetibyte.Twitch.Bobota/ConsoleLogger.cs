using System;

namespace Yetibyte.Twitch.Bobota
{
    internal class ConsoleLogger : ILogger
    {
        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void LogInformation(string message)
        {
            Console.WriteLine(message);
        }
    }
}
