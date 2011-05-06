using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wowarmory.Network;
using wowarmory.Chat;
using Meebey.SmartIrc4net;
using System.Threading;

namespace ircbot {
    public class IRCBridge {
        string ircChannel;
        Queue<string> ircMessageQueue = new Queue<string>();

        IrcClient irc = new IrcClient();

        ChatModule module;
        public IRCBridge(ChatModule module) {
            this.module = module;

            module.OnMessageMOTD += new ChatModule.OnMessageDelegate(module_OnMessageMOTD);
            module.OnMessageGuildChat += new ChatModule.OnMessageDelegate(module_OnMessageGuildChat);
            module.OnMessageOfficerChat += new ChatModule.OnMessageDelegate(module_OnMessageOfficerChat);
            module.OnMessageWhisper += new ChatModule.OnMessageDelegate(module_OnMessageWhisper);
            module.OnPresenceChange += new ChatModule.OnPresenceDelegate(module_OnPresenceChange);
        }

        public void Start(string server, string channel, string nick, string key, int port) {
            if (irc.IsConnected && irc.IsRegistered)
                return;

            ircChannel = channel;

            irc.OnChannelMessage += new IrcEventHandler(irc_OnChannelMessage);
            irc.OnJoin += new JoinEventHandler(irc_OnJoin);
            irc.Connect(server, port);

            irc.AutoRelogin = true;
            irc.AutoRetry = true;
            irc.AutoNickHandling = true;
            irc.AutoRejoin = true;
            irc.AutoReconnect = true;

            irc.Login(nick, nick, 4, nick);
            irc.RfcJoin(channel, key);

            new Thread(new ThreadStart(IrcListen)).Start();
        }

        public void IrcListen() {
            irc.Listen();
        }

        public void IrcSendMessage(string msg) {
            if (irc.IsConnected && irc.IsRegistered) {
                Console.WriteLine("IRC: " + msg);
                irc.SendMessage(SendType.Message, ircChannel, msg);
            } else {
                ircMessageQueue.Enqueue(msg);
            }
        }

        void irc_OnChannelMessage(object sender, IrcEventArgs args) {
            var msgArray = args.Data.MessageArray;
            var msg = String.Join(" ", msgArray, 1, msgArray.Length - 1);
            if (msgArray.Length > 0 && msgArray[0] == ".gchat") {
                //msg = String.Format("<{0}> {1}", args.Data.Nick, msg);
                if (msg.Length > 255) // TODO: confirm max length
                    msg = msg.Substring(0, 255);

                module.SendMessage(msg);
            }
        }

        void irc_OnJoin(object sender, JoinEventArgs e) {
            while (ircMessageQueue.Count > 0) {
                var msg = ircMessageQueue.Dequeue();
                IrcSendMessage(msg);
            }
        }

        void module_OnPresenceChange(ChatModule module, Presence presence) {
            var arrow = "->";
            if (presence.Offline)
                arrow = "<-";

            IrcSendMessage(String.Format("{0} {1} ({2})", arrow, presence.Name, presence.Type));
        }

        void module_OnMessageWhisper(ChatModule module, Message message) {
            IrcSendMessage(String.Format("whisper <{0}> {1}", message.CharacterName, message.Body));
        }

        void module_OnMessageOfficerChat(ChatModule module, Message message) {
            IrcSendMessage(String.Format("officer <{0}> {1}", message.CharacterName, message.Body));
        }

        void module_OnMessageGuildChat(ChatModule module, Message message) {
            IrcSendMessage(String.Format("<{0}> {1}", message.CharacterName, message.Body));
        }

        void module_OnMessageMOTD(ChatModule module, Message message) {
            IrcSendMessage("MOTD: " + message.Body);
        }
    }
}
