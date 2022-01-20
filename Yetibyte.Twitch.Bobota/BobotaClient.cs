using System;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;

namespace Yetibyte.Twitch.Bobota
{
    internal class BobotaClient
    {
        private const string FALLBACK_MESSAGE = "I have nothing to say.";
        private const string FALLBACK_COMPETING_MESSAGE = "Don't listen to {COMPETING_BOT_NAME}, {USER}!";

        private readonly TwitchClient _client;
        private readonly BobotaConfig _config;
        private readonly ILogger _logger;
        private readonly Random _random;

        private string _lastUserRespondedTo;

        public string TwitchUserName => _config.BotTwitchUserName;
        public string AuthToken => _config.OAuthToken;
        public string ChannelName => _config.TargetTwitchChannel;

        public bool IsRunning { get; private set; } = false;

        public bool IsTestMode => _config.IsTestMode;

        public BobotaClient(BobotaConfig config, ILogger logger = null)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
            _logger = logger;

            WebSocketClient webSocketClient = new WebSocketClient();
            webSocketClient.OnError += WebSocketClient_OnError;

            _client = new TwitchClient(webSocketClient);
            _client.OnConnected += client_OnConnected;
            _client.OnDisconnected += client_OnDisconnected;
            _client.OnMessageReceived += client_OnMessageReceived;
            _client.OnError += client_OnError;
            _client.OnConnectionError += client_OnConnectionError;
            _client.OnNoPermissionError += client_OnNoPermissionError;
            _client.OnJoinedChannel += client_OnJoinedChannel;
            _client.OnFailureToReceiveJoinConfirmation += client_OnFailureToReceiveJoinConfirmation;

            _random = new Random();
        }

        private void client_OnFailureToReceiveJoinConfirmation(object sender, TwitchLib.Client.Events.OnFailureToReceiveJoinConfirmationArgs e)
        {
            _logger?.LogError($"Client error. Failed to join channel {ChannelName}. Check credentials. {(!string.IsNullOrWhiteSpace(e.Exception?.Details) ? e.Exception?.Details : string.Empty)}");
            IsRunning = false;
        }

        private void client_OnJoinedChannel(object sender, TwitchLib.Client.Events.OnJoinedChannelArgs e)
        {
            _logger?.LogInformation($"Bot joined Channel {ChannelName}.");
            SendMessage(_config.Greeting);
        }

        private void WebSocketClient_OnError(object sender, TwitchLib.Communication.Events.OnErrorEventArgs e)
        {
            //_logger?.LogError($"Web Socket error: {e.Exception?.Message}");
        }

        private void client_OnNoPermissionError(object sender, EventArgs e)
        {
            _logger?.LogError("Client permission error. Check OAuth token and user permissions.");
        }

        private void client_OnConnectionError(object sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
        {
            _logger?.LogError($"Connection error: {e.Error?.Message}");
        }

        private void client_OnError(object sender, TwitchLib.Communication.Events.OnErrorEventArgs e)
        {
            _logger?.LogError($"Client error: {e.Exception?.Message}");
        }

        private void client_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            _logger?.LogInformation("Connected.");
        }

        private void client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            _logger?.LogInformation("Connection closed.");
        }

        private void client_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (!e.ChatMessage.Username.Equals(TwitchUserName, System.StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogInformation($"Message received from User {e.ChatMessage.Username}: {e.ChatMessage.Message}");

                string normalizedMessage = e.ChatMessage.Message?.ToLower()?.Trim() ?? string.Empty;

                if (normalizedMessage.StartsWith(_config.Command, StringComparison.OrdinalIgnoreCase)) {

                    PostResponse(e.ChatMessage);
                }
                else if(_config.HasCompetingBot)
                {
                    if(normalizedMessage.StartsWith(_config.CompetingBot.Command, StringComparison.OrdinalIgnoreCase)) {
                        _lastUserRespondedTo = e.ChatMessage.Username;
                    }
                    else if(e.ChatMessage.Username.Equals(_config.CompetingBot.BotTwitchUserName, StringComparison.OrdinalIgnoreCase) 
                        && !string.IsNullOrWhiteSpace(_lastUserRespondedTo))
                    {
                        PostCompetingMessage();
                        _lastUserRespondedTo = null;
                    }
                }
            }
        }

        private void PostCompetingMessage()
        {
            string message = FALLBACK_COMPETING_MESSAGE;

            if (_config.CompetingBot.ReactionMessages?.Any() ?? false)
            {
                message = _config.CompetingBot.ReactionMessages[_random.Next(_config.CompetingBot.ReactionMessages.Length)];
            }

            SendMessage(message, _lastUserRespondedTo);
        }

        private void PostResponse(ChatMessage originalChatMessage)
        {
            string message = FALLBACK_MESSAGE;

            if (_config.Messages?.Any() ?? false)
            {
                message = _config.Messages[_random.Next(_config.Messages.Length)];
            }

            SendMessage(message, originalChatMessage.Username);        
        }

        public bool Run()
        {
            if (IsRunning)
                return false;

            ConnectionCredentials connectionCredentials = new ConnectionCredentials(TwitchUserName, AuthToken);
            _client.Initialize(connectionCredentials, ChannelName);

            bool connected = _client.Connect();

            if (connected)
                _logger?.LogInformation(IsTestMode ? "Bot started in test mode." : "Bot started.");
            else
                _logger?.LogError("Bot could not be started. Check your settings.");

            IsRunning = connected;

            return connected;
        }

        private void SendMessage(string message, string addressedUser = null, CompetingBotConfig competingBot = null)
        {
            if (message is null)
                return;

            message = PreprocessMessage(message, addressedUser, competingBot);

            bool isError = false;

            if (!IsTestMode)
            { 
                try
                {
                    _client.SendMessage(ChannelName, message);
                }
                catch(Exception ex)
                {
                    _logger?.LogError($"Error sending message: {ex.Message}.{Environment.NewLine}Possibly, there is a problem with the provided credentials (e. g. OAuth token is invalid).");
                    isError = true;
                }
            }

            if (!isError)
                _logger?.LogInformation("Message sent: " + message);
        }

        private string PreprocessMessage(string message, string addressedUser = null, CompetingBotConfig competingBot = null)
        {
            message = message
                .Replace("{USER}", addressedUser ?? string.Empty)
                .Replace("{BOT_NAME}", _config.BotTwitchUserName)
                .Replace("{COMMAND}", _config.Command)
                .Replace("{COMPETING_BOT_NAME}", competingBot?.BotTwitchUserName ?? string.Empty);

            return message;
        }

        public bool Stop()
        {
            if (!IsRunning)
                return false;

            if (_client.IsConnected)
            {
                SendMessage(_config.GoodbyeMessage);
                _client.Disconnect();
            }

            IsRunning = false;

            _logger?.LogInformation("Bot stopped.");

            return true;
        }
    }
}
