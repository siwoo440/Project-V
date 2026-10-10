using System.Text; // 문자열 조립 기능

// 전투 전에 보여 주는 상대 안내 (기획서 6.14 / 10.18)
// 지역 화면이 정해 둔 전투(BattleSetup)를 읽어 글로 만든다. 추천 덱, 추천 계열, 추천 시너지는 적지 않는다.
public static class BattleBriefing
{
    public static string GetKindName() // 전투 종류 이름
    {
        switch (BattleSetup.Kind)
        {
            case BattleKind.Normal: return "일반전";
            case BattleKind.Capture: return "포획전";
        }

        if (BattleSetup.Heroine == null) { return "시험 전투"; }

        return BattleSetup.Heroine.IsMain ? "주요 히로인전" : "서브 히로인전";
    }

    public static string GetOpponentTitle() // 상대 칸의 제목 줄
    {
        if (BattleSetup.IsEnemyBattle)
        {
            return $"적 마물 {BattleSetup.Enemies.Count}체";
        }

        HeroineBattleData heroine = BattleSetup.Heroine;

        return heroine == null
            ? "시험 히로인"
            : $"{heroine.HeroineName}  ({heroine.HeroineTitle})";
    }

    public static string GetOpponentText() // 상대의 능력치와 행동
    {
        if (BattleSetup.IsEnemyBattle) { return GetEnemyText(); }

        return BattleSetup.Heroine == null
            ? "전투 씬에 적힌 시험 히로인과 싸웁니다. 난이도와 승리 기록이 없습니다."
            : GetHeroineText(BattleSetup.Heroine);
    }

    // 히로인: 고른 난이도의 능력치, 행동 횟수, 행동 목록, 페이즈 전환
    private static string GetHeroineText(HeroineBattleData heroine)
    {
        BattleDifficulty difficulty = BattleSetup.Difficulty;
        int attack = HeroineBattleRules.GetAttack(heroine, difficulty);

        StringBuilder builder = new StringBuilder();

        builder.Append(
            $"{UISkin.IconOr(UIIcons.Hp, "HP")} {HeroineBattleRules.GetMaxHp(heroine, difficulty)}    " +
            $"{UISkin.IconOr(UIIcons.Attack, "공격")} {attack}    " +
            $"{UISkin.IconOr(UIIcons.Defense, "방어")} {heroine.Defense}    " +
            $"성욕 저항 {HeroineBattleRules.GetLustResistanceName(heroine.LustResistance)}\n"
        );

        builder.Append($"행동 횟수: {HeroineBattleRules.GetActionCountText(heroine)}\n\n");

        foreach (HeroineActionData action in heroine.Actions)
        {
            if (action == null) { continue; }

            builder.Append($"{HeroineBattleRules.GetActionText(action, attack)}\n");
        }

        if (heroine.HasPhase)
        {
            builder.Append(
                $"\nHP {heroine.PhaseHpPercent}% 이하: {heroine.PhaseName}" +
                (heroine.PhaseDefenseBonus > 0 ? $" (방어 +{heroine.PhaseDefenseBonus})" : string.Empty) +
                "\n"
            );
        }

        return builder.ToString();
    }

    // 적 마물: 마물마다 계열, 희귀도, 고른 난이도의 능력치, 공격에 딸린 상태 효과
    private static string GetEnemyText()
    {
        int regionPercent = EnemyBattleRules.GetRegionStatPercent(BattleSetup.RegionOrder);

        int defenseBonus =
            EnemyBattleRules.GetRegionDefenseBonus(BattleSetup.RegionOrder) + BattleSetup.DefenseBonus;

        BattleDifficulty difficulty = BattleSetup.Difficulty;
        StringBuilder builder = new StringBuilder();

        foreach (MonsterData enemy in BattleSetup.Enemies)
        {
            if (enemy == null) { continue; }

            builder.Append(
                $"{enemy.MonsterName}  ({MonsterTypeRules.GetDisplayName(enemy.MainType)}, " +
                $"{CardRarityRules.GetDisplayName(enemy.Rarity)})\n"
            );

            builder.Append(
                $"    {UISkin.IconOr(UIIcons.Hp, "HP")} {EnemyBattleRules.GetHp(enemy, regionPercent, BattleSetup.HpPercent, difficulty)}    " +
                $"{UISkin.IconOr(UIIcons.Attack, "공격")} {EnemyBattleRules.GetAttack(enemy, regionPercent, BattleSetup.AttackPercent, difficulty)}    " +
                $"{UISkin.IconOr(UIIcons.Defense, "방어")} {EnemyBattleRules.GetDefense(enemy, defenseBonus)}" +
                (enemy.AttackStatusEffect == null
                    ? string.Empty
                    : $"    공격 효과: {enemy.AttackStatusEffect.DisplayName}") +
                "\n"
            );
        }

        builder.Append("\n적 마물은 기본 공격을 합니다. 도발 중인 마물, 그다음 HP가 가장 낮은 마물을 노립니다.\n");

        if (BattleSetup.IsCaptureBattle)
        {
            builder.Append("포획전은 지역마다 난이도가 고정입니다.\n");
        }

        return builder.ToString();
    }
}
