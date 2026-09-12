using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

// 한글 표시용 TMP 글꼴 설정 도구
// 시스템에 설치된 한글 글꼴로 동적 글꼴 에셋을 만들고 TMP 전역 대체 글꼴로 등록한다.
public static class KoreanFontSetup
{
    private const string FontFolder = "Assets/ProjectV/Fonts";
    private const string FontAssetPath = FontFolder + "/KoreanDynamic SDF.asset";
    private const string SettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    private static readonly string[] CandidateFamilies =
    {
        "Malgun Gothic",   // 맑은 고딕
        "NanumGothic",     // 나눔고딕
        "Noto Sans KR",
        "Gulim",           // 굴림
        "Dotum",           // 돋움
        "Batang",          // 바탕
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

    private static TMP_FontAsset CreateKoreanFontAsset()
    {
        if (!Directory.Exists(FontFolder))
        {
            Directory.CreateDirectory(FontFolder);
            AssetDatabase.Refresh();
        }

        foreach (string familyName in CandidateFamilies)
        {
            TMP_FontAsset createdFont = null;

            try
            {
                createdFont = TMP_FontAsset.CreateFontAsset(
                    familyName,
                    "Regular",
                    90
                ); // 시스템 글꼴 기반 동적 글꼴 생성 (DynamicOS)
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(
                    $"{familyName} 글꼴 생성 실패: {exception.Message}"
                );
            }

            if (createdFont == null) { continue; }

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

            Debug.Log($"한글 글꼴 에셋을 만들었습니다: {familyName}");

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
