using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 마물 카드 강화 화면 연결 (기획서 6.9 / 11.13)
// 왼쪽에서 보유 카드를 고르면 오른쪽 강화대에 올라가고, 사본을 골라 강화한다.
public class EnhanceFlow : MonoBehaviour
{
    [Header("화면 이동")]
    [SerializeField] private Button backButton;            // 돌아가기
    [SerializeField] private Button deckBuilderButton;     // 덱 편성으로 이동

    [Header("필터와 정렬")]
    [SerializeField] private Button typeFilterButton;    // 계열 필터
    [SerializeField] private Button rarityFilterButton;  // 희귀도 필터
    [SerializeField] private Button manaFilterButton;    // 마나 필터
    [SerializeField] private Button sortButton;          // 정렬 방식
    [SerializeField] private TMP_InputField searchInput; // 이름 검색

    [Header("카드 목록")]
    [SerializeField] private Transform ownedCardsContent; // 보유 카드 배치 영역

    [Header("강화대")]
    [SerializeField] private Image slotHeaderImage;    // 희귀도 색 띠 (카드 표시로 대체되어 숨긴다)
    [SerializeField] private Image slotImage;          // 강화대 자리 (카드를 올리면 카드 틀이 된다)
    [SerializeField] private TMP_Text slotNameText;    // 올린 카드 이름
    [SerializeField] private TMP_Text slotInfoText;    // 계열과 희귀도
    [SerializeField] private TMP_Text slotLevelText;   // 올린 사본의 단계
    [SerializeField] private Transform copyListContent; // 사본 버튼 배치 영역
    [SerializeField] private TMP_Text enhanceStatText;  // 강화 전후 능력치
    [SerializeField] private TMP_Text enhanceCostText;  // 강화 비용과 사유
    [SerializeField] private Button enhanceButton;      // 강화 실행
    [SerializeField] private Button clearSlotButton;    // 강화대 비우기

    [Header("화면 텍스트")]
    [SerializeField] private TMP_Text resourceText; // 보유 골드와 정수
    [SerializeField] private TMP_Text capText;      // 플레이어 레벨과 강화 상한
    [SerializeField] private TMP_Text messageText;  // 안내 문구

    private readonly CardFilterState filterState =
        new CardFilterState(); // 필터와 정렬 상태

    private readonly List<GameObject> generatedEntries =
        new List<GameObject>(); // 생성한 보유 카드 항목

    private readonly List<GameObject> copyEntries =
        new List<GameObject>(); // 생성한 사본 버튼

    private CardData slotCard;          // 강화대에 올린 카드
    private CardCopy selectedCopy;      // 선택한 사본
    private bool enhanceConfirmPending; // 되돌릴 수 없어 한 번 더 확인

    private void Awake()
    {
        backButton =
            SceneUIBinder.Bind(backButton, "BackButton");

        deckBuilderButton =
            SceneUIBinder.Bind(deckBuilderButton, "DeckBuilderButton");

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

        ownedCardsContent =
            SceneUIBinder.Bind(ownedCardsContent, "OwnedCardsContent");

        slotHeaderImage =
            SceneUIBinder.Bind(slotHeaderImage, "EnhanceSlotHeader");

        slotImage =
            SceneUIBinder.Bind(slotImage, "EnhanceSlot");

        slotNameText =
            SceneUIBinder.Bind(slotNameText, "EnhanceSlotNameText");

        slotInfoText =
            SceneUIBinder.Bind(slotInfoText, "EnhanceSlotInfoText");

        slotLevelText =
            SceneUIBinder.Bind(slotLevelText, "EnhanceSlotLevelText");

        copyListContent =
            SceneUIBinder.Bind(copyListContent, "EnhanceCopyContent");

        enhanceStatText =
            SceneUIBinder.Bind(enhanceStatText, "EnhanceStatText");

        enhanceCostText =
            SceneUIBinder.Bind(enhanceCostText, "EnhanceCostText");

        enhanceButton =
            SceneUIBinder.Bind(enhanceButton, "EnhanceButton");

        clearSlotButton =
            SceneUIBinder.Bind(clearSlotButton, "ClearSlotButton");

        resourceText =
            SceneUIBinder.Bind(resourceText, "ResourceText");

        capText =
            SceneUIBinder.Bind(capText, "EnhanceCapText");

        messageText =
            SceneUIBinder.Bind(messageText, "MessageText");
    }

    private void Start()
    {
        AddClickListener(backButton, SceneFlow.ReturnToPreviousScene);
        AddClickListener(deckBuilderButton, SceneFlow.LoadDeckBuilder);
        AddClickListener(typeFilterButton, CycleTypeFilter);
        AddClickListener(rarityFilterButton, CycleRarityFilter);
        AddClickListener(manaFilterButton, CycleManaFilter);
        AddClickListener(sortButton, CycleSortMode);
        AddClickListener(enhanceButton, TryEnhanceSelectedCopy);
        AddClickListener(clearSlotButton, ClearSlot);

        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveAllListeners();
            searchInput.onValueChanged.AddListener(OnSearchTextChanged);
        }

        ShowMessage("강화할 카드를 왼쪽에서 고르세요.");
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

    // ---------- 강화대 ----------

    public void PutCardOnSlot(CardData cardData) // 강화대에 카드 올리기
    {
        slotCard = cardData;
        selectedCopy = null;
        enhanceConfirmPending = false;

        ShowMessage(
            cardData == null
                ? "강화할 카드를 왼쪽에서 고르세요."
                : $"{cardData.CardName}을 강화대에 올렸습니다."
        );

        Refresh();
    }

    public void ClearSlot() // 강화대 비우기
    {
        slotCard = null;
        selectedCopy = null;
        enhanceConfirmPending = false;

        ShowMessage("강화대를 비웠습니다.");
        Refresh();
    }

    private void SelectCopy(CardCopy targetCopy) // 강화할 사본 선택
    {
        selectedCopy = targetCopy;
        enhanceConfirmPending = false;

        Refresh();
    }

    // 강화는 되돌릴 수 없으므로(기획서 6.9.7) 두 번 눌러야 실행한다.
    private void TryEnhanceSelectedCopy()
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; }

        if (selectedCopy == null)
        {
            ShowMessage("강화할 사본을 선택하세요.");
            return;
        }

        if (!progress.CanEnhanceCopy(selectedCopy, out string reason))
        {
            enhanceConfirmPending = false;
            ShowMessage(reason);
            Refresh();

            return;
        }

        if (!enhanceConfirmPending)
        {
            enhanceConfirmPending = true;

            ShowMessage(
                $"{selectedCopy.DisplayName} → " +
                $"Lv.{selectedCopy.EnhanceLevel + 1} 강화는 되돌릴 수 없습니다. " +
                $"한 번 더 누르면 진행합니다."
            );

            UpdateEnhanceSlot(progress);

            return;
        }

        progress.TryEnhanceCopy(selectedCopy, out string resultMessage);

        enhanceConfirmPending = false;
        ShowMessage(resultMessage);
        Refresh();
    }

    // ---------- 화면 갱신 ----------

    public void Refresh()
    {
        ClearEntries();
        ClearCopyEntries();

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null)
        {
            ShowMessage("진행 데이터가 없습니다");
            return;
        }

        UpdateControlLabels();
        BuildOwnedCardList(progress);
        UpdateEnhanceSlot(progress);
    }

    private void UpdateControlLabels()
    {
        SetButtonLabel(typeFilterButton, filterState.TypeLabel);
        SetButtonLabel(rarityFilterButton, filterState.RarityLabel);
        SetButtonLabel(manaFilterButton, filterState.ManaLabel);
        SetButtonLabel(sortButton, filterState.SortLabel);
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

            GameObject entryObject = CardEntryFactory.CreateCardEntry(
                ownedCardsContent,
                cardData,
                ownedCard.HighestEnhanceLevel,
                cardData == slotCard
                    ? "강화대에 있음"
                    : $"사본 {ownedCard.OwnedCount}장",
                true,
                () => PutCardOnSlot(cardData)
            ); // 보유 카드 항목 생성

            if (entryObject == null) { continue; }

            generatedEntries.Add(entryObject);
        }
    }

    private void UpdateEnhanceSlot(PlayerProgressManager progress)
    {
        ClearCopyEntries();

        if (resourceText != null)
        {
            resourceText.text =
                $"{UISkin.IconOr(UIIcons.Gold, "골드")} {progress.Gold}      " +
                $"{UISkin.IconOr(UIIcons.Essence, "마물의 정수")} {progress.MonsterEssence}";
        }

        if (capText != null)
        {
            capText.text = BuildCapText(progress);
        }

        if (slotCard == null)
        {
            ShowEmptySlot();
            return;
        }

        IReadOnlyList<CardCopy> copies =
            progress.GetCardCopies(slotCard);

        if (copies.Count == 0)
        {
            ShowEmptySlot();
            return;
        }

        bool isSelectionValid = false;

        foreach (CardCopy copy in copies)
        {
            if (copy == selectedCopy) { isSelectionValid = true; }
        }

        if (!isSelectionValid) { selectedCopy = null; }

        if (selectedCopy == null)
        {
            selectedCopy = copies[0]; // 첫 사본을 기본 선택
        }

        ShowSlotCard(copies.Count); // 강화대에 카드 표시

        foreach (CardCopy copy in copies)
        {
            if (copy == null) { continue; }

            CreateCopyEntry(
                copy,
                copy == selectedCopy,
                progress.IsCopyInDeck(copy)
            );
        }

        MonsterData monsterData = slotCard.SummonMonster;

        if (monsterData == null)
        {
            if (enhanceStatText != null)
            {
                enhanceStatText.text = "마물 데이터가 없는 카드입니다.";
            }

            if (enhanceCostText != null)
            {
                enhanceCostText.text = string.Empty;
            }

            if (enhanceButton != null)
            {
                enhanceButton.interactable = false;
            }

            return;
        }

        int currentLevel = selectedCopy.EnhanceLevel;

        int nextLevel = Mathf.Min(
            currentLevel + 1,
            CardEnhanceRules.MaxLevel
        );

        if (enhanceStatText != null)
        {
            enhanceStatText.text =
                $"{selectedCopy.CopyNumber}번 사본   " +
                $"Lv.{currentLevel} → Lv.{nextLevel}\n\n" +
                BuildStatLine(
                    "체력", monsterData.MaxHp,
                    monsterData.HpGrowthPerLevel,
                    currentLevel, nextLevel
                ) + "\n" +
                BuildStatLine(
                    "공격", monsterData.Attack,
                    monsterData.AttackGrowthPerLevel,
                    currentLevel, nextLevel
                ) + "\n" +
                BuildStatLine(
                    "방어", monsterData.Defense,
                    monsterData.DefenseGrowthPerLevel,
                    currentLevel, nextLevel
                ) + "\n" +
                BuildStatLine(
                    "성욕", monsterData.LustDamage,
                    monsterData.LustGrowthPerLevel,
                    currentLevel, nextLevel
                );
        }

        bool canEnhance =
            progress.CanEnhanceCopy(selectedCopy, out string reason);

        if (enhanceCostText != null)
        {
            enhanceCostText.text =
                selectedCopy.IsMaxLevel
                    ? "최대 단계입니다."
                    : $"필요 {UISkin.IconOr(UIIcons.Essence, "정수")} {selectedCopy.NextEssenceCost}   " +
                      $"필요 {UISkin.IconOr(UIIcons.Gold, "골드")} {selectedCopy.NextGoldCost}\n" +
                      (canEnhance ? "강화할 수 있습니다." : reason);
        }

        if (enhanceButton != null)
        {
            enhanceButton.interactable = canEnhance;
        }

        SetButtonLabel(
            enhanceButton,
            enhanceConfirmPending ? "확인: 되돌릴 수 없음" : "강화"
        );
    }

    // 플레이어 레벨이 마물 강화 상한을 정한다. (기획서 6.3.6)
    private string BuildCapText(PlayerProgressManager progress)
    {
        int enhanceCap = progress.EnhanceLevelCap;

        string capText =
            $"플레이어 Lv.{progress.PlayerLevel}   " +
            $"강화 상한 Lv.{enhanceCap}";

        if (CardEnhanceRules.IsMaxLevel(enhanceCap)) { return capText; }

        int nextCap = enhanceCap + 1;

        return
            $"{capText}   " +
            $"(Lv.{PlayerLevelRules.GetEnhanceUnlockLevel(nextCap)}에 " +
            $"Lv.{nextCap} 해금)";
    }

    private const float SlotCardWidth = 240f; // 강화대 카드 너비 (씬의 EnhanceSlot 크기와 같다)

    private static readonly Color EmptySlotColor =
        new Color(0.20f, 0.16f, 0.31f, 1f); // 빈 자리 이미지가 없을 때 색

    // 강화대에 올린 카드를 덱 편성 화면과 같은 카드 표시로 보여준다.
    private void ShowSlotCard(int ownedCopyCount)
    {
        SetSlotGuideVisible(false);

        if (slotImage == null) { return; }

        CardEntryFactory.BuildFace(
            slotImage.transform,
            slotImage,
            slotCard,
            selectedCopy.EnhanceLevel,
            $"{selectedCopy.CopyNumber}번 사본 ({ownedCopyCount} / {slotCard.MaxCopies})",
            false,
            SlotCardWidth,
            true,
            slotCard.ManaCost,
            false
        );
    }

    private void SetSlotGuideVisible(bool isVisible) // 빈 강화대 안내 문구 표시 전환
    {
        if (slotHeaderImage != null) { slotHeaderImage.gameObject.SetActive(false); }
        if (slotLevelText != null) { slotLevelText.gameObject.SetActive(false); }
        if (slotNameText != null) { slotNameText.gameObject.SetActive(isVisible); }
        if (slotInfoText != null) { slotInfoText.gameObject.SetActive(isVisible); }
    }

    private void ShowEmptySlot() // 강화대가 비어 있을 때 표시
    {
        if (slotImage != null)
        {
            CardEntryFactory.ClearFace(slotImage.transform);

            if (!UISkin.ApplySliced(slotImage, UIKeys.SlotEmpty, 30f))
            {
                slotImage.color = EmptySlotColor;
            }
        }

        SetSlotGuideVisible(true);

        if (slotNameText != null)
        {
            slotNameText.text = "카드를 올리세요";
        }

        if (slotInfoText != null)
        {
            slotInfoText.text = "왼쪽 보유 카드를\n눌러 주세요";
        }

        if (enhanceStatText != null)
        {
            enhanceStatText.text = string.Empty;
        }

        if (enhanceCostText != null)
        {
            enhanceCostText.text = string.Empty;
        }

        if (enhanceButton != null)
        {
            enhanceButton.interactable = false;
        }

        SetButtonLabel(enhanceButton, "강화");
    }

    private string BuildStatLine( // 강화 전후 능력치 문구
        string label,
        int baseValue,
        int growthPerLevel,
        int currentLevel,
        int nextLevel
    )
    {
        int currentValue = CardEnhanceRules.GetStatValue(
            baseValue, growthPerLevel, currentLevel
        );

        int nextValue = CardEnhanceRules.GetStatValue(
            baseValue, growthPerLevel, nextLevel
        );

        if (nextLevel <= currentLevel)
        {
            return $"{label} {currentValue}";
        }

        return $"{label} {currentValue} → {nextValue}";
    }

    private void CreateCopyEntry( // 사본 선택 버튼 생성
        CardCopy targetCopy,
        bool isSelected,
        bool isInDeck
    )
    {
        if (copyListContent == null) { return; }

        GameObject entryObject =
            new GameObject("CopyEntry", typeof(RectTransform));

        entryObject.transform.SetParent(copyListContent, false);

        LayoutElement layoutElement =
            entryObject.AddComponent<LayoutElement>();

        layoutElement.minHeight = 46f;
        layoutElement.preferredHeight = 46f;

        Image background = entryObject.AddComponent<Image>();

        UISkin.ApplySelectable(
            background, isSelected,
            UIKeys.RowNormal, UIKeys.RowSelected,
            new Color(0.17f, 0.14f, 0.26f, 1f),
            new Color(0.42f, 0.34f, 0.16f, 1f)
        ); // 선택한 사본은 밝은 줄로 표시

        Button entryButton = entryObject.AddComponent<Button>();
        entryButton.targetGraphic = background;

        CardCopy captured = targetCopy;

        entryButton.onClick.AddListener(() => SelectCopy(captured));

        CardEntryFactory.CreateLabel(
            entryObject.transform,
            "CopyText",
            $"{targetCopy.CopyNumber}번 사본   " +
            $"Lv.{targetCopy.EnhanceLevel}" +
            (isInDeck ? "   (편성 중)" : string.Empty),
            18f,
            CardEnhanceRules.GetLevelColor(targetCopy.EnhanceLevel),
            TextAlignmentOptions.MidlineLeft,
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(26f, 0f), new Vector2(-26f, 0f)
        );

        copyEntries.Add(entryObject);
    }

    private void SetButtonLabel(Button targetButton, string label)
    {
        if (targetButton == null) { return; }

        TMP_Text buttonLabel =
            targetButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel == null) { return; }

        buttonLabel.text = label;
    }

    private void ShowMessage(string message)
    {
        if (messageText == null) { return; }

        messageText.text = message;
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

    private void ClearCopyEntries()
    {
        foreach (GameObject copyEntry in copyEntries)
        {
            if (copyEntry == null) { continue; }

            Destroy(copyEntry);
        }

        copyEntries.Clear();
    }
}
