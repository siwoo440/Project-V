using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

public partial class BattleManager // 적 마물 전투: 아군 마물이 적 마물을 공격
{
    // 적 마물을 눌렀을 때: 고른 아군 마물이 그 적을 공격한다.
    private void OnEnemyClicked(MonsterUnit enemyUnit)
    {
        if (!isPlayerTurn || isBattleEnded) { return; }
        if (enemyUnit == null || enemyUnit.IsDead) { return; }
        if (isSelectingSkillTarget || isSelectingItemTarget) { return; } // 아군 대상을 고르는 중에는 무시한다.

        if (selectedMonster == null || !selectedMonster.CanAttack)
        {
            resultText.text = "먼저 공격할 아군 마물을 선택하세요";
            return;
        }

        if (!GetAttackableEnemies().Contains(enemyUnit))
        {
            resultText.text = "도발 중인 적 마물을 먼저 공격해야 합니다";
            return;
        }

        ExecuteMonsterAttackOnEnemy(selectedMonster, enemyUnit);
    }

    // 공격 버튼을 눌렀을 때: 공격할 수 있는 적이 하나뿐이면 바로 공격하고, 여럿이면 대상을 고르게 한다.
    private void AttackEnemyWithButton()
    {
        List<MonsterUnit> attackableEnemies = GetAttackableEnemies();

        if (attackableEnemies.Count == 0)
        {
            resultText.text = "공격할 적 마물이 없습니다";
            return;
        }

        if (attackableEnemies.Count == 1)
        {
            ExecuteMonsterAttackOnEnemy(selectedMonster, attackableEnemies[0]);
            return;
        }

        resultText.text = "공격할 적 마물을 누르세요";
    }

    private void ExecuteMonsterAttackOnEnemy(MonsterUnit attackingMonster, MonsterUnit enemyUnit)
    {
        if (attackingMonster == null || enemyUnit == null) { return; }

        string enemyName = enemyUnit.MonsterName;
        DamageResult damageResult = enemyUnit.TakeDamage(attackingMonster.Attack); // 보호막과 방어력 포함 피해 처리

        string attackText =
            $"[공격] {attackingMonster.MonsterName} → {CreateDamageResultText(enemyName, damageResult)}";

        string statusText = string.Empty;
        StatusEffectData statusData = attackingMonster.AttackStatusEffect;

        if (statusData != null && !enemyUnit.IsDead)
        {
            enemyUnit.ApplyOrRefreshStatus(statusData); // 공격에 딸린 상태 효과
            statusText = $"{enemyName}: {statusData.DisplayName} {GetStatusAmountDisplay(statusData)}";
        }

        attackingMonster.MarkActed();
        ClearMonsterSelection();

        resultText.text = string.IsNullOrEmpty(statusText)
            ? attackText
            : $"{attackText}\n{statusText}";

        AddBattleLog(BattleLogCategory.PlayerAction, attackText);

        if (!string.IsNullOrEmpty(statusText))
        {
            AddBattleLog(BattleLogCategory.StatusEffect, statusText);
        }

        if (enemyUnit.IsDead)
        {
            HandleEnemyDefeated(enemyUnit);
        }

        RefreshSynergies();
        RefreshEnemyIntents(); // 적이 줄었거나 상태가 바뀌었으므로 예고를 다시 정한다.
        UpdateBattleUI();
        CheckEnemyBattleVictory();
    }
}
