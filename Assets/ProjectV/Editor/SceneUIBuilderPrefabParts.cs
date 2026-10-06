using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 프리팹 구성: 마물 판 보조, 손패 카드, 도감 줄, 상태 효과 아이콘
public static partial class SceneUIBuilder
{
    // 마물 카드에 스킬 표시 행을 만들고 컴포넌트에 연결한다.
    private static void EnsureMonsterSkillRow(GameObject prefabRoot)
    {
        GameObject skillObject = EnsureChild(prefabRoot.transform, "MonsterSkillText");

        TextMeshProUGUI skillText =
            skillObject.AddComponentIfMissing<TextMeshProUGUI>();

        skillText.text = string.Empty;

        MonsterUnit monsterUnit = prefabRoot.GetComponent<MonsterUnit>();

        if (monsterUnit != null)
        {
            AssignReference(monsterUnit, "monsterSkillText", skillText);
        }
    }

    // 마물 판의 상태 표시 색. 판 이미지가 있으면 그림을 살리는 옅은 색조를 쓴다.
    private static void ApplyMonsterUnitColors(MonsterUnit monsterUnit, bool hasPlate)
    {
        SerializedObject serializedUnit = new SerializedObject(monsterUnit);

        SetColor(serializedUnit, "normalColor",
            hasPlate ? Color.white : new Color(0.34f, 0.24f, 0.45f, 1f));

        SetColor(serializedUnit, "selectedColor",
            hasPlate ? new Color(0.62f, 1f, 0.66f, 1f) : new Color(0.25f, 0.65f, 0.35f, 1f));

        SetColor(serializedUnit, "heroineTargetColor",
            hasPlate ? new Color(1f, 0.70f, 0.64f, 1f) : new Color(0.9f, 0.35f, 0.2f, 1f));

        SetColor(serializedUnit, "skillTargetColor",
            hasPlate ? new Color(0.64f, 0.84f, 1f, 1f) : new Color(0.25f, 0.5f, 0.85f, 1f));

        serializedUnit.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetColor(SerializedObject target, string fieldName, Color color)
    {
        SerializedProperty property = target.FindProperty(fieldName);

        if (property != null) { property.colorValue = color; }
    }

    // 손패 카드 프리팹: 카드 비율 2:3 (기획서 12.7.1). 내용은 전투 중 CardEntryFactory가 그린다.
    private static void BuildCardButtonPrefab()
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents(CardButtonPrefabPath);

        if (prefabRoot == null)
        {
            Debug.LogWarning(
                "손패 카드 프리팹을 찾지 못했습니다: " + CardButtonPrefabPath);
            return;
        }

        Vector2 cardSize = new Vector2(140f, 140f * CardEntryFactory.CardRatio);

        RectTransform rootRect = prefabRoot.GetComponent<RectTransform>();

        if (rootRect != null) { rootRect.sizeDelta = cardSize; }

        LayoutElement rootLayout =
            prefabRoot.AddComponentIfMissing<LayoutElement>();

        rootLayout.minWidth = cardSize.x;
        rootLayout.preferredWidth = cardSize.x;
        rootLayout.minHeight = cardSize.y;
        rootLayout.preferredHeight = cardSize.y;

        Image cardImage = prefabRoot.GetComponent<Image>();

        if (cardImage != null &&
            !UISkin.ApplySimple(cardImage, UIKeys.CardFrameCommon, false))
        {
            cardImage.color = new Color(0.22f, 0.18f, 0.34f, 1f);
        }

        Transform cardText = FindRecursive(prefabRoot.transform, "CardText");

        if (cardText != null)
        {
            TextMeshProUGUI label = cardText.GetComponent<TextMeshProUGUI>();

            if (label != null)
            {
                label.fontSize = 16f;
                label.color = TextColor;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                label.overflowMode = TextOverflowModes.Truncate;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, CardButtonPrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("손패 카드 프리팹을 정리했습니다.");
    }

    // 도감 목록 줄 프리팹
    private static void BuildCollectionButtonPrefab()
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents(CollectionButtonPrefabPath);

        if (prefabRoot == null) { return; }

        RectTransform rootRect = prefabRoot.GetComponent<RectTransform>();

        if (rootRect != null)
        {
            rootRect.sizeDelta = new Vector2(rootRect.sizeDelta.x, 46f);
        }

        LayoutElement rootLayout = prefabRoot.AddComponentIfMissing<LayoutElement>();
        rootLayout.minHeight = 46f;
        rootLayout.preferredHeight = 46f;

        Image rowImage = prefabRoot.GetComponent<Image>();

        if (rowImage != null && !UISkin.ApplyBar(rowImage, UIKeys.RowNormal))
        {
            rowImage.color = ButtonColor;
        }

        TextMeshProUGUI label =
            prefabRoot.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
        {
            label.fontSize = 19f;
            label.color = TextColor;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.margin = new Vector4(28f, 0f, 28f, 0f); // 줄 양 끝 장식을 피한다.
            label.raycastTarget = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
        }

        FinalizeTheme(prefabRoot.transform);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, CollectionButtonPrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }
}
