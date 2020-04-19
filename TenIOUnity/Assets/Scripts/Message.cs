/*
The MIT License

Copyright (c) 2016-2020 kong <congcoi123@gmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
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
