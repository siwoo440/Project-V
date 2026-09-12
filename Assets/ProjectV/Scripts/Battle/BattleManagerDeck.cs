using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 분리된 전투 기능
{
    private void ApplyProgressDeck() // 진행 데이터 덱 적용
    {
        if (!useProgressDeck) { return; } // 씬 덱 사용 설정

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return; } // 진행 데이터 없음
        if (progress.CurrentDeck.Count == 0) { return; } // 구성된 덱 없음

        deckCards.Clear();
        deckCards.AddRange(progress.CurrentDeck); // 플레이어 덱 적용

        Debug.Log(
            $"플레이어 덱을 적용했습니다. {deckCards.Count}장"
        ); // 덱 적용 기록
    }

    private bool ValidateBattleDeckBeforeStart()
    {
        if (!validateDeckOnStart) { return true; }

        ICardOwnershipSource ownershipSource =
            PlayerProgressManager.Instance; // 보유 카드 조회 대상

        if (ownershipSource == null)
        {
            Debug.LogWarning(
                "진행 데이터가 없어 " +
                "카드 보유 검증을 생략합니다."
            ); // 보유 검증 생략 경고
        }

        bool isValid = DeckValidator.TryValidate(
            deckCards,
            requiredDeckSize,
            ownershipSource,
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
                $"덱 검증 실패: {errorMessage}"
            );
        }

        if (turnText != null)
        {
            turnText.text = "덱 오류";
        }

        if (turnNumberText != null)
        {
            turnNumberText.text = "0턴";
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
                    $"손패가 가득 찼습니다. ({handButtons.Count} / {maxHandSize})"
                );

                break;
            }

            if (!RefillDrawPileIfNeeded())
            {
                AddBattleLog(
                    BattleLogCategory.System,
                    "드로우할 카드가 없습니다."
                );

                break;
            }

            CardData drawnCard = drawPile[0];
            drawPile.RemoveAt(0);

            if (drawnCard == null)
            {
                AddBattleLog(
                    BattleLogCategory.System,
                    "빈 카드를 드로우 더미에서 제거했습니다."
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
            $"버린 카드 더미를 다시 섞었습니다. {drawPile.Count}장"
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
                : "없음";

        if (cardText != null)
        {
            cardText.text =
                $"{cardData.CardName}\n" +
                $"비용 {cardData.ManaCost}\n" +
                $"소환 {monsterName}";
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
            resultText.text = "마물 데이터가 없습니다"; // 데이터 누락 안내
            return; // 카드 사용 차단
        }

        if (fieldMonsters.Count >= maxFieldMonsterCount) // 필드 최대 수 확인
        {
            resultText.text = "마물 필드가 가득 찼습니다"; // 필드 초과 안내
            return; // 카드 사용 차단
        }

        if (currentMana < cardData.ManaCost) // 마나 부족 확인
        {
            resultText.text = "마나가 부족합니다"; // 마나 부족 안내
            return; // 카드 사용 차단
        }

        currentMana -= cardData.ManaCost; // 카드 비용 차감
        SummonMonster(cardData.SummonMonster); // 마물 필드 소환
        AddBattleLog(BattleLogCategory.PlayerAction, $"{cardData.CardName}: {cardData.SummonMonster.MonsterName}을 소환했습니다."); // 카드 소환 기록

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
            $"드로우 {drawPile.Count} | " +
            $"손패 {handButtons.Count} / {maxHandSize} | " +
            $"버림 {discardPile.Count}\n" +
            $"덱 {configuredDeckCount} / {requiredDeckSize}";
    }






}