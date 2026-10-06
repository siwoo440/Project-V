using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 상태 효과 아이콘 프리팹 구성 (기획서 11.10.1)
public static partial class SceneUIBuilder
{
    private const float StatusIconSize = 30f; // 마물 판 안에 들어가는 크기

    private static void BuildStatusIconPrefab()
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents(StatusIconPrefabPath);

        if (prefabRoot == null) { return; }

        RectTransform rootRect = prefabRoot.GetComponent<RectTransform>();

        if (rootRect != null)
        {
            rootRect.sizeDelta = new Vector2(StatusIconSize, StatusIconSize);
        }

        // 배치 그룹이 크기를 정하는 곳에서도 같은 크기를 유지한다.
        LayoutElement rootLayout = prefabRoot.AddComponentIfMissing<LayoutElement>();
        rootLayout.minWidth = StatusIconSize;
        rootLayout.preferredWidth = StatusIconSize;
        rootLayout.minHeight = StatusIconSize;
        rootLayout.preferredHeight = StatusIconSize;

        Transform icon = FindRecursive(prefabRoot.transform, "IconImage");

        if (icon != null)
        {
            SetStretch(icon.gameObject, Vector4.zero);

            Image iconImage = icon.GetComponent<Image>();

            if (iconImage != null)
            {
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false; // 마우스 감지는 바탕 이미지가 맡는다.
            }
        }

        Transform symbol = FindRecursive(prefabRoot.transform, "SymbolText");

        if (symbol != null)
        {
            SetStretch(symbol.gameObject, Vector4.zero);

            TextMeshProUGUI symbolText = symbol.GetComponent<TextMeshProUGUI>();

            if (symbolText != null)
            {
                symbolText.fontSize = 13f;
                symbolText.alignment = TextAlignmentOptions.Center;
                symbolText.raycastTarget = false;
            }
        }

        Transform duration = FindRecursive(prefabRoot.transform, "DurationText");

        if (duration != null)
        {
            // 남은 턴 숫자는 오른쪽 아래에 작은 받침과 함께 둔다.
            GameObject badge = EnsureChild(prefabRoot.transform, "DurationBadge");

            Image badgeImage = badge.AddComponentIfMissing<Image>();
            badgeImage.raycastTarget = false;

            if (!UISkin.ApplyBar(badgeImage, UIKeys.PlateLabel))
            {
                badgeImage.color = new Color(0f, 0f, 0f, 0.7f);
            }

            PlaceCornerRect(badge, new Vector2(18f, 14f));
            PlaceCornerRect(duration.gameObject, new Vector2(18f, 14f));
            PlaceBehind(badge, duration.gameObject);

            TextMeshProUGUI durationText = duration.GetComponent<TextMeshProUGUI>();

            if (durationText != null)
            {
                durationText.fontSize = 11f;
                durationText.fontStyle = FontStyles.Bold;
                durationText.color = TextColor;
                durationText.alignment = TextAlignmentOptions.Center;
                durationText.raycastTarget = false;
            }
        }

        FinalizeTheme(prefabRoot.transform);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, StatusIconPrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }

    // 부모의 오른쪽 아래 모서리에 살짝 걸치도록 놓는다.
    private static void PlaceCornerRect(GameObject target, Vector2 size)
    {
        RectTransform rect = target.GetComponent<RectTransform>();

        if (rect == null) { rect = target.AddComponent<RectTransform>(); }

        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(3f, -3f);
        rect.sizeDelta = size;
    }
}
