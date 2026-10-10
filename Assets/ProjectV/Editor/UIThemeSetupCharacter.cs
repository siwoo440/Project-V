using UnityEditor;
using UnityEngine;

// 캐릭터 그림 가져오기 설정 (기획서 12.14)
// Tools/UIThemeGenerator가 Resources/Characters/<인물> 아래에 넣은 그림을 스프라이트 한 장으로 맞춘다.
// 캐릭터 그림은 테마 에셋에 넣지 않고, 전투에 나온 인물의 그림만 실행 중에 불러 쓴다.
public static partial class UIThemeSetup
{
    public const string CharacterFolder = ResourcesFolder + "/Characters";

    // 그림 하나의 가져오기 설정을 캐릭터 그림용으로 바꾼다. 다시 가져오기는 부르는 쪽이 한다.
    public static void ApplyCharacterSettings(TextureImporter importer)
    {
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);

        settings.textureType = TextureImporterType.Sprite;
        settings.spriteMode = (int)SpriteImportMode.Single;
        settings.spritePixelsPerUnit = 100f;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spriteGenerateFallbackPhysicsShape = false;
        settings.mipmapEnabled = false;
        settings.alphaIsTransparency = true;
        settings.wrapMode = TextureWrapMode.Clamp;
        settings.filterMode = FilterMode.Bilinear;
        settings.npotScale = TextureImporterNPOTScale.None;

        importer.SetTextureSettings(settings);
        importer.spriteImportMode = SpriteImportMode.Single; // 프로젝트 기본값(여러 장 자동 분할)을 한 장으로 바꾼다.
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.CompressedHQ; // 그림이 커서 압축한다.
    }

    private static bool HasCharacterSettings(TextureImporter importer)
    {
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);

        return
            settings.textureType == TextureImporterType.Sprite &&
            importer.spriteImportMode == SpriteImportMode.Single &&
            settings.spriteMeshType == SpriteMeshType.FullRect &&
            !settings.mipmapEnabled &&
            settings.alphaIsTransparency &&
            settings.wrapMode == TextureWrapMode.Clamp &&
            importer.textureCompression == TextureImporterCompression.CompressedHQ &&
            importer.maxTextureSize == 2048;
    }

    // 이미 들어와 있는 캐릭터 그림의 설정을 점검한다. 씬 UI를 다시 구성할 때마다 부른다.
    private static void ConfigureCharacterArt()
    {
        if (!AssetDatabase.IsValidFolder(CharacterFolder)) { return; }

        int changedCount = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { CharacterFolder }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer == null || HasCharacterSettings(importer)) { continue; }

            ApplyCharacterSettings(importer);
            importer.SaveAndReimport();

            changedCount++;
        }

        if (changedCount > 0)
        {
            Debug.Log($"캐릭터 그림 {changedCount}장의 가져오기 설정을 맞췄습니다.");
        }
    }
}
