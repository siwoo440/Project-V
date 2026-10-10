using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 전투 준비 화면 (기획서 6.14 / 10.18)
// 지역 화면에서 고른 전투의 상대와 보상을 보여 주고, 덱과 스킬, 패시브, 소모품, 난이도를 바꾼 뒤 전투를 시작한다.
public partial class BattlePrepareFlow : MonoBehaviour
{
    [Header("화면 이동")]
    [SerializeField] private Button startButton; // 전투 시작
    [SerializeField] private Button backButton;  // 지역 화면으로 돌아가기

    [Header("전투 정보")]
    [SerializeField] private TMP_Text stageText;         // 지역과 스테이지 이름, 전투 종류
    [SerializeField] private TMP_Text opponentTitleText; // 상대 이름
    [SerializeField] private TMP_Text opponentText;      // 상대의 능력치와 행동
    [SerializeField] private GameObject portraitFrame;   // 히로인 그림 칸
    [SerializeField] private Image portraitImage;        // 히로인 그림

    [Header("안내")]
    [SerializeField] private TMP_Text messageText; // 바꾼 결과와 시작할 수 없는 이유

    private string lastMessage = string.Empty; // 방금 바꾼 결과 안내

    private void Start()
    {
        AddListener(startButton, StartBattle);
        AddListener(backButton, SceneFlow.LoadStageSelect);

        StartDifficultyButtons();
        StartLoadoutButtons();

        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.ProgressChanged += RefreshAll; // 덱, 장착이 바뀌면 다시 그린다.
        }

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.ProgressChanged -= RefreshAll;
        }
    }

    private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) { return; }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void RefreshAll()
    {
        RefreshOpponent();
        RefreshDifficultyButtons();
        RefreshReward();
        RefreshLoadout();
        RefreshStartButton();
    }

    private void RefreshOpponent() // 상대 칸: 이름, 능력치, 행동, 그림
    {
        bool hasBattle = BattleSetup.HasBattle;

        SetText(
            stageText,
            hasBattle
                ? $"{BattleSetup.StageTitle}  [{BattleBriefing.GetKindName()}]"
                : "고른 전투가 없습니다"
        );

        SetText(opponentTitleText, hasBattle ? BattleBriefing.GetOpponentTitle() : string.Empty);

        SetText(
            opponentText,
            hasBattle
                ? BattleBriefing.GetOpponentText()
                : "지역 화면에서 스테이지를 고른 뒤 들어오세요."
        );

        RefreshPortrait();
    }

    private void RefreshPortrait() // 그림이 있는 히로인이면 보여 준다.
    {
        if (portraitFrame == null || portraitImage == null) { return; }

        HeroineBattleData heroine =
            BattleSetup.HasBattle && !BattleSetup.IsEnemyBattle ? BattleSetup.Heroine : null;

        Sprite portrait = heroine == null
            ? null
            : HeroineArtLibrary.Get(heroine.ArtKey, HeroineArtState.Normal);

        portraitFrame.SetActive(portrait != null);

        if (portrait == null) { return; }

        portraitImage.sprite = portrait;
        portraitImage.preserveAspect = true;
        portraitImage.enabled = true;
    }

    // 덱 장수가 맞아야 전투를 시작할 수 있다. 맞지 않으면 이유를 안내한다.
    private void RefreshStartButton()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        string blockReason = string.Empty;

        if (!BattleSetup.HasBattle)
        {
            blockReason = "지역 화면에서 스테이지를 먼저 고르세요.";
        }
        else if (progress != null && progress.CurrentDeck.Count != progress.RequiredDeckSize)
        {
            blockReason =
                $"덱이 {progress.CurrentDeck.Count}장입니다. {progress.RequiredDeckSize}장을 채워야 시작할 수 있습니다.";
        }

        if (startButton != null)
        {
            startButton.interactable = blockReason.Length == 0;
        }

        SetText(messageText, blockReason.Length > 0 ? blockReason : lastMessage);
    }

    private void ShowMessage(string message) // 바꾼 결과를 안내 줄에 적는다.
    {
        lastMessage = message ?? string.Empty;
        RefreshStartButton();
    }

    private void StartBattle()
    {
        if (!BattleSetup.HasBattle) { return; }

        StorySceneData beforeStory = GetUnseenBeforeStory();

        if (beforeStory != null)
        {
            SceneFlow.LoadStory(beforeStory, SceneNames.Battle); // 전투 전 장면을 보여 준 뒤 전투로 간다.
            return;
        }

        SceneFlow.LoadBattle(); // 전투 시작 직전에 자동 저장한다.
    }

    // 히로인전에 처음 도전할 때 보여 줄 전투 전 장면. 한 번 본 뒤에는 재도전해도 나오지 않는다. (기획서 F.3.1)
    private static StorySceneData GetUnseenBeforeStory()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;
        HeroineBattleData battle = BattleSetup.IsEnemyBattle ? null : BattleSetup.Heroine;

        if (progress == null || battle == null || battle.BeforeStory == null) { return null; }

        return progress.IsStorySeen(battle.BeforeStory) ? null : battle.BeforeStory;
    }

    private static void SetText(TMP_Text target, string content)
    {
        if (target != null) { target.text = content; }
    }
}
