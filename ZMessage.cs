// Message class for HHSAdvSDL

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HHSAdvAvalonia
{
    public class ZMessage
    {
        public const int MaxMessage = 127;
        private string[] messages = new string[MaxMessage];
        private string fileName = string.Empty;

        public ZMessage(string f)
        {
            fileName = f;
            int i = 0;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    List<byte> buf = new List<byte>();
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        byte c = br.ReadByte();
                        if (c == 0)
                        {
                            messages[i++] = Encoding.UTF8.GetString(buf.ToArray());
                            buf.Clear();
                            continue;
                        }
                        buf.Add(c);
                    }
                }
            }
            while (i < MaxMessage) messages[i++] = string.Empty;
        }

        public string GetMessage(int id)
        {
            id &= 0x7f;
            if (id < 0 || id >= MaxMessage) return string.Empty;
            return messages[id];
        }
    }
}