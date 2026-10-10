using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 적 마물 전투 = 일반전의 전투 방식 (기획서 8.10 / F.6)
// 히로인 대신 고정 편성의 적 마물과 싸운다. 적 마물이 모두 쓰러지면 승리하고, 성욕 게이지로 이기는 방법은 없다.
// 적 마물도 아군과 같은 마물 판(MonsterUnit)을 쓴다.
public partial class BattleManager
{
    [Header("적 마물 전투")]
    [SerializeField] private GameObject enemyFieldPanel;    // 적 마물 필드 (적 마물 전투에서만 보인다)
    [SerializeField] private Transform enemyFieldContainer; // 적 마물 배치 영역
    [SerializeField] private GameObject heroinePanel;       // 히로인 정보 (적 마물 전투에서는 숨긴다)
    [SerializeField] private RectTransform turnPanel;       // 턴 표시 (적 마물 전투에서는 오른쪽 위로 옮긴다)

    private readonly List<MonsterUnit> enemyUnits =
        new List<MonsterUnit>(); // 살아 있는 적 마물 (왼쪽부터)

    private readonly Dictionary<MonsterUnit, MonsterUnit> enemyTargets =
        new Dictionary<MonsterUnit, MonsterUnit>(); // 적 마물별 예고 대상 (값이 없으면 플레이어)

    private EnemyFormationData enemyFormation; // 이번 전투의 적 편성
    private bool isEnemyBattle;                // 적 마물 전투 여부

    private bool hasTurnPanelHome;     // 턴 표시의 원래 자리를 기억했는지 여부
    private Vector2 turnPanelHomeAnchor;
    private Vector2 turnPanelHomePosition;

    // 전투를 시작할 때 전투 종류에 맞게 화면을 바꾸고 적 마물을 놓는다.
    private void PrepareEnemyBattle()
    {
        ClearEnemyField();

        enemyFormation = BattleSetup.Formation;

        isEnemyBattle =
            enemyFormation != null &&
            enemyFieldContainer != null &&
            monsterUnitPrefab != null; // 씬을 다시 구성하기 전이면 히로인 전투로 진행한다.

        if (enemyFieldPanel != null) { enemyFieldPanel.SetActive(isEnemyBattle); }
        if (heroinePanel != null) { heroinePanel.SetActive(!isEnemyBattle); }

        if (lustAttackButton != null)
        {
            lustAttackButton.gameObject.SetActive(!isEnemyBattle); // 일반전에는 성욕 공격이 없다.
        }

        PlaceTurnPanel();

        if (!isEnemyBattle) { return; }

        SpawnEnemies();

        AddBattleLog(
            BattleLogCategory.System,
            $"{BattleSetup.StageTitle}: 적 마물 {enemyUnits.Count}체 ({enemyFormation.EnemyListText})"
        );
    }

    // 적 마물 필드가 위쪽 가운데를 쓰므로 턴 표시를 히로인 정보가 있던 오른쪽 위로 옮긴다.
    private void PlaceTurnPanel()
    {
        if (turnPanel == null) { return; }

        if (!hasTurnPanelHome)
        {
            hasTurnPanelHome = true;
            turnPanelHomeAnchor = turnPanel.anchorMin;
            turnPanelHomePosition = turnPanel.anchoredPosition;
        }

        Vector2 anchor = isEnemyBattle ? new Vector2(1f, 1f) : turnPanelHomeAnchor;

        turnPanel.anchorMin = anchor;
        turnPanel.anchorMax = anchor;

        turnPanel.anchoredPosition = isEnemyBattle
            ? new Vector2(-180f, turnPanelHomePosition.y)
            : turnPanelHomePosition;
    }

    private void SpawnEnemies() // 편성의 적 마물을 왼쪽부터 놓는다. 능력치는 기획서 F.5.1의 계수를 적용한다.
    {
        int regionPercent = EnemyBattleRules.GetRegionStatPercent(BattleSetup.RegionOrder);
        int regionDefense = EnemyBattleRules.GetRegionDefenseBonus(BattleSetup.RegionOrder);

        foreach (MonsterData enemyData in enemyFormation.Enemies)
        {
            if (enemyData == null) { continue; }
            if (enemyUnits.Count >= EnemyBattleRules.MaxEnemyCount) { break; }

            MonsterUnit enemyUnit = Instantiate(monsterUnitPrefab, enemyFieldContainer);

            enemyUnit.Initialize(
                enemyData,
                CardEnhanceRules.MinLevel,
                OnEnemyClicked,
                statusEffectIconPrefab,
                statusEffectTooltipUI
            );

            enemyUnit.SetupAsEnemy(
                regionPercent,
                enemyFormation.HpPercent,
                enemyFormation.AttackPercent,
                regionDefense + enemyFormation.DefenseBonus
            );

            enemyUnits.Add(enemyUnit);
        }
    }

    private void ClearEnemyField()
    {
        foreach (MonsterUnit enemyUnit in enemyUnits)
        {
            if (enemyUnit != null) { Destroy(enemyUnit.gameObject); }
        }

        enemyUnits.Clear();
        enemyTargets.Clear();
    }

    private List<MonsterUnit> GetLivingEnemies()
    {
        enemyUnits.RemoveAll(enemyUnit => enemyUnit == null || enemyUnit.IsDead);

        return new List<MonsterUnit>(enemyUnits);
    }

    // 공격할 수 있는 적 마물. 도발 중인 적이 있으면 그 적부터 쓰러뜨려야 한다.
    private List<MonsterUnit> GetAttackableEnemies()
    {
        List<MonsterUnit> livingEnemies = GetLivingEnemies();
        List<MonsterUnit> tauntingEnemies = GetTauntingMonsterCandidates(livingEnemies);

        return tauntingEnemies.Count > 0 ? tauntingEnemies : livingEnemies;
    }

    private void MarkEnemyTargets(bool show) // 공격 대상 후보 표시
    {
        if (!isEnemyBattle) { return; }

        List<MonsterUnit> attackableEnemies = show
            ? GetAttackableEnemies()
            : new List<MonsterUnit>();

        foreach (MonsterUnit enemyUnit in enemyUnits)
        {
            if (enemyUnit == null) { continue; }

            enemyUnit.SetAttackTargetable(attackableEnemies.Contains(enemyUnit));
        }
    }

    private void SetEnemyInteractable(bool isInteractable) // 플레이어 턴에만 적 마물을 누를 수 있다.
    {
        foreach (MonsterUnit enemyUnit in enemyUnits)
        {
            if (enemyUnit != null) { enemyUnit.SetPlayerTurnInteraction(isInteractable); }
        }
    }

    private bool CheckEnemyBattleVictory() // 적 마물이 모두 쓰러졌으면 승리로 끝낸다.
    {
        if (!isEnemyBattle || isBattleEnded) { return false; }
        if (GetLivingEnemies().Count > 0) { return false; }

        EndBattle(BattleOutcome.VictoryHp);

        return true;
    }

    private void HandleEnemyDefeated(MonsterUnit defeatedEnemy) // 쓰러진 적 마물 제거
    {
        if (defeatedEnemy == null) { return; }

        enemyUnits.Remove(defeatedEnemy);
        enemyTargets.Remove(defeatedEnemy);

        AddBattleLog(
            BattleLogCategory.System,
            $"적 처치: {defeatedEnemy.MonsterName} (남은 적 {enemyUnits.Count}체)"
        );

        Destroy(defeatedEnemy.gameObject);
    }
}
