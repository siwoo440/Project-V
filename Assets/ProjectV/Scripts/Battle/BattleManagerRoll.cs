// 공격의 회피와 치명타 판정 (기획서 5.12 / A.7)
// 기본 공격과 히로인의 공격 행동, 적 마물의 공격에 적용한다. 회피를 먼저 보고, 맞았을 때 치명타를 본다.
// 상태 효과만 거는 행동, 독 피해, 반격에는 적용하지 않는다.
public partial class BattleManager
{
    private bool lastAttackEvaded; // 방금 한 내 마물의 공격이 회피됐는지 여부 (딸린 상태 효과와 반격을 건너뛴다)

    // 확률은 지금 모두 기본값이다. 마물 효과나 스킬로 확률이 바뀌는 데이터가 생기면 아래 함수에서 더한다.
    private int GetMonsterCritPercent(MonsterUnit attacker) { return CombatRollRules.BaseCritPercent; }
    private int GetMonsterEvasionPercent(MonsterUnit target) { return CombatRollRules.BaseEvasionPercent; }
    private int GetHeroineCritPercent() { return CombatRollRules.BaseCritPercent; }
    private int GetHeroineEvasionPercent() { return CombatRollRules.BaseEvasionPercent; }
    private int GetPlayerEvasionPercent() { return CombatRollRules.BaseEvasionPercent; }

    private AttackRoll RollAttack(int attackPower, int evasionPercent, int critPercent)
    {
        if (CombatRollRules.Roll(CombatRollRules.ClampEvasion(evasionPercent)))
        {
            return new AttackRoll(true, false, 0); // 회피: 피해도 치명타도 없다.
        }

        bool isCritical =
            attackPower > 0 &&
            CombatRollRules.Roll(CombatRollRules.ClampCrit(critPercent));

        return new AttackRoll(
            false,
            isCritical,
            isCritical ? CombatRollRules.ApplyCrit(attackPower) : attackPower
        );
    }

    private static string GetCritTag(AttackRoll roll) // 결과 문구 뒤에 붙이는 치명타 표시
    {
        return roll.IsCritical ? " 치명타!" : string.Empty;
    }

    // 플레이어를 직접 공격한다. 플레이어도 기본 확률로 공격을 회피한다.
    private void AttackPlayerWithRoll(int attackPower, string actionName, int critPercent)
    {
        AttackRoll roll = RollAttack(attackPower, GetPlayerEvasionPercent(), critPercent);

        if (roll.IsEvaded)
        {
            resultText.text = $"{actionName}: 플레이어가 회피했습니다";
            AddBattleLog(BattleLogCategory.HeroineAction, resultText.text);
            return;
        }

        ApplyDamageToPlayer(roll.AttackPower, actionName + GetCritTag(roll));
    }
}
