using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 전투 화면 소환사 패널과 소환사 데이터 연결
public static partial class SceneUIBuilder
{
    // 플레이어 정보 아래에 소환사 스킬 버튼과 장착 패시브를 표시한다. (기획서 11.7.2)
    private static void BuildBattleSummonerPanel(Canvas canvas, BattleManager battleManager)
    {
        GameObject panel = EnsurePanel("SummonerPanel", canvas.transform, PanelColor);
        SetAnchored(panel, new Vector2(0f, 1f), new Vector2(190f, -321f), new Vector2(340f, 240f));

        TextMeshProUGUI titleText = EnsureText(
            "SummonerTitleText", panel.transform, "소환사",
            22f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(titleText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(290f, 30f));

        Button skillButton = EnsureButton(
            "SummonerSkillButton", panel.transform, "스킬 없음", ButtonColor);

        SetAnchored(skillButton.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(296f, 54f));

        StyleButtonByName("SummonerSkillButton", ButtonColor, 20f);

        TextMeshProUGUI skillText = EnsureText(
            "SummonerSkillText", panel.transform, "",
            16f, SubTextColor, TextAlignmentOptions.Center);

        SetAnchored(skillText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -128f), new Vector2(296f, 44f));

        TextMeshProUGUI passiveText = EnsureText(
            "SummonerPassiveText", panel.transform, "패시브: 없음",
            16f, TextColor, TextAlignmentOptions.Center);

        SetAnchored(passiveText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -188f), new Vector2(296f, 68f));

        AssignReference(battleManager, "summonerSkillButton", skillButton);
        AssignReference(battleManager, "summonerSkillText", skillText);
        AssignReference(battleManager, "summonerPassiveText", passiveText);

        // 전투 씬만 따로 실행해도 스킬을 쓸 수 있도록 이 씬의 진행 데이터에도 목록을 넣는다.
        GameObject progressObject = FindInScene("PlayerProgressManager");

        if (progressObject != null)
        {
            ApplySummonerDataLists(progressObject.GetComponent<PlayerProgressManager>());
            ApplyGrimoireNodeList(progressObject.GetComponent<PlayerProgressManager>());
        }
    }

    // 프로젝트의 소환사 스킬과 패시브 데이터를 해금 레벨 순으로 진행 데이터에 연결한다.
    private static void ApplySummonerDataLists(PlayerProgressManager progress)
    {
        if (progress == null) { return; }

        List<SummonerSkillData> skills = LoadAssets<SummonerSkillData>();
        List<SummonerPassiveData> passives = LoadAssets<SummonerPassiveData>();

        skills.Sort((left, right) =>
            left.UnlockLevel != right.UnlockLevel
                ? left.UnlockLevel.CompareTo(right.UnlockLevel)
                : string.CompareOrdinal(left.SkillId, right.SkillId));

        passives.Sort((left, right) =>
            left.UnlockLevel != right.UnlockLevel
                ? left.UnlockLevel.CompareTo(right.UnlockLevel)
                : string.CompareOrdinal(left.PassiveId, right.PassiveId));

        SerializedObject serializedProgress = new SerializedObject(progress);

        AssignList(serializedProgress, "summonerSkills", skills);
        AssignList(serializedProgress, "summonerPassives", passives);

        serializedProgress.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"소환사 스킬 {skills.Count}종, 패시브 {passives.Count}종을 연결했습니다.");
    }

    private static List<T> LoadAssets<T>() where T : Object
    {
        List<T> assets = new List<T>();

        foreach (string assetGuid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(
                AssetDatabase.GUIDToAssetPath(assetGuid));

            if (asset != null) { assets.Add(asset); }
        }

        return assets;
    }

    private static void AssignList<T>(
        SerializedObject serializedTarget,
        string fieldName,
        List<T> values) where T : Object
    {
        SerializedProperty listProperty = serializedTarget.FindProperty(fieldName);

        if (listProperty == null)
        {
            Debug.LogWarning($"{fieldName} 항목을 찾지 못했습니다.");
            return;
        }

        listProperty.arraySize = values.Count;

        for (int i = 0; i < values.Count; i++)
        {
            listProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
