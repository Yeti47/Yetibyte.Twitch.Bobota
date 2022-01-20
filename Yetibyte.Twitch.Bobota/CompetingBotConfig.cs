using System;

namespace Yetibyte.Twitch.Bobota
{
    [Serializable]
    internal record CompetingBotConfig
    {
        public string BotTwitchUserName { get; init; }
        public string Command { get; init; }
        public string[] ReactionMessages { get; init; }
    }
}
