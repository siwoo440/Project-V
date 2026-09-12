using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.Events; // 버튼 이벤트 기능
using UnityEngine.UI; // Unity UI 기능

public class MainMenuFlow : MonoBehaviour // 메인 메뉴 연결
{
    [Header("메뉴 버튼")]
    [SerializeField] private Button storyButton;       // 스토리 진행
    [SerializeField] private Button stageSelectButton; // 지역 선택
    [SerializeField] private Button deckBuilderButton; // 덱 편성
    [SerializeField] private Button quitButton;        // 게임 종료

    [Header("메뉴 텍스트")]
    [SerializeField] private TMP_Text progressText; // 진행 정보 표시

    private void Awake()
    {
        storyButton =
            SceneUIBinder.Bind(storyButton, "StoryButton");

        stageSelectButton =
            SceneUIBinder.Bind(stageSelectButton, "StageSelectButton");

        deckBuilderButton =
            SceneUIBinder.Bind(deckBuilderButton, "DeckBuilderButton");

        quitButton =
            SceneUIBinder.Bind(quitButton, "QuitButton");

        progressText =
            SceneUIBinder.Bind(progressText, "ProgressText");
    }

    private void Start()
    {
        AddListener(storyButton, SceneFlow.LoadStory);
        AddListener(stageSelectButton, SceneFlow.LoadStageSelect);
        AddListener(deckBuilderButton, SceneFlow.LoadDeckBuilder);
        AddListener(quitButton, SceneFlow.QuitGame);

        RefreshProgressText(); // 진행 정보 갱신
    }

    private void AddListener(
        Button targetButton,
        UnityAction buttonAction
    )
    {
        if (targetButton == null) { return; } // 빈 버튼 차단

        targetButton.onClick.RemoveAllListeners();
        targetButton.onClick.AddListener(buttonAction);
    }

    private void RefreshProgressText()
    {
        if (progressText == null) { return; } // 빈 텍스트 차단

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null)
        {
            progressText.text = "진행 데이터가 없습니다";
            return;
        }

        progressText.text =
            $"골드 {progress.Gold}    " +
            $"정수 {progress.MonsterEssence}    " +
            $"보유 카드 {progress.TotalOwnedCardCount}    " +
            $"덱 {progress.CurrentDeck.Count}"; // 진행 요약 표시
    }
}
