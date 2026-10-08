using UnityEngine; // Unity 기본 기능

// 재화 규칙 (기획서 9.2 / 9.3 / A.47)
public static class CurrencyRules
{
    public const int GoldLimit = 999999;  // 골드 보유 한도
    public const int EssenceLimit = 9999; // 마물의 정수 보유 한도
    public const int ShardLimit = 9999;   // 욕망의 파편 보유 한도

    public const int StartingGold = 500;  // 새 게임 시작 골드 (기획서 9.2)
    public const int StartingShards = 3;  // 새 게임 시작 욕망의 파편
    public const int StartingEssence = 5; // 새 게임 시작 마물의 정수

    // 지금 보유량에서 실제로 더할 수 있는 양. 한도를 넘는 양은 받지 못한다.
    public static int GetAddable(int current, int amount, int limit)
    {
        if (amount <= 0) { return 0; }

        return Mathf.Clamp(limit - current, 0, amount);
    }
}
