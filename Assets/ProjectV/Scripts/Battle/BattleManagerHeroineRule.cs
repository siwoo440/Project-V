using UnityEngine; // Unity 기본 기능

// 히로인전의 공통 전투 규칙 (기획서 5.14.4 / D.2.2 / D.2.6 / D.4)
// 절정 상태, 반격, 페이즈 전환을 다룬다. 행동 횟수와 행동 선택은 BattleManagerHeroinePlan.cs에 있다.
public partial class BattleManager
{
    private int heroineCounterPercent; // 반격 준비: 다음에 공격해 온 마물에게 공격력의 이 비율로 반격한다. (0이면 없음)
    private bool isHeroinePhaseActive; // 페이즈 전환이 발동했는지 여부 (전투당 한 번)
    private bool isHeroineClimax;      // 절정 상태: 성욕이 최대인 상태

    private void ResetHeroineRules() // 전투를 시작할 때 규칙 상태를 비운다.
    {
        plannedHeroineActions.Clear();
        heroineTurnsTaken = 0;
        heroineCounterPercent = 0;
        isHeroinePhaseActive = false;
        isHeroineClimax = false;
    }

    private string GetLustStageName() // 성욕 단계 이름 (기획서 5.14.2)
    {
        if (heroineLust >= heroineMaxLust) { return "절정"; }

        return IsHeroineShaken ? "동요" : "정상";
    }

    // 히로인의 HP가 0이면 전투를 끝낸다. 같은 공격으로 성욕도 최대가 됐으면 절정 승리를 우선한다. (기획서 5.14.4)
    private bool TryEndBattleByHeroineHp()
    {
        if (heroineCurrentHp > 0) { return false; }

        EndBattle(
            heroineLust >= heroineMaxLust
                ? BattleOutcome.VictoryLust
                : BattleOutcome.VictoryHp
        );

        return true;
    }

    // 성욕이 최대가 되어도 바로 이기지 않는다. 히로인이 한 턴 행동한 뒤에도 최대이면 내 턴이 시작될 때 절정 승리한다.
    private bool TryEndBattleByClimax()
    {
        if (isEnemyBattle || heroineLust < heroineMaxLust) { return false; }

        EndBattle(BattleOutcome.VictoryLust);

        return true;
    }

    private void UpdateHeroineClimaxState() // 절정 상태에 들어가거나 벗어날 때 전투 로그에 알린다.
    {
        bool isFull = !isEnemyBattle && heroineLust >= heroineMaxLust;

        if (isFull == isHeroineClimax) { return; }

        isHeroineClimax = isFull;

        AddBattleLog(
            BattleLogCategory.System,
            isFull
                ? "절정 상태: 히로인의 턴이 지난 뒤에도 성욕이 최대이면 절정 승리합니다."
                : "히로인이 절정 상태에서 벗어났습니다."
        );
    }

    private void ShowHeroineClimaxNotice() // 절정 상태에 들어간 공격의 결과 문구에 안내를 덧붙인다.
    {
        if (!isHeroineClimax || isBattleEnded || resultText == null) { return; }

        resultText.text += "\n절정 상태: 히로인의 턴 뒤에도 유지되면 승리";
    }

    private void SetHeroineCounter(int percent) // 반격 준비 (수호 자세 등)
    {
        if (percent <= 0) { return; }

        heroineCounterPercent = percent;

        AddBattleLog(
            BattleLogCategory.HeroineAction,
            $"반격 준비: 다음에 공격한 마물에게 공격력의 {percent}%로 반격합니다."
        );
    }

    // 반격을 준비한 히로인을 공격했으면 공격한 마물이 반격을 받는다. 한 번 쓰면 사라진다. (기획서 D.4 수호 자세)
    private string TryHeroineCounter(MonsterUnit attackingMonster)
    {
        if (heroineCounterPercent <= 0 || heroineCurrentHp <= 0) { return string.Empty; }
        if (attackingMonster == null || attackingMonster.IsDead) { return string.Empty; }

        int counterPower = GetHeroineCurrentAttack(
            HeroineBattleRules.ScalePercent(heroineAttack, heroineCounterPercent)
        );

        heroineCounterPercent = 0;

        string monsterName = attackingMonster.MonsterName;
        DamageResult damageResult = attackingMonster.TakeDamage(counterPower);

        if (attackingMonster.IsDead)
        {
            HandleMonsterDefeated(attackingMonster); // 사망 효과 실행 후 제거

            return $"반격: {monsterName} 사망";
        }

        return $"반격: {CreateDamageResultText(monsterName, damageResult)}";
    }

    // HP가 기준 이하로 내려가면 페이즈 전환이 한 번 발동한다. 상태 효과와 보호막, 성욕은 그대로 둔다. (기획서 D.2.6)
    private void CheckHeroinePhase()
    {
        if (isHeroinePhaseActive || heroineBattle == null || !heroineBattle.HasPhase) { return; }
        if (heroineCurrentHp <= 0) { return; }
        if (heroineCurrentHp * 100 > heroineMaxHp * heroineBattle.PhaseHpPercent) { return; }

        isHeroinePhaseActive = true;

        AddBattleLog(
            BattleLogCategory.HeroineAction,
            $"{heroineBattle.HeroineName}: {heroineBattle.PhaseName} 발동" +
            (heroineBattle.PhaseDefenseBonus > 0
                ? $" (방어 +{heroineBattle.PhaseDefenseBonus})"
                : string.Empty)
        );
    }

    private int GetHeroinePhaseDefenseBonus() // 페이즈 전환으로 오른 방어력
    {
        return isHeroinePhaseActive && heroineBattle != null
            ? heroineBattle.PhaseDefenseBonus
            : 0;
    }
}
