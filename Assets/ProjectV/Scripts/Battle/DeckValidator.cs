using System.Collections.Generic;

public static class DeckValidator
{
    public static bool TryValidate(
        IReadOnlyList<CardData> deckCards,
        int requiredDeckSize,
        int maxCopiesPerCard,
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

        if (maxCopiesPerCard <= 0)
        {
            errorMessage = "Card copy limit must be greater than 0.";
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
            new Dictionary<string, int>();

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

            if (!cardCounts.ContainsKey(cardId))
            {
                cardCounts.Add(cardId, 0);
            }

            cardCounts[cardId] += 1;

            if (cardCounts[cardId] > maxCopiesPerCard)
            {
                errorMessage =
                    $"{cardData.CardName} exceeds the copy limit. " +
                    $"({cardCounts[cardId]} / {maxCopiesPerCard})";
                return false;
            }
        }

        errorMessage = string.Empty;
        return true;
    }
}