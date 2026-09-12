using System;
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class StageSelectFlow : MonoBehaviour // 지역 선택 화면 연결
{
    [Serializable]
    public class StageEntry // 스테이지 항목
    {
        [SerializeField] private string stageName = "새 스테이지"; // 스테이지 이름
        [SerializeField] private string stageType = "일반전";    // 콘텐츠 구분
        [SerializeField, Min(1)] private int recommendedLevel = 1; // 권장 레벨
        [SerializeField, TextArea] private string description = ""; // 설명
        [SerializeField] private bool isUnlocked = true; // 해금 여부

        public string StageName => stageName;
        public string StageType => stageType;
        public int RecommendedLevel => recommendedLevel;
        public string Description => description;
        public bool IsUnlocked => isUnlocked;
    }

    [Header("화면 이동")]
    [SerializeField] private Button startBattleButton; // 전투 시작
    [SerializeField] private Button deckBuilderButton; // 덱 편성
    [SerializeField] private Button backButton;        // 돌아가기

    [Header("스테이지 목록")]
    [SerializeField] private Transform stageListContent; // 스테이지 목록 영역

    [Header("스테이지 텍스트")]
    [SerializeField] private TMP_Text stageNameText;        // 스테이지 이름
    [SerializeField] private TMP_Text stageDescriptionText; // 스테이지 설명

    [Header("스테이지 데이터")]
    [SerializeField]
    private List<StageEntry> stages = new List<StageEntry>(); // 스테이지 목록

    private readonly List<GameObject> generatedEntries =
        new List<GameObject>(); // 생성한 항목 목록

    private StageEntry selectedStage; // 선택한 스테이지

    private void Awake()
    {
        startBattleButton =
            SceneUIBinder.Bind(startBattleButton, "StartBattleButton");

        deckBuilderButton =
            SceneUIBinder.Bind(deckBuilderButton, "DeckBuilderButton");

        backButton =
            SceneUIBinder.Bind(backButton, "BackButton");

        stageListContent =
            SceneUIBinder.Bind(stageListContent, "StageListContent");

        stageNameText =
            SceneUIBinder.Bind(stageNameText, "StageNameText");

        stageDescriptionText =
            SceneUIBinder.Bind(stageDescriptionText, "StageDescriptionText");
    }

    private void Start()
    {
        if (startBattleButton != null)
        {
            startBattleButton.onClick.RemoveAllListeners();
            startBattleButton.onClick.AddListener(StartSelectedStage);
        }

        if (deckBuilderButton != null)
        {
            deckBuilderButton.onClick.RemoveAllListeners();
            deckBuilderButton.onClick.AddListener(SceneFlow.LoadDeckBuilder);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(SceneFlow.LoadMainMenu);
        }

        BuildStageList(); // 스테이지 목록 생성
    }

    private void BuildStageList()
    {
        foreach (GameObject generatedEntry in generatedEntries)
        {
            if (generatedEntry == null) { continue; }

            Destroy(generatedEntry);
        }

        generatedEntries.Clear();

        if (stages.Count == 0)
        {
            ShowStageDetail(null); // 데이터 없음 표시
            return;
        }

        foreach (StageEntry stage in stages)
        {
            if (stage == null) { continue; }

            CreateStageButton(stage);
        }

        SelectStage(stages[0]); // 첫 스테이지 선택
    }

    private void CreateStageButton(StageEntry stage)
    {
        if (stageListContent == null) { return; } // 배치 영역 누락 차단

        GameObject entryObject =
            new GameObject($"StageButton_{stage.StageName}", typeof(RectTransform));

        entryObject.transform.SetParent(stageListContent, false);

        Image entryImage = entryObject.AddComponent<Image>();
        entryImage.color = new Color(0.18f, 0.15f, 0.26f, 1f);

        Button entryButton = entryObject.AddComponent<Button>();
        entryButton.targetGraphic = entryImage;
        entryButton.interactable = stage.IsUnlocked;

        LayoutElement entryLayout =
            entryObject.AddComponent<LayoutElement>();

        entryLayout.minHeight = 56f;
        entryLayout.preferredHeight = 56f;

        GameObject labelObject =
            new GameObject("Label", typeof(RectTransform));

        labelObject.transform.SetParent(entryObject.transform, false);

        TextMeshProUGUI entryLabel =
            labelObject.AddComponent<TextMeshProUGUI>();

        entryLabel.text = stage.IsUnlocked
            ? $"{stage.StageName}\n{stage.StageType}   권장 레벨 {stage.RecommendedLevel}"
            : $"{stage.StageName}\n잠김";

        entryLabel.fontSize = 20f;
        entryLabel.color = stage.IsUnlocked
            ? new Color(0.95f, 0.94f, 1f, 1f)
            : new Color(0.5f, 0.5f, 0.55f, 1f);

        entryLabel.alignment = TextAlignmentOptions.Left;
        entryLabel.margin = new Vector4(16f, 0f, 8f, 0f);

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        StageEntry targetStage = stage;

        entryButton.onClick.AddListener(
            () => SelectStage(targetStage)
        );

        generatedEntries.Add(entryObject);
    }

    private void SelectStage(StageEntry stage)
    {
        selectedStage = stage;
        ShowStageDetail(stage);
    }

    private void ShowStageDetail(StageEntry stage)
    {
        bool hasStage = stage != null && stage.IsUnlocked;

        if (stageNameText != null)
        {
            stageNameText.text = stage == null
                ? "스테이지 데이터 없음"
                : stage.StageName;
        }

        if (stageDescriptionText != null)
        {
            if (stage == null)
            {
                stageDescriptionText.text =
                    "스테이지 데이터를 추가하세요.";
            }
            else
            {
                PlayerProgressManager progress =
                    PlayerProgressManager.Instance;

                string deckInfo = progress == null
                    ? "덱: 알 수 없음"
                    : $"덱 {progress.CurrentDeck.Count} / {progress.RequiredDeckSize}";

                stageDescriptionText.text =
                    $"{stage.StageType}   권장 레벨 {stage.RecommendedLevel}\n" +
                    $"{stage.Description}\n{deckInfo}";
            }
        }

        if (startBattleButton != null)
        {
            startBattleButton.interactable = hasStage;
        }
    }

    public void StartSelectedStage()
    {
        if (selectedStage == null)
        {
            Debug.LogWarning("선택한 스테이지가 없습니다."); // 선택 누락 경고
            return;
        }

        if (!selectedStage.IsUnlocked)
        {
            Debug.LogWarning("선택한 스테이지는 잠겨 있습니다."); // 잠금 스테이지 차단
            return;
        }

        SceneFlow.LoadBattle(); // 전투 씬 로드
    }
}
