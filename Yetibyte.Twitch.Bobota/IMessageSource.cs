using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yetibyte.Twitch.Bobota
{
    public interface IMessageSource
    {
        void Initialize();
        string GetRandomMessage(string command, string[] parameters);
    }
}
