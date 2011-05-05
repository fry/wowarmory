using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wowarmory.Network {
    public class Response: Dictionary<string, object> {
        public readonly string Target;
        public readonly int Status;
        public readonly int Id;

        public static Response Parse(BinaryReader reader) {
            var length = reader.ReadInt32(); // Unused, guessed

            var status = reader.ReadInt16();
            var target = reader.ReadString();
            var id = reader.ReadInt32();

            var dict = (Dictionary<string, object>)reader.Parse();
            return new Response(target, status, id, dict);
        }

        public Response(string target, int status, int id, Dictionary<string, object> dict): base(dict) {
            Target = target;
            Status = status;
            Id = id;
        }
    }
}
