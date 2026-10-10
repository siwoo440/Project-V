using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 시작 손패 교환 (기획서 5.4.2)
// 전투를 시작할 때 받은 손패 가운데 바꿀 카드를 고르고 한 번 교환한다. 고르지 않은 카드는 그대로 둔다.
public partial class BattleManager
{
    [Header("시작 손패 교환")]
    [SerializeField] private GameObject mulliganPanel;     // 교환 안내 창
    [SerializeField] private TMP_Text mulliganText;        // 안내 문구
    [SerializeField] private Button mulliganConfirmButton; // 고른 카드 교환
    [SerializeField] private Button mulliganSkipButton;    // 그대로 시작

    private readonly List<Button> mulliganSelection = new List<Button>(); // 교환하려고 고른 손패

    private bool isMulliganPhase; // 손패를 교환하는 중인지 여부

    private void HideMulligan() // 교환 창을 닫고 교환 상태를 푼다.
    {
        isMulliganPhase = false;
        mulliganSelection.Clear();

        if (mulliganPanel != null) { mulliganPanel.SetActive(false); }
    }

    // 시작 손패를 받은 뒤에 부른다. 교환이 끝날 때까지 카드 사용과 턴 종료를 막는다.
    private void BeginMulligan()
    {
        HideMulligan();

        handButtons.RemoveAll(handButton => handButton == null);

        if (mulliganPanel == null || handButtons.Count == 0) { return; } // 씬을 다시 구성하기 전이면 교환 없이 시작한다.

        isMulliganPhase = true;
        mulliganPanel.SetActive(true);

        if (mulliganConfirmButton != null)
        {
            mulliganConfirmButton.onClick.RemoveAllListeners();
            mulliganConfirmButton.onClick.AddListener(ConfirmMulligan);
        }

        if (mulliganSkipButton != null)
        {
            mulliganSkipButton.onClick.RemoveAllListeners();
            mulliganSkipButton.onClick.AddListener(SkipMulligan);
        }

        endTurnButton.interactable = false; // 교환을 마쳐야 턴을 진행할 수 있다.

        RefreshMulliganUI();
        UpdateSummonerUI();
        UpdateBattleItemUI();

        AddBattleLog(BattleLogCategory.System, "시작 손패 교환: 바꿀 카드를 고르세요.");
    }

    private void ToggleMulliganCard(Button cardButton) // 손패를 눌러 교환할 카드로 고르거나 해제한다.
    {
        if (cardButton == null) { return; }

        if (!mulliganSelection.Remove(cardButton))
        {
            mulliganSelection.Add(cardButton);
        }

        SetMulliganMark(cardButton, mulliganSelection.Contains(cardButton));
        RefreshMulliganUI();
    }

    private static void SetMulliganMark(Button cardButton, bool isSelected) // 고른 카드는 흐리고 작게 보인다.
    {
        CanvasGroup cardGroup = cardButton.GetComponent<CanvasGroup>();

        if (cardGroup == null) { cardGroup = cardButton.gameObject.AddComponent<CanvasGroup>(); }

        cardGroup.alpha = isSelected ? 0.45f : 1f;
        cardButton.transform.localScale = isSelected ? Vector3.one * 0.92f : Vector3.one;
    }

    private void RefreshMulliganUI()
    {
        int selectedCount = mulliganSelection.Count;

        if (mulliganText != null)
        {
            mulliganText.text =
                "시작 손패 교환\n" +
                $"바꿀 카드를 눌러 고르세요. 한 번만 바꿀 수 있습니다. (고른 카드 {selectedCount}장)";
        }

        if (mulliganConfirmButton == null) { return; }

        mulliganConfirmButton.interactable = selectedCount > 0;

        TMP_Text confirmLabel = mulliganConfirmButton.GetComponentInChildren<TMP_Text>(true);

        if (confirmLabel != null) { confirmLabel.text = $"교환 ({selectedCount}장)"; }
    }

    // 고른 카드를 덱으로 돌려보내고 같은 수만큼 새로 받는다.
    // 새 카드를 먼저 받은 뒤에 돌려보낸 카드를 넣고 덱을 다시 섞으므로, 방금 돌려보낸 카드를 바로 다시 받지 않는다.
    private void ConfirmMulligan()
    {
        if (!isMulliganPhase || mulliganSelection.Count == 0) { return; }

        List<CardCopy> returnedCards = new List<CardCopy>();

        foreach (Button cardButton in mulliganSelection)
        {
            if (cardButton == null) { continue; }

            if (handCardCopies.TryGetValue(cardButton, out CardCopy cardCopy) && cardCopy != null)
            {
                returnedCards.Add(cardCopy);
            }

            handCardCopies.Remove(cardButton);
            handButtons.Remove(cardButton);
            Destroy(cardButton.gameObject);
        }

        DrawCards(returnedCards.Count);

        drawPile.AddRange(returnedCards);
        ShuffleCards(drawPile);

        AddBattleLog(BattleLogCategory.System, $"시작 손패 {returnedCards.Count}장을 교환했습니다.");

        EndMulligan();
    }

    private void SkipMulligan() // 교환하지 않고 시작한다.
    {
        if (!isMulliganPhase) { return; }

        foreach (Button cardButton in mulliganSelection)
        {
            if (cardButton != null) { SetMulliganMark(cardButton, false); }
        }

        AddBattleLog(BattleLogCategory.System, "시작 손패를 교환하지 않았습니다.");

        EndMulligan();
    }

    private void EndMulligan() // 교환을 마치고 첫 턴을 진행한다.
    {
        HideMulligan();
        ShowPlayerTurn();
        UpdateBattleUI();
    }
}
