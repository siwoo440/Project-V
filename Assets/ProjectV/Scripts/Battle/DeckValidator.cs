using System.Collections.Generic;

public static class DeckValidator
{
    private const int MaxLegendaryCards = 3; // 덱 전설 제한
    private const int MaxBossOriginCards = 1; // 덱 보스 출신 제한

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
            errorMessage = "덱 데이터가 없습니다.";
            return false;
        }

        if (requiredDeckSize <= 0)
        {
            errorMessage = "필요 덱 장수는 1 이상이어야 합니다.";
            return false;
        }

        if (deckCards.Count != requiredDeckSize)
        {
            errorMessage =
                $"덱은 정확히 {requiredDeckSize}장이어야 합니다. " +
                $"현재 {deckCards.Count}장.";
            return false;
        }

        Dictionary<string, int> cardCounts =
            new Dictionary<string, int>(); // 카드별 편성 수량

        Dictionary<string, CardData> cardSamples =
            new Dictionary<string, CardData>(); // 카드별 데이터 참조

        List<string> cardOrder = new List<string>(); // 검사 순서 유지

        int legendaryCount = 0; // 전설 편성 수량
        int bossOriginCount = 0; // 보스 출신 편성 수량

        for (int i = 0; i < deckCards.Count; i++)
        {
            CardData cardData = deckCards[i];

            if (cardData == null)
            {
                errorMessage = $"{i}번 자리에 빈 카드가 있습니다.";
                return false;
            }

            string cardId = cardData.CardId?.Trim();

            if (string.IsNullOrEmpty(cardId))
            {
                errorMessage =
                    $"{i}번 카드의 카드 ID가 비어 있습니다.";
                return false;
            }

            if (cardData.SummonMonster == null)
            {
                errorMessage =
                    $"{cardData.CardName}에 마물 데이터가 없습니다."; // 손상된 카드 차단
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
                    $"{cardData.CardName}은 " +
                    $"{CardRarityRules.GetDisplayName(cardData.Rarity)} " +
                    $"중복 제한을 초과했습니다. ({cardCounts[cardId]} / {maxCopies})";
                return false;
            }

            if (cardData.Rarity == CardRarity.Legendary)
            {
                legendaryCount += 1; // 전설 수량 집계

                if (legendaryCount > MaxLegendaryCards)
                {
                    errorMessage =
                        $"덱의 전설 마물 제한을 초과했습니다. " +
                        $"({legendaryCount} / {MaxLegendaryCards})";
                    return false;
                }
            }

            if (cardData.IsBossOrigin)
            {
                bossOriginCount += 1; // 보스 출신 수량 집계

                if (bossOriginCount > MaxBossOriginCards)
                {
                    errorMessage =
                        $"덱의 보스 출신 마물 제한을 초과했습니다. " +
                        $"({bossOriginCount} / {MaxBossOriginCards})";
                    return false;
                }
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
                        $"{cardData.CardName}은 보유하지 않은 카드입니다."; // 미보유 카드 차단
                    return false;
                }

                if (cardCounts[cardId] > ownedCount)
                {
                    errorMessage =
                        $"{cardData.CardName}의 보유 수량을 초과했습니다. " +
                        $"({cardCounts[cardId]} / {ownedCount})"; // 보유 초과 차단
                    return false;
                }
            }
        }

        errorMessage = string.Empty;
        return true;
    }
}
