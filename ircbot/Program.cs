using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wowarmory.Network;
using System.IO;
using System.Diagnostics;

namespace ircbot {
    class Program {
        static Session session;
        static void Main(string[] args) {
            if (args.Length < 7) {
                var exename = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                Console.WriteLine("Usage: {0} account password characterName realmName ircHost ircChannel ircNick", exename);
                return;
            }

            session = new Session();

            var chat = new ChatModule(session, args[2], args[3]);
            var irc = new IRCBridge(chat);
            irc.Start(args[4], args[5], args[6], "", 6667);

            session.Start(args[0], args[1]);
        }
    }
}
