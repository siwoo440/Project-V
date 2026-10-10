using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 전투 편의 기능: 전투 속도, 전투 포기, 패배 뒤 재도전 (기획서 10.19 / 10.22 / 10.23.1 / 11.11)
public partial class BattleManager
{
    private const float ForfeitConfirmSeconds = 3f; // 이 시간 안에 포기 버튼을 다시 눌러야 포기한다.

    [Header("편의 기능")]
    [SerializeField] private Button speedButton;   // 전투 속도 (1배, 1.5배, 2배)
    [SerializeField] private Button forfeitButton; // 전투 포기

    private float forfeitConfirmUntil; // 이 시각 전에 다시 누르면 포기한다.
    private bool isForfeited;          // 포기로 끝난 전투인지 여부
    private bool isBattleStarted;      // 전투가 정상으로 시작됐는지 여부 (덱 오류로 시작하지 못하면 false)

    private void StartComfortButtons() // 버튼 연결. 전투 씬이 열릴 때 한 번 부른다.
    {
        if (speedButton != null)
        {
            speedButton.onClick.RemoveAllListeners();
            speedButton.onClick.AddListener(CycleBattleSpeed);
        }

        if (forfeitButton != null)
        {
            forfeitButton.onClick.RemoveAllListeners();
            forfeitButton.onClick.AddListener(ForfeitBattle);
        }

        if (battleResultUI != null)
        {
            battleResultUI.SetRetryHandler(RetryBattle);
        }

        RefreshSpeedButton();
    }

    // 누를 때마다 1배 → 1.5배 → 2배로 바꾼다. 행동 사이의 대기와 그림 연출만 빨라지고 판정은 그대로다.
    public void CycleBattleSpeed()
    {
        BattleSpeed.Next();
        RefreshSpeedButton();

        AddBattleLog(BattleLogCategory.System, $"전투 속도: {BattleSpeed.Label}");
    }

    private void RefreshSpeedButton()
    {
        if (speedButton == null) { return; }

        TMP_Text buttonLabel = speedButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel != null) { buttonLabel.text = BattleSpeed.Label; }
    }

    // 전투를 포기한다. 잘못 누르는 일을 막으려고 두 번 눌러야 한다. 패배와 같게 처리하되 보상과 경험치가 없다.
    // 쓴 소모품은 이미 소모됐고, 쓰지 않은 소모품과 전투 전의 재화, 카드는 그대로다. (기획서 10.23.1)
    public void ForfeitBattle()
    {
        if (isBattleEnded) { return; }

        if (!isPlayerTurn)
        {
            resultText.text = "내 턴에만 포기할 수 있습니다";
            return;
        }

        if (Time.unscaledTime > forfeitConfirmUntil)
        {
            forfeitConfirmUntil = Time.unscaledTime + ForfeitConfirmSeconds;
            resultText.text = "한 번 더 누르면 전투를 포기하고 지역으로 돌아갑니다. 보상과 경험치를 받지 못합니다.";
            return;
        }

        isForfeited = true;

        EndBattle(BattleOutcome.Defeat);
        ClaimBattleRewards(); // 받을 것은 없지만 포획전이면 목록을 새로 뽑고 결과를 저장한다.

        SceneFlow.LoadStageSelect(); // 지역 화면으로 돌아간다.
    }

    // 포기한 전투의 결과: 보상, 경험치, 포획 결과가 모두 없다.
    private BattleResultData CreateForfeitResult(BattleOutcome outcome)
    {
        BattleResultData resultData = new BattleResultData(outcome, 0, 0, false, false, null);

        if (isEnemyBattle && BattleSetup.IsCaptureBattle)
        {
            resultData.SetCaptureResult(null); // 포획전이 끝났으므로 목록은 새로 바뀐다.
        }

        return resultData;
    }

    // 패배 결과 화면의 재도전: 패배 경험치를 반영하고 같은 전투를 같은 난이도로 다시 시작한다. (기획서 10.19)
    public void RetryBattle()
    {
        if (lastBattleResult == null || lastBattleResult.IsVictory) { return; }

        ClaimBattleRewards();
        SceneFlow.LoadBattle();
    }
}
