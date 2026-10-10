using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using System.Text; // 문자열 조립 기능
using UnityEngine; // Unity 기본 기능

public partial class BattleManager // 적 마물 전투: 적의 행동 예고와 적 턴 (기획서 16.13)
{
    // 적 마물의 대상 규칙 (임시): 도발 중인 아군 마물, 없으면 HP가 가장 낮은 아군 마물, 아군 마물이 없으면 플레이어.
    // 히로인의 단일 공격과 같은 흐름이다. 적마다 다른 행동은 적 AI 데이터를 만드는 일차에 넣는다.
    private MonsterUnit ResolveEnemyTarget()
    {
        List<MonsterUnit> livingMonsters = GetLivingMonsterCandidates();
        List<MonsterUnit> tauntingMonsters = GetTauntingMonsterCandidates(livingMonsters);

        return GetLowestHpMonsterFromCandidates(
            tauntingMonsters.Count > 0 ? tauntingMonsters : livingMonsters
        ); // 없으면 플레이어를 공격한다.
    }

    private void RefreshEnemyIntents() // 적 마물마다 다음 공격 대상을 정해 예고한다.
    {
        ClearHeroineTargetPreview();
        enemyTargets.Clear();

        if (!isEnemyBattle || isBattleEnded) { return; }

        foreach (MonsterUnit enemyUnit in GetLivingEnemies())
        {
            MonsterUnit target = ResolveEnemyTarget();

            enemyTargets[enemyUnit] = target;

            if (target != null) { target.SetHeroineTargeted(true); } // 공격받을 아군 마물 표시

            enemyUnit.SetEnemyIntent($"→ {GetEnemyTargetName(target)}");
        }

        UpdateEnemyIntentText();
    }

    private static string GetEnemyTargetName(MonsterUnit target)
    {
        return target == null ? "플레이어" : target.MonsterName;
    }

    private void UpdateEnemyIntentText() // 오른쪽 예고 창에 적 전체의 다음 행동을 적는다.
    {
        if (heroineIntentText == null) { return; }

        SetHeroineIntentIcon(null);

        if (isBattleEnded)
        {
            heroineIntentText.text = "적 행동 예고: 없음";
            return;
        }

        StringBuilder builder = new StringBuilder("적 행동 예고");

        foreach (MonsterUnit enemyUnit in GetLivingEnemies())
        {
            enemyTargets.TryGetValue(enemyUnit, out MonsterUnit target);

            builder.Append(
                $"\n{enemyUnit.MonsterName}: {UISkin.IconOr(UIIcons.Attack, "공격")} {enemyUnit.Attack} → {GetEnemyTargetName(target)}"
            );
        }

        heroineIntentText.text = builder.ToString();
    }

    private IEnumerator EnemyTurnRoutine() // 적 턴: 왼쪽 적부터 차례로 행동한다.
    {
        AddBattleLog(BattleLogCategory.System, "적 턴을 시작했습니다.");

        ReduceMonsterStatusDurations(StatusDurationTiming.AfterPlayerTurn);
        ReduceMonsterCooldowns(); // 아군 마물 재사용 대기시간 감소

        foreach (MonsterUnit enemyUnit in GetLivingEnemies())
        {
            enemyUnit.ReduceStatusDurations(StatusDurationTiming.AfterPlayerTurn);

            int poisonDamage = enemyUnit.ApplyStartTurnStatusEffects(); // 독 같은 턴 시작 효과

            if (poisonDamage > 0)
            {
                AddBattleLog(
                    BattleLogCategory.StatusEffect,
                    $"독: {enemyUnit.MonsterName} HP -{poisonDamage}"
                );
            }

            if (enemyUnit.IsDead) { HandleEnemyDefeated(enemyUnit); }
        }

        UpdateBattleUI();

        if (CheckEnemyBattleVictory()) { yield break; } // 독으로 마지막 적이 쓰러진 경우

        yield return BattleSpeed.Wait(heroineActionDelay);

        foreach (MonsterUnit enemyUnit in GetLivingEnemies())
        {
            if (enemyUnit == null || enemyUnit.IsDead) { continue; }

            ExecuteEnemyAttack(enemyUnit);
            UpdateBattleUI();

            if (playerCurrentHp <= 0)
            {
                EndBattle(BattleOutcome.Defeat);
                yield break;
            }

            yield return BattleSpeed.Wait(heroineActionDelay * 0.6f);
        }

        foreach (MonsterUnit enemyUnit in GetLivingEnemies())
        {
            enemyUnit.ReduceStatusDurations(StatusDurationTiming.AfterHeroineTurn);
        }

        ReduceMonsterStatusDurations(StatusDurationTiming.AfterHeroineTurn);
        UpdateBattleUI();

        yield return BattleSpeed.Wait(heroineActionDelay * 0.5f);

        BeginNextPlayerTurn();
    }

    private void ExecuteEnemyAttack(MonsterUnit enemyUnit) // 적 마물 하나의 기본 공격
    {
        enemyTargets.TryGetValue(enemyUnit, out MonsterUnit target);

        if (target == null || target.IsDead)
        {
            target = ResolveEnemyTarget(); // 예고한 대상이 사라졌으면 같은 규칙으로 다시 고른다.
        }

        int attackPower = enemyUnit.Attack;

        if (target == null)
        {
            AttackPlayerWithRoll(attackPower, enemyUnit.MonsterName, GetMonsterCritPercent(enemyUnit)); // 아군 마물이 없으면 플레이어를 공격
            return;
        }

        string targetName = target.MonsterName;

        AttackRoll roll = RollAttack(
            attackPower, GetMonsterEvasionPercent(target), GetMonsterCritPercent(enemyUnit)
        );

        if (roll.IsEvaded)
        {
            resultText.text = $"{enemyUnit.MonsterName} → {targetName} 회피";
            AddBattleLog(BattleLogCategory.HeroineAction, resultText.text);
            return; // 회피하면 피해와 딸린 상태 효과가 모두 없다.
        }

        DamageResult damageResult = target.TakeDamage(roll.AttackPower);

        string attackText =
            $"{enemyUnit.MonsterName} → {CreateDamageResultText(targetName, damageResult)}{GetCritTag(roll)}";

        StatusEffectData statusData = enemyUnit.AttackStatusEffect;

        if (statusData != null && !target.IsDead)
        {
            target.ApplyOrRefreshStatus(statusData); // 공격에 딸린 상태 효과
            attackText += $", {statusData.DisplayName} {GetStatusAmountDisplay(statusData)}";
        }

        if (target.IsDead)
        {
            HandleMonsterDefeated(target); // 사망 효과 실행 후 제거
            attackText = $"{enemyUnit.MonsterName} → {targetName} 사망";
        }

        resultText.text = attackText;
        AddBattleLog(BattleLogCategory.HeroineAction, attackText);
    }
}
