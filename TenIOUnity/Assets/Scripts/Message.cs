using System;
using System.IO;

public class Message {
    public ushort length { get; set; }
    public byte[] content { get; set; }

    public static Message ReadFromStream(BinaryReader reader) {
        ushort len;
        byte[] len_buf;
        byte[] buffer;

        len_buf = reader.ReadBytes(2);
        if (len_buf.Length > 0) {
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(len_buf);
            }
            len = BitConverter.ToUInt16(len_buf, 0);

            buffer = reader.ReadBytes(len);

            return new Message(buffer);
        }

        return null;
    }

    public void WriteToStream(BinaryWriter writer) {
        byte[] len_bytes = BitConverter.GetBytes(length);

        if (BitConverter.IsLittleEndian) {
            Array.Reverse(len_bytes);
        }
        writer.Write(len_bytes);

        writer.Write(content);
    }

    public Message(byte[] data) {
        length= (ushort)data.Length;
        content = data;
    }
}
