# Yetibyte.Twitch.Bobota
A simple configurable Twitch chat bot that responds to chat messages and/or other chat bots.

## Description

Bobota is a multi-purpose Twitch chat bot that is completely customizable. It can be configured to respond to a specified chat command using a message randomly pulled from a 
pre-defined set of messages.

## Usage

1. Configure the bot by editing the file bobota.config included in the directory of the app (see Configuration).
2. Run the console application.
3. The app will validate your configuration.
4. Hit any key to run the bot.
5. Leave app running. It will log any events and disconnect automatically when closed.

## Configuration

### Example

There is an example configuration file (example.config) included in the app directory which looks like this:

    {
      "Command": "!motivation",
      "BotTwitchUserName": "YourBotsTwitchUserName",
      "OAuthToken": "PasteValidTokenHere",
      "TargetTwitchChannel": "TheChannelYouWantToUseTheBotOn",
      "IsTestMode": false,
      "CompetingBot": {
        "BotTwitchUserName": "SomeOtherBotsTwitchUserName",
        "Command": "!demotivation",
        "ReactionMessages": [ 
          "Don't listen to {COMPETING_BOT_NAME}, {USER}!", 
          "{COMPETING_BOT_NAME} is a liar, {USER}!" 
        ]
      },
      "Messages": [ 
        "{USER}, never give up!",
        "You can do it, {USER}!",
        "You're invincible, {USER}!",
      ],
      "Greeting": "Hi! I am {BOT_NAME}. Enter {COMMAND} and I'll respond.",
      "GoodbyeMessage": "Okay, bye for now!"
    }
    
This example configuration describes a bot that is triggered by the command message "!motivation" and responds with a random motivational message.
It is configured to also react to a "rival" demotivational bot which is triggered using the command "!demotivation".

Note: The property "CompetingBot" is optional. Either set it to null or leave its properties blank if you do not wish to use this feature.

### Variables

Messages can include a variety of variables that will be resolved at runtime. Variables names must be spelled in all caps and surrounded with braces.
The following variables are available:
| Variable | Effect |
|----------|--------|
| {USER} | Name of the user the bot responds to. |
| {BOT_NAME} | Twitch user name of the bot. |
| {COMMAND} | The command that triggers the bot. |
| {COMPETING_BOT_NAME} | Twitch user name of another bot that this bot should interact with. |
