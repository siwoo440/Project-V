using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 프리팹 구성: 필드 마물 판, 손패 카드
public static partial class SceneUIBuilder
{
    private const string CollectionButtonPrefabPath =
        "Assets/ProjectV/Prefabs/UI/MonsterCollectionButton.prefab";

    private const string StatusIconPrefabPath =
        "Assets/ProjectV/Prefabs/UI/StatusEffectIconPrefab.prefab";

    // 필드 마물 판 프리팹 (기획서 11.7.3)
    // 위에서부터 계열 문양, 이름, 체력 게이지, 공격·성욕, 방어·보호막, 스킬, 행동 상태, 상태 효과.
    private static void BuildMonsterUnitPrefab()
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents(MonsterUnitPrefabPath);

        if (prefabRoot == null)
        {
            Debug.LogWarning(
                "마물 카드 프리팹을 찾지 못했습니다: " + MonsterUnitPrefabPath);
            return;
        }

        RectTransform rootRect = prefabRoot.GetComponent<RectTransform>();

        if (rootRect != null)
        {
            rootRect.sizeDelta = new Vector2(130f, 220f);
        }

        Image cardImage = prefabRoot.GetComponent<Image>();

        bool hasPlate =
            cardImage != null &&
            UISkin.ApplySliced(cardImage, UIKeys.SlotUnit, 20f); // 밝은 양피지 판

        if (!hasPlate && cardImage != null)
        {
            cardImage.color = new Color(0.18f, 0.15f, 0.27f, 1f);
        }

        VerticalLayoutGroup layout =
            prefabRoot.AddComponentIfMissing<VerticalLayoutGroup>();

        layout.spacing = 2f;
        layout.padding = new RectOffset(8, 8, 9, 8);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        EnsureMonsterSkillRow(prefabRoot); // 스킬 표시 행 준비

        MonsterUnit monsterUnit = prefabRoot.GetComponent<MonsterUnit>();

        // 계열 문양 줄 (일러스트가 생기기 전까지의 자리 표시)
        GameObject typeIcon = EnsureChild(prefabRoot.transform, "MonsterTypeIcon");
        Image typeIconImage = typeIcon.AddComponentIfMissing<Image>();
        typeIconImage.raycastTarget = false;
        typeIconImage.preserveAspect = true;

        // 체력 게이지 줄
        Transform hpText = FindRecursive(prefabRoot.transform, "MonsterHPText");
        UIGauge hpGauge = null;

        if (hpText != null)
        {
            hpGauge = BuildGaugeRow(
                hpText.gameObject, EnsureChild(prefabRoot.transform, "MonsterHPRow"), 0,
                UIKeys.GaugeRed, HpGaugeColor, 20f, 13f);
        }

        // 상태 효과 아이콘 칸: 판 가운데에 모아 놓는다.
        Transform statusContainer =
            FindRecursive(prefabRoot.transform, "MonsterStatusIconContainer");

        HorizontalLayoutGroup statusLayout = statusContainer != null
            ? statusContainer.GetComponent<HorizontalLayoutGroup>()
            : null;

        if (statusLayout != null)
        {
            statusLayout.childAlignment = TextAnchor.MiddleCenter;
        }

        if (monsterUnit != null)
        {
            AssignReference(monsterUnit, "typeIconImage", typeIconImage);
            AssignReference(monsterUnit, "hpGauge", hpGauge);
            AssignReference(monsterUnit, "statusIconContainer", statusContainer);
            ApplyMonsterUnitColors(monsterUnit, hasPlate);
        }

        // 공격·성욕, 방어·보호막은 한 줄씩 묶어 쓰므로 예전 단독 줄은 숨긴다.
        HidePrefabRow(prefabRoot, "MonsterLustDamageText");
        HidePrefabRow(prefabRoot, "MonsterShieldText");

        string[] rowNames =
        {
            "MonsterTypeIcon",
            "MonsterNameText",
            "MonsterHPRow",
            "MonsterAttackText",
            "MonsterDefenseText",
            "MonsterSkillText",
            "MonsterStateText",
            "MonsterStatusIconContainer",
        };

        float[] rowSizes = { 0f, 16f, 0f, 15f, 15f, 13f, 13f, 0f };
        float[] rowHeights = { 36f, 22f, 20f, 20f, 20f, 18f, 18f, 30f };

        Color nameColor = hasPlate ? UISkin.InkAccent : AccentColor;
        Color bodyColor = hasPlate ? UISkin.Ink : TextColor;

        for (int i = 0; i < rowNames.Length; i++)
        {
            Transform row = prefabRoot.transform.Find(rowNames[i]);

            if (row == null)
            {
                Debug.LogWarning("마물 카드 항목을 찾지 못했습니다: " + rowNames[i]);
                continue;
            }

            row.SetSiblingIndex(i);

            RectTransform rowRect = row.GetComponent<RectTransform>();

            if (rowRect != null)
            {
                rowRect.sizeDelta = new Vector2(0f, rowHeights[i]);
            }

            LayoutElement rowLayout =
                row.gameObject.AddComponentIfMissing<LayoutElement>();

            rowLayout.minHeight = rowHeights[i];
            rowLayout.preferredHeight = rowHeights[i];

            TextMeshProUGUI rowText = row.GetComponent<TextMeshProUGUI>();

            if (rowText == null) { continue; } // 게이지와 아이콘 줄

            rowText.fontSize = rowSizes[i];
            rowText.alignment = TextAlignmentOptions.Center;
            rowText.raycastTarget = false;
            rowText.color = i == 1 ? nameColor : bodyColor;
            rowText.overflowMode = TextOverflowModes.Truncate;
            rowText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        FinalizeTheme(prefabRoot.transform);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, MonsterUnitPrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("마물 카드 프리팹을 정리했습니다.");
    }

    private static void HidePrefabRow(GameObject prefabRoot, string rowName)
    {
        Transform row = FindRecursive(prefabRoot.transform, rowName);

        if (row != null) { row.gameObject.SetActive(false); }
    }
}
