using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace wowarmory.Network {
    public class Request: Dictionary<string, object> {
        static int id_counter = 0;
        
        public readonly string Target;
        public readonly int Id;

        public Request(string target) {
            this.Target = target;
            Id = id_counter;
            id_counter++;
        }

        public void WriteTo(BinaryWriter writer) {
            writer.WriteString(Target);
            writer.WriteInt32(Id);
            
            foreach (var entry in this) {
                if (entry.Value is IConvertible) {
                    writer.WriteByte(5);
                    writer.WriteString(entry.Key);
                    writer.WriteString(Convert.ToString(entry.Value));
                } else {
                    writer.WriteByte(4);
                    writer.WriteString(entry.Key);
                    // Write to temporary memory stream to determine length of data
                    var stream = new MemoryStream();
                    var temp_writer = new BinaryWriter(stream);
                    temp_writer.Write(entry.Value);

                    var length = (int)stream.Length;
                    writer.WriteInt32(length);
                    writer.WriteBytes(stream.GetBuffer(), 0, length);
                }

                writer.WriteByte(0xFF);
            }

            writer.WriteByte(0xFF);
        }
    }
}
