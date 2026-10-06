using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

// UI 테마 이미지 가져오기 설정과 글자 사이 아이콘 묶음 작성
public static partial class UIThemeSetup
{
    // UI용 스프라이트 가져오기 설정. 이미 같은 설정이면 다시 가져오지 않는다.
    private static void ConfigureSprite(string assetPath, Vector4 border, bool isBackground)
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer == null)
        {
            Debug.LogWarning("테마 이미지 파일을 찾지 못했습니다: " + assetPath);
            return;
        }

        TextureImporterCompression compression = isBackground
            ? TextureImporterCompression.Compressed
            : TextureImporterCompression.Uncompressed; // UI 조각은 선명도 우선

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);

        bool isSame =
            settings.textureType == TextureImporterType.Sprite &&
            settings.spriteMode == (int)SpriteImportMode.Single &&
            importer.spriteImportMode == SpriteImportMode.Single &&
            settings.spriteBorder == border &&
            settings.spriteMeshType == SpriteMeshType.FullRect &&
            !settings.mipmapEnabled &&
            settings.alphaIsTransparency == !isBackground &&
            settings.wrapMode == TextureWrapMode.Clamp &&
            Mathf.Approximately(settings.spritePixelsPerUnit, 100f) &&
            importer.textureCompression == compression &&
            importer.maxTextureSize == 2048;

        if (isSame) { return; }

        settings.textureType = TextureImporterType.Sprite;
        settings.spriteMode = (int)SpriteImportMode.Single;
        settings.spritePixelsPerUnit = 100f;
        settings.spriteBorder = border;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spriteGenerateFallbackPhysicsShape = false;
        settings.mipmapEnabled = false;
        settings.alphaIsTransparency = !isBackground;
        settings.wrapMode = TextureWrapMode.Clamp;
        settings.filterMode = FilterMode.Bilinear;
        settings.npotScale = TextureImporterNPOTScale.None;

        importer.SetTextureSettings(settings);
        importer.spriteImportMode = SpriteImportMode.Single; // 프로젝트 기본값(여러 장 자동 분할)을 한 장으로 바꾼다.
        importer.maxTextureSize = 2048;
        importer.textureCompression = compression;
        importer.SaveAndReimport();
    }

    // 아이콘 묶음 이미지로 TextMeshPro 스프라이트 에셋을 만든다.
    // 글자 안에 <sprite="UIIcons" name="hp">처럼 적으면 아이콘이 들어간다.
    private static TMP_SpriteAsset BuildIconAsset(string atlasPath, List<GlyphInfo> glyphs)
    {
        if (string.IsNullOrEmpty(atlasPath) || glyphs.Count == 0) { return null; }

        Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);

        if (atlas == null)
        {
            Debug.LogWarning("아이콘 묶음 이미지를 불러오지 못했습니다: " + atlasPath);
            return null;
        }

        EnsureFolder(IconFolder);

        TMP_SpriteAsset iconAsset =
            AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(IconAssetPath);

        if (iconAsset == null)
        {
            iconAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            AssetDatabase.CreateAsset(iconAsset, IconAssetPath);
        }

        // 버전이 비어 있으면 TextMeshPro가 옛 형식으로 보고 표를 다시 만들어 버린다.
        SerializedObject serializedAsset = new SerializedObject(iconAsset);
        SerializedProperty versionProperty = serializedAsset.FindProperty("m_Version");

        if (versionProperty != null)
        {
            versionProperty.stringValue = "1.1.0";
            serializedAsset.ApplyModifiedPropertiesWithoutUndo();
        }

        iconAsset.spriteSheet = atlas;
        iconAsset.spriteGlyphTable.Clear();
        iconAsset.spriteCharacterTable.Clear();

        for (int i = 0; i < glyphs.Count; i++)
        {
            GlyphInfo info = glyphs[i];

            // 목록은 위쪽 기준, 텍스처 좌표는 아래쪽 기준이다.
            int bottom = atlas.height - (info.y + info.height);

            TMP_SpriteGlyph spriteGlyph = new TMP_SpriteGlyph();

            spriteGlyph.index = (uint)i;
            spriteGlyph.glyphRect = new GlyphRect(info.x, bottom, info.width, info.height);
            spriteGlyph.scale = IconScale;

            spriteGlyph.metrics = new GlyphMetrics(
                info.width,
                info.height,
                0f,
                info.height * IconBaselineRatio,
                info.width * IconAdvanceRatio
            );

            iconAsset.spriteGlyphTable.Add(spriteGlyph);

            TMP_SpriteCharacter spriteCharacter =
                new TMP_SpriteCharacter(0xFFFE, spriteGlyph);

            spriteCharacter.name = info.name;
            spriteCharacter.scale = 1f;

            iconAsset.spriteCharacterTable.Add(spriteCharacter);
        }

        Material material = iconAsset.material;

        if (material == null)
        {
            Shader shader = Shader.Find("TextMeshPro/Sprite");

            if (shader == null)
            {
                Debug.LogWarning("TextMeshPro/Sprite 셰이더를 찾지 못했습니다.");
                return null;
            }

            material = new Material(shader);
            material.name = iconAsset.name + " Material";

            AssetDatabase.AddObjectToAsset(material, iconAsset);
            iconAsset.material = material;
        }

        material.mainTexture = atlas;

        iconAsset.UpdateLookupTables();

        EditorUtility.SetDirty(material);
        EditorUtility.SetDirty(iconAsset);

        return iconAsset;
    }

    // 상태 효과 데이터에 종류별 아이콘을 연결한다.
    private static void ApplyStatusIcons(UITheme theme)
    {
        foreach (string assetGuid in AssetDatabase.FindAssets("t:StatusEffectData"))
        {
            StatusEffectData statusData = AssetDatabase.LoadAssetAtPath<StatusEffectData>(
                AssetDatabase.GUIDToAssetPath(assetGuid)
            );

            if (statusData == null) { continue; }

            Sprite icon = theme.GetSprite(UISkin.StatusIconKey(statusData.StatusType));

            if (icon == null) { continue; }

            SerializedObject serializedStatus = new SerializedObject(statusData);
            SerializedProperty iconProperty = serializedStatus.FindProperty("icon");

            if (iconProperty == null) { continue; }
            if (iconProperty.objectReferenceValue == icon) { continue; }

            iconProperty.objectReferenceValue = icon;
            serializedStatus.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(statusData);
        }
    }
}
