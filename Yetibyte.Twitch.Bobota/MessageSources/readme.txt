To implement your own message source, reference the assembly Yetibyte.Twitch.Bobota and create a class that implements
the interface Yetibyte.Twitch.Bobota.IMessageSource. Then drop your compiled DLL file in this folder (MessageSources).
Your class must provide a parameterless default constructor.
In the bobota.config file then set the "MessageSourceClass" property to the fully qualified name of your class.