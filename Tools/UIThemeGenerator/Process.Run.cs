// 실행 진입점: 목록(theme_spec.txt)에 따라 시트를 가공하고 결과 목록 파일을 쓴다.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

public static partial class ThemeProc
{
    private sealed class SheetSpec
    {
        public string File;
        public int Rows;
        public int Cols;
        public string Kind;
        public string Folder;
        public string[] Names;
    }

    private static readonly Dictionary<string, int[]> BorderOverrides = new Dictionary<string, int[]>();
    private static readonly HashSet<string> NoCompact = new HashSet<string>();

    public static string Run(string rawDir, string specPath, string outRoot, string previewDir)
    {
        StringBuilder log = new StringBuilder();
        List<SheetSpec> sheets = new List<SheetSpec>();
        List<string[]> backgrounds = new List<string[]>();

        BorderOverrides.Clear();
        NoCompact.Clear();

        foreach (string rawLine in File.ReadAllLines(specPath, Encoding.UTF8))
        {
            string line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith("#")) { continue; }

            string[] f = line.Split('|');

            for (int i = 0; i < f.Length; i++) { f[i] = f[i].Trim(); }

            if (f[0] == "sheet" && f.Length >= 6)
            {
                SheetSpec sheet = new SheetSpec();
                string[] grid = f[2].Split('x');

                sheet.File = f[1];
                sheet.Rows = int.Parse(grid[0]);
                sheet.Cols = int.Parse(grid[1]);
                sheet.Kind = f[3];
                sheet.Folder = f[4];
                sheet.Names = f[5].Split(',');
                sheets.Add(sheet);
            }
            else if (f[0] == "bg" && f.Length >= 3)
            {
                backgrounds.Add(new string[] { f[1], f[2] });
            }
            else if (f[0] == "border" && f.Length >= 3)
            {
                string[] v = f[2].Split(',');
                BorderOverrides[f[1]] = new int[] { int.Parse(v[0]), int.Parse(v[1]), int.Parse(v[2]), int.Parse(v[3]) };
            }
            else if (f[0] == "nocompact" && f.Length >= 2)
            {
                NoCompact.Add(f[1]);
            }
        }

        Directory.CreateDirectory(previewDir);

        List<Piece> pieces = new List<Piece>();
        List<string> manifest = new List<string>();

        manifest.Add("# UI theme manifest. Written by Tools/UIThemeGenerator/process_theme.ps1. Do not edit by hand.");

        foreach (SheetSpec sheet in sheets)
        {
            string path = FindRaw(rawDir, sheet.File);

            if (path == null)
            {
                log.AppendLine("SKIP  " + sheet.File + " (not found)");
                continue;
            }

            Pix p = Pix.Load(path);
            string mode = RemoveBackground(p);

            log.AppendLine("SHEET " + Path.GetFileName(path) + "  " + p.W + "x" + p.H + "  background=" + mode);

            if (mode == "opaque")
            {
                log.AppendLine("    ERROR background could not be removed");
                continue;
            }

            List<Rectangle> cells = Split(p, sheet.Rows, sheet.Cols, log);
            List<Piece> made = new List<Piece>();

            for (int i = 0; i < cells.Count && i < sheet.Names.Length; i++)
            {
                if (cells[i].IsEmpty)
                {
                    log.AppendLine("    WARN empty cell " + (i + 1) + " (" + sheet.Names[i].Trim() + ")");
                    continue;
                }

                Piece piece = MakePiece(p.Crop(cells[i]), sheet, sheet.Names[i], log);
                string relative = piece.Folder + "/" + piece.Key + ".png";

                piece.Image.SavePng(Path.Combine(outRoot, relative));
                manifest.Add("sprite|" + piece.Key + "|" + relative + "|" + piece.L + "|" + piece.B + "|" + piece.R + "|" + piece.T + "|" + piece.Kind);
                log.AppendLine("    " + piece.Key + "  " + piece.Image.W + "x" + piece.Image.H + "  border L" + piece.L + " B" + piece.B + " R" + piece.R + " T" + piece.T);
                made.Add(piece);
            }

            SavePreview(previewDir, Path.GetFileNameWithoutExtension(sheet.File), made);
            pieces.AddRange(made);
        }

        foreach (string[] background in backgrounds)
        {
            string path = FindRaw(rawDir, background[0]);

            if (path == null)
            {
                log.AppendLine("SKIP  " + background[0] + " (not found)");
                continue;
            }

            Pix source = Pix.Load(path);
            string key = Path.GetFileNameWithoutExtension(background[0]);
            string relative = background[1] + "/" + key + ".jpg";

            SaveJpg(CropToAspect(source, 1920, 1080), Path.Combine(outRoot, relative), 92L);
            manifest.Add("background|" + key + "|" + relative);
            log.AppendLine("BG    " + Path.GetFileName(path) + "  " + source.W + "x" + source.H + " -> 1920x1080");
        }

        BuildAtlas(pieces, outRoot, manifest, log);

        string manifestPath = Path.Combine(outRoot, "UI/UIThemeManifest.txt");

        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
        File.WriteAllLines(manifestPath, manifest.ToArray(), new UTF8Encoding(false));
        log.AppendLine("MANIFEST " + (manifest.Count - 1) + " entries");
        File.WriteAllText(Path.Combine(previewDir, "report.txt"), log.ToString(), new UTF8Encoding(false));

        return log.ToString();
    }

    private static string Normalize(string name)
    {
        StringBuilder result = new StringBuilder();

        foreach (char c in Path.GetFileNameWithoutExtension(name).ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c)) { result.Append(c); }
        }

        return result.ToString();
    }

    // 이름의 대소문자, 밑줄, 폴더 차이를 무시하고 원본 파일을 찾는다.
    private static string FindRaw(string rawDir, string fileName)
    {
        if (!Directory.Exists(rawDir)) { return null; }

        string wanted = Normalize(fileName);

        foreach (string path in Directory.GetFiles(rawDir, "*.*", SearchOption.AllDirectories))
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();

            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg") { continue; }
            if (Normalize(path) == wanted) { return path; }
        }

        return null;
    }

    private static void SaveJpg(Pix p, string path, long quality)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        using (Bitmap source = p.ToBitmap())
        using (Bitmap flat = new Bitmap(p.W, p.H, PixelFormat.Format24bppRgb))
        {
            using (Graphics g = Graphics.FromImage(flat))
            {
                g.Clear(Color.Black);
                g.DrawImage(source, new Rectangle(0, 0, p.W, p.H));
            }

            ImageCodecInfo codec = null;

            foreach (ImageCodecInfo info in ImageCodecInfo.GetImageEncoders())
            {
                if (info.FormatID == ImageFormat.Jpeg.Guid) { codec = info; }
            }

            using (EncoderParameters parameters = new EncoderParameters(1))
            {
                parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                flat.Save(path, codec, parameters);
            }
        }
    }
}
