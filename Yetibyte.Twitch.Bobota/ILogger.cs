namespace Yetibyte.Twitch.Bobota
{
    internal interface ILogger
    {
        void LogInformation(string message);
        void LogError(string message);
    }
}
