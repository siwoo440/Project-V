// 조각 종류별 가공과 아이콘 묶음(아틀라스) 작성
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static partial class ThemeProc
{
    private const int NineMaxSide = 512;  // 9분할 패널의 최대 변 길이
    private const int ThreeMaxHeight = 160; // 좌우로 늘어나는 막대의 최대 높이
    private const int NineMiddle = 64;    // 패널을 줄인 뒤 남기는 가운데 길이
    private const int ThreeMiddle = 96;   // 막대를 줄인 뒤 남기는 가운데 길이
    private const int IconSize = 256;
    private const int AtlasSize = 1024;
    private const int AtlasCell = 120;
    private const int AtlasPitch = 128;

    // 이름 표기: Key, Key=글자아이콘이름, Key:종류, Key=이름:종류
    private static Piece MakePiece(Pix crop, SheetSpec sheet, string nameSpec, StringBuilder log)
    {
        Piece piece = new Piece();
        string[] kindParts = nameSpec.Trim().Split(':');
        string[] nameParts = kindParts[0].Split('=');

        piece.Key = nameParts[0].Trim();
        piece.TmpName = nameParts.Length > 1 ? nameParts[1].Trim() : string.Empty;
        piece.Kind = kindParts.Length > 1 ? kindParts[1].Trim() : sheet.Kind;
        piece.Folder = sheet.Folder;

        bool compact = !NoCompact.Contains(piece.Key);
        int[] manual;
        bool hasManual = BorderOverrides.TryGetValue(piece.Key, out manual);

        if (piece.Kind == "nine")
        {
            int left, right, top, bottom;

            piece.Source = FitMax(crop, NineMaxSide);
            EstimateCaps(piece.Source, true, out left, out right);
            EstimateCaps(piece.Source, false, out top, out bottom);

            piece.L = piece.R = Math.Max(left, right);
            piece.T = piece.B = Math.Max(top, bottom);

            if (hasManual) { piece.L = manual[0]; piece.B = manual[1]; piece.R = manual[2]; piece.T = manual[3]; }

            WarnWideCaps(piece.Key, piece.L + piece.R, piece.Source.W, log);
            WarnWideCaps(piece.Key, piece.T + piece.B, piece.Source.H, log);

            piece.Image = compact
                ? CompactV(CompactH(piece.Source, piece.L, piece.R, NineMiddle), piece.T, piece.B, NineMiddle)
                : piece.Source;
        }
        else if (piece.Kind == "three")
        {
            int left, right;

            piece.Source = FitHeight(crop, ThreeMaxHeight);
            EstimateCaps(piece.Source, true, out left, out right);

            piece.L = piece.R = Math.Max(left, right);

            if (hasManual) { piece.L = manual[0]; piece.R = manual[2]; }

            WarnWideCaps(piece.Key, piece.L + piece.R, piece.Source.W, log);

            piece.Image = compact ? CompactH(piece.Source, piece.L, piece.R, ThreeMiddle) : piece.Source;
        }
        else if (piece.Kind == "card")
        {
            float ratio = crop.W / (float)crop.H;

            if (ratio < 0.61f || ratio > 0.72f)
            {
                log.AppendLine("    WARN " + piece.Key + " is not 2:3 (ratio " + ratio.ToString("0.00") + ")");
            }

            piece.Source = piece.Image = crop.Resize(400, 600);

            int filled = FillInterior(piece.Image);

            if (filled > 0) { log.AppendLine("    FILL " + piece.Key + " interior " + filled + " px"); }
        }
        else if (piece.Kind == "icon")
        {
            int side = (int)(Math.Max(crop.W, crop.H) * 1.06f) + 2;

            piece.Source = piece.Image = crop.Pad(side, side).Resize(IconSize, IconSize);
        }
        else if (piece.Kind == "strip")
        {
            piece.Source = piece.Image = FitMax(crop, 1200); // 통째로 쓰는 긴 장식
        }
        else
        {
            piece.Source = piece.Image = FitMax(crop, NineMaxSide); // emblem
        }

        return piece;
    }

    private static void WarnWideCaps(string key, int caps, int length, StringBuilder log)
    {
        if (caps < length * 0.85f) { return; }

        log.AppendLine("    WARN " + key + " has almost no plain middle (caps " + caps + " of " + length + "): check the preview");
    }

    // 글자 사이에 넣을 아이콘을 한 장으로 모은다. (TextMeshPro 스프라이트용)
    private static void BuildAtlas(List<Piece> pieces, string outRoot, List<string> manifest, StringBuilder log)
    {
        List<Piece> icons = pieces.FindAll(delegate(Piece piece)
        {
            return piece.Kind == "icon" && piece.TmpName.Length > 0;
        });

        if (icons.Count == 0) { return; }

        int perRow = AtlasSize / AtlasPitch;

        if (icons.Count > perRow * perRow)
        {
            log.AppendLine("WARN  atlas is full: " + icons.Count + " icons, room for " + (perRow * perRow));
            icons.RemoveRange(perRow * perRow, icons.Count - perRow * perRow);
        }

        Pix atlas = new Pix(AtlasSize, AtlasSize);
        string relative = "UI/Atlases/UI_IconAtlas_01.png";

        manifest.Add("atlas|" + relative + "|" + AtlasSize + "|" + AtlasSize);

        for (int i = 0; i < icons.Count; i++)
        {
            Pix small = icons[i].Image.Resize(AtlasCell, AtlasCell);
            int x = (i % perRow) * AtlasPitch + (AtlasPitch - AtlasCell) / 2;
            int y = (i / perRow) * AtlasPitch + (AtlasPitch - AtlasCell) / 2;

            for (int row = 0; row < AtlasCell; row++)
            {
                Buffer.BlockCopy(small.D, row * AtlasCell * 4, atlas.D, ((y + row) * AtlasSize + x) * 4, AtlasCell * 4);
            }

            manifest.Add("glyph|" + icons[i].TmpName + "|" + x + "|" + y + "|" + AtlasCell + "|" + AtlasCell);
        }

        atlas.SavePng(Path.Combine(outRoot, relative));
        log.AppendLine("ATLAS " + icons.Count + " icons -> " + relative);
    }
}
