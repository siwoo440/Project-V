using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.Events; // 버튼 이벤트 기능
using UnityEngine.UI; // Unity UI 기능

// 메인 메뉴 연결
// 덱 편성, 강화, 상점 같은 성장 메뉴는 월드맵으로 옮겼다. (기획서 4.3.1: 월드맵이 중심 화면)
public partial class MainMenuFlow : MonoBehaviour
{
    [Header("메뉴 버튼")]
    [SerializeField] private Button worldMapButton; // 월드맵 (게임을 시작한 뒤에 열린다)
    [SerializeField] private Button storyButton;    // 스토리 진행
    [SerializeField] private Button quitButton;     // 게임 종료

    [Header("저장 메뉴")]
    [SerializeField] private Button continueButton; // 이어하기
    [SerializeField] private Button newGameButton;  // 새 게임
    [SerializeField] private Button loadButton;     // 불러오기
    [SerializeField] private TMP_Text saveInfoText; // 최근 저장 정보와 안내

    [Header("메뉴 텍스트")]
    [SerializeField] private TMP_Text progressText; // 진행 정보 표시

    private void Awake()
    {
        worldMapButton =
            SceneUIBinder.Bind(worldMapButton, "WorldMapButton");

        storyButton =
            SceneUIBinder.Bind(storyButton, "StoryButton");

        quitButton =
            SceneUIBinder.Bind(quitButton, "QuitButton");

        progressText =
            SceneUIBinder.Bind(progressText, "ProgressText");

        continueButton =
            SceneUIBinder.Bind(continueButton, "ContinueButton");

        newGameButton =
            SceneUIBinder.Bind(newGameButton, "NewGameButton");

        loadButton =
            SceneUIBinder.Bind(loadButton, "LoadButton");

        saveInfoText =
            SceneUIBinder.Bind(saveInfoText, "SaveInfoText");
    }

    private void Start()
    {
        AddListener(worldMapButton, SceneFlow.LoadWorldMap);
        AddListener(storyButton, SceneFlow.LoadStory);
        AddListener(quitButton, SceneFlow.QuitGame);

        StartSaveMenu(); // 이어하기, 새 게임, 불러오기 연결
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

        if (!progress.IsSessionActive)
        {
            progressText.text = "이어하기 또는 새 게임을 선택하세요"; // 시작 전의 값은 보여주지 않는다.
            return;
        }

        progressText.text =
            $"{progress.PlayerLevelText}    " +
            $"{UISkin.IconOr(UIIcons.Gold, "골드")} {progress.Gold}    " +
            $"{UISkin.IconOr(UIIcons.Essence, "정수")} {progress.MonsterEssence}    " +
            $"{UISkin.IconOr(UIIcons.Shard, "파편")} {progress.DesireShards}    " +
            $"클리어 지역 {progress.ClearedRegionCount} / {progress.Regions.Count}    " +
            $"보유 카드 {progress.TotalOwnedCardCount}"; // 진행 요약 표시
    }
}
