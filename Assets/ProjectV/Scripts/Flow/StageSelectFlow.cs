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
    [SerializeField] private Button summonerButton;    // 소환사 스킬과 패시브
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

    private readonly Dictionary<StageEntry, Image> stageImages =
        new Dictionary<StageEntry, Image>(); // 스테이지별 줄 이미지 (선택 표시용)

    private static readonly Color StageRowColor = new Color(0.18f, 0.15f, 0.26f, 1f);         // 줄 이미지가 없을 때 기본 색
    private static readonly Color StageRowSelectedColor = new Color(0.42f, 0.34f, 0.16f, 1f); // 줄 이미지가 없을 때 선택 색

    private StageEntry selectedStage; // 선택한 스테이지

    private void Awake()
    {
        startBattleButton =
            SceneUIBinder.Bind(startBattleButton, "StartBattleButton");

        deckBuilderButton =
            SceneUIBinder.Bind(deckBuilderButton, "DeckBuilderButton");

        backButton =
            SceneUIBinder.Bind(backButton, "BackButton");

        summonerButton =
            SceneUIBinder.Bind(summonerButton, "SummonerButton");

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

        if (summonerButton != null)
        {
            summonerButton.onClick.RemoveAllListeners();
            summonerButton.onClick.AddListener(SceneFlow.LoadSummoner);
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
        stageImages.Clear();

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
        entryImage.color = StageRowColor;
        stageImages[stage] = entryImage;

        Button entryButton = entryObject.AddComponent<Button>();
        entryButton.targetGraphic = entryImage;
        entryButton.interactable = stage.IsUnlocked;

        LayoutElement entryLayout =
            entryObject.AddComponent<LayoutElement>();

        entryLayout.minHeight = 76f;
        entryLayout.preferredHeight = 76f;

        // 전투 종류 아이콘. 잠긴 스테이지는 자물쇠를 보여준다.
        Sprite iconSprite = UISkin.Get(
            stage.IsUnlocked ? UISkin.StageIconKey(stage.StageType) : UIKeys.IconLock
        );

        if (iconSprite != null)
        {
            Image iconImage = CardEntryFactory.CreateImage(
                entryObject.transform, "StageIcon", Color.white,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(34f, -25f), new Vector2(84f, 25f)
            );

            iconImage.sprite = iconSprite;
            iconImage.preserveAspect = true;
        }

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
        entryLabel.raycastTarget = false;
        entryLabel.margin = new Vector4(iconSprite != null ? 100f : 36f, 0f, 36f, 0f);

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

        foreach (KeyValuePair<StageEntry, Image> stageImage in stageImages)
        {
            if (stageImage.Value == null) { continue; }

            UISkin.ApplySelectable(
                stageImage.Value, stageImage.Key == stage,
                UIKeys.RowNormal, UIKeys.RowSelected,
                StageRowColor, StageRowSelectedColor
            ); // 선택한 스테이지는 밝은 줄로 표시
        }
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
