using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wowarmory.Network;
using wowarmory.Chat;
using System.ComponentModel;

namespace chatclient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window, INotifyPropertyChanged {
        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string info) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        Session session;

        StringBuilder chatLog = new StringBuilder();
        ChatModule chat;

        public string ChatLog {
            get {
                return chatLog.ToString();
            }

            set {
                chatLog = new StringBuilder(value);
            }
        }

        public MainWindow(string accountName, string password, string characterName, string realmName) {
            InitializeComponent();

            session = new Session();

            this.Closed += new EventHandler(MainWindow_Closed);

            DataContext = this;

            if (chat == null) {
                chat = new ChatModule(session, characterName, realmName);

                var msgDele = new ChatModule.OnMessageDelegate(OnMessage);
                chat.OnMessageGuildChat += msgDele;
                chat.OnMessageOfficerChat += msgDele;
                chat.OnMessageWhisper += msgDele;
                chat.OnMessageMOTD += msgDele;
                chat.OnPresenceChange += new ChatModule.OnPresenceDelegate(OnPresenceChange);
                chat.OnChatLoggedOut += new ChatModule.OnChatLoggedOutDelegate(OnChatLoggedOut);

                chat.OnLoginFailed += new ChatModule.OnLoginFailedDelegate(OnLoginFailed);
            }

            session.Start(accountName, password);
            session.OnSessionClosed += new Session.OnSessionClosedDelegate(OnSessionClosed);

            Title = String.Format("Guild Chat ({0}/{1})", characterName, realmName);
        }

        void OnLoginFailed(string reason) {
            OnSessionClosed(reason);
            session.Close();
        }

        void OnChatLoggedOut() {
            session.Close();
        }

        void OnSessionClosed(string reason) {
            if (!String.IsNullOrEmpty(reason))
                MessageBox.Show("Connection closed: " + reason);
            Dispatcher.BeginInvoke(new Action(Close));
        }

        void MainWindow_Closed(object sender, EventArgs e) {
            chat.Close();
            //session.Close();
        }

        void OnPresenceChange(ChatModule module, Presence presence) {
            var arrow = "->";
            if (presence.Offline)
                arrow = "<-";

            chatLog.AppendLine(String.Format("{0} {1} ({2})", arrow, presence.Name, presence.Type));
            NotifyPropertyChanged("ChatLog");
        }

        void OnMessage(ChatModule module, Message m) {
            if (m.Type == Message.CHAT_MSG_TYPE_GUILD_CHAT || m.Type == Message.CHAT_MSG_TYPE_OFFICER_CHAT) {
                chatLog.AppendLine(String.Format("<{0}> {1}", m.CharacterName, m.Body));
            } else if (m.Type == Message.CHAT_MSG_TYPE_GUILD_MOTD) {
                chatLog.AppendLine("MOTD: " + m.Body);
            } else if (m.Type == Message.CHAT_MSG_TYPE_WHISPER) {
                chatLog.AppendLine(String.Format("whisper <{0}> {1}", m.CharacterName, m.Body));
            }

            NotifyPropertyChanged("ChatLog");
        }

        private void chatEntry_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key != Key.Return)
                return;
            var msg = chatEntry.Text;
            if (msg.StartsWith("/")) {
                var tokens = msg.Split(' ');
                if (tokens[0] == "/whisper" || tokens[0] == "/w") {
                    if (tokens.Length < 3) {
                        chatLog.AppendLine("Usage: /whisper <target> <message>");
                    } else {
                        chat.SendWhisper(tokens[1], String.Join(" ", tokens, 2, tokens.Length - 2));
                    }
                } else if (tokens[0] == "/officer" || tokens[0] == "/o") {
                    if (tokens.Length < 2) {
                        chatLog.AppendLine("Usage: /officer <message>");
                    } else {
                        chat.SendMessage(String.Join(" ", tokens, 1, tokens.Length - 1), wowarmory.Chat.Message.CHAT_MSG_TYPE_OFFICER_CHAT);
                    }
                } else {
                    chatLog.AppendLine("Unknown command: " + tokens[0]);
                    chatLog.AppendLine("Available commands:");
                    chatLog.AppendLine("/w, /whisper");
                }
            } else {
                chat.SendMessage(msg);
                chatEntry.Text = "";
            }
        }
    }
}
