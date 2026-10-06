// 미리보기: 조각을 실제 화면에서 쓰는 크기로 늘려 그린 확인용 그림을 만든다.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static partial class ThemeProc
{
    private static readonly Color LightTile = Color.FromArgb(255, 251, 242, 216);
    private static readonly Color DarkTile = Color.FromArgb(255, 22, 38, 79);

    private static void SavePreview(string previewDir, string sheetName, List<Piece> pieces)
    {
        if (pieces.Count == 0) { return; }

        const int rowHeight = 250;
        const int width = 1600;
        int height = pieces.Count * rowHeight + 10;

        using (Bitmap canvas = new Bitmap(width, height, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(canvas))
        using (Font font = new Font("Consolas", 15f, FontStyle.Bold, GraphicsUnit.Pixel))
        using (SolidBrush tileA = new SolidBrush(Color.FromArgb(255, 150, 156, 168)))
        using (SolidBrush tileB = new SolidBrush(Color.FromArgb(255, 122, 128, 140)))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            for (int y = 0; y < height; y += 20)
            {
                for (int x = 0; x < width; x += 20)
                {
                    g.FillRectangle(((x + y) / 20) % 2 == 0 ? tileA : tileB, x, y, 20, 20);
                }
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                Piece piece = pieces[i];
                int top = i * rowHeight + 6;
                string label = piece.Key + "  " + piece.Image.W + "x" + piece.Image.H + "  " + piece.Kind +
                               "  L" + piece.L + " B" + piece.B + " R" + piece.R + " T" + piece.T +
                               "  (source " + piece.Source.W + "x" + piece.Source.H + ")";

                g.DrawString(label, font, Brushes.Black, 11f, top + 1f);
                g.DrawString(label, font, Brushes.White, 10f, top);

                using (Bitmap source = piece.Source.ToBitmap())
                using (Bitmap image = piece.Image.ToBitmap())
                {
                    DrawFit(g, source, new RectangleF(10, top + 26, 340, 210));
                    DrawSamples(g, piece, image, top + 26);
                }
            }

            canvas.Save(Path.Combine(previewDir, sheetName + "_preview.png"), ImageFormat.Png);
        }
    }

    private static void DrawSamples(Graphics g, Piece piece, Bitmap image, int top)
    {
        if (piece.Kind == "nine")
        {
            float cap = Math.Max(1, Math.Max(piece.L, piece.T));

            DrawSliced(g, image, new RectangleF(380, top, 360, 210), piece, 40f / cap);
            DrawSliced(g, image, new RectangleF(760, top, 520, 150), piece, 40f / cap);
            DrawSliced(g, image, new RectangleF(1300, top, 260, 90), piece, 24f / cap);
            DrawSliced(g, image, new RectangleF(1300, top + 100, 130, 110), piece, 16f / cap);
            return;
        }

        if (piece.Kind == "three")
        {
            DrawSliced(g, image, new RectangleF(380, top, 620, 84), piece, 84f / image.Height);
            DrawSliced(g, image, new RectangleF(380, top + 96, 300, 64), piece, 64f / image.Height);
            DrawSliced(g, image, new RectangleF(700, top + 96, 180, 44), piece, 44f / image.Height);
            DrawSliced(g, image, new RectangleF(900, top + 96, 90, 28), piece, 28f / image.Height);
            DrawSliced(g, image, new RectangleF(1020, top, 520, 28), piece, 28f / image.Height);
            DrawSliced(g, image, new RectangleF(1020, top + 40, 1920 * 0.28f, 110 * 0.28f), piece, 110f * 0.28f / image.Height);
            return;
        }

        using (SolidBrush light = new SolidBrush(LightTile))
        using (SolidBrush dark = new SolidBrush(DarkTile))
        {
            g.FillRectangle(light, 380, top, 560, 210);
            g.FillRectangle(dark, 960, top, 560, 210);
        }

        if (piece.Kind == "card")
        {
            DrawFit(g, image, new RectangleF(400, top + 5, 133, 200));
            DrawFit(g, image, new RectangleF(560, top + 35, 93, 140));
            DrawFit(g, image, new RectangleF(980, top + 5, 133, 200));
            DrawFit(g, image, new RectangleF(1140, top + 35, 93, 140));
            return;
        }

        float[] sizes = piece.Kind == "icon" ? new float[] { 128f, 72f, 44f, 28f } : new float[] { 200f, 110f, 60f, 0f };
        float x = 20f;

        foreach (float size in sizes)
        {
            if (size <= 0f) { continue; }

            DrawFit(g, image, new RectangleF(380 + x, top + (210 - size) / 2f, size, size));
            DrawFit(g, image, new RectangleF(960 + x, top + (210 - size) / 2f, size, size));
            x += size + 24f;
        }
    }

    // 비율을 지키며 영역 안에 맞춰 그린다.
    private static void DrawFit(Graphics g, Bitmap image, RectangleF area)
    {
        float scale = Math.Min(area.Width / image.Width, area.Height / image.Height);
        float w = image.Width * scale;
        float h = image.Height * scale;

        g.DrawImage(image, new RectangleF(area.X + (area.Width - w) / 2f, area.Y + (area.Height - h) / 2f, w, h));
    }

    // Unity의 Sliced 이미지처럼 끝 장식은 유지하고 가운데만 늘려 그린다.
    private static void DrawSliced(Graphics g, Bitmap image, RectangleF dest, Piece piece, float capScale)
    {
        float left = piece.L * capScale, right = piece.R * capScale;
        float top = piece.T * capScale, bottom = piece.B * capScale;

        if (left + right > dest.Width) { float f = dest.Width / (left + right); left *= f; right *= f; }
        if (top + bottom > dest.Height) { float f = dest.Height / (top + bottom); top *= f; bottom *= f; }

        float[] dx = { dest.Left, dest.Left + left, dest.Right - right, dest.Right };
        float[] dy = { dest.Top, dest.Top + top, dest.Bottom - bottom, dest.Bottom };
        int[] sx = { 0, piece.L, image.Width - piece.R, image.Width };
        int[] sy = { 0, piece.T, image.Height - piece.B, image.Height };

        using (ImageAttributes attributes = new ImageAttributes())
        {
            attributes.SetWrapMode(WrapMode.TileFlipXY);

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float dw = dx[col + 1] - dx[col], dh = dy[row + 1] - dy[row];
                    int sw = sx[col + 1] - sx[col], sh = sy[row + 1] - sy[row];

                    if (dw <= 0f || dh <= 0f || sw <= 0 || sh <= 0) { continue; }

                    PointF[] corners =
                    {
                        new PointF(dx[col], dy[row]),
                        new PointF(dx[col] + dw, dy[row]),
                        new PointF(dx[col], dy[row] + dh),
                    };

                    g.DrawImage(image, corners, new RectangleF(sx[col], sy[row], sw, sh), GraphicsUnit.Pixel, attributes);
                }
            }
        }
    }
}
