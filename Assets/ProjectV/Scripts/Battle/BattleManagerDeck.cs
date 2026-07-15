using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 분리된 전투 기능
{
    private bool ValidateBattleDeckBeforeStart()
    {
        if (!validateDeckOnStart) { return true; }

        bool isValid = DeckValidator.TryValidate(
            deckCards,
            requiredDeckSize,
            maxCopiesPerCard,
            out string errorMessage
        );

        if (isValid) { return true; }

        turnNumber = 0;
        isPlayerTurn = false;
        isBattleEnded = true;

        if (battleLogUI != null)
        {
            battleLogUI.Clear();
            battleLogUI.AddEntry(
                0,
                BattleLogCategory.System,
                $"Deck validation failed: {errorMessage}"
            );
        }

        if (turnText != null)
        {
            turnText.text = "Deck Error";
        }

        if (turnNumberText != null)
        {
            turnNumberText.text = "Turn 0";
        }

        if (resultText != null)
        {
            resultText.text = errorMessage;
        }

        if (endTurnButton != null)
        {
            endTurnButton.interactable = false;
        }

        ClearMonsterSelection();
        SetHandInteractable(false);
        SetMonsterInteractable(false);
        UpdateDeckStatusUI();

        return false;
    }
    private void DrawCards(int drawCount)
    {
        int safeDrawCount = Mathf.Max(0, drawCount);

        for (int i = 0; i < safeDrawCount; i++)
        {
            handButtons.RemoveAll(
                handButton => handButton == null
            );

            if (handButtons.Count >= maxHandSize)
            {
                AddBattleLog(
                    BattleLogCategory.System,
                    $"Hand is full. ({handButtons.Count} / {maxHandSize})"
                );

                break;
            }

            if (!RefillDrawPileIfNeeded())
            {
                AddBattleLog(
                    BattleLogCategory.System,
                    "No cards available to draw."
                );

                break;
            }

            CardData drawnCard = drawPile[0];
            drawPile.RemoveAt(0);

            if (drawnCard == null)
            {
                AddBattleLog(
                    BattleLogCategory.System,
                    "An empty card was removed from the draw pile."
                );

                continue;
            }

            CreateCardButton(drawnCard);
        }

        UpdateDeckStatusUI();
    }

    private void ShuffleCards(List<CardData> cards)
    {
        if (cards == null || cards.Count <= 1) { return; }

        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            CardData temporaryCard = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temporaryCard;
        }
    }

    private bool RefillDrawPileIfNeeded()
    {
        if (drawPile.Count > 0) { return true; }
        if (discardPile.Count == 0) { return false; }

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleCards(drawPile);

        AddBattleLog(
            BattleLogCategory.System,
            $"Discard pile reshuffled: {drawPile.Count} cards."
        );

        UpdateDeckStatusUI();

        return true;
    }

    private void CreateCardButton(CardData cardData)
    {
        if (cardData == null) { return; }
        if (cardButtonPrefab == null || handPanel == null) { return; }

        Button newCardButton = Instantiate(
            cardButtonPrefab,
            handPanel
        );

        TMP_Text cardText =
            newCardButton.GetComponentInChildren<TMP_Text>();

        string monsterName =
            cardData.SummonMonster != null
                ? cardData.SummonMonster.MonsterName
                : "None";

        if (cardText != null)
        {
            cardText.text =
                $"{cardData.CardName}\n" +
                $"Cost: {cardData.ManaCost}\n" +
                $"Summon: {monsterName}";
        }

        newCardButton.onClick.RemoveAllListeners();
        newCardButton.onClick.AddListener(
            () => TryPlayCard(cardData, newCardButton)
        );

        handButtons.Add(newCardButton);
    }
    private void TryPlayCard(CardData cardData, Button cardButton) // 카드 사용 처리
    {
        if (!isPlayerTurn || isBattleEnded) { return; } // 카드 사용 차단

        if (cardData.SummonMonster == null) // 마물 데이터 누락 확인
        {
            resultText.text = "Missing Monster Data"; // 데이터 누락 안내
            return; // 카드 사용 차단
        }

        if (fieldMonsters.Count >= maxFieldMonsterCount) // 필드 최대 수 확인
        {
            resultText.text = "Field Full"; // 필드 초과 안내
            return; // 카드 사용 차단
        }

        if (currentMana < cardData.ManaCost) // 마나 부족 확인
        {
            resultText.text = "Not Enough Mana"; // 마나 부족 안내
            return; // 카드 사용 차단
        }

        currentMana -= cardData.ManaCost; // 카드 비용 차감
        SummonMonster(cardData.SummonMonster); // 마물 필드 소환
        AddBattleLog(BattleLogCategory.PlayerAction, $"{cardData.CardName}: Summoned {cardData.SummonMonster.MonsterName}."); // 카드 소환 기록

        discardPile.Add(cardData); // 사용 카드 버린 더미 이동
        handButtons.Remove(cardButton); // 손패 버튼 목록 제거
        Destroy(cardButton.gameObject); // 카드 버튼 제거
        resultText.text = string.Empty; // 안내 텍스트 초기화
        UpdateBattleUI(); // 카드 사용 결과 표시
    }

    private void UpdateDeckStatusUI()
    {
        if (deckStatusText == null) { return; }

        handButtons.RemoveAll(
            handButton => handButton == null
        );

        int configuredDeckCount =
            deckCards != null ? deckCards.Count : 0;

        deckStatusText.text =
            $"Draw: {drawPile.Count} | " +
            $"Hand: {handButtons.Count} / {maxHandSize} | " +
            $"Discard: {discardPile.Count}\n" +
            $"Deck: {configuredDeckCount} / {requiredDeckSize}";
    }






}