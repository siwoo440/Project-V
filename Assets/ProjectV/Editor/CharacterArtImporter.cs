using UnityEditor;

// 캐릭터 그림을 처음 가져올 때부터 스프라이트 한 장으로 맞춘다. (기획서 12.14)
// 프로젝트 기본값은 여러 장 자동 분할이라, 그대로 두면 실행 중에 그림을 찾지 못한다.
public class CharacterArtImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(UIThemeSetup.CharacterFolder + "/")) { return; }

        UIThemeSetup.ApplyCharacterSettings((TextureImporter)assetImporter);
    }
}
