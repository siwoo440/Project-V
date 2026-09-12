using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 세로형 카드 항목을 만든다. 덱 편성 화면과 강화 화면이 함께 쓴다.
public static class CardEntryFactory
{
    public static GameObject CreateCardEntry(
        Transform parentContent,
        CardData cardData,
        string levelLabel,
        Color levelColor,
        string countText,
        bool isInteractable,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        if (parentContent == null) { return null; } // 배치 영역 누락 차단
        if (cardData == null) { return null; } // 빈 카드 차단

        Color rarityColor = CardRarityRules.GetDisplayColor(cardData.Rarity);
        Color typeColor = MonsterTypeRules.GetDisplayColor(cardData.MainType);

        GameObject entryObject =
            new GameObject("CardEntry", typeof(RectTransform));

        entryObject.transform.SetParent(parentContent, false);

        Image cardBackground = entryObject.AddComponent<Image>();

        cardBackground.color = isInteractable
            ? new Color(0.17f, 0.14f, 0.26f, 1f)
            : new Color(0.11f, 0.10f, 0.14f, 1f);

        Button entryButton = entryObject.AddComponent<Button>();
        entryButton.targetGraphic = cardBackground;
        entryButton.interactable = isInteractable;

        ColorBlock colors = entryButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.94f, 0.78f, 1f);
        colors.pressedColor = new Color(0.78f, 0.72f, 0.62f, 1f);
        colors.disabledColor = new Color(0.75f, 0.75f, 0.78f, 1f);
        entryButton.colors = colors;

        if (clickAction != null)
        {
            entryButton.onClick.AddListener(clickAction);
        }

        float dimRate = isInteractable ? 1f : 0.45f;

        Color nameColor = isInteractable
            ? new Color(0.96f, 0.95f, 0.99f, 1f)
            : new Color(0.55f, 0.54f, 0.60f, 1f);

        Color accentColor = Dim(rarityColor, dimRate);
        Color typeTextColor = Dim(typeColor, dimRate);

        // 상단 희귀도 띠
        CreateImage(
            entryObject.transform, "RarityHeader", accentColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -10f), new Vector2(0f, 0f)
        );

        // 마나 배지
        CreateImage(
            entryObject.transform, "ManaBadge",
            new Color(0.10f, 0.09f, 0.16f, 1f),
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(10f, -54f), new Vector2(52f, -16f)
        );

        CreateLabel(
            entryObject.transform, "ManaText", cardData.ManaCost.ToString(),
            20f, nameColor, TextAlignmentOptions.Center,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(10f, -54f), new Vector2(52f, -16f)
        );

        // 강화 단계 배지
        CreateImage(
            entryObject.transform, "LevelBadge",
            new Color(0.10f, 0.09f, 0.16f, 1f),
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-76f, -54f), new Vector2(-10f, -16f)
        );

        CreateLabel(
            entryObject.transform, "LevelText", levelLabel,
            17f, Dim(levelColor, dimRate), TextAlignmentOptions.Center,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-76f, -54f), new Vector2(-10f, -16f)
        );

        // 카드 이름
        CreateLabel(
            entryObject.transform, "NameText", cardData.CardName,
            19f, nameColor, TextAlignmentOptions.Center,
            new Vector2(0f, 0.42f), new Vector2(1f, 0.76f),
            new Vector2(8f, 0f), new Vector2(-8f, 0f)
        );

        // 계열
        CreateLabel(
            entryObject.transform, "TypeText",
            MonsterTypeRules.GetDisplayName(cardData.MainType),
            16f, typeTextColor, TextAlignmentOptions.Center,
            new Vector2(0f, 0.29f), new Vector2(1f, 0.42f),
            new Vector2(8f, 0f), new Vector2(-8f, 0f)
        );

        // 희귀도
        CreateLabel(
            entryObject.transform, "RarityText",
            CardRarityRules.GetDisplayName(cardData.Rarity),
            16f, accentColor, TextAlignmentOptions.Center,
            new Vector2(0f, 0.17f), new Vector2(1f, 0.29f),
            new Vector2(8f, 0f), new Vector2(-8f, 0f)
        );

        // 수량 영역
        CreateImage(
            entryObject.transform, "CountBackground",
            new Color(0.10f, 0.09f, 0.16f, 1f),
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(8f, 8f), new Vector2(-8f, 40f)
        );

        CreateLabel(
            entryObject.transform, "CountText", countText,
            19f, accentColor, TextAlignmentOptions.Center,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(8f, 8f), new Vector2(-8f, 40f)
        );

        return entryObject;
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
