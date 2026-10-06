using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 카드 한 장의 내용(틀, 비용, 강화 단계, 계열 문양, 이름, 능력치)을 그린다.
public static partial class CardEntryFactory
{
    // root 아래에 카드 내용을 새로 만든다. 이미 있으면 지우고 다시 만든다.
    // cardWidth는 화면에 놓일 카드 너비이며 글자와 배지 크기의 기준이 된다.
    public static void BuildFace(
        Transform root,
        Image background,
        CardData cardData,
        int enhanceLevel,
        string footerText,
        bool isDimmed,
        float cardWidth,
        bool showTypeLine,
        int shownCost,
        bool isCostReduced
    )
    {
        if (root == null || cardData == null) { return; }

        ClearFace(root);

        float scale = Mathf.Max(0.5f, cardWidth / BaseWidth);
        float dimRate = isDimmed ? 0.45f : 1f;

        Color rarityColor = Dim(CardRarityRules.GetDisplayColor(cardData.Rarity), dimRate);
        Color typeColor = Dim(MonsterTypeRules.GetDisplayColor(cardData.MainType), dimRate);
        Color levelColor = Dim(CardEnhanceRules.GetLevelColor(enhanceLevel), dimRate);
        Color nameColor = Dim(UISkin.Cream, isDimmed ? 0.6f : 1f);

        GameObject faceObject = new GameObject(FaceName, typeof(RectTransform));
        faceObject.transform.SetParent(root, false);

        RectTransform faceRect = faceObject.GetComponent<RectTransform>();
        faceRect.anchorMin = Vector2.zero;
        faceRect.anchorMax = Vector2.one;
        faceRect.offsetMin = Vector2.zero;
        faceRect.offsetMax = Vector2.zero;

        Transform face = faceObject.transform;

        // 카드 틀 (희귀도별, 기획서 11.4.2)
        bool hasFrame =
            background != null &&
            UISkin.ApplySimple(background, UISkin.CardFrameKey(cardData.Rarity), false);

        if (background != null)
        {
            background.color = hasFrame
                ? (isDimmed ? new Color(0.5f, 0.5f, 0.56f, 1f) : Color.white)
                : Dim(PlainCardColor, isDimmed ? 0.65f : 1f);
        }

        if (!hasFrame)
        {
            CreateImage(
                face, "RarityHeader", rarityColor,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -10f * scale), Vector2.zero
            ); // 틀 이미지가 없을 때는 희귀도 띠로 대신한다.
        }

        // 일러스트 자리: 일러스트가 생기기 전까지 계열 문양을 크게 보여준다.
        Sprite typeSprite = UISkin.Get(UISkin.TypeIconKey(cardData.MainType));

        if (typeSprite != null)
        {
            float emblemSize = cardWidth * (showTypeLine ? 0.46f : 0.5f);

            Image emblem = CreateImage(
                face, "TypeEmblem", new Color(1f, 1f, 1f, isDimmed ? 0.45f : 1f),
                new Vector2(0.5f, 0.685f), new Vector2(0.5f, 0.685f),
                new Vector2(-emblemSize * 0.5f, -emblemSize * 0.5f),
                new Vector2(emblemSize * 0.5f, emblemSize * 0.5f)
            );

            emblem.sprite = typeSprite;
            emblem.preserveAspect = true;
        }

        BuildCostBadge(face, shownCost, isCostReduced, scale, dimRate);
        BuildLevelBadge(face, enhanceLevel, levelColor, scale);

        // 카드 이름
        TextMeshProUGUI nameLabel = CreateLabel(
            face, "NameText", cardData.CardName,
            Mathf.Max(14f, 19f * scale), nameColor, TextAlignmentOptions.Center,
            new Vector2(0.07f, showTypeLine ? 0.385f : 0.33f),
            new Vector2(0.93f, showTypeLine ? 0.50f : 0.47f),
            Vector2.zero, Vector2.zero
        );

        nameLabel.enableAutoSizing = true; // 긴 이름은 글자를 줄여 한 줄에 넣는다.
        nameLabel.fontSizeMin = Mathf.Max(10f, 12f * scale);
        nameLabel.fontSizeMax = Mathf.Max(14f, 19f * scale);
        nameLabel.textWrappingMode = TextWrappingModes.NoWrap;

        // 계열과 희귀도 (기획서 11.4.3: 색, 문양, 이름을 함께 표시)
        if (showTypeLine)
        {
            string typeHex = ColorUtility.ToHtmlStringRGB(typeColor);
            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);

            CreateLabel(
                face, "TypeText",
                $"<color=#{typeHex}>{MonsterTypeRules.GetDisplayName(cardData.MainType)}</color>" +
                $"  <color=#{rarityHex}>{CardRarityRules.GetDisplayName(cardData.Rarity)}</color>",
                Mathf.Max(11f, 15f * scale), nameColor, TextAlignmentOptions.Center,
                new Vector2(0.07f, 0.305f), new Vector2(0.93f, 0.385f),
                Vector2.zero, Vector2.zero
            );
        }

        // 능력치 (강화 단계 반영)
        TextMeshProUGUI statLabel = CreateLabel(
            face, "StatText", BuildStatText(cardData.SummonMonster, enhanceLevel),
            Mathf.Max(12f, 15f * scale), nameColor, TextAlignmentOptions.Center,
            new Vector2(0.05f, showTypeLine ? 0.165f : 0.06f),
            new Vector2(0.95f, showTypeLine ? 0.305f : 0.33f),
            Vector2.zero, Vector2.zero
        );

        statLabel.overflowMode = TextOverflowModes.Overflow;

        // 아래쪽 수량 칸
        if (!string.IsNullOrEmpty(footerText))
        {
            Image footerPlate = CreateImage(
                face, "CountBackground", Dim(BadgeColor, 1f),
                new Vector2(0.08f, 0.045f), new Vector2(0.92f, 0.155f),
                Vector2.zero, Vector2.zero
            );

            UISkin.ApplyBar(footerPlate, UIKeys.PlateLabel);

            CreateLabel(
                face, "CountText", footerText,
                Mathf.Max(12f, 17f * scale), rarityColor, TextAlignmentOptions.Center,
                new Vector2(0.08f, 0.045f), new Vector2(0.92f, 0.155f),
                Vector2.zero, Vector2.zero
            );
        }
    }
}
