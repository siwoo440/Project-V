using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 세로형 카드 표시를 만든다. 덱 편성, 강화, 전투 손패가 함께 쓴다. (기획서 12.7.1: 세로 2:3)
public static partial class CardEntryFactory
{
    public const float BaseWidth = 190f;  // 글자 크기의 기준이 되는 카드 너비
    public const float CardRatio = 1.5f;  // 세로 / 가로

    private const string FaceName = "CardFace"; // 카드 내용 묶음 이름

    private static readonly Color PlainCardColor = new Color(0.17f, 0.14f, 0.26f, 1f); // 틀 이미지가 없을 때 카드 색
    private static readonly Color BadgeColor = new Color(0.10f, 0.09f, 0.16f, 1f);     // 틀 이미지가 없을 때 배지 색
    private static readonly Color ReducedCostColor = new Color(0.60f, 1f, 0.62f, 1f);  // 줄어든 비용 표시 색

    // 목록에 넣을 카드 항목을 만든다.
    public static GameObject CreateCardEntry(
        Transform parentContent,
        CardData cardData,
        int enhanceLevel,
        string countText,
        bool isInteractable,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        if (parentContent == null) { return null; } // 배치 영역 누락 차단
        if (cardData == null) { return null; } // 빈 카드 차단

        GameObject entryObject =
            new GameObject("CardEntry", typeof(RectTransform));

        entryObject.transform.SetParent(parentContent, false);

        Image cardBackground = entryObject.AddComponent<Image>();

        Button entryButton = entryObject.AddComponent<Button>();
        entryButton.targetGraphic = cardBackground;
        entryButton.interactable = isInteractable;

        ColorBlock colors = entryButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.94f, 0.78f, 1f);
        colors.pressedColor = new Color(0.78f, 0.72f, 0.62f, 1f);
        colors.disabledColor = Color.white; // 흐림 처리는 카드 내용에서 직접 한다.
        entryButton.colors = colors;

        if (clickAction != null)
        {
            entryButton.onClick.AddListener(clickAction);
        }

        BuildFace(
            entryObject.transform, cardBackground, cardData, enhanceLevel,
            countText, !isInteractable, BaseWidth, true, cardData.ManaCost, false
        );

        return entryObject;
    }

    public static void ClearFace(Transform root) // 카드 내용 제거 (다시 그리기 전)
    {
        if (root == null) { return; }

        Transform oldFace = root.Find(FaceName);

        if (oldFace == null) { return; }

        oldFace.gameObject.SetActive(false);
        oldFace.SetParent(null, false); // 같은 프레임 안에 새 내용이 같은 이름으로 생겨도 겹치지 않게 한다.
        Object.Destroy(oldFace.gameObject);
    }

    public static Color Dim(Color source, float rate) // 비활성 표시용 감광
    {
        return new Color(
            source.r * rate,
            source.g * rate,
            source.b * rate,
            1f
        );
    }

    public static Image CreateImage(
        Transform parent,
        string objectName,
        Color imageColor,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax
    )
    {
        GameObject imageObject =
            new GameObject(objectName, typeof(RectTransform));

        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.AddComponent<Image>();
        image.color = imageColor;
        image.raycastTarget = false;

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = anchorMin;
        imageRect.anchorMax = anchorMax;
        imageRect.offsetMin = offsetMin;
        imageRect.offsetMax = offsetMax;

        return image;
    }

    public static TextMeshProUGUI CreateLabel(
        Transform parent,
        string objectName,
        string content,
        float fontSize,
        Color textColor,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax
    )
    {
        GameObject labelObject =
            new GameObject(objectName, typeof(RectTransform));

        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI label =
            labelObject.AddComponent<TextMeshProUGUI>();

        label.text = content;
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.overflowMode = TextOverflowModes.Ellipsis;

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin = anchorMin;
        labelRect.anchorMax = anchorMax;
        labelRect.offsetMin = offsetMin;
        labelRect.offsetMax = offsetMax;

        return label;
    }
}
