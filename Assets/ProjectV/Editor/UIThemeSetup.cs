using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

// UI 테마 이미지 가져오기 (기획서 12.14 / 12.17)
// Tools/UIThemeGenerator가 Art 폴더에 넣은 이미지와 목록 파일을 읽어
// 가져오기 설정을 맞추고 UITheme 에셋과 글자 사이 아이콘 묶음을 만든다.
public static partial class UIThemeSetup
{
    private const string ArtRoot = "Assets/ProjectV/Art/";
    private const string ManifestPath = ArtRoot + "UI/UIThemeManifest.txt";
    private const string ResourcesFolder = "Assets/ProjectV/Resources";
    private const string ThemeAssetPath = ResourcesFolder + "/UITheme.asset";
    private const string IconFolder = ResourcesFolder + "/Sprite Assets";
    private const string IconAssetPath = IconFolder + "/UIIcons.asset";

    private const float IconScale = 1.2f;         // 글자 높이 대비 아이콘 크기
    private const float IconBaselineRatio = 0.8f; // 아이콘 높이 중 기준선 위로 올라가는 비율
    private const float IconAdvanceRatio = 1.05f; // 아이콘 너비 대비 다음 글자까지의 간격

    private class GlyphInfo // 아이콘 묶음 안의 아이콘 한 칸
    {
        public string name;
        public int x;
        public int y; // 이미지 위쪽 기준
        public int width;
        public int height;
    }

    [MenuItem("Project V/UI 테마 이미지 적용", false, 11)]
    public static void RefreshFromMenu()
    {
        UITheme theme = Refresh();

        Debug.Log(
            theme == null
                ? "UI 테마 이미지가 없습니다. Tools/UIThemeGenerator/process_theme.ps1을 먼저 실행하세요."
                : $"UI 테마를 갱신했습니다. 이미지 {theme.SpriteCount}개"
        );
    }

    // 목록 파일을 읽어 테마를 갱신한다. 이미지가 없으면 null을 돌려준다.
    public static UITheme Refresh()
    {
        AssetDatabase.Refresh(); // 도구가 방금 넣은 파일 반영

        if (!File.Exists(ManifestPath))
        {
            UITheme existing =
                AssetDatabase.LoadAssetAtPath<UITheme>(ThemeAssetPath);

            UITheme.SetCurrent(existing);

            return existing;
        }

        List<UITheme.SpriteEntry> entries = new List<UITheme.SpriteEntry>();
        List<GlyphInfo> glyphs = new List<GlyphInfo>();
        string atlasPath = string.Empty;

        foreach (string rawLine in File.ReadAllLines(ManifestPath))
        {
            string line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith("#")) { continue; }

            string[] fields = line.Split('|');

            if (fields[0] == "sprite" && fields.Length >= 7)
            {
                string assetPath = ArtRoot + fields[2];

                Vector4 border = new Vector4(
                    ParseInt(fields[3]), ParseInt(fields[4]),
                    ParseInt(fields[5]), ParseInt(fields[6])
                ); // 왼쪽, 아래, 오른쪽, 위

                ConfigureSprite(assetPath, border, false);
                AddEntry(entries, fields[1], assetPath);
            }
            else if (fields[0] == "background" && fields.Length >= 3)
            {
                string assetPath = ArtRoot + fields[2];

                ConfigureSprite(assetPath, Vector4.zero, true);
                AddEntry(entries, fields[1], assetPath);
            }
            else if (fields[0] == "atlas" && fields.Length >= 2)
            {
                atlasPath = ArtRoot + fields[1];
                ConfigureSprite(atlasPath, Vector4.zero, false);
            }
            else if (fields[0] == "glyph" && fields.Length >= 6)
            {
                glyphs.Add(new GlyphInfo
                {
                    name = fields[1],
                    x = ParseInt(fields[2]),
                    y = ParseInt(fields[3]),
                    width = ParseInt(fields[4]),
                    height = ParseInt(fields[5]),
                });
            }
        }

        EnsureFolder(ResourcesFolder);

        TMP_SpriteAsset iconAsset = BuildIconAsset(atlasPath, glyphs);
        List<string> iconNames = new List<string>();

        if (iconAsset != null)
        {
            foreach (GlyphInfo glyph in glyphs) { iconNames.Add(glyph.name); }
        }

        UITheme theme = AssetDatabase.LoadAssetAtPath<UITheme>(ThemeAssetPath);

        if (theme == null)
        {
            theme = ScriptableObject.CreateInstance<UITheme>();
            AssetDatabase.CreateAsset(theme, ThemeAssetPath);
        }

        theme.SetContents(entries, iconAsset, iconNames);
        EditorUtility.SetDirty(theme);
        UITheme.SetCurrent(theme);

        ApplyStatusIcons(theme);
        AssetDatabase.SaveAssets();

        return theme;
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value.Trim(), out int result) ? result : 0;
    }

    private static void AddEntry(
        List<UITheme.SpriteEntry> entries,
        string key,
        string assetPath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

        if (sprite == null)
        {
            Debug.LogWarning("테마 이미지를 불러오지 못했습니다: " + assetPath);
            return;
        }

        entries.Add(new UITheme.SpriteEntry { key = key.Trim(), sprite = sprite });
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) { return; }

        int slash = folderPath.LastIndexOf('/');
        string parent = folderPath.Substring(0, slash);
        string folderName = folderPath.Substring(slash + 1);

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
