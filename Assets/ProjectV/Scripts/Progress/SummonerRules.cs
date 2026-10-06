using UnityEngine; // Unity 기본 기능

public static class SummonerRules // 소환사 성장 규칙 (기획서 6.3.5 / 6.5)
{
    public const int PassiveSystemUnlockLevel = 2; // 패시브 시스템 해금 레벨
    public const int FirstPassivePointLevel = 3;   // 첫 패시브 포인트를 받는 레벨

    // 플레이어 레벨까지 받은 패시브 포인트 총량.
    // 기획서에 지급 표가 없어 Lv.3부터 레벨마다 1개로 둔 임시 규칙이다. (Lv.30에 28개)
    public static int GetTotalPassivePoints(int playerLevel)
    {
        return Mathf.Max(0, playerLevel - (FirstPassivePointLevel - 1));
    }

    public static bool IsPassiveSystemUnlocked(int playerLevel) // 패시브 사용 가능 여부
    {
        return playerLevel >= PassiveSystemUnlockLevel;
    }
}
