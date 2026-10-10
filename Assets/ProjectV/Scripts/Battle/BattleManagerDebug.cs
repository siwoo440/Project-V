using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 시험용 버튼 (에디터에서만 보인다)
// 긴 전투를 끝까지 하지 않고도 승패 뒤의 흐름(보상, 해금, 지역 클리어, 절정 상태)을 확인하려고 둔다. 빌드에서는 숨긴다.
public partial class BattleManager
{
    [Header("시험용 (에디터 전용)")]
    [SerializeField] private GameObject debugPanel;   // 시험 버튼 묶음
    [SerializeField] private Button debugWinButton;   // 즉시 승리 (상대의 HP를 0으로)
    [SerializeField] private Button debugLustButton;  // 성욕 가득 (절정 상태로)
    [SerializeField] private Button debugLoseButton;  // 즉시 패배 (플레이어 HP를 0으로)
    [SerializeField] private Button debugManaButton;  // 이번 턴 마나 가득

    private void StartDebugButtons()
    {
        if (debugPanel == null) { return; }

        debugPanel.SetActive(Application.isEditor);

        if (!Application.isEditor) { return; }

        AddDebugListener(debugWinButton, DebugWin);
        AddDebugListener(debugLustButton, DebugFillLust);
        AddDebugListener(debugLoseButton, DebugLose);
        AddDebugListener(debugManaButton, DebugFillMana);
    }

    private static void AddDebugListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) { return; }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private bool CanUseDebugButton() // 내 턴이고 손패 교환을 마쳤을 때만 쓴다.
    {
        if (isBattleEnded || !isPlayerTurn) { return false; }

        if (isMulliganPhase)
        {
            resultText.text = "[시험] 손패 교환을 먼저 마치세요";
            return false;
        }

        return true;
    }

    private void DebugWin() // 상대의 HP를 0으로 만들어 정상 승리 절차를 탄다.
    {
        if (!CanUseDebugButton()) { return; }

        AddBattleLog(BattleLogCategory.System, "[시험] 즉시 승리");

        if (isEnemyBattle)
        {
            foreach (MonsterUnit enemyUnit in new List<MonsterUnit>(enemyUnits))
            {
                if (enemyUnit != null) { HandleEnemyDefeated(enemyUnit); }
            }

            UpdateBattleUI();
            CheckEnemyBattleVictory();
            return;
        }

        heroineCurrentHp = 0;
        UpdateBattleUI();
        TryEndBattleByHeroineHp();
    }

    // 성욕을 가득 채운다. 바로 이기지 않고 절정 상태가 되므로, 턴을 넘겨 다음 내 턴에 절정 승리하는지 볼 수 있다.
    private void DebugFillLust()
    {
        if (!CanUseDebugButton()) { return; }

        if (isEnemyBattle)
        {
            resultText.text = "[시험] 적 마물 전투에는 성욕이 없습니다";
            return;
        }

        heroineLust = heroineMaxLust;

        AddBattleLog(BattleLogCategory.System, "[시험] 성욕 가득");
        UpdateHeroineClimaxState();
        UpdateBattleUI();

        resultText.text = "[시험] 성욕 가득: 턴을 넘겨 절정 승리를 확인하세요";
    }

    private void DebugLose() // 플레이어 HP를 0으로 만들어 패배 결과를 본다.
    {
        if (!CanUseDebugButton()) { return; }

        AddBattleLog(BattleLogCategory.System, "[시험] 즉시 패배");

        playerCurrentHp = 0;
        UpdateBattleUI();
        EndBattle(BattleOutcome.Defeat);
    }

    private void DebugFillMana() // 이번 턴의 마나를 상한까지 채운다.
    {
        if (!CanUseDebugButton()) { return; }

        currentMana = MaximumManaLimit;

        AddBattleLog(BattleLogCategory.System, "[시험] 마나 가득");
        RefreshHandCardViews();
        UpdateBattleUI();
    }

    // 전투 화면을 떠나도 되는지 확인한다. 지역으로 돌아가는 버튼이 부른다.
    // 진행 중인 전투는 포기 절차(두 번 누르기)를 거쳐야 떠날 수 있고, 끝난 전투의 결과를 아직 받지 않았으면 여기서 받는다.
    public bool PrepareToLeave()
    {
        if (isBattleStarted && !isBattleEnded)
        {
            ForfeitBattle(); // 두 번째로 눌렀으면 포기 처리 뒤 스스로 지역 화면으로 간다.
            return false;
        }

        if (lastBattleResult != null && !lastBattleResult.RewardsApplied)
        {
            ClaimBattleRewards(); // 계속 버튼을 누르지 않고 나가도 보상과 승리 기록이 빠지지 않는다.
        }

        return true;
    }
}
