// ZWords -- High high school adventure Words DIctionary

using System.IO;
using System.Text;

namespace HHSAdvAvalonia
{
    public class ZWord
    {
        public string word { get; private set; }
        public int id { get; private set; }
        public ZWord()
        {
            word = string.Empty;
            id = -1;
        }

        public ZWord(byte[] b)
        {
            StringBuilder sb = new StringBuilder();
            word = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                char z = (char)(b[i] - 1);
                if (z == 0) break;
                sb.Append(z);
            }
            word = sb.ToString().ToLower();
            id = (int)b[4];
        }

        public bool IsMatch(string s)
        {
            string z = (s + "    ").Substring(0, 4).ToLower();
            return word == z;
        }

        public bool IsValid() => id >= 0;
    }

    public class ZWords
    {
        const int WORDS_MAX = 100;
        ZWord[] verbs = new ZWord[WORDS_MAX];
        ZWord[] objs = new ZWord[WORDS_MAX];

        public ZWords(string dictFileName)
        {
            if (!File.Exists(dictFileName)) return;
            using (var fs = new FileStream(dictFileName, FileMode.Open, FileAccess.Read))
            {
                int v = 0;
                using (var br = new BinaryReader(fs))
                {
                    int len = 0;
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        byte[] b = br.ReadBytes(5);
                        len += b.Length;
                        if (b[0] == 0 || len >= 0x200) break; ;
                        verbs[v++] = new ZWord(b);
                    }
                }
            }
            using (var fs = new FileStream(dictFileName, FileMode.Open, FileAccess.Read))
            {
                int o = 0;
                fs.Seek(0x200, SeekOrigin.Begin);
                using (var br = new BinaryReader(fs))
                {
                    int len = 0;
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        byte[] b = br.ReadBytes(5);
                        len += b.Length;
                        if (b[0] == 0 || len >= 0x200) break; ;
                        objs[o++] = new ZWord(b);
                    }
                }
            }
        }

        public int findVerb(string s)
        {
            for (int i = 0; i < WORDS_MAX; i++)
            {
                if (verbs[i] == null) return -1;
                if (verbs[i].IsValid() && verbs[i].IsMatch(s)) return verbs[i].id;
            }
            return -1;
        }
        public int findObj(string s)
        {
            for (int i = 0; i < WORDS_MAX; i++)
            {
                if (objs[i] == null) return -1;
                if (objs[i].IsValid() && objs[i].IsMatch(s)) return objs[i].id;
            }
            return -1;
        }
    }   
}