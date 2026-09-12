using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class DeckBuilderFlow : MonoBehaviour // 덱 편성 화면 연결
{
    [Header("화면 이동")]
    [SerializeField] private Button backButton; // 돌아가기

    [Header("카드 목록")]
    [SerializeField] private Transform ownedCardsContent;  // 보유 카드 배치 영역
    [SerializeField] private Transform currentDeckContent; // 현재 덱 배치 영역

    [Header("덱 텍스트")]
    [SerializeField] private TMP_Text deckCountText; // 덱 통계 표시
    [SerializeField] private TMP_Text messageText;   // 안내 문구

    private readonly List<GameObject> generatedEntries =
        new List<GameObject>(); // 생성한 항목 목록

    private void Awake()
    {
        backButton =
            SceneUIBinder.Bind(backButton, "BackButton");

        ownedCardsContent =
            SceneUIBinder.Bind(ownedCardsContent, "OwnedCardsContent");

        currentDeckContent =
            SceneUIBinder.Bind(currentDeckContent, "CurrentDeckContent");

        deckCountText =
            SceneUIBinder.Bind(deckCountText, "DeckCountText");

        messageText =
            SceneUIBinder.Bind(messageText, "MessageText");
    }

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(
                SceneFlow.ReturnToPreviousScene
            );
        }

        Refresh(); // 목록 갱신
    }

    public void Refresh()
    {
        ClearEntries();

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null)
        {
            if (messageText != null)
            {
                messageText.text = "진행 데이터가 없습니다";
            }

            return;
        }

        foreach (OwnedCardData ownedCard in progress.OwnedCards)
        {
            if (ownedCard == null) { continue; }
            if (ownedCard.CardData == null) { continue; }

            CreateEntry(
                ownedCardsContent,
                $"{ownedCard.CardData.CardName}   " +
                $"{CardRarityRules.GetDisplayName(ownedCard.Rarity)}   " +
                $"{MonsterTypeRules.GetDisplayName(ownedCard.CardData.MainType)}   " +
                $"마나 {ownedCard.CardData.ManaCost}   " +
                $"{ownedCard.OwnedCount} / {ownedCard.MaxOwnedCount}",
                CardRarityRules.GetDisplayColor(ownedCard.Rarity)
            ); // 보유 카드 항목 생성
        }

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
            CreateEntry(
                currentDeckContent,
                $"{deckCard.CardName}   " +
                $"마나 {deckCard.ManaCost}   " +
                $"x{deckCounts[deckCard.CardId]}",
                CardRarityRules.GetDisplayColor(deckCard.Rarity)
            ); // 편성 카드 항목 생성
        }

        if (deckCountText != null)
        {
            deckCountText.text =
                $"덱 {progress.CurrentDeck.Count} / " +
                $"{progress.RequiredDeckSize}      " +
                $"평균 마나 {progress.CurrentDeckAverageMana:0.00}"; // 덱 통계 표시
        }

        if (messageText != null)
        {
            messageText.text = progress.CurrentDeck.Count == 0
                ? "구성된 덱이 없습니다."
                : "덱 목록은 읽기 전용입니다. 편성 기능은 이후 일차에 추가됩니다."; // 안내 문구
        }
    }

    private void CreateEntry(
        Transform parentContent,
        string entryText,
        Color entryColor
    )
    {
        if (parentContent == null) { return; } // 배치 영역 누락 차단

        GameObject entryObject =
            new GameObject("CardEntry", typeof(RectTransform));

        entryObject.transform.SetParent(parentContent, false);

        TextMeshProUGUI entryLabel =
            entryObject.AddComponent<TextMeshProUGUI>();

        entryLabel.text = entryText;
        entryLabel.color = entryColor;
        entryLabel.fontSize = 20f;
        entryLabel.alignment = TextAlignmentOptions.MidlineLeft;

        LayoutElement entryLayout =
            entryObject.AddComponent<LayoutElement>();

        entryLayout.minHeight = 30f;
        entryLayout.preferredHeight = 30f;

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
