using UnityEngine; // Unity 기본 기능

public static class CardRarityRules // 희귀도 규칙 정의
{
    public static Color GetDisplayColor(CardRarity rarity) // 희귀도 표시 색상
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return new Color(0.78f, 0.78f, 0.80f, 1f); // 회색

            case CardRarity.Rare:
                return new Color(0.42f, 0.66f, 1f, 1f); // 파란색

            case CardRarity.Special:
                return new Color(0.72f, 0.48f, 1f, 1f); // 보라색

            case CardRarity.Legendary:
                return new Color(1f, 0.82f, 0.36f, 1f); // 금색

            default:
                return Color.white;
        }
    }

    public static int GetMaxCopies(CardRarity rarity) // 보유 및 편성 제한 수량
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return 3; // 일반 3장

            case CardRarity.Rare:
                return 2; // 희귀 2장

            case CardRarity.Special:
                return 1; // 특수 1장

            case CardRarity.Legendary:
                return 1; // 전설 1장

            default:
                return 1; // 알 수 없는 희귀도 최소 제한
        }
    }

    public static int GetEssenceReward(CardRarity rarity) // 초과 마물 변환량
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return 2; // 일반 정수 2개

            case CardRarity.Rare:
                return 4; // 희귀 정수 4개

            case CardRarity.Special:
                return 8; // 특수 정수 8개

            case CardRarity.Legendary:
                return 16; // 전설 정수 16개

            default:
                return 0; // 알 수 없는 희귀도 변환 없음
        }
    }

    public static string GetDisplayName(CardRarity rarity) // 희귀도 표시 이름
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return "일반";

            case CardRarity.Rare:
                return "희귀";

            case CardRarity.Special:
                return "특수";

            case CardRarity.Legendary:
                return "전설";

            default:
                return "알 수 없음";
        }
    }
}
