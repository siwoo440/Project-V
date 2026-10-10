using UnityEngine; // Unity 기본 기능

// 필드 마물 판을 적 마물로 쓸 때의 처리 (기획서 8.10 / F.5)
// 적 마물은 플레이어가 조작하지 않는다. 공격 대상으로 고를 수 있을 때만 눌리고, 행동 상태 자리에 다음 행동을 예고한다.
public partial class MonsterUnit
{
    private bool isEnemy;            // 적 마물 여부
    private bool isAttackTargetable; // 지금 공격 대상으로 고를 수 있는지 여부
    private string enemyIntentLabel = string.Empty; // 다음 행동 예고 문구

    public bool IsEnemy => isEnemy; // 적 마물 여부 반환

    // 적 마물로 설정한다. 능력치는 기본 수치에 지역 계수, 단계 계수, 난이도 배율을 곱해 정한다. (기획서 F.5.1 / 8.11)
    public void SetupAsEnemy(
        int regionPercent,
        int hpPercent,
        int attackPercent,
        int defenseBonus,
        BattleDifficulty difficulty
    )
    {
        if (monsterData == null) { return; }

        isEnemy = true;
        isAttackTargetable = false;
        enemyIntentLabel = string.Empty;

        runtimeMaxHp = EnemyBattleRules.GetHp(monsterData, regionPercent, hpPercent, difficulty);
        runtimeAttack = EnemyBattleRules.GetAttack(monsterData, regionPercent, attackPercent, difficulty);
        runtimeDefense = EnemyBattleRules.GetDefense(monsterData, defenseBonus);

        runtimeLustDamage = 0; // 일반전에는 성욕 승리가 없다. (기획서 F.6)

        currentHp = MaxHp;
        actionState = MonsterActionState.Ready;

        RefreshSelectButton();
        UpdateBackgroundColor();
        UpdateMonsterUI();
    }

    public void SetAttackTargetable(bool targetable) // 공격 대상 후보 표시
    {
        isAttackTargetable = targetable;
        RefreshSelectButton();
        UpdateBackgroundColor();
    }

    public void SetEnemyIntent(string label) // 다음 행동 예고 표시
    {
        enemyIntentLabel = label ?? string.Empty;
        UpdateMonsterUI();
    }
}
