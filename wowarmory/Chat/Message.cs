using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wowarmory.Chat {
    public class Message {
        // from["chatIdType"]
        public const string CHAT_ID_TYPE_CHARACTER = "character";
        public const string CHAT_ID_TYPE_GUILD = "guild";
        public const string CHAT_ID_TYPE_GUILD_MEMBER = "guild_member";

        // messageType
        public const string CHAT_MSG_TYPE_AFK = "afk";
        public const string CHAT_MSG_TYPE_DND = "dnd";
        public const string CHAT_MSG_TYPE_GUILD_CHAT = "guild_chat";
        public const string CHAT_MSG_TYPE_GUILD_MOTD = "motd";
        public const string CHAT_MSG_TYPE_OFFICER_CHAT = "officer_chat";
        public const string CHAT_MSG_TYPE_WHISPER = "whisper";

        public string Type;
        public string Body;
        public string FromType;
        public string CharacterId;

        public Message(Dictionary<string, object> response) {
            var from = (Dictionary<string, object>)response["from"];
            FromType = (string)from["chatIdType"];
            Type = (string)response["messageType"];

            var bodyFormat = (string)response["bodyFormat"];
            Body = (string)response["body"];

            // TODO: special class to parse "from" data
            if (FromType == CHAT_ID_TYPE_CHARACTER || FromType == CHAT_ID_TYPE_GUILD_MEMBER)
                CharacterId = (string)from["characterId"];
        }

        public string CharacterName {
            get {
                var first = CharacterId.IndexOf(":");
                return CharacterId.Substring(first + 1, CharacterId.LastIndexOf(":") - first - 1);
            }
        }

        public string RealmId {
            get {
                return CharacterId.Substring(CharacterId.LastIndexOf(":"));
            }
        }
    }
}
