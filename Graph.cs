// Graphics library for Avalonia UI
//

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;


namespace HHSAdvAvalonia
{
    public class Canvas
    {
        private class Point
        {
            public int x { get; set; }
            public int y { get; set; }
            public Point(int x, int y) { this.x = x; this.y = y; }
        }
        private Avalonia.Media.Color[]? pixels = null;
        private WriteableBitmap? bitmap = null;
        public WriteableBitmap Bitmap { get { return bitmap!; } }
        private Image? image = null;
        private readonly Avalonia.Media.Color[] palette = {
            Colors.Black,
            Colors.Blue,
            Colors.Red,
            Colors.Magenta,
            Colors.Green,
            Colors.Cyan,
            Colors.Yellow,
            Colors.White,
            Colors.DarkGray,
            Colors.DarkBlue,
            Colors.DarkRed,
            Colors.DarkMagenta,
            Colors.DarkGreen,
            Colors.DarkCyan,
            Colors.DarkOrange,
            Colors.Gray
        };
        private static readonly float[] blueFilter = {
            0.0F, 0.0F, 0.1F,
            0.0F, 0.0F, 0.2F,
            0.0F, 0.0F, 0.7F,
        };

        private readonly float[] redFilter = {
            0.7F, 0.0F, 0.0F,
            0.2F, 0.0F, 0.0F,
            0.1F, 0.0F, 0.0F,
        };

        private readonly float[] sepiaFilter = {
            0.269021F, 0.527950F, 0.103030F,
            0.209238F, 0.410628F, 0.080135F,
            0.119565F, 0.234644F, 0.045791F,
        };


        public Canvas(int w, int h)
        {
            bitmap = new WriteableBitmap(new PixelSize(w, h), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);
            pixels = new Color[w * h];
        }
        public void Cls(Color c)
        {
            for (int i = 0; i < pixels!.Length; i++) pixels[i] = c;
            this.Draw();
        }

        private int GetColorIndex(Color c) =>
            ((c.B == 255) ? 1 : 0) + ((c.R == 255) ? 2 : 0) + ((c.G == 255) ? 4 : 0);
        public void Draw()
        {
            using (var fb = bitmap!.Lock())
            {
                unsafe
                {
                    byte* ptr = (byte*)fb.Address;
                    int stride = fb.RowBytes;
                    for (int y = 0; y < bitmap.Size.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Size.Width; x++)
                        {
                            int offset = y * stride + x * 4;
                            Color c = pixels![y * (int)bitmap.Size.Width + x];
                            ptr[offset + 0] = c.R;
                            ptr[offset + 1] = c.G;
                            ptr[offset + 2] = c.B;
                            ptr[offset + 3] = c.A;
                        }
                    }
                }
            }
        }
        public void Cls()
        {
            Cls(palette[0]);
        }
        public void Invalidate() => image!.Source = bitmap;

        public void pset(int x, int y, Color c)
        {
            if (x >= bitmap!.Size.Width || y >= bitmap!.Size.Height || x < 0 || y < 0) return;
            pixels![y * (int)bitmap!.Size.Width + x] = c;
            using (var fb = bitmap.Lock())
            {
                unsafe
                {
                    int offset = y * fb.RowBytes + x * 4;
                    byte* ptr = (byte*)fb.Address;
                    ptr[offset + 0] = c.R;
                    ptr[offset + 1] = c.G;
                    ptr[offset + 2] = c.B;
                    ptr[offset + 3] = c.A;
                }
            }
        }
        public void pset(int x, int y, int col) => pset(x, y, palette[col]);

        public Color pget(int x, int y)
        {
            if (x >= bitmap!.Size.Width || y >= bitmap!.Size.Height || x < 0 || y < 0) return palette[0];
            return pixels![y * (int)bitmap.Size.Width + x];
        }
        public void line(int x1, int y1, int x2, int y2, Color col)
        {
            int dx, ddx, dy, ddy;
            int wx, wy;
            int x, y;
            dy = y2 - y1;
            ddy = 1;
            if (dy < 0)
            {
                dy = -dy;
                ddy = -1;
            }
            wy = dy / 2;
            dx = x2 - x1;
            ddx = 1;
            if (dx < 0)
            {
                dx = -dx;
                ddx = -1;
            }
            wx = dx / 2;
            if (dx == 0 && dy == 0)
            {
                pset(x1, y1, col);
                return;
            }
            if (dy == 0)
            {
                for (x = x1; x != x2; x += ddx) pset(x, y1, col);
                pset(x2, y1, col);
                return;
            }
            if (dx == 0)
            {
                for (y = y1; y != y2; y += ddy) pset(x1, y, col);
                pset(x1, y2, col);
                return;
            }
            pset(x1, y1, col);
            if (dx > dy)
            {
                y = y1;
                for (x = x1; x != x2; x += ddx)
                {
                    pset(x, y, col);
                    wx -= dy;
                    if (wx < 0)
                    {
                        wx += dx;
                        y += ddy;
                    }
                }
            }
            else
            {
                x = x1;
                for (y = y1; y != y2; y += ddy)
                {
                    pset(x, y, col);
                    wy -= dx;
                    if (wy < 0)
                    {
                        wy += dy;
                        x += ddx;
                    }
                }
            }
            pset(x2, y2, col);
        }

        public void line(int x1, int y1, int x2, int y2, int col) => line(x1, y1, x2, y2, palette[col]);

        public void paint(int x, int y, Color f, Color b)
        {
            int l, r;
            int wx;
            Queue<Point> q = new Queue<Point>();
            Color c = pget(x, y);
            if (c.Equals(f) || c.Equals(b))
            {
                return;
            }
            q.Enqueue(new Point(x, y));
            while (q.Count > 0)
            {
                Point p = q.Dequeue();
                c = pget(p.x, p.y);
                if (c.Equals(f) || c.Equals(b)) continue;
                for (l = p.x - 1; l >= 0; l--)
                {
                    c = pget(l, p.y);
                    if (c.Equals(f) || c.Equals(b)) break;
                }
                ++l;
                for (r = p.x + 1; r < bitmap!.Size.Width; r++)
                {
                    c = pget(r, p.y);
                    if (c.Equals(f) || c.Equals(b)) break;
                }
                --r;
                line(l, p.y, r, p.y, f);
                for (wx = l; wx <= r; wx++)
                {
                    int uy = p.y - 1;
                    if (uy >= 0)
                    {
                        c = pget(wx, uy);
                        if (!c.Equals(f) && !c.Equals(b))
                        {
                            if (wx == r)
                            {
                                q.Enqueue(new Point(wx, uy));
                            }
                            else
                            {
                                c = pget(wx + 1, uy);
                                if (c.Equals(f) || c.Equals(b)) q.Enqueue(new Point(wx, uy));
                            }
                        }
                    }
                    int ly = p.y + 1;
                    if (ly < bitmap!.Size.Height)
                    {
                        c = pget(wx, ly);
                        if (!c.Equals(f) && !c.Equals(b))
                        {
                            if (wx == r)
                            {
                                q.Enqueue(new Point(wx, ly));
                            }
                            else
                            {
                                c = pget(wx + 1, ly);
                                if (c.Equals(f) || c.Equals(b)) q.Enqueue(new Point(wx, ly));
                            }
                        }
                    }
                }
            }
        }
        public void paint(int x, int y, int fc, int bc)
        {
            paint(x, y, palette[fc], palette[bc]);
        }
        public void tonePaint(byte[] tone, bool tiling = false)
        {
            Color[] pat = new Color[8];
            Color[] col = new Color[pat.Length];
            Array.Copy(palette, pat, pat.Length);
            Array.Copy(pat, col, pat.Length);
            int p = 0;
            int n = (int)tone[p++];
            for (int i = 1; i <= n; i++)
            {
                byte b = tone[p++], r = tone[p++], g = tone[p++];
                pat[i] = Color.FromArgb(255, r, g, b);
                b = 0;
                r = 0;
                g = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    byte mask = (byte)(1 << bit);
                    if ((pat[i].R & mask) != 0) r++;
                    if ((pat[i].G & mask) != 0) g++;
                    if ((pat[i].B & mask) != 0) b++;
                }
                if (r > 0) r = (byte)(r * 32 - 1);
                if (g > 0) g = (byte)(g * 32 - 1);
                if (b > 0) b = (byte)(b * 32 - 1);
                col[i] = Color.FromArgb(255, r, g, b);
            }
            for (int wy = 0; wy < bitmap!.Size.Height; wy++)
            {
                for (int wx = 0; wx < bitmap.Size.Width; wx++)
                {
                    Color c = pget(wx, wy);
                    int ci = GetColorIndex(c);
                    if (ci > 0 && ci <= n)
                    {
                        Color nc = col[ci];
                        if (tiling)
                        {
                            byte bits = (byte)(7 - wx % 8);
                            byte nr = 0, ng = 0, nb = 0;
                            if (((pat[ci].R >> bits) & 1) != 0) nr = 255;
                            if (((pat[ci].G >> bits) & 1) != 0) ng = 255;
                            if (((pat[ci].B >> bits) & 1) != 0) nb = 255;
                            nc = Color.FromArgb(255, nr, ng, nb);
                        }
                        pset(wx, wy, nc);
                    }
                }
            }
        }
        public enum FilterType
        {
            None,
            Blue,
            Red,
            Sepia
        };

        public FilterType ColorFilterType { get; set; } = FilterType.None;
        public void colorFilter()
        {
            float[] f;
            switch (ColorFilterType)
            {
                case FilterType.Blue:
                    f = blueFilter;
                    break;
                case FilterType.Red:
                    f = redFilter;
                    break;
                case FilterType.Sepia:
                    f = sepiaFilter;
                    break;
                default:
                    return;
            }
            using (var fb = bitmap!.Lock())
            {
                unsafe
                {
                    byte* ptr = (byte*)fb.Address;
                    int stride = fb.RowBytes;
                    for (int y = 0; y < bitmap.Size.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Size.Width; x++)
                        {
                            int o = y * (int)bitmap.Size.Width + x;
                            int offset = y * stride + x * 4;
                            pixels![o] = applyFilter(pixels[o], f);
                            ptr[offset + 0] = pixels[o].R;
                            ptr[offset + 1] = pixels[o].G;
                            ptr[offset + 2] = pixels[o].B;
                            ptr[offset + 3] = pixels[o].A;
                        }
                    }
                }
            }
        }
        private Color applyFilter(Color c, float[] f)
        {
            int r = (int)(c.R * f[0] + c.G * f[1] + c.B * f[2]);
            int g = (int)(c.R * f[3] + c.G * f[4] + c.B * f[5]);
            int b = (int)(c.R * f[6] + c.G * f[7] + c.B * f[8]);
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }
        public void drawRect(int x0, int y0, int x1, int y1, Color c)
        {
            line(x0, y0, x1, y0, c);
            line(x1, y0, x1, y1, c);
            line(x0, y1, x1, y1, c);
            line(x0, y1, x0, y0, c);
        }

        public void fillRect(int x0, int y0, int x1, int y1, Color c)
        {
            if (y0 > y1)
            {
                int y = y0;
                y0 = y1;
                y1 = y;
            }
            for (int y = y0; y <= y1; y++)
            {
                line(x0, y, x1, y, c);
            }
        }

        public Color GetPaletteColor(int i) => palette[i];
    }
}