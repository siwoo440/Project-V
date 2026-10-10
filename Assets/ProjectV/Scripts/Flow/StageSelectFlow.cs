using System;
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 지역 화면 연결: 월드맵에서 고른 지역의 스테이지 목록을 보여 준다.
public partial class StageSelectFlow : MonoBehaviour
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

        [NonSerialized] private string lockHint = string.Empty; // 잠긴 줄에 적는 해금 조건

        public string LockHint => lockHint ?? string.Empty;

        public StageEntry() { } // 씬에 저장된 항목용

        public StageEntry( // 실행 중에 지역의 데이터로 만드는 항목용
            string name,
            string type,
            int level,
            string text,
            bool unlocked,
            string hint = ""
        )
        {
            stageName = name;
            stageType = type;
            recommendedLevel = Mathf.Max(1, level);
            description = text;
            isUnlocked = unlocked;
            lockHint = hint ?? string.Empty;
        }
    }

    [Header("화면 이동")]
    [SerializeField] private Button startBattleButton; // 전투 시작
    [SerializeField] private Button deckBuilderButton; // 덱 편성
    [SerializeField] private Button itemSlotButton;    // 전투에 가져갈 소모성 아이템 선택
    [SerializeField] private Button backButton;        // 월드맵으로 돌아가기

    [Header("지역 표시")]
    [SerializeField] private TMP_Text titleText;    // 화면 제목 (지역 이름)
    [SerializeField] private Image backgroundImage; // 화면 배경 (지역 배경 그림이 있으면 바꾼다)

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

        itemSlotButton =
            SceneUIBinder.Bind(itemSlotButton, "ItemSlotButton");

        titleText =
            SceneUIBinder.Bind(titleText, "TitleText");

        backgroundImage =
            SceneUIBinder.Bind(backgroundImage, "Background");

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
            backButton.onClick.AddListener(SceneFlow.LoadWorldMap);
        }

        if (itemSlotButton != null)
        {
            itemSlotButton.onClick.RemoveAllListeners();
            itemSlotButton.onClick.AddListener(CycleBattleItem);
        }

        ShowRegion(); // 들어온 지역의 이름과 배경 표시
        StartTabButtons(); // 메인 진행과 서브 콘텐츠 탭 연결
        StartDifficultyButtons(); // 난이도 버튼 연결
        StartRerollButton(); // 포획 목록 다시 뽑기 버튼 연결
        RefreshItemSlot(); // 장착한 소모성 아이템 표시

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

        List<StageEntry> shownStages = GetShownStages(); // 이 지역에서 고를 수 있는 스테이지

        RefreshTabButtons(); // 고른 탭 표시

        if (shownStages.Count == 0)
        {
            ShowStageDetail(null); // 데이터 없음 표시
            return;
        }

        foreach (StageEntry stage in shownStages)
        {
            if (stage == null) { continue; }

            CreateStageButton(stage);
        }

        SelectStage(GetDefaultStage(shownStages)); // 다음에 진행할 스테이지 선택
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

        entryLayout.minHeight = 68f; // 탭 하나에 여섯 줄이 들어가는 높이
        entryLayout.preferredHeight = 68f;

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
            ? $"{GetStageTitle(stage)}\n{GetStageSubtitle(stage)}"
            : string.IsNullOrEmpty(stage.LockHint)
                ? $"{GetStageTitle(stage)}\n잠김"
                : $"{GetStageTitle(stage)}\n잠김: {stage.LockHint}";

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

        CreateClearMark(entryObject.transform, stage); // 이긴 스테이지의 왕관

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
                : GetStageTitle(stage);
        }

        if (stageDescriptionText != null)
        {
            stageDescriptionText.text = GetStageDetailText(stage);
        }

        if (startBattleButton != null)
        {
            startBattleButton.interactable = hasStage;
        }

        RefreshDifficultyButtons(); // 일반전과 히로인전을 골랐을 때만 난이도 버튼을 보여 준다.
        RefreshRerollButton(); // 포획 목록을 골랐을 때만 다시 뽑기 버튼을 보여 준다.
        RefreshHeroinePortrait(); // 히로인전을 골랐을 때만 얼굴 그림을 보여 준다.
    }

    // 상세 칸에 적는 글. 잠긴 스테이지는 해금 조건만 적는다.
    private string GetStageDetailText(StageEntry stage)
    {
        if (stage == null) { return "스테이지 데이터를 추가하세요."; }

        if (!stage.IsUnlocked)
        {
            return string.IsNullOrEmpty(stage.LockHint)
                ? "아직 열리지 않은 스테이지입니다."
                : $"아직 열리지 않은 스테이지입니다.\n{stage.LockHint}";
        }

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        string deckInfo = progress == null
            ? "덱: 알 수 없음"
            : $"덱 {progress.CurrentDeck.Count} / {progress.RequiredDeckSize}";

        if (stageFormations.TryGetValue(stage, out EnemyFormationData formation))
        {
            return GetNormalStageDetail(stage, formation) + deckInfo; // 일반전: 적, 난이도, 보상, 승리 기록
        }

        if (stageHeroines.TryGetValue(stage, out HeroineBattleData battle))
        {
            return GetHeroineStageDetail(stage, battle) + deckInfo; // 히로인전: 능력치, 행동, 보상, 승리 기록
        }

        if (captureSlots.ContainsKey(stage))
        {
            return GetCaptureStageDetail(stage) + deckInfo; // 포획전: 마물, 보유 수량, 보상
        }

        return $"{GetStageSubtitle(stage)}\n{stage.Description}\n{deckInfo}";
    }

    // 이긴 일반전과 히로인전 줄의 오른쪽 끝에 왕관을 단다. 포획전은 반복 콘텐츠라 달지 않는다.
    private void CreateClearMark(Transform row, StageEntry stage)
    {
        if (!stage.IsUnlocked || captureSlots.ContainsKey(stage)) { return; }
        if (!IsStageEntryCleared(stage)) { return; }

        Sprite crownSprite = UISkin.Get(UIKeys.StageCrown);

        if (crownSprite == null) { return; } // 그림이 없으면 줄의 클리어 글자로만 알린다.

        Image crownImage = CardEntryFactory.CreateImage(
            row, "ClearMark", Color.white,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-78f, -22f), new Vector2(-34f, 22f)
        );

        crownImage.sprite = crownSprite;
        crownImage.preserveAspect = true;
        crownImage.raycastTarget = false;
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

        PrepareBattleSetup(selectedStage); // 전투 종류와 상대를 정한다.
        SceneFlow.LoadBattle(); // 전투 씬 로드
    }
}
