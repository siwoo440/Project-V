using UnityEngine; // Unity 기본 기능

public static class CardEnhanceRules // 마물 카드 강화 규칙 (기획서 6.9 / 9.11 / A.45)
{
    public const int MinLevel = 1; // 최소 강화 단계
    public const int MaxLevel = 5; // 최대 강화 단계 (기획서 6.9.2)

    // 기획서 9.11.1: 정수 비용은 모든 희귀도가 같다. (Lv.1→2, 2→3, 3→4, 4→5)
    private static readonly int[] EssenceCosts = { 5, 10, 20, 40 };

    // 기획서 9.11.2: 골드 비용은 희귀도마다 다르다.
    private static readonly int[] CommonGoldCosts = { 200, 500, 1000, 2000 };
    private static readonly int[] RareGoldCosts = { 300, 750, 1500, 3000 };
    private static readonly int[] SpecialGoldCosts = { 400, 1000, 2000, 4000 };
    private static readonly int[] LegendaryGoldCosts = { 600, 1500, 3000, 6000 };

    public static int ClampLevel(int level) // 강화 단계 범위 보정
    {
        return Mathf.Clamp(level, MinLevel, MaxLevel);
    }

    public static bool IsMaxLevel(int level) // 최대 단계 도달 여부
    {
        return level >= MaxLevel;
    }

    private static int GetStepCost(int[] costs, int currentLevel) // 단계별 비용 반환
    {
        int stepIndex = ClampLevel(currentLevel) - MinLevel;

        if (stepIndex < 0 || stepIndex >= costs.Length)
        {
            return 0; // 최대 단계에는 다음 단계가 없다.
        }

        return costs[stepIndex];
    }

    private static int[] GetGoldCosts(CardRarity rarity) // 희귀도별 골드 비용표
    {
        switch (rarity)
        {
            case CardRarity.Rare: return RareGoldCosts;           // 희귀
            case CardRarity.Special: return SpecialGoldCosts;     // 특수
            case CardRarity.Legendary: return LegendaryGoldCosts; // 전설
            default: return CommonGoldCosts;                      // 일반
        }
    }

    public static int GetEssenceCost(CardRarity rarity, int currentLevel) // 다음 단계 정수 비용
    {
        return GetStepCost(EssenceCosts, currentLevel);
    }

    public static int GetGoldCost(CardRarity rarity, int currentLevel) // 다음 단계 골드 비용
    {
        return GetStepCost(GetGoldCosts(rarity), currentLevel);
    }

    public static int GetStatValue(int baseValue, int growthPerLevel, int enhanceLevel) // 강화 반영 능력치
    {
        int levelStep = ClampLevel(enhanceLevel) - MinLevel;

        return Mathf.Max(0, baseValue) +
               Mathf.Max(0, growthPerLevel) * levelStep;
    }

    public static Color GetLevelColor(int enhanceLevel) // 강화 단계 표시 색상
    {
        switch (ClampLevel(enhanceLevel))
        {
            case 1: return new Color(0.72f, 0.72f, 0.76f, 1f); // 회색
            case 2: return new Color(0.55f, 0.82f, 0.62f, 1f); // 연녹색
            case 3: return new Color(0.45f, 0.72f, 1f, 1f);    // 하늘색
            case 4: return new Color(0.78f, 0.55f, 1f, 1f);    // 보라색
            default: return new Color(1f, 0.80f, 0.34f, 1f);   // 금색
        }
    }
}
