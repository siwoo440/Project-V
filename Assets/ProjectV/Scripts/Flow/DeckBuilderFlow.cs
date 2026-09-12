using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class DeckBuilderFlow : MonoBehaviour // 덱 편성 화면 연결
{
    [Header("화면 이동")]
    [SerializeField] private Button backButton; // 돌아가기

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

    private static readonly MonsterType[] TypeFilters =
    {
        MonsterType.None, MonsterType.Tentacle, MonsterType.Slime,
        MonsterType.Goblin, MonsterType.Demon, MonsterType.Undead,
        MonsterType.Beast, MonsterType.Spirit, MonsterType.Machine,
        MonsterType.Angel,
    }; // 0번은 전체

    private static readonly CardRarity[] RarityFilters =
    {
        CardRarity.Common, CardRarity.Rare,
        CardRarity.Special, CardRarity.Legendary,
    };

    private static readonly string[] ManaFilterNames =
    {
        "전체", "0~2", "3~5", "6 이상",
    };

    private static readonly CardSortMode[] SortModes =
    {
        CardSortMode.Name, CardSortMode.ManaAsc, CardSortMode.ManaDesc,
        CardSortMode.Rarity, CardSortMode.MonsterType,
    };

    private readonly List<GameObject> generatedEntries =
        new List<GameObject>(); // 생성한 항목 목록

    private int typeFilterIndex;   // 0이면 전체
    private int rarityFilterIndex; // 0이면 전체
    private int manaFilterIndex;   // 0이면 전체
    private int sortModeIndex;     // 정렬 방식 번호
    private string searchText = string.Empty; // 이름 검색어
    private int previousPresetIndex = -1;     // 직전 선택 프리셋

    private void Awake()
    {
        backButton =
            SceneUIBinder.Bind(backButton, "BackButton");

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
        typeFilterIndex = (typeFilterIndex + 1) % TypeFilters.Length;
        Refresh();
    }

    private void CycleRarityFilter()
    {
        rarityFilterIndex = (rarityFilterIndex + 1) % (RarityFilters.Length + 1);
        Refresh();
    }

    private void CycleManaFilter()
    {
        manaFilterIndex = (manaFilterIndex + 1) % ManaFilterNames.Length;
        Refresh();
    }

    private void CycleSortMode()
    {
        sortModeIndex = (sortModeIndex + 1) % SortModes.Length;
        Refresh();
    }

    private void OnSearchTextChanged(string newText)
    {
        searchText = newText == null ? string.Empty : newText.Trim();
        Refresh();
    }

    private bool PassesFilter(CardData cardData)
    {
        if (cardData == null) { return false; }

        if (typeFilterIndex > 0)
        {
            MonsterType filterType = TypeFilters[typeFilterIndex];

            if (cardData.SummonMonster == null) { return false; }
            if (!cardData.SummonMonster.HasType(filterType)) { return false; }
        }

        if (rarityFilterIndex > 0)
        {
            CardRarity filterRarity = RarityFilters[rarityFilterIndex - 1];

            if (cardData.Rarity != filterRarity) { return false; }
        }

        if (manaFilterIndex > 0)
        {
            int manaCost = cardData.ManaCost;

            bool inRange =
                (manaFilterIndex == 1 && manaCost <= 2) ||
                (manaFilterIndex == 2 && manaCost >= 3 && manaCost <= 5) ||
                (manaFilterIndex == 3 && manaCost >= 6);

            if (!inRange) { return false; }
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            if (cardData.CardName == null) { return false; }
            if (!cardData.CardName.Contains(searchText)) { return false; }
        }

        return true;
    }

    private void SortOwnedCards(List<OwnedCardData> ownedCardList)
    {
        CardSortMode sortMode = SortModes[sortModeIndex];

        ownedCardList.Sort((left, right) =>
        {
            CardData leftCard = left.CardData;
            CardData rightCard = right.CardData;

            switch (sortMode)
            {
                case CardSortMode.ManaAsc:
                    return leftCard.ManaCost.CompareTo(rightCard.ManaCost);

                case CardSortMode.ManaDesc:
                    return rightCard.ManaCost.CompareTo(leftCard.ManaCost);

                case CardSortMode.Rarity:
                    return rightCard.Rarity.CompareTo(leftCard.Rarity);

                case CardSortMode.MonsterType:
                    return leftCard.MainType.CompareTo(rightCard.MainType);

                default:
                    return string.Compare(
                        leftCard.CardName,
                        rightCard.CardName,
                        System.StringComparison.Ordinal
                    );
            }
        });
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
                presetImage.color = i == progress.SelectedPresetIndex
                    ? new Color(0.42f, 0.34f, 0.16f, 1f)
                    : new Color(0.20f, 0.16f, 0.31f, 1f); // 선택 프리셋 강조
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

        SetButtonLabel(
            typeFilterButton,
            typeFilterIndex == 0
                ? "계열: 전체"
                : $"계열: {MonsterTypeRules.GetDisplayName(TypeFilters[typeFilterIndex])}"
        );

        SetButtonLabel(
            rarityFilterButton,
            rarityFilterIndex == 0
                ? "희귀도: 전체"
                : $"희귀도: {CardRarityRules.GetDisplayName(RarityFilters[rarityFilterIndex - 1])}"
        );

        SetButtonLabel(
            manaFilterButton,
            $"마나: {ManaFilterNames[manaFilterIndex]}"
        );

        SetButtonLabel(
            sortButton,
            $"정렬: {GetSortModeName(SortModes[sortModeIndex])}"
        );
    }

    private string GetSortModeName(CardSortMode sortMode)
    {
        switch (sortMode)
        {
            case CardSortMode.ManaAsc:
                return "마나 낮은 순";

            case CardSortMode.ManaDesc:
                return "마나 높은 순";

            case CardSortMode.Rarity:
                return "희귀도순";

            case CardSortMode.MonsterType:
                return "계열순";

            default:
                return "이름순";
        }
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

    // 덱에서 카드를 1장 제거한다.
    public void RemoveCardFromDeck(CardData cardData)
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; }

        bool removed = progress.RemoveCardFromDeck(cardData);

        ShowMessage(
            removed
                ? $"{cardData.CardName}을 덱에서 뺐습니다."
                : $"{cardData.CardName}은 덱에 없습니다."
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
            if (!PassesFilter(ownedCard.CardData)) { continue; }

            visibleCards.Add(ownedCard);
        }

        SortOwnedCards(visibleCards);

        foreach (OwnedCardData ownedCard in visibleCards)
        {
            CardData cardData = ownedCard.CardData;
            int deckCount = progress.GetDeckCardCount(cardData);

            bool canAdd = deckCount < ownedCard.OwnedCount &&
                          deckCount < ownedCard.MaxOwnedCount;

            CreateCardEntry(
                ownedCardsContent,
                cardData,
                $"{deckCount} / {ownedCard.OwnedCount}",
                canAdd,
                () => AddCardToDeck(cardData)
            ); // 보유 카드 항목 생성
        }
    }

    private void BuildDeckList(PlayerProgressManager progress)
    {
        Dictionary<string, int> deckCounts =
            new Dictionary<string, int>(); // 카드별 편성 수량

        List<CardData> deckOrder = new List<CardData>(); // 표시 순서 유지

        foreach (CardData deckCard in progress.CurrentDeck)
        {
            if (deckCard == null) { continue; }

            if (!deckCounts.ContainsKey(deckCard.CardId))
            {
                deckCounts.Add(deckCard.CardId, 0);
                deckOrder.Add(deckCard);
            }

            deckCounts[deckCard.CardId] += 1;
        }

        foreach (CardData deckCard in deckOrder)
        {
            CardData targetCard = deckCard;

            CreateCardEntry(
                currentDeckContent,
                deckCard,
                $"×{deckCounts[deckCard.CardId]}",
                true,
                () => RemoveCardFromDeck(targetCard)
            ); // 편성 카드 항목 생성
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

        foreach (CardData deckCard in progress.CurrentDeck)
        {
            if (deckCard == null) { continue; }

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

    // 세로형 카드 항목을 만든다. 크기는 그리드 배치가 결정한다.
    private void CreateCardEntry(
        Transform parentContent,
        CardData cardData,
        string countText,
        bool isInteractable,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        if (parentContent == null) { return; } // 배치 영역 누락 차단
        if (cardData == null) { return; } // 빈 카드 차단

        Color rarityColor = CardRarityRules.GetDisplayColor(cardData.Rarity);
        Color typeColor = MonsterTypeRules.GetDisplayColor(cardData.MainType);

        GameObject entryObject =
            new GameObject("CardEntry", typeof(RectTransform));

        entryObject.transform.SetParent(parentContent, false);

        Image cardBackground = entryObject.AddComponent<Image>();

        cardBackground.color = isInteractable
            ? new Color(0.17f, 0.14f, 0.26f, 1f)
            : new Color(0.11f, 0.10f, 0.14f, 1f);

        Button entryButton = entryObject.AddComponent<Button>();
        entryButton.targetGraphic = cardBackground;
        entryButton.interactable = isInteractable;

        ColorBlock colors = entryButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.94f, 0.78f, 1f);
        colors.pressedColor = new Color(0.78f, 0.72f, 0.62f, 1f);
        colors.disabledColor = new Color(0.75f, 0.75f, 0.78f, 1f);
        entryButton.colors = colors;

        if (clickAction != null)
        {
            entryButton.onClick.AddListener(clickAction);
        }

        float dimRate = isInteractable ? 1f : 0.45f;

        Color nameColor = isInteractable
            ? new Color(0.96f, 0.95f, 0.99f, 1f)
            : new Color(0.55f, 0.54f, 0.60f, 1f);

        Color accentColor = new Color(
            rarityColor.r * dimRate,
            rarityColor.g * dimRate,
            rarityColor.b * dimRate,
            1f
        );

        Color typeTextColor = new Color(
            typeColor.r * dimRate,
            typeColor.g * dimRate,
            typeColor.b * dimRate,
            1f
        );

        // 상단 희귀도 띠
        CreateCardImage(
            entryObject.transform, "RarityHeader", accentColor,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -10f), new Vector2(0f, 0f)
        );

        // 마나 배지
        CreateCardImage(
            entryObject.transform, "ManaBadge",
            new Color(0.10f, 0.09f, 0.16f, 1f),
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(10f, -54f), new Vector2(52f, -16f)
        );

        CreateCardLabel(
            entryObject.transform, "ManaText", cardData.ManaCost.ToString(),
            20f, nameColor, TextAlignmentOptions.Center,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(10f, -54f), new Vector2(52f, -16f)
        );

        // 카드 이름
        CreateCardLabel(
            entryObject.transform, "NameText", cardData.CardName,
            19f, nameColor, TextAlignmentOptions.Center,
            new Vector2(0f, 0.42f), new Vector2(1f, 0.76f),
            new Vector2(8f, 0f), new Vector2(-8f, 0f)
        );

        // 계열
        CreateCardLabel(
            entryObject.transform, "TypeText",
            MonsterTypeRules.GetDisplayName(cardData.MainType),
            16f, typeTextColor, TextAlignmentOptions.Center,
            new Vector2(0f, 0.29f), new Vector2(1f, 0.42f),
            new Vector2(8f, 0f), new Vector2(-8f, 0f)
        );

        // 희귀도
        CreateCardLabel(
            entryObject.transform, "RarityText",
            CardRarityRules.GetDisplayName(cardData.Rarity),
            16f, accentColor, TextAlignmentOptions.Center,
            new Vector2(0f, 0.17f), new Vector2(1f, 0.29f),
            new Vector2(8f, 0f), new Vector2(-8f, 0f)
        );

        // 수량 영역
        CreateCardImage(
            entryObject.transform, "CountBackground",
            new Color(0.10f, 0.09f, 0.16f, 1f),
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(8f, 8f), new Vector2(-8f, 40f)
        );

        CreateCardLabel(
            entryObject.transform, "CountText", countText,
            19f, accentColor, TextAlignmentOptions.Center,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(8f, 8f), new Vector2(-8f, 40f)
        );

        generatedEntries.Add(entryObject);
    }

    private void CreateCardImage(
        Transform parent,
        string objectName,
        Color imageColor,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax
    )
    {
        GameObject imageObject =
            new GameObject(objectName, typeof(RectTransform));

        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.AddComponent<Image>();
        image.color = imageColor;
        image.raycastTarget = false;

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = anchorMin;
        imageRect.anchorMax = anchorMax;
        imageRect.offsetMin = offsetMin;
        imageRect.offsetMax = offsetMax;
    }

    private void CreateCardLabel(
        Transform parent,
        string objectName,
        string content,
        float fontSize,
        Color textColor,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax
    )
    {
        GameObject labelObject =
            new GameObject(objectName, typeof(RectTransform));

        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI label =
            labelObject.AddComponent<TextMeshProUGUI>();

        label.text = content;
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.overflowMode = TextOverflowModes.Ellipsis;

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin = anchorMin;
        labelRect.anchorMax = anchorMax;
        labelRect.offsetMin = offsetMin;
        labelRect.offsetMax = offsetMax;
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
