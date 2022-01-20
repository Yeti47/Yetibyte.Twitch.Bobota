using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yetibyte.Twitch.Bobota
{
    [Serializable]
    internal record BobotaConfig
    {
        private const string CONFIG_FILE_NAME = "bobota.config";

        public static BobotaConfig Empty { get; } = new BobotaConfig
        {
            Command = string.Empty,
            BotTwitchUserName = string.Empty,
            OAuthToken = string.Empty,
            TargetTwitchChannel = string.Empty,
            IsTestMode = false,
            CompetingBot = new CompetingBotConfig
            {
                BotTwitchUserName = string.Empty,
                Command = string.Empty,
                ReactionMessages = Array.Empty<string>()
            },
            GoodbyeMessage = string.Empty,
            Greeting = string.Empty,
            Messages = Array.Empty<string>()
        };

        public static bool FileExists => System.IO.File.Exists(CONFIG_FILE_NAME);

        public string Command { get; init; }

        public string BotTwitchUserName { get; init; }
        public string OAuthToken { get; init; }
        public string TargetTwitchChannel { get; init; }

        public bool IsTestMode { get; init; }

        public CompetingBotConfig CompetingBot { get; init; }

        [JsonIgnore]
        public bool HasCompetingBot => !string.IsNullOrWhiteSpace(CompetingBot?.BotTwitchUserName);

        public string[] Messages { get; init; }
        public string Greeting { get; init; }
        public string GoodbyeMessage { get; init; }

        public static BobotaConfig Load(ILogger logger = null)
        {
            try
            {
                string fileContent = System.IO.File.ReadAllText(CONFIG_FILE_NAME);
                return JsonSerializer.Deserialize<BobotaConfig>(fileContent);
            }
            catch(Exception ex)
            {
                logger?.LogError(ex.Message);
                return null;
            }
        }

        public override string ToString() => Serialize();

        public string Serialize() => JsonSerializer.Serialize(this, GetType(), new JsonSerializerOptions { WriteIndented = true });

        public bool Save(ILogger logger = null)
        {
            try
            {
                string json = Serialize();
                System.IO.File.WriteAllText(CONFIG_FILE_NAME, json);
            }
            catch(Exception ex)
            {
                logger?.LogError(ex.Message);
                return false;
            }

            return true;
        }

        internal bool Validate(ILogger logger = null)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(BotTwitchUserName))
            {
                logger?.LogError($"No valid {nameof(BotTwitchUserName)} provided.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(OAuthToken))
            {
                logger?.LogError($"No {nameof(OAuthToken)} provided.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(TargetTwitchChannel))
            {
                logger?.LogError($"No {nameof(TargetTwitchChannel)} provided.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Command))
            {
                logger?.LogError($"No {nameof(Command)} provided. Example: !motivation");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Greeting))
            {
                logger?.LogError($"No {nameof(Greeting)} provided. Example: Hello, I'm " + "{BOT_NAME}. Just type {COMMAND} and I'll respond.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(GoodbyeMessage))
            {
                logger?.LogError($"No {nameof(GoodbyeMessage)} provided. Example: Okay, bye for now!");
                isValid = false;
            }

            if (Messages is null || Messages.Length <= 0)
            {
                logger?.LogError($"No {nameof(Messages)} provided. Example: " + "{USER_NAME}, you are great!");
                isValid = false;
            }

            if (HasCompetingBot)
            {
                if (string.IsNullOrWhiteSpace(CompetingBot.Command))
                {
                    logger?.LogError($"No Command provided for the competing bot. Example: !demotivation");
                    isValid = false;
                }
                if (string.IsNullOrWhiteSpace(CompetingBot.BotTwitchUserName))
                {
                    logger?.LogError($"No BotTwitchUserName provided for the competing bot. Example: NotSoNiceBot");
                    isValid = false;
                }
                if (CompetingBot.ReactionMessages is null || CompetingBot.ReactionMessages.Length <= 0)
                {
                    logger?.LogError("No ReactionMessages provided for the competing bot. Example: Don't listen to {COMPETING_BOT_NAME}, {USER}!");
                    isValid = false;
                }
            }

            return isValid;
        }


    }
}
