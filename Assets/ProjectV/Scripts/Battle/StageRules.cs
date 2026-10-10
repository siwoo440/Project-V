using UnityEngine; // Unity 기본 기능

// 스테이지의 난이도와 보상 규칙 (기획서 4.17 / 5.21 / 9.4 ~ 9.8)
public static class StageRules
{
    public const int RepeatRewardPercent = 60;     // 반복 승리 보상: 최초 보상의 60% (기획서 9.4)
    public const int DefeatExperiencePercent = 25; // 패배 경험치: 승리 경험치의 25% (기획서 9.8.3)

    public static readonly BattleDifficulty[] Difficulties =
    {
        BattleDifficulty.Easy, BattleDifficulty.Normal, BattleDifficulty.Hard
    }; // 화면에 보여 주는 순서

    // 일반전 단계별 기준 보상 (챕터 1, 보통 난이도. 기획서 9.7.1 / 9.8.2)
    private static readonly int[] NormalFirstGold = { 100, 140, 200 };
    private static readonly int[] NormalRepeatGold = { 60, 90, 120 }; // 2단계는 표의 값(90)을 따른다. 60%로 계산하면 84다.
    private static readonly int[] NormalExperience = { 40, 60, 80 };
    private static readonly int[] NormalEssence = { 5, 8, 12 }; // 마물의 정수 (기획서 9.10.2)

    public const int CaptureExperience = 60; // 포획전 경험치 (기획서 9.8.2)
    public const int CaptureBaseEssence = 3; // 포획전 기본 정수. 쓰러뜨린 마물 수만큼 더한다. (기획서 9.10.3)

    public static string GetDifficultyName(BattleDifficulty difficulty) // 난이도 표시 이름
    {
        switch (difficulty)
        {
            case BattleDifficulty.Easy: return "쉬움";
            case BattleDifficulty.Hard: return "어려움";
            default: return "보통";
        }
    }

    public static string GetDifficultyIconKey(BattleDifficulty difficulty) // 난이도 아이콘 이미지 이름
    {
        switch (difficulty)
        {
            case BattleDifficulty.Easy: return UIKeys.DifficultyEasy;
            case BattleDifficulty.Hard: return UIKeys.DifficultyHard;
            default: return UIKeys.DifficultyNormal;
        }
    }

    public static int GetEnemyHpPercent(BattleDifficulty difficulty) // 적 HP 배율 (기획서 8.11)
    {
        switch (difficulty)
        {
            case BattleDifficulty.Easy: return 80;
            case BattleDifficulty.Hard: return 130;
            default: return 100;
        }
    }

    public static int GetEnemyAttackPercent(BattleDifficulty difficulty) // 적 ATK 배율 (기획서 8.11)
    {
        switch (difficulty)
        {
            case BattleDifficulty.Easy: return 80;
            case BattleDifficulty.Hard: return 120;
            default: return 100;
        }
    }

    public static int GetRewardPercent(BattleDifficulty difficulty) // 골드, 경험치, 정수 배율 (기획서 9.6)
    {
        switch (difficulty)
        {
            case BattleDifficulty.Easy: return 80;
            case BattleDifficulty.Hard: return 130;
            default: return 100;
        }
    }

    public static int GetRegionRewardPercent(int regionOrder) // 지역 보상 배율: 1 + 0.2 × (지역 순서 - 1) (기획서 9.5)
    {
        return 100 + 20 * (Mathf.Max(1, regionOrder) - 1);
    }

    // 기준 보상에 지역 배율과 난이도 배율을 곱한다. 소수점은 버린다.
    public static int ApplyMultipliers(int baseAmount, int regionOrder, BattleDifficulty difficulty)
    {
        long scaled =
            (long)Mathf.Max(0, baseAmount) *
            GetRegionRewardPercent(regionOrder) *
            GetRewardPercent(difficulty);

        return (int)(scaled / 10000L);
    }

    public static int GetNormalGold(int stage, int regionOrder, BattleDifficulty difficulty, bool isFirstClear)
    {
        int index = Mathf.Clamp(stage - 1, 0, NormalFirstGold.Length - 1);

        return ApplyMultipliers(
            isFirstClear ? NormalFirstGold[index] : NormalRepeatGold[index],
            regionOrder, difficulty
        );
    }

    public static int GetNormalExperience(int stage, int regionOrder, BattleDifficulty difficulty, bool isFirstClear)
    {
        int index = Mathf.Clamp(stage - 1, 0, NormalExperience.Length - 1);
        int firstAmount = ApplyMultipliers(NormalExperience[index], regionOrder, difficulty);

        return ApplyRepeat(firstAmount, isFirstClear);
    }

    public static int ApplyRepeat(int firstAmount, bool isFirstClear) // 반복 승리면 최초 보상의 60% (소수점 버림)
    {
        return isFirstClear ? firstAmount : firstAmount * RepeatRewardPercent / 100;
    }

    public static int GetNormalEssence(int stage, int regionOrder, BattleDifficulty difficulty, bool isFirstClear)
    {
        int index = Mathf.Clamp(stage - 1, 0, NormalEssence.Length - 1);

        return ApplyRepeat(ApplyMultipliers(NormalEssence[index], regionOrder, difficulty), isFirstClear);
    }

    // 포획전은 난이도가 지역마다 고정이라 난이도 배율이 없다. 골드도 주지 않는다. (기획서 8.12 / 9.7.4)
    public static int GetCaptureExperience(int regionOrder, bool isFirstClear)
    {
        return ApplyRepeat(
            ApplyMultipliers(CaptureExperience, regionOrder, BattleDifficulty.Normal),
            isFirstClear
        );
    }

    public static int GetCaptureEssence(int defeatedCount, bool isFirstClear) // 기본 3개 + 쓰러뜨린 마물 수
    {
        return ApplyRepeat(CaptureBaseEssence + Mathf.Max(0, defeatedCount), isFirstClear);
    }

    // 패배 경험치. 지역과 난이도 배율은 적용하고 반복 감소는 적용하지 않은 승리 경험치를 넣는다.
    public static int GetDefeatExperience(int firstVictoryExperience)
    {
        return Mathf.Max(0, firstVictoryExperience) * DefeatExperiencePercent / 100;
    }
}
