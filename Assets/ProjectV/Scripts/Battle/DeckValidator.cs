using System.Collections.Generic;

public static class DeckValidator
{
    public static bool TryValidate(
        IReadOnlyList<CardData> deckCards,
        int requiredDeckSize,
        out string errorMessage
    )
    {
        return TryValidate(
            deckCards,
            requiredDeckSize,
            null,
            out errorMessage
        ); // 보유 검증 없는 기본 검사
    }

    public static bool TryValidate(
        IReadOnlyList<CardData> deckCards,
        int requiredDeckSize,
        ICardOwnershipSource ownershipSource,
        out string errorMessage
    )
    {
        if (deckCards == null)
        {
            errorMessage = "Deck data is missing.";
            return false;
        }

        if (requiredDeckSize <= 0)
        {
            errorMessage = "Required deck size must be greater than 0.";
            return false;
        }

        if (deckCards.Count != requiredDeckSize)
        {
            errorMessage =
                $"Deck requires exactly {requiredDeckSize} cards. " +
                $"Current: {deckCards.Count}.";
            return false;
        }

        Dictionary<string, int> cardCounts =
            new Dictionary<string, int>(); // 카드별 편성 수량

        Dictionary<string, CardData> cardSamples =
            new Dictionary<string, CardData>(); // 카드별 데이터 참조

        List<string> cardOrder = new List<string>(); // 검사 순서 유지

        for (int i = 0; i < deckCards.Count; i++)
        {
            CardData cardData = deckCards[i];

            if (cardData == null)
            {
                errorMessage = $"Deck contains an empty card at index {i}.";
                return false;
            }

            string cardId = cardData.CardId?.Trim();

            if (string.IsNullOrEmpty(cardId))
            {
                errorMessage =
                    $"Card at index {i} has an empty Card ID.";
                return false;
            }

            if (cardData.SummonMonster == null)
            {
                errorMessage =
                    $"{cardData.CardName} has no monster data."; // 손상된 카드 차단
                return false;
            }

            if (!cardCounts.ContainsKey(cardId))
            {
                cardCounts.Add(cardId, 0);
                cardSamples.Add(cardId, cardData);
                cardOrder.Add(cardId);
            }

            cardCounts[cardId] += 1;

            int maxCopies = cardData.MaxCopies; // 희귀도별 편성 제한

            if (cardCounts[cardId] > maxCopies)
            {
                errorMessage =
                    $"{cardData.CardName} exceeds the " +
                    $"{CardRarityRules.GetDisplayName(cardData.Rarity)} " +
                    $"copy limit. ({cardCounts[cardId]} / {maxCopies})";
                return false;
            }
        }

        if (ownershipSource != null)
        {
            for (int i = 0; i < cardOrder.Count; i++)
            {
                string cardId = cardOrder[i];
                CardData cardData = cardSamples[cardId];

                int ownedCount =
                    ownershipSource.GetOwnedCardCount(cardData); // 보유 수량 조회

                if (ownedCount <= 0)
                {
                    errorMessage =
                        $"{cardData.CardName} is not owned."; // 미보유 카드 차단
                    return false;
                }

                if (cardCounts[cardId] > ownedCount)
                {
                    errorMessage =
                        $"{cardData.CardName} exceeds the owned count. " +
                        $"({cardCounts[cardId]} / {ownedCount})"; // 보유 초과 차단
                    return false;
                }
            }
        }

        errorMessage = string.Empty;
        return true;
    }
}
