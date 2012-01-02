using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace wowarmory.Network {
    public class BinaryReader: System.IO.BinaryReader {
        public BinaryReader(Stream stream): base(stream) {
        }

        public object Parse() {
            var type = ReadByte();

            switch (type) {
                case 1:
                    return ReadHash();
                case 2:
                    return ReadList();
                case 3:
                    return ReadInt32();
                case 4:
                    return ReadBytes(ReadInt32());
                case 5:
                    return ReadString();
                case 6:
                    return ReadBoolean();
                case 7:
                    return ReadInt64();
                default:
                    throw new IOException("Unrecognized type: " + type);
            }
        }

        public List<object> ReadList() {
            var item_count = ReadInt32();
            var result = new List<object>(item_count);

            for (int i = 0; i < item_count; i++) {
                result.Add(Parse());
            }

            return result;
        }

        public Dictionary<string, object> ReadHash() {
            var pair_count = ReadInt32();
            var result = new Dictionary<string, object>(pair_count);

            for (int i = 0; i < pair_count; i++) {
                var key = ReadString();
                result.Add(key, Parse());
            }

            return result;
        }    

        public override short ReadInt16() {
            var bytes = ReadBytes(2);
            return (short)((bytes[0] << 8) |
                            bytes[1]);
        }

        public override Int32 ReadInt32() {
            var bytes = ReadBytes(4);
            return (bytes[0] << 24) |
                   (bytes[1] << 16) |
                   (bytes[2] << 8) |
                    bytes[3];
        }

		public override Int64 ReadInt64() {
			var bytes = ReadBytes(8);
			var result =
				   (((Int64)bytes[0]) << 56) |
				   (((Int64)bytes[1]) << 48) |
				   (((Int64)bytes[2]) << 40) |
				   (((Int64)bytes[3]) << 32) |
				   (((Int64)bytes[4]) << 24) |
				   (((Int64)bytes[5]) << 16) |
				   (((Int64)bytes[6]) << 8) |
					((Int64)bytes[7]);
			return result;
		}

        public override string ReadString() {
            var length = ReadInt32();
            return Encoding.Default.GetString(ReadBytes(length));
        }
    }
}
