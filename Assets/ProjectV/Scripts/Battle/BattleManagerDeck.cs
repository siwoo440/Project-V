using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 분리된 전투 기능
{
    private void ApplyProgressDeck() // 전투에 사용할 사본 덱 구성
    {
        battleDeck.Clear();

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (useProgressDeck && progress != null)
        {
            // 덱을 비운 상태도 그대로 반영해 덱 검증이 사유를 알려주도록 한다.
            battleDeck.AddRange(progress.CurrentDeck); // 플레이어 덱 적용

            Debug.Log(
                $"플레이어 덱을 적용했습니다. {battleDeck.Count}장"
            ); // 덱 적용 기록

            return;
        }

        // 씬에 직접 넣은 덱은 강화되지 않은 임시 사본으로 다룬다.
        for (int i = 0; i < deckCards.Count; i++)
        {
            if (deckCards[i] == null) { continue; }

            battleDeck.Add(new CardCopy(deckCards[i], i + 1));
        }

        Debug.Log(
            $"씬 설정 덱을 적용했습니다. {battleDeck.Count}장"
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
            battleDeck,
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

            CardCopy drawnCard = drawPile[0];
            drawPile.RemoveAt(0);

            if (drawnCard == null || drawnCard.CardData == null)
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
        DrawReshuffleBonus(); // 재활용 지식 패시브의 추가 드로우
    }

    // 덱을 다시 섞었을 때 순환 기록과 재활용 지식 패시브만큼 카드를 더 뽑는다.
    private void DrawReshuffleBonus()
    {
        if (pendingReshuffleBonus <= 0 || isDrawingReshuffleBonus) { return; }

        int bonusCount = pendingReshuffleBonus;

        pendingReshuffleBonus = 0;
        isDrawingReshuffleBonus = true;

        AddBattleLog(
            BattleLogCategory.PlayerAction,
            $"덱 재구성: 카드 {bonusCount}장 추가 드로우"
        );

        DrawCards(bonusCount);

        isDrawingReshuffleBonus = false;
    }

    private void ShuffleCards(List<CardCopy> cards)
    {
        if (cards == null || cards.Count <= 1) { return; }

        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            CardCopy temporaryCard = cards[i];
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

        pendingReshuffleBonus +=
            Grimoire(GrimoireEffectType.CycleRecord) +
            GetPassiveAmount(SummonerPassiveType.RecycleKnowledge); // 순환 기록, 재활용 지식 패시브

        UpdateDeckStatusUI();

        return true;
    }

    private void CreateCardButton(CardCopy cardCopy)
    {
        if (cardCopy == null || cardCopy.CardData == null) { return; }
        if (cardButtonPrefab == null || handPanel == null) { return; }

        Button newCardButton = Instantiate(
            cardButtonPrefab,
            handPanel
        );

        newCardButton.onClick.RemoveAllListeners();
        newCardButton.onClick.AddListener(
            () => TryPlayCard(cardCopy, newCardButton)
        );

        handButtons.Add(newCardButton);
        handCardCopies[newCardButton] = cardCopy; // 버튼과 사본 연결

        UpdateHandCardView(newCardButton, cardCopy);
    }

    private void RefreshHandCardViews() // 손패 전체 표시 갱신 (비용 변동 반영)
    {
        foreach (KeyValuePair<Button, CardCopy> handCard in handCardCopies)
        {
            UpdateHandCardView(handCard.Key, handCard.Value);
        }
    }
    private void TryPlayCard(CardCopy cardCopy, Button cardButton) // 카드 사용 처리
    {
        if (isMulliganPhase && !isBattleEnded)
        {
            ToggleMulliganCard(cardButton); // 손패 교환 중에는 카드를 쓰지 않고 교환할 카드로 고른다.
            return;
        }

        if (!isPlayerTurn || isBattleEnded) { return; } // 카드 사용 차단
        if (cardCopy == null || cardCopy.CardData == null) { return; } // 빈 카드 차단

        if (cardCopy.SummonMonster == null) // 마물 데이터 누락 확인
        {
            resultText.text = "마물 데이터가 없습니다"; // 데이터 누락 안내
            return; // 카드 사용 차단
        }

        if (fieldMonsters.Count >= maxFieldMonsterCount) // 필드 최대 수 확인
        {
            resultText.text = "마물 필드가 가득 찼습니다"; // 필드 초과 안내
            return; // 카드 사용 차단
        }

        int playCost = GetCardPlayCost(cardCopy); // 그리모어와 패시브를 반영한 실제 비용
        bool usedCheapContract = GetGrimoireCardDiscount(cardCopy) > 0;
        bool usedThriftySummon = GetThriftyDiscount(cardCopy) > 0;

        if (currentMana < playCost) // 마나 부족 확인
        {
            resultText.text = "마나가 부족합니다"; // 마나 부족 안내
            return; // 카드 사용 차단
        }

        currentMana -= playCost; // 카드 비용 차감

        handButtons.Remove(cardButton); // 손패 버튼 목록 제거
        handCardCopies.Remove(cardButton); // 손패 카드 연결 제거

        if (usedCheapContract) { grimoireCheapCardUsed = true; } // 저비용 계약: 전투당 한 번
        if (usedThriftySummon) { thriftySummonUsed = true; }     // 절약 소환: 전투당 한 번

        if (usedCheapContract || usedThriftySummon)
        {
            AddBattleLog(
                BattleLogCategory.PlayerAction,
                $"비용 감소: {cardCopy.CardName} 비용 {cardCopy.ManaCost} → {playCost}"
            );

            RefreshHandCardViews(); // 다른 카드의 비용 표시를 원래대로 되돌린다.
        }

        SummonMonster(
            cardCopy.SummonMonster,
            cardCopy.EnhanceLevel,
            cardCopy
        ); // 강화 단계를 반영해 마물 소환

        AddBattleLog(
            BattleLogCategory.PlayerAction,
            $"{cardCopy.CardName} Lv.{cardCopy.EnhanceLevel}: " +
            $"{cardCopy.SummonMonster.MonsterName}을 소환했습니다."
        ); // 카드 소환 기록

        discardPile.Add(cardCopy); // 사용 카드 버린 더미 이동
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

        int configuredDeckCount = battleDeck.Count;

        deckStatusText.text =
            $"{UISkin.IconOr(UIIcons.Draw, "드로우")} {drawPile.Count}    " +
            $"손패 {handButtons.Count} / {maxHandSize}    " +
            $"{UISkin.IconOr(UIIcons.Discard, "버림")} {discardPile.Count}    " +
            $"덱 {configuredDeckCount}"; // 한 줄로 표시
    }
}
