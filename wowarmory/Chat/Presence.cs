using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wowarmory.Chat {
    public class Presence {
        string type;
        public string Name;

        public bool Offline {
            get {
                return type != null && type.Contains("offline");
            }
        }

        public string Type {
            get {
                if (type == null)
                    return "unknown";
                return type.Substring(type.IndexOf("_") + 1);
            }
        }

        public Presence(Dictionary<string, object> response) {
            type = (string)response["presenceType"];
            var character = (Dictionary<string, object>)response["character"];
            Name = (string)character["n"];
        }
    }
}
