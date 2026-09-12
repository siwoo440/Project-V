using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// 한글 표시용 TMP 글꼴 설정 도구
// 프로젝트에 한글 글꼴 파일이 있으면 그 파일로, 없으면 시스템 글꼴로 동적 글꼴 에셋을 만든다.
public static class KoreanFontSetup
{
    private const string FontFolder = "Assets/ProjectV/Fonts";
    private const string FontAssetPath = FontFolder + "/KoreanDynamic SDF.asset";
    private const string SettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    private const int SamplingPointSize = 48; // 아틀라스 크기를 줄이기 위한 표본 크기
    private const int AtlasPadding = 5;
    private const int AtlasSize = 1024;

    private static readonly string[] CandidateFamilies =
    {
        "Malgun Gothic",   // 맑은 고딕
        "NanumGothic",     // 나눔고딕
        "Noto Sans KR",
        "Gulim",           // 굴림
        "Dotum",           // 돋움
        "Batang",          // 바탕
    };

    private static readonly string[] ScanExtensions =
    {
        ".cs", ".asset", ".unity", ".prefab",
    };

    [MenuItem("Project V/한글 글꼴 설정", false, 1)]
    public static void SetupKoreanFont()
    {
        TMP_FontAsset fontAsset =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

        if (fontAsset == null)
        {
            fontAsset = CreateKoreanFontAsset();
        }

        if (fontAsset == null)
        {
            Debug.LogError(
                "한글 글꼴을 만들지 못했습니다. " +
                "시스템에 한글 글꼴이 설치되어 있는지 확인하세요."
            );

            return;
        }

        RegisterFallbackFont(fontAsset);

        Debug.Log(
            "한글 글꼴 설정을 완료했습니다: " + fontAsset.name
        );
    }

    // 프로젝트에서 사용하는 한글 글자를 글꼴 아틀라스에 미리 추가한다.
    // 아틀라스가 사용 중에 계속 바뀌면서 발생하는 임포터 경고를 줄인다.
    [MenuItem("Project V/한글 글꼴 글리프 굽기", false, 2)]
    public static void BakeKoreanGlyphs()
    {
        TMP_FontAsset fontAsset =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

        if (fontAsset == null)
        {
            Debug.LogError(
                "한글 글꼴 에셋이 없습니다. 먼저 한글 글꼴 설정을 실행하세요."
            );

            return;
        }

        string projectCharacters = CollectProjectCharacters();

        if (string.IsNullOrEmpty(projectCharacters))
        {
            Debug.LogWarning("프로젝트에서 한글 글자를 찾지 못했습니다.");
            return;
        }

        bool addedAll = fontAsset.TryAddCharacters(
            projectCharacters,
            out string missingCharacters
        );

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(FontAssetPath);

        Debug.Log(
            $"한글 글리프 {projectCharacters.Length}자를 글꼴에 추가했습니다."
        );

        if (!addedAll && !string.IsNullOrEmpty(missingCharacters))
        {
            Debug.LogWarning(
                $"아틀라스에 담지 못한 글자가 있습니다: {missingCharacters}"
            );
        }
    }

    // 프로젝트 파일에서 사용 중인 한글 글자를 모은다.
    private static string CollectProjectCharacters()
    {
        HashSet<char> characters = new HashSet<char>();

        string[] projectFiles = Directory.GetFiles(
            "Assets/ProjectV",
            "*.*",
            SearchOption.AllDirectories
        );

        foreach (string filePath in projectFiles)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (System.Array.IndexOf(ScanExtensions, extension) < 0) { continue; }

            string fileText;

            try
            {
                fileText = File.ReadAllText(filePath);
            }
            catch (System.Exception)
            {
                continue; // 읽을 수 없는 파일은 건너뛴다
            }

            foreach (char character in fileText)
            {
                bool isHangulSyllable =
                    character >= '가' && character <= '힣';

                bool isHangulJamo =
                    character >= '㄰' && character <= '㆏';

                if (isHangulSyllable || isHangulJamo)
                {
                    characters.Add(character);
                }
            }
        }

        StringBuilder builder = new StringBuilder(characters.Count);

        foreach (char character in characters)
        {
            builder.Append(character);
        }

        return builder.ToString();
    }

    private static TMP_FontAsset CreateKoreanFontAsset()
    {
        if (!Directory.Exists(FontFolder))
        {
            Directory.CreateDirectory(FontFolder);
            AssetDatabase.Refresh();
        }

        TMP_FontAsset createdFont = CreateFromProjectFontFile();

        if (createdFont == null)
        {
            createdFont = CreateFromSystemFont();
        }

        if (createdFont == null) { return null; }

        AssetDatabase.CreateAsset(createdFont, FontAssetPath);

        if (createdFont.material != null)
        {
            createdFont.material.name = createdFont.name + " Material";
            AssetDatabase.AddObjectToAsset(
                createdFont.material,
                createdFont
            ); // 머티리얼을 하위 에셋으로 저장
        }

        if (createdFont.atlasTextures != null &&
            createdFont.atlasTextures.Length > 0 &&
            createdFont.atlasTextures[0] != null)
        {
            createdFont.atlasTextures[0].name =
                createdFont.name + " Atlas";

            AssetDatabase.AddObjectToAsset(
                createdFont.atlasTextures[0],
                createdFont
            ); // 아틀라스를 하위 에셋으로 저장
        }

        EditorUtility.SetDirty(createdFont);
        AssetDatabase.SaveAssets();

        return createdFont;
    }

    // 프로젝트에 포함된 글꼴 파일로 글꼴 에셋을 만든다. (배포에 권장)
    private static TMP_FontAsset CreateFromProjectFontFile()
    {
        string[] fontGuids = AssetDatabase.FindAssets("t:Font", new[] { FontFolder });

        foreach (string fontGuid in fontGuids)
        {
            string fontPath = AssetDatabase.GUIDToAssetPath(fontGuid);
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);

            if (sourceFont == null) { continue; }

            TMP_FontAsset createdFont = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                SamplingPointSize,
                AtlasPadding,
                GlyphRenderMode.SDFAA,
                AtlasSize,
                AtlasSize,
                AtlasPopulationMode.Dynamic,
                true
            ); // 프로젝트 글꼴 파일 기반 생성

            if (createdFont == null) { continue; }

            Debug.Log($"프로젝트 글꼴 파일로 만들었습니다: {fontPath}");

            return createdFont;
        }

        return null;
    }

    // 시스템에 설치된 한글 글꼴로 글꼴 에셋을 만든다. (개발용)
    private static TMP_FontAsset CreateFromSystemFont()
    {
        foreach (string familyName in CandidateFamilies)
        {
            TMP_FontAsset createdFont = null;

            try
            {
                createdFont = TMP_FontAsset.CreateFontAsset(
                    familyName,
                    "Regular",
                    SamplingPointSize
                ); // 시스템 글꼴 기반 동적 글꼴 생성 (DynamicOS)
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(
                    $"{familyName} 글꼴 생성 실패: {exception.Message}"
                );
            }

            if (createdFont == null) { continue; }

            Debug.Log($"시스템 글꼴로 만들었습니다: {familyName}");

            return createdFont;
        }

        return null;
    }

    private static void RegisterFallbackFont(TMP_FontAsset fontAsset)
    {
        TMP_Settings settings =
            AssetDatabase.LoadAssetAtPath<TMP_Settings>(SettingsPath);

        if (settings == null)
        {
            Debug.LogWarning(
                "TMP Settings 에셋을 찾지 못했습니다: " + SettingsPath
            );

            return;
        }

        SerializedObject serializedSettings = new SerializedObject(settings);

        SerializedProperty fallbackList =
            serializedSettings.FindProperty("m_fallbackFontAssets");

        if (fallbackList == null)
        {
            Debug.LogWarning(
                "TMP Settings에 대체 글꼴 목록이 없습니다."
            );

            return;
        }

        for (int i = 0; i < fallbackList.arraySize; i++)
        {
            SerializedProperty element =
                fallbackList.GetArrayElementAtIndex(i);

            if (element.objectReferenceValue == fontAsset)
            {
                Debug.Log("이미 대체 글꼴로 등록되어 있습니다.");
                return;
            }
        }

        fallbackList.arraySize += 1;

        fallbackList
            .GetArrayElementAtIndex(fallbackList.arraySize - 1)
            .objectReferenceValue = fontAsset; // 전역 대체 글꼴 등록

        serializedSettings.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log("TMP 전역 대체 글꼴에 등록했습니다.");
    }
}
