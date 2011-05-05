using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace wowarmory.Network {
    public class BinaryWriter {
        System.IO.BinaryWriter writer;
        public BinaryWriter(Stream stream) {
            writer = new System.IO.BinaryWriter(stream);
        }

        public void Write(object obj) {
            if (obj is string) {
                WriteByte(5);
                WriteString((string)obj);
            } else if (obj is int) {
                WriteByte(3);
                WriteInt32((int)obj);
            } else if (obj is long) {
                WriteByte(7);
                WriteInt64((long)obj);
            } else if (obj is IDictionary) {
                var dict = (IDictionary)obj;
                WriteByte(1);
                WriteInt32(dict.Count);

                foreach (DictionaryEntry entry in dict) {
                    var key = entry.Key as string;
                    if (key == null)
                        continue;
                    WriteString(key); // TODO: make sure key actually is a string
                    Write(entry.Value);
                }
            } else if (obj is byte[]) { // byte[] before the more general IList
                var bytes = (byte[])obj;
                WriteByte(4);
                WriteInt32(bytes.Length);
                writer.Write(bytes);
            } else if (obj is IList) {
                var list = (IList)obj;
                WriteByte(2);
                WriteInt32(list.Count);
                foreach (var entry in list) {
                    Write(entry);
                }
            } else if (obj is bool) {
                WriteByte(6);
                writer.Write((bool)obj);
            }
        }

        public void WriteInt16(short num) {
            writer.Write((byte)((num >> 8) & 0xFF));
            writer.Write((byte)(num & 0xFF));
        }

        public void WriteInt32(Int32 num) {
            writer.Write((byte)((num >> 24) & 0xFF));
            writer.Write((byte)((num >> 16) & 0xFF));
            writer.Write((byte)((num >> 8) & 0xFF));
            writer.Write((byte)(num & 0xFF));
        }

        public void WriteInt64(Int64 num) {
            //base.Write((Int64)EndianUtil.SwapBitShift(num));
            writer.Write((byte)((num >> 56) & 0xFF));
            writer.Write((byte)((num >> 48) & 0xFF));
            writer.Write((byte)((num >> 40) & 0xFF));
            writer.Write((byte)((num >> 32) & 0xFF));
            writer.Write((byte)((num >> 24) & 0xFF));
            writer.Write((byte)((num >> 16) & 0xFF));
            writer.Write((byte)((num >> 8) & 0xFF));
            writer.Write((byte)(num & 0xFF));
        }

        public void WriteByte(byte val) {
            writer.Write(val);
        }

        public void WriteBytes(byte[] val) {
            writer.Write(val);
        }

        public void WriteBytes(byte[] val, int index, int length) {
            writer.Write(val, index, length);
        }

        public void WriteString(string str) {
            WriteInt32(Encoding.Default.GetByteCount(str));
            WriteBytes(Encoding.Default.GetBytes(str));
        }
    }
}
