// UI 테마 원본 시트(GPT 생성 이미지)를 Unity용 조각으로 가공한다. (GDI+)
// Unity가 컴파일하지 않도록 Assets 폴더 밖에 둔다. process_theme.ps1로 실행한다.
// 하는 일: 배경 제거 -> 시트 분할 -> 여백 다듬기 -> 9분할 경계 계산 -> 크기 정리 -> 목록 파일 작성
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public static partial class ThemeProc
{
    public const int OpaqueAlpha = 24; // 이 값보다 불투명해야 그림으로 본다.

    // 32비트 픽셀 버퍼 (B, G, R, A 순서)
    public sealed class Pix
    {
        public int W;
        public int H;
        public byte[] D;

        public Pix(int w, int h)
        {
            W = w;
            H = h;
            D = new byte[w * h * 4];
        }

        public static Pix Load(string path)
        {
            using (Bitmap file = new Bitmap(path))
            {
                return FromBitmap(file);
            }
        }

        public static Pix FromBitmap(Bitmap source)
        {
            Pix p = new Pix(source.Width, source.Height);
            Rectangle area = new Rectangle(0, 0, p.W, p.H);

            using (Bitmap copy = new Bitmap(p.W, p.H, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(copy))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(source, area, 0, 0, p.W, p.H, GraphicsUnit.Pixel);
                }

                BitmapData data = copy.LockBits(area, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                for (int y = 0; y < p.H; y++)
                {
                    Marshal.Copy(new IntPtr(data.Scan0.ToInt64() + (long)y * data.Stride), p.D, y * p.W * 4, p.W * 4);
                }

                copy.UnlockBits(data);
            }

            return p;
        }

        public Bitmap ToBitmap()
        {
            Bitmap result = new Bitmap(W, H, PixelFormat.Format32bppArgb);
            Rectangle area = new Rectangle(0, 0, W, H);
            BitmapData data = result.LockBits(area, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            for (int y = 0; y < H; y++)
            {
                Marshal.Copy(D, y * W * 4, new IntPtr(data.Scan0.ToInt64() + (long)y * data.Stride), W * 4);
            }

            result.UnlockBits(data);
            return result;
        }

        public void SavePng(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (Bitmap b = ToBitmap()) { b.Save(path, ImageFormat.Png); }
        }

        public int Alpha(int x, int y) { return D[(y * W + x) * 4 + 3]; }

        public bool Solid(int x, int y) { return D[(y * W + x) * 4 + 3] > OpaqueAlpha; }

        public Pix Crop(Rectangle r)
        {
            Pix result = new Pix(r.Width, r.Height);

            for (int y = 0; y < r.Height; y++)
            {
                Buffer.BlockCopy(D, ((r.Y + y) * W + r.X) * 4, result.D, y * r.Width * 4, r.Width * 4);
            }

            return result;
        }

        // 고품질 크기 조정. 투명 가장자리가 어두워지지 않도록 가장자리를 접어서 읽는다.
        public Pix Resize(int w, int h)
        {
            if (w == W && h == H) { return Crop(new Rectangle(0, 0, W, H)); }

            using (Bitmap source = ToBitmap())
            using (Bitmap target = new Bitmap(w, h, PixelFormat.Format32bppPArgb))
            {
                using (Graphics g = Graphics.FromImage(target))
                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetWrapMode(WrapMode.TileFlipXY);
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawImage(source, new Rectangle(0, 0, w, h), 0, 0, W, H, GraphicsUnit.Pixel, attributes);
                }

                return FromBitmap(target);
            }
        }

        // 조각을 더 큰 투명 캔버스 가운데에 놓는다.
        public Pix Pad(int w, int h)
        {
            Pix result = new Pix(w, h);
            int offsetX = (w - W) / 2;
            int offsetY = (h - H) / 2;

            for (int y = 0; y < H; y++)
            {
                Buffer.BlockCopy(D, y * W * 4, result.D, ((offsetY + y) * w + offsetX) * 4, W * 4);
            }

            return result;
        }

        public Rectangle Bounds()
        {
            return BoundsIn(new Rectangle(0, 0, W, H));
        }

        // 지정한 영역 안에서 그림이 있는 범위를 구한다.
        public Rectangle BoundsIn(Rectangle area)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;

            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    if (!Solid(x, y)) { continue; }

                    if (x < minX) { minX = x; }
                    if (x > maxX) { maxX = x; }
                    if (y < minY) { minY = y; }
                    if (y > maxY) { maxY = y; }
                }
            }

            if (maxX < 0) { return Rectangle.Empty; }

            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }
    }
}
