using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 월드맵 화면 연결 (기획서 4.3 / 8.3 / 11.6)
// 지도 위의 지역을 고르고 들어간다. 덱 편성, 강화, 상점, 저장 같은 성장 메뉴도 여기에서 들어간다.
// 화면의 패널과 버튼은 씬 구성 도구(SceneUIBuilderWorldMap.cs)가 만들고, 지역 표시는 실행 중에 지역 데이터로 만든다.
public partial class WorldMapFlow : MonoBehaviour
{
    [Header("상단 메뉴")]
    [SerializeField] private Button deckButton;     // 덱 편성
    [SerializeField] private Button enhanceButton;  // 마물 강화
    [SerializeField] private Button summonerButton; // 소환사 스킬과 패시브
    [SerializeField] private Button grimoireButton; // 그리모어 영구 강화
    [SerializeField] private Button shopButton;     // 상점
    [SerializeField] private Button saveButton;     // 저장과 불러오기
    [SerializeField] private Button titleButton;    // 메인 메뉴로

    [Header("지도")]
    [SerializeField] private RectTransform mapArea; // 지역 표시를 놓는 영역 (지도 그림과 같은 크기)

    [Header("지역 정보")]
    [SerializeField] private Image regionIconImage;    // 고른 지역의 문양
    [SerializeField] private TMP_Text regionNameText;  // 고른 지역의 이름
    [SerializeField] private TMP_Text regionInfoText;  // 챕터, 히로인, 상태, 전투 주제
    [SerializeField] private Button enterButton;       // 지역 진입

    [Header("화면 텍스트")]
    [SerializeField] private TMP_Text resourceText; // 레벨과 보유 재화
    [SerializeField] private TMP_Text messageText;  // 안내 문구

    private readonly List<GameObject> generatedNodes =
        new List<GameObject>(); // 생성한 지역 표시

    private RegionData selectedRegion; // 고른 지역

    private void Awake()
    {
        deckButton = SceneUIBinder.Bind(deckButton, "DeckBuilderButton");
        enhanceButton = SceneUIBinder.Bind(enhanceButton, "EnhanceButton");
        summonerButton = SceneUIBinder.Bind(summonerButton, "SummonerButton");
        grimoireButton = SceneUIBinder.Bind(grimoireButton, "GrimoireButton");
        shopButton = SceneUIBinder.Bind(shopButton, "ShopButton");
        saveButton = SceneUIBinder.Bind(saveButton, "SaveButton");
        titleButton = SceneUIBinder.Bind(titleButton, "TitleButton");
        mapArea = SceneUIBinder.Bind(mapArea, "MapArea");

        regionIconImage =
            SceneUIBinder.Bind(regionIconImage, "RegionIconImage");

        regionNameText =
            SceneUIBinder.Bind(regionNameText, "RegionNameText");

        regionInfoText =
            SceneUIBinder.Bind(regionInfoText, "RegionInfoText");

        enterButton = SceneUIBinder.Bind(enterButton, "EnterButton");
        resourceText = SceneUIBinder.Bind(resourceText, "ResourceText");
        messageText = SceneUIBinder.Bind(messageText, "MessageText");
    }

    private void Start()
    {
        AddClickListener(deckButton, SceneFlow.LoadDeckBuilder);
        AddClickListener(enhanceButton, SceneFlow.LoadEnhance);
        AddClickListener(summonerButton, SceneFlow.LoadSummoner);
        AddClickListener(grimoireButton, SceneFlow.LoadGrimoire);
        AddClickListener(shopButton, SceneFlow.LoadShop);
        AddClickListener(saveButton, () => SceneFlow.LoadSaveScreen(true)); // 월드맵에서는 저장도 할 수 있다. (기획서 15.5)
        AddClickListener(titleButton, SceneFlow.LoadMainMenu);
        AddClickListener(enterButton, OnEnterButton);

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress != null)
        {
            RegionData unlockedRegion = progress.TakeNewlyUnlockedRegion();

            // 방금 열린 지역, 마지막으로 들어간 지역, 첫 지역 순으로 고른다.
            selectedRegion = unlockedRegion != null
                ? unlockedRegion
                : progress.CurrentRegion != null ? progress.CurrentRegion : progress.GetFirstRegion();

            ShowMessage(unlockedRegion != null
                ? $"새 지역이 열렸습니다: {unlockedRegion.DisplayName}"
                : "지역을 고르고 들어가세요.");
        }

        Refresh();
    }

    private void AddClickListener(
        Button targetButton,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        if (targetButton == null) { return; }

        targetButton.onClick.RemoveAllListeners();
        targetButton.onClick.AddListener(clickAction);
    }

    private void SelectRegion(RegionData region) // 지역 선택
    {
        selectedRegion = region;
        Refresh();
    }

    private void OnEnterButton() // 고른 지역으로 들어간다. 잠긴 지역은 이유를 안내한다.
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        if (!progress.TryEnterRegion(selectedRegion, out string message))
        {
            ShowMessage(message);
            Refresh();
            return;
        }

        SceneFlow.LoadStageSelect(); // 지역 화면으로 이동
    }

    public void Refresh() // 화면 갱신
    {
        ClearNodes();

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null)
        {
            ShowMessage("진행 데이터가 없습니다");
            return;
        }

        if (resourceText != null)
        {
            resourceText.text =
                $"{progress.PlayerLevelText}    " +
                $"{UISkin.IconOr(UIIcons.Gold, "골드")} {progress.Gold}    " +
                $"{UISkin.IconOr(UIIcons.Essence, "정수")} {progress.MonsterEssence}    " +
                $"{UISkin.IconOr(UIIcons.Shard, "파편")} {progress.DesireShards}";
        }

        BuildNodes(progress);
        UpdateDetail(progress);
    }

    private void ShowMessage(string message)
    {
        if (messageText == null) { return; }

        messageText.text = message;
    }

    private void ClearNodes()
    {
        foreach (GameObject generatedNode in generatedNodes)
        {
            if (generatedNode == null) { continue; }

            Destroy(generatedNode);
        }

        generatedNodes.Clear();
    }
}
