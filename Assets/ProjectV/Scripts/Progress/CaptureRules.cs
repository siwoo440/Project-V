using UnityEngine; // Unity 기본 기능

// 포획 콘텐츠 규칙 (기획서 8.12 / 9.13 / 9.14 / F.18)
public static class CaptureRules
{
    public const int SlotCount = 3;              // 지역마다 동시에 보이는 포획 목록 수
    public const int RerollCost = 200;           // 목록 다시 뽑기 비용. 지역과 횟수에 관계없다. (기획서 9.14)
    public const int LegendPityRerolls = 20;     // 유료 리롤을 이만큼 하는 동안 전설이 없으면 다음 리롤에 확정 (기획서 9.13.4)
    public const int UncapturedWeightPercent = 150; // 미포획 마물의 출현 가중치 (같은 희귀도 안에서 1.5배)

    public static int GetRarityWeight(CardRarity rarity) // 희귀도 기본 출현 비율 (기획서 9.13.3)
    {
        switch (rarity)
        {
            case CardRarity.Rare: return 25;
            case CardRarity.Special: return 12;
            case CardRarity.Legendary: return 3;
            default: return 60;
        }
    }

    // 포획전 한 번에 나오는 마물 수의 범위 (기획서 F.18.4)
    public static void GetCountRange(int regionOrder, out int minimum, out int maximum)
    {
        switch (Mathf.Clamp(regionOrder, 1, 9))
        {
            case 1: minimum = 1; maximum = 2; break;
            case 2: minimum = 1; maximum = 3; break;
            case 3:
            case 4: minimum = 2; maximum = 3; break;
            case 5:
            case 6: minimum = 2; maximum = 4; break;
            case 7: minimum = 3; maximum = 4; break;
            default: minimum = 3; maximum = 5; break;
        }
    }
}
