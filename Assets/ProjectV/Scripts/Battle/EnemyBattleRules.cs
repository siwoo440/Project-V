using UnityEngine; // Unity 기본 기능

// 적 마물 전투 규칙 (기획서 8.10 / F.5 / F.6)
public static class EnemyBattleRules
{
    public const int MaxEnemyCount = 5;  // 한 전투에 나오는 적 마물 수의 상한
    public const int DefenseLimit = 20;  // 적 DEF 상한

    private static readonly int[] RegionStatPercents =
    {
        100, 108, 116, 124, 132, 142, 152, 164, 178
    }; // 지역 계수 (R01 ~ R09)

    public static int GetRegionStatPercent(int regionOrder) // 지역 순서에 해당하는 능력치 계수
    {
        int index = Mathf.Clamp(regionOrder - 1, 0, RegionStatPercents.Length - 1);

        return RegionStatPercents[index];
    }

    public static int GetRegionDefenseBonus(int regionOrder) // 지역 DEF 보정
    {
        if (regionOrder >= 9) { return 3; }
        if (regionOrder >= 7) { return 2; }
        if (regionOrder >= 4) { return 1; }

        return 0;
    }

    // 기본 수치에 지역 계수, 단계 계수, 난이도 배율을 곱한다. 최종 수치는 반올림한다.
    public static int ScaleStat(int baseValue, int regionPercent, int stagePercent, int difficultyPercent)
    {
        float scaled =
            baseValue *
            (regionPercent / 100f) *
            (stagePercent / 100f) *
            (difficultyPercent / 100f);

        return Mathf.Max(0, Mathf.FloorToInt(scaled + 0.5f));
    }

    // 적 마물의 실제 능력치. 전투의 마물 판과 전투 준비 화면이 같은 계산을 쓴다.
    public static int GetHp(MonsterData monster, int regionPercent, int stagePercent, BattleDifficulty difficulty)
    {
        return Mathf.Max(
            1,
            ScaleStat(monster.MaxHp, regionPercent, stagePercent, StageRules.GetEnemyHpPercent(difficulty))
        );
    }

    public static int GetAttack(MonsterData monster, int regionPercent, int stagePercent, BattleDifficulty difficulty)
    {
        return ScaleStat(monster.Attack, regionPercent, stagePercent, StageRules.GetEnemyAttackPercent(difficulty));
    }

    public static int GetDefense(MonsterData monster, int defenseBonus)
    {
        return ClampDefense(monster.Defense + defenseBonus);
    }

    public static int ClampDefense(int defense) // DEF는 0 ~ 20 사이로 맞춘다.
    {
        return Mathf.Clamp(defense, 0, DefenseLimit);
    }
}
