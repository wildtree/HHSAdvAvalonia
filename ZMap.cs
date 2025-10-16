// Map handler for HHS Adventure SDL

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace HHSAdvAvalonia
{
    public class ZMap
    {
        public class ZMapData
        {
            private class Message
            {
                private byte verbId, objId;
                private string message;
                public Message()
                {
                    verbId = 0;
                    objId = 0;
                    message = string.Empty;
                }
                public Message(byte v, byte o, string m)
                {
                    verbId = v;
                    objId = o;
                    message = m;
                }
                public bool IsValid() => verbId != 0 || objId != 0;
                public bool Match(byte v, byte o) => verbId == v && objId == o;
                public string GetMessage() => message;
            }
            private const int vectorSize = 0x400;
            private const int relSize = 0x100;
            private const int msgSize = 0x500;
            private const int maxMapElements = 0x100;
            private const int fileBlockSize = 0xa00;

            private byte[] vector;
            private Message[] messages;
            private bool blank;
            private string mm = string.Empty, bm = string.Empty;

            public ZMapData()
            {
                vector = new byte[vectorSize];
                messages = new Message[msgSize];
                blank = true;
                mm = string.Empty;
                bm = string.Empty;
            }
            public ZMapData(byte[] buf)
            {
                vector = new byte[vectorSize];
                messages = new Message[maxMapElements];
                Array.Copy(buf, 0, vector, 0, vectorSize);
                int m = vectorSize + relSize;
                int j = vectorSize;
                List<string> msgList = new List<string>();
                for (int n = 0; m < fileBlockSize; n++)
                {
                    int len = ((int)buf[m++] << 8) | buf[m++];
                    if (len == 0) break;
                    string s = Encoding.UTF8.GetString(buf, m, len);
                    msgList.Add(s);
                    m += len;
                }
                if (msgList.Count > 0) mm = msgList[0];
                for (int i = 0; i <= msgList.Count; i++)
                {
                    byte v = buf[j++];
                    byte o = buf[j++];
                    int idx = (int)buf[j++] - 1;
                    if (v == 0) break;
                    string s = string.Empty;
                    if (idx >= 0 && idx < msgList.Count) s = msgList[idx];
                    messages[i] = new Message(v, o, s);
                }
            }
            public bool IsBlank() => blank;
            public string GetMapMessage() => mm;

            public void SetBlank(string s) { blank = true; bm = s; }
            public void SetBlank() => blank = true;
            public void ResetBlank() => blank = false;

            public string Find(byte v, byte o)
            {
                foreach (var msg in messages)
                {
                    if (msg == null) continue;
                    if (msg.IsValid() && msg.Match(v, o))
                    {
                        return msg.GetMessage();
                    }
                }
                return string.Empty;
            }

            private int DrawOutline(Canvas c, int offset, Color col)
            {
                int x0 = vector[offset++];
                int y0 = vector[offset++];
                int x1 = 0, y1 = 0;
                while (true)
                {
                    x1 = vector[offset++];
                    y1 = vector[offset++];
                    if (y1 == 0xff)
                    {
                        if (x1 == 0xff) break;
                        x0 = vector[offset++];
                        y0 = vector[offset++];
                        continue;
                    }
                    c.line(x0, y0, x1, y1, col);
                    x0 = x1; y0 = y1;
                }
                return offset;
            }
            public void Draw(Canvas c)
            {
                if (blank)
                {
                    c.Cls();
                    return;
                }
                c.Cls(c.GetPaletteColor(1));
                int i = DrawOutline(c, vector[0] * 3 + 1, c.GetPaletteColor(7));
                int x0 = vector[i++];
                int y0 = vector[i++];
                while (x0 != 0xff || y0 != 0xff)
                {
                    c.paint(x0, y0, vector[i++], 7);
                    x0 = vector[i++];
                    y0 = vector[i++];
                }
                if (vector[i] == 0xff && vector[i + 1] == 0xff)
                {
                    i += 2;
                }
                else
                {
                    i = DrawOutline(c, i, c.GetPaletteColor(7));
                }
                if (vector[i] == 0xff && vector[i + 1] == 0xff)
                {
                    i += 2;
                }
                else
                {
                    i = DrawOutline(c, i, c.GetPaletteColor(0));
                }
                c.tonePaint(vector);
            }
        }

        private const int maxRooms = 100;
        private const int fileBlockSize = 0xa00;
        private ZMapData mapData = new ZMapData();
        private bool blank = false;
        private int p = 1, l = 0, v = 84;
        private string mapFileName = string.Empty;

        public ZMap(string fileName)
        {
            mapFileName = fileName;
        }
        public int Cursor
        {
            get => p;
            set
            {
                if (value < 0 || value >= maxRooms) throw new ArgumentOutOfRangeException("Invalid map index");
                p = value;
                ZCore.Instance.MapId = (byte)value;
                Load();
            }
        }
        public string Find(byte verb, byte obj)
        {
            return mapData.Find(verb, obj);
        }
        public bool IsBlank { get => mapData.IsBlank(); }
        public string Message { get => mapData.GetMapMessage(); }
        public int Look(int look)
        {
            v = p;
            p = look;
            return p;
        }
        public int Back()
        {
            p = v;
            return p;
        }
        public bool Load()
        {
            if (p == l) return true;
            using (var fs = new System.IO.FileStream(mapFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                byte[] buf = new byte[fileBlockSize];
                fs.Seek(p * fileBlockSize, System.IO.SeekOrigin.Begin);
                using (var br = new BinaryReader(fs))
                {
                    buf = br.ReadBytes(fileBlockSize);
                }
                mapData = new ZMapData(buf);
                if (p == 0 || p == 84 || p == 85)
                {
                    mapData.SetBlank();
                }
                blank = mapData.IsBlank();
                l = p;
            }
            return true;
        }

        public void Draw(Canvas cv)
        {
            mapData.Draw(cv);
        }
    }
}