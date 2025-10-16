// ZObject / ZTeacher for HHSAdvSDL

using System;
using System.Data.Common;
using System.IO;
using Avalonia.Media;

namespace HHSAdvAvalonia
{
    public class ZObjects
    {
        private const int blockSize = 0x200;
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    fs.Seek((value + 1) * blockSize, SeekOrigin.Begin);
                    byte[] b = new byte[blockSize];
                    using (var br = new BinaryReader(fs))
                    {
                        b = br.ReadBytes(b.Length);
                    }
                    obj = (value == 14) ? new ZTeacher(b) : new ZObject(b);
                    id = value;
                }
            }
        }
        private string fileName = string.Empty;
        public ZObjects(string f)
        {
            fileName = f;
        }
        public void Draw(Canvas cv, bool shift = false)
        {
            obj.Draw(cv, shift);
        }

        private int id = -1;
        private ZObject obj = new ZObject();
        private class ZObject
        {
            protected byte[] buf;
            public ZObject()
            {
                buf = new byte[blockSize];
            }
            public ZObject(byte[] b)
            {
                buf = new byte[blockSize];
                Array.Copy(b, buf, blockSize);
            }

            public virtual void Draw(Canvas cv, bool shift = false)
            {
                int offset = (shift) ? 256 : 0;
                draw(cv, true, offset);
                draw(cv, false, offset);
            }
            private void draw(Canvas cv, bool draft, int offset)
            {
                int o = offset;
                Color b = cv.GetPaletteColor(buf[o++]);
                if (draft) b = Color.FromArgb(255, 192, 192, 0);
                int xs = buf[o++] / 2;
                int ys = buf[o++];
                o = OutLine(cv, o, b, xs, ys);
                int x0 = buf[o++];
                int y0 = buf[o++];
                while (x0 != 0xff || y0 != 0xff)
                {
                    Color c = cv.GetPaletteColor(buf[o++]);
                    if (draft) c = b;
                    cv.paint(xs + x0, ys + y0, c, b);
                    x0 = buf[o++];
                    y0 = buf[o++];
                }
            }
            private int OutLine(Canvas cv, int offset, Color col, int ox, int oy)
            {
                int x0, y0, x1, y1;

                x0 = buf[offset++];
                y0 = buf[offset++];
                while (true)
                {
                    x1 = buf[offset++];
                    y1 = buf[offset++];
                    if (y1 == 0xff)
                    {
                        if (x1 == 0xff) break;
                        x0 = buf[offset++];
                        y0 = buf[offset++];
                        continue;
                    }
                    cv.line(x0 + ox, y0 + oy, x1 + ox, y1 + oy, col);
                    y0 = y1;
                    x0 = x1;
                }
                return offset;
            }
        }
        private class ZTeacher : ZObject
        {
            public ZTeacher() : base()
            {

            }
            public ZTeacher(byte[] b) : base(b)
            {

            }
            public override void Draw(Canvas cv, bool shift = false)
            {
                const int ox = -32;
                int x0, y0, x1, y1;
                int[] r1 = new int [] { 18, 24, 2, 2, 2, 22, 9, 0xffff };
                int[] r2 = new int [] { 148,14, 126, 6, 0, 0 };

                y0 = 63;
                int i = 0;
                for (i = 0 ; i <= 172 ; i += 2)
                {
                    x0 = buf[i];
                    x1 = buf[i + 1];
                    cv.line(ox + x0, y0, ox + x1, y0, 1);
                    ++y0;
                }
                int c = 0;
                for (int j = 0 ; r1[j] != 0xffff ; j++)
                {
                    c = buf[i++];
                    //if (c == TFT_RED) c = 0xf81f; //MAROON;
                    x0 = buf[i++];
                    y0 = buf[i++];
                    for (int k = 0 ; k <= r1[j] + 1 ; k++)
                    {
                        x1 = buf[i++];
                        y1 = buf[i++];
                        cv.line(ox + x0, y0, ox + x1, y1, c);
                        x0 = x1;
                        y0 = y1;
                    }
                    x0 = buf[i++];
                    y0 = buf[i++];
                    cv.paint(ox + x0, y0, c, c);
                }
                x0 = buf[i++];
                y0 = buf[i++];
                cv.paint(ox + x0, y0, c, c);
                for (int j = 120 ; j < 124 ; j++)
                {
                    cv.line(ox + j, 64, ox + j + 8, 110, 6);
                    cv.line(ox + j + 9, 110, ox + j + 11, 126, 7);
                }
                cv.line(ox + 125, 111, ox + 133, 109, 2);
                cv.line(ox + 133, 109, ox + 134, 110, 2);
                cv.line(ox + 134, 110, ox + 125, 112, 2);
                cv.line(ox + 125, 112, ox + 125, 111, 2);

                cv.line(ox + 120, 65, ox + 123, 64, 7);
                cv.line(ox + 123, 64, ox + 121, 62, 7);
                cv.line(ox + 121, 62, ox + 120, 65, 7);

                cv.paint(ox + 122, 63, 7, 7);

                for(int k = 0 ; r2[k + 1] != 0 ; k += 2)
                {
                    x0 = r2[k];
                    Color col = Color.FromArgb(255, 255, 192, 192);
                    for (int j = 0 ; j < r2[k + 1] ; j += 2)
                    {
                        y0 = buf[i++];
                        y1 = buf[i++];
                        cv.line(ox + x0, y0, ox + x0, y1, col);
                        ++x0;
                        cv.line(ox + x0, y0, ox + x0, y1, col);
                        ++x0;
                        y0 = buf[i++];
                        y1 = buf[i++];
                        cv.line(ox + x0, y0, ox + x0, y1, col);
                        ++x0;
                    }
                }
                cv.drawRect(ox + 148, 78, ox + 164, 84, cv.GetPaletteColor(0));
                cv.fillRect(ox + 149, 79, ox + 163, 83, cv.GetPaletteColor(7));
                cv.fillRect(ox + 155, 78, ox + 156, 84, cv.GetPaletteColor(0));

                while(true)
                {
                    x1 = buf[i++];
                    y1 = buf[i++];
                    if (y1 == 0xff)
                    {
                        if (x1 == 0xff) break;
                        x0 = buf[i++];
                        y0 = buf[i++];
                        continue;
                    }
                    cv.line(ox + x0, y0, ox + x1, y1, 0);
                    x0 = x1;
                    y0 = y1;
                }
            }
        }
    }
}