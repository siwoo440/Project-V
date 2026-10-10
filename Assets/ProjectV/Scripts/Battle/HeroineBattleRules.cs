using UnityEngine; // Unity 기본 기능

// 히로인 전투의 능력치 보정과 보상 규칙 (기획서 D.2 / 9.7.2 / 9.8.2 / 9.9 / C.24 ~ C.28)
public static class HeroineBattleRules
{
    public const int MainStageCount = 3; // 주요 히로인전은 1, 2, 3차전
    public const int SubBattleCount = 3; // 지역마다 서브 히로인전 3개 (기획서 F.17)

    // 챕터 1, 보통 난이도, 최초 승리 기준. 서브 히로인전은 1차전과 같은 골드와 경험치를 준다.
    private static readonly int[] MainGold = { 200, 300, 500 };
    private static readonly int[] MainExperience = { 80, 120, 200 };

    // 욕망의 파편은 지역 배율과 난이도 배율을 쓰지 않고 표의 값을 쓴다. [난이도, 차수] (기획서 C.26 / C.27)
    private static readonly int[,] MainShards = { { 2, 3, 4 }, { 3, 5, 7 }, { 4, 6, 9 } };
    private static readonly int[] SubShards = { 3, 5, 7 };

    // 지역 클리어 보상 골드. 난이도와 관계없이 한 번만 준다. (기획서 C.28)
    private static readonly int[] RegionClearGold = { 500, 700, 900, 1200, 1500, 1900, 2400, 3000, 3800 };

    public static int GetLustDamagePercent(LustResistance resistance) // 성욕 피해 배율 (기획서 D.2.2)
    {
        switch (resistance)
        {
            case LustResistance.Low: return 120;
            case LustResistance.High: return 75;
            case LustResistance.VeryHigh: return 50;
            default: return 100;
        }
    }

    public static string GetLustResistanceName(LustResistance resistance) // 성욕 저항 표시 이름
    {
        switch (resistance)
        {
            case LustResistance.Low: return "낮음";
            case LustResistance.High: return "높음";
            case LustResistance.VeryHigh: return "매우 높음";
            default: return "보통";
        }
    }

    public static string GetActionCountText(HeroineBattleData battle) // 한 턴의 행동 횟수 안내 (기획서 D.2.3)
    {
        if (battle == null) { return string.Empty; }

        return battle.ExtraActionInterval > 0
            ? $"매 턴 {battle.ActionsPerTurn}회, {battle.ExtraActionInterval}턴마다 {battle.ActionsPerTurn + 1}회"
            : $"매 턴 {battle.ActionsPerTurn}회";
    }

    public static int ScalePercent(int baseValue, int percent) // 배율을 곱하고 소수점은 반올림한다. (기획서 D.2.1)
    {
        return (Mathf.Max(0, baseValue) * Mathf.Max(0, percent) + 50) / 100;
    }

    public static int GetMaxHp(HeroineBattleData battle, BattleDifficulty difficulty) // 난이도를 반영한 최대 HP
    {
        return battle == null
            ? 1
            : Mathf.Max(1, ScalePercent(battle.MaxHp, StageRules.GetEnemyHpPercent(difficulty)));
    }

    public static int GetAttack(HeroineBattleData battle, BattleDifficulty difficulty) // 난이도를 반영한 공격력
    {
        return battle == null
            ? 0
            : ScalePercent(battle.Attack, StageRules.GetEnemyAttackPercent(difficulty));
    }

    // 성욕 저항을 반영한 성욕 증가량. 0보다 큰 증가는 적어도 1이 된다.
    public static int ApplyLustResistance(int amount, int lustDamagePercent)
    {
        return amount <= 0 ? 0 : Mathf.Max(1, ScalePercent(amount, lustDamagePercent));
    }

    private static int GetStageIndex(HeroineBattleData battle) // 보상표에서 읽을 칸. 서브 히로인전은 1차전 칸을 쓴다.
    {
        return battle != null && battle.IsMain
            ? Mathf.Clamp(battle.Stage - 1, 0, MainStageCount - 1)
            : 0;
    }

    public static int GetGold(HeroineBattleData battle, int regionOrder, BattleDifficulty difficulty, bool isFirstClear)
    {
        return StageRules.ApplyRepeat(
            StageRules.ApplyMultipliers(MainGold[GetStageIndex(battle)], regionOrder, difficulty),
            isFirstClear
        );
    }

    public static int GetExperience(HeroineBattleData battle, int regionOrder, BattleDifficulty difficulty, bool isFirstClear)
    {
        return StageRules.ApplyRepeat(
            StageRules.ApplyMultipliers(MainExperience[GetStageIndex(battle)], regionOrder, difficulty),
            isFirstClear
        );
    }

    public static int GetShards(HeroineBattleData battle, BattleDifficulty difficulty, bool isFirstClear)
    {
        int difficultyIndex = Mathf.Clamp((int)difficulty, 0, SubShards.Length - 1);

        int firstAmount = battle != null && battle.IsMain
            ? MainShards[difficultyIndex, GetStageIndex(battle)]
            : SubShards[difficultyIndex];

        return StageRules.ApplyRepeat(firstAmount, isFirstClear); // 재전투는 최초의 60%, 소수점 버림
    }

    public static int GetRegionClearGold(int regionOrder) // 지역 클리어 골드
    {
        return RegionClearGold[Mathf.Clamp(regionOrder - 1, 0, RegionClearGold.Length - 1)];
    }

    public static int GetRegionClearShards(int regionOrder) // 지역 클리어 파편: 30개에서 지역마다 5개씩 늘어난다.
    {
        return 30 + 5 * (Mathf.Clamp(regionOrder, 1, RegionClearGold.Length) - 1);
    }
}
