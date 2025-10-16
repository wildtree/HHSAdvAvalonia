// High high School Adventure SDL -- User data

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace HHSAdvAvalonia
{
    public class ZUserData
    {
        public class ZMapLink
        {
            private byte[] raw = new byte[8]; // 8 bytes
            public ZMapLink() { raw = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }; }
            public ZMapLink(byte[] b)
            {
                if (b.Length != 8) throw new ArgumentException("ZMapLink must be 8 bytes length");
                raw = b;
            }
            public void set(byte dir, byte map_id)
            {
                if (dir > 7) throw new ArgumentException("Invalid direction");
                raw[dir] = map_id;
            }
            public byte get(byte dir)
            {
                if (dir > 7) throw new ArgumentException("Invalid direction");
                return raw[dir];
            }

            public byte[] pack() => raw;
            public void unpack(byte[] b)
            {
                if (b.Length != 8) throw new ArgumentException("ZMapLink must be 8 bytes length");
                raw = b;
            }
            public byte N { get { return raw[0];} set { raw[0] = value; }}
            public byte S { get { return raw[1];} set { raw[1] = value; }}
            public byte W { get { return raw[2];} set { raw[2] = value; }}
            public byte E { get { return raw[3];} set { raw[3] = value; }}
            public byte U { get { return raw[4];} set { raw[4] = value; }}
            public byte D { get { return raw[5];} set { raw[5] = value; }}
            public byte I { get { return raw[6];} set { raw[6] = value; }}
            public byte O { get { return raw[7];} set { raw[7] = value; }}
        }
        public const int Links = 87;
        public const int Items = 12;
        public const int Flags = 15;
        private ZMapLink[] links = new ZMapLink[Links];
        private byte[] places = new byte[Items];
        private byte[] flags = new byte[Flags];

        private readonly string[] itemLabels = new string[]
        {
            "ネクタイ",
            "制服",
            "鍵",
            "懐中電灯",
            "乾電池",
            "ビデオテープ",
            "ファイル",
            "ダイナマイト",
            "塩酸",
            "ジャッキ",
            "マッチ",
            "ペンチ",
        };
        private ZUserData()
        {
        }

        public void load(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    for (int i = 0; i < 87; i++)
                    {
                        links[i] = new ZMapLink(br.ReadBytes(8));
                    }
                }
            }
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(0x301, SeekOrigin.Begin);
                using (var br = new BinaryReader(fs))
                {
                    places = br.ReadBytes(12);
                }
            }
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(0x311, SeekOrigin.Begin);
                using (var br = new BinaryReader(fs))
                {
                    flags = br.ReadBytes(15);
                }
            }
        }

        private static ZUserData? instance = null;
        public static ZUserData Instance
        {
            get
            {
                if (instance == null) instance = new ZUserData();
                return instance;
            }
        }

        public byte[] pack()
        {
            byte[] buf = new byte[packedSize];
            for (int i = 0; i < 87; i++)
            {
                Array.Copy(links[i].pack(), 0, buf, i * 8, 8);
            }
            Array.Copy(places, 0, buf, 87 * 8, 12);
            Array.Copy(flags, 0, buf, 87 * 8 + 12, 15);
            return buf;
        }
        public void unpack(byte[] buf)
        {
            for (int i = 0; i < 87; i++)
            {
                byte[] b = new byte[8];
                Array.Copy(buf, i * 8, b, 0, 8);
                links[i].unpack(b);
            }
            Array.Copy(buf, 87 * 8, places, 0, 12);
            Array.Copy(buf, 87 * 8 + 12, flags, 0, 15);
        }

        public int packedSize { get { return 12 + 15 + 87 * 8; } }


        public ZMapLink getLink(int map_id)
        {
            if (map_id >= links.Length) throw new ArgumentException("Invalid map_id");
            return links[map_id];
        }
        public void setFact(int id, byte value)
        {
            if (id >= flags.Length) throw new ArgumentException("Invalid flag id");
            flags[id] = value;
        }
        public byte getFact(int id)
        {
            if (id >= flags.Length) throw new ArgumentException("Invalid flag id");
            return flags[id];
        }
        public void setPlace(int id, byte value)
        {
            if (id >= places.Length) throw new ArgumentException("Invalid place id");
            places[id] = value;
        }
        public byte getPlace(int id)
        {
            if (id >= places.Length) throw new ArgumentException("Invalid place id");
            return places[id];
        }
        public string getItemList()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < places.Length; i++)
            {
                if (places[i] == 0xff)
                {
                    sb.Append(itemLabels[i]);
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }
    }
}