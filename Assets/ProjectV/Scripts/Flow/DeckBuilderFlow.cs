using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class DeckBuilderFlow : MonoBehaviour // 덱 편성 화면 연결
{
    [Header("화면 이동")]
    [SerializeField] private Button backButton;         // 돌아가기
    [SerializeField] private Button enhanceSceneButton; // 마물 강화 화면으로 이동

    [Header("편성 조작")]
    [SerializeField] private Button clearDeckButton; // 덱 비우기
    [SerializeField] private Button fillDeckButton;  // 보유 카드로 채우기

    [Header("덱 프리셋")]
    [SerializeField]
    private List<Button> presetButtons = new List<Button>(); // 프리셋 선택 버튼

    [SerializeField] private TMP_InputField presetNameInput; // 프리셋 이름 입력
    [SerializeField] private Button copyPresetButton;        // 직전 프리셋 복사

    [Header("필터와 정렬")]
    [SerializeField] private Button typeFilterButton;    // 계열 필터
    [SerializeField] private Button rarityFilterButton;  // 희귀도 필터
    [SerializeField] private Button manaFilterButton;    // 마나 필터
    [SerializeField] private Button sortButton;          // 정렬 방식
    [SerializeField] private TMP_InputField searchInput; // 이름 검색

    [Header("카드 목록")]
    [SerializeField] private Transform ownedCardsContent;  // 보유 카드 배치 영역
    [SerializeField] private Transform currentDeckContent; // 현재 덱 배치 영역

    [Header("덱 텍스트")]
    [SerializeField] private TMP_Text deckCountText; // 덱 통계 표시
    [SerializeField] private TMP_Text deckStatsText; // 계열 및 희귀도 분포
    [SerializeField] private TMP_Text messageText;   // 안내 문구

    private readonly CardFilterState filterState =
        new CardFilterState(); // 필터와 정렬 상태

    private readonly List<GameObject> generatedEntries =
        new List<GameObject>(); // 생성한 항목 목록

    private int previousPresetIndex = -1; // 직전 선택 프리셋

    private void Awake()
    {
        backButton =
            SceneUIBinder.Bind(backButton, "BackButton");

        enhanceSceneButton =
            SceneUIBinder.Bind(enhanceSceneButton, "EnhanceSceneButton");

        clearDeckButton =
            SceneUIBinder.Bind(clearDeckButton, "ClearDeckButton");

        fillDeckButton =
            SceneUIBinder.Bind(fillDeckButton, "FillDeckButton");

        ownedCardsContent =
            SceneUIBinder.Bind(ownedCardsContent, "OwnedCardsContent");

        currentDeckContent =
            SceneUIBinder.Bind(currentDeckContent, "CurrentDeckContent");

        deckCountText =
            SceneUIBinder.Bind(deckCountText, "DeckCountText");

        deckStatsText =
            SceneUIBinder.Bind(deckStatsText, "DeckStatsText");

        messageText =
            SceneUIBinder.Bind(messageText, "MessageText");

        presetNameInput =
            SceneUIBinder.Bind(presetNameInput, "PresetNameInput");

        copyPresetButton =
            SceneUIBinder.Bind(copyPresetButton, "CopyPresetButton");

        typeFilterButton =
            SceneUIBinder.Bind(typeFilterButton, "TypeFilterButton");

        rarityFilterButton =
            SceneUIBinder.Bind(rarityFilterButton, "RarityFilterButton");

        manaFilterButton =
            SceneUIBinder.Bind(manaFilterButton, "ManaFilterButton");

        sortButton =
            SceneUIBinder.Bind(sortButton, "SortButton");

        searchInput =
            SceneUIBinder.Bind(searchInput, "SearchInput");

        BindPresetButtons();
    }

    private void BindPresetButtons() // 프리셋 버튼 연결
    {
        presetButtons.RemoveAll(presetButton => presetButton == null);

        if (presetButtons.Count >= PlayerProgressManager.PresetCount) { return; }

        presetButtons.Clear();

        for (int i = 0; i < PlayerProgressManager.PresetCount; i++)
        {
            Button presetButton = SceneUIBinder.Bind<Button>(
                null,
                $"PresetButton{i + 1}"
            );

            if (presetButton == null) { continue; }

            presetButtons.Add(presetButton);
        }
    }

    private void Start()
    {
        AddClickListener(backButton, SceneFlow.ReturnToPreviousScene);
        AddClickListener(enhanceSceneButton, SceneFlow.LoadEnhance);
        AddClickListener(clearDeckButton, ClearDeck);
        AddClickListener(fillDeckButton, FillDeck);
        AddClickListener(copyPresetButton, CopyPreviousPreset);
        AddClickListener(typeFilterButton, CycleTypeFilter);
        AddClickListener(rarityFilterButton, CycleRarityFilter);
        AddClickListener(manaFilterButton, CycleManaFilter);
        AddClickListener(sortButton, CycleSortMode);

        for (int i = 0; i < presetButtons.Count; i++)
        {
            int presetIndex = i;

            AddClickListener(
                presetButtons[i],
                () => SelectPreset(presetIndex)
            );
        }

        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveAllListeners();
            searchInput.onValueChanged.AddListener(OnSearchTextChanged);
        }

        if (presetNameInput != null)
        {
            presetNameInput.onEndEdit.RemoveAllListeners();
            presetNameInput.onEndEdit.AddListener(OnPresetNameChanged);
        }

        Refresh(); // 목록 갱신
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

    // ---------- 프리셋 ----------

    public void SelectPreset(int presetIndex)
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; }

        int currentIndex = progress.SelectedPresetIndex;

        if (progress.SelectPreset(presetIndex))
        {
            previousPresetIndex = currentIndex; // 복사 대상 기록
            ShowMessage($"{progress.GetPresetName(presetIndex)}을 선택했습니다.");
        }

        Refresh();
    }

    public void CopyPreviousPreset()
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; }

        if (previousPresetIndex < 0)
        {
            ShowMessage("복사할 프리셋이 없습니다. 다른 프리셋을 먼저 선택하세요.");
            return;
        }

        bool copied = progress.CopyPresetToSelected(previousPresetIndex);

        ShowMessage(
            copied
                ? $"{progress.GetPresetName(previousPresetIndex)} 구성을 복사했습니다."
                : "복사하지 못했습니다."
        );

        Refresh();
    }

    private void OnPresetNameChanged(string newName)
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; }
        if (string.IsNullOrWhiteSpace(newName)) { return; }

        progress.RenameSelectedPreset(newName);
        ShowMessage("프리셋 이름을 변경했습니다.");
        Refresh();
    }

    // ---------- 필터와 정렬 ----------

    private void CycleTypeFilter()
    {
        filterState.CycleType();
        Refresh();
    }

    private void CycleRarityFilter()
    {
        filterState.CycleRarity();
        Refresh();
    }

    private void CycleManaFilter()
    {
        filterState.CycleMana();
        Refresh();
    }

    private void CycleSortMode()
    {
        filterState.CycleSort();
        Refresh();
    }

    private void OnSearchTextChanged(string newText)
    {
        filterState.SetSearchText(newText);
        Refresh();
    }

    private void UpdateControlLabels(PlayerProgressManager progress)
    {
        for (int i = 0; i < presetButtons.Count; i++)
        {
            Button presetButton = presetButtons[i];

            if (presetButton == null) { continue; }

            SetButtonLabel(presetButton, progress.GetPresetName(i));

            Image presetImage = presetButton.GetComponent<Image>();

            if (presetImage != null)
            {
                bool isSelected = i == progress.SelectedPresetIndex;

                bool hasSkin = UISkin.ApplySelectable(
                    presetImage, isSelected,
                    UIKeys.ButtonBlue, UIKeys.ButtonGold,
                    new Color(0.20f, 0.16f, 0.31f, 1f),
                    new Color(0.42f, 0.34f, 0.16f, 1f)
                ); // 선택 프리셋 강조

                TMP_Text presetLabel =
                    presetButton.GetComponentInChildren<TMP_Text>(true);

                if (hasSkin && presetLabel != null)
                {
                    presetLabel.color = isSelected
                        ? UISkin.ButtonInk
                        : UISkin.Cream; // 금색 버튼 위에는 어두운 글자
                }
            }
        }

        if (presetNameInput != null && !presetNameInput.isFocused)
        {
            presetNameInput.SetTextWithoutNotify(
                progress.GetPresetName(progress.SelectedPresetIndex)
            );
        }

        SetButtonLabel(
            copyPresetButton,
            previousPresetIndex >= 0
                ? $"{progress.GetPresetName(previousPresetIndex)} 복사"
                : "덱 복사"
        );

        SetButtonLabel(typeFilterButton, filterState.TypeLabel);
        SetButtonLabel(rarityFilterButton, filterState.RarityLabel);
        SetButtonLabel(manaFilterButton, filterState.ManaLabel);
        SetButtonLabel(sortButton, filterState.SortLabel);
    }

    private void SetButtonLabel(Button targetButton, string label)
    {
        if (targetButton == null) { return; }

        TMP_Text buttonLabel =
            targetButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel == null) { return; }

        buttonLabel.text = label;
    }

    // 보유 카드를 덱에 1장 추가한다.
    public void AddCardToDeck(CardData cardData)
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; }

        bool added = progress.TryAddCardToDeck(
            cardData,
            out string errorMessage
        );

        ShowMessage(
            added
                ? $"{cardData.CardName}을 덱에 넣었습니다."
                : errorMessage
        );

        Refresh();
    }

    // 덱에서 사본 1장을 제거한다.
    public void RemoveCopyFromDeck(CardCopy targetCopy)
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; }
        if (targetCopy == null) { return; }

        bool removed = progress.RemoveCopyFromDeck(targetCopy);

        ShowMessage(
            removed
                ? $"{targetCopy.DisplayName}을 덱에서 뺐습니다."
                : $"{targetCopy.DisplayName}은 덱에 없습니다."
        );

        Refresh();
    }

    public void ClearDeck()
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; }

        progress.ClearDeck();
        ShowMessage("덱을 비웠습니다.");
        Refresh();
    }

    public void FillDeck()
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; }

        progress.FillDeckFromOwnedCards();
        ShowMessage("보유 카드로 덱을 채웠습니다.");
        Refresh();
    }

    public void Refresh()
    {
        ClearEntries();

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null)
        {
            ShowMessage("진행 데이터가 없습니다");
            return;
        }

        UpdateControlLabels(progress);
        BuildOwnedCardList(progress);
        BuildDeckList(progress);
        UpdateDeckTexts(progress);
    }

    private void BuildOwnedCardList(PlayerProgressManager progress)
    {
        List<OwnedCardData> visibleCards = new List<OwnedCardData>();

        foreach (OwnedCardData ownedCard in progress.OwnedCards)
        {
            if (ownedCard == null) { continue; }
            if (ownedCard.CardData == null) { continue; }
            if (!filterState.Passes(ownedCard.CardData)) { continue; }

            visibleCards.Add(ownedCard);
        }

        filterState.Sort(visibleCards);

        foreach (OwnedCardData ownedCard in visibleCards)
        {
            CardData cardData = ownedCard.CardData;
            int deckCount = progress.GetDeckCardCount(cardData);

            bool canAdd = deckCount < ownedCard.OwnedCount &&
                          deckCount < ownedCard.MaxOwnedCount;

            AddEntry(CardEntryFactory.CreateCardEntry(
                ownedCardsContent,
                cardData,
                ownedCard.HighestEnhanceLevel,
                $"{deckCount} / {ownedCard.OwnedCount}",
                canAdd,
                () => AddCardToDeck(cardData)
            )); // 보유 카드 항목 생성
        }
    }

    // 같은 카드라도 사본마다 강화 단계가 다르므로 사본 단위로 표시한다.
    private void BuildDeckList(PlayerProgressManager progress)
    {
        foreach (CardCopy deckCopy in progress.CurrentDeck)
        {
            if (deckCopy == null) { continue; }
            if (deckCopy.CardData == null) { continue; }

            CardCopy targetCopy = deckCopy;

            AddEntry(CardEntryFactory.CreateCardEntry(
                currentDeckContent,
                deckCopy.CardData,
                deckCopy.EnhanceLevel,
                $"{deckCopy.CopyNumber}번 사본",
                true,
                () => RemoveCopyFromDeck(targetCopy)
            )); // 편성 사본 항목 생성
        }
    }

    private void UpdateDeckTexts(PlayerProgressManager progress)
    {
        if (deckCountText != null)
        {
            deckCountText.text =
                $"{progress.GetPresetName(progress.SelectedPresetIndex)}      " +
                $"덱 {progress.CurrentDeck.Count} / " +
                $"{progress.RequiredDeckSize}      " +
                $"평균 마나 {progress.CurrentDeckAverageMana:0.00}";
        }

        if (deckStatsText == null) { return; }

        Dictionary<MonsterType, int> typeCounts =
            new Dictionary<MonsterType, int>(); // 계열 분포

        Dictionary<CardRarity, int> rarityCounts =
            new Dictionary<CardRarity, int>(); // 희귀도 분포

        foreach (CardCopy deckCopy in progress.CurrentDeck)
        {
            if (deckCopy == null) { continue; }
            if (deckCopy.CardData == null) { continue; }

            CardData deckCard = deckCopy.CardData;

            MonsterType mainType = deckCard.MainType;

            if (mainType != MonsterType.None)
            {
                if (!typeCounts.ContainsKey(mainType))
                {
                    typeCounts.Add(mainType, 0);
                }

                typeCounts[mainType] += 1;
            }

            if (!rarityCounts.ContainsKey(deckCard.Rarity))
            {
                rarityCounts.Add(deckCard.Rarity, 0);
            }

            rarityCounts[deckCard.Rarity] += 1;
        }

        string typeText = string.Empty;

        foreach (KeyValuePair<MonsterType, int> typeCount in typeCounts)
        {
            if (!string.IsNullOrEmpty(typeText)) { typeText += "   "; }

            typeText +=
                $"{MonsterTypeRules.GetDisplayName(typeCount.Key)} {typeCount.Value}";
        }

        string rarityText = string.Empty;

        foreach (KeyValuePair<CardRarity, int> rarityCount in rarityCounts)
        {
            if (!string.IsNullOrEmpty(rarityText)) { rarityText += "   "; }

            rarityText +=
                $"{CardRarityRules.GetDisplayName(rarityCount.Key)} {rarityCount.Value}";
        }

        deckStatsText.text =
            $"계열 분포 (시너지는 필드 기준)\n{(string.IsNullOrEmpty(typeText) ? "없음" : typeText)}\n" +
            $"희귀도 분포\n{(string.IsNullOrEmpty(rarityText) ? "없음" : rarityText)}";
    }

    private void ShowMessage(string message)
    {
        if (messageText == null) { return; }

        messageText.text = message;
    }

    private void AddEntry(GameObject entryObject)
    {
        if (entryObject == null) { return; }

        generatedEntries.Add(entryObject);
    }

    private void ClearEntries()
    {
        foreach (GameObject generatedEntry in generatedEntries)
        {
            if (generatedEntry == null) { continue; }

            Destroy(generatedEntry);
        }

        generatedEntries.Clear();
    }
}
