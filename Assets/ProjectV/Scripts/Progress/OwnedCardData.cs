using System;
using UnityEngine;

[Serializable]
public class OwnedCardData // 보유 마물 카드 데이터
{
    [SerializeField] private CardData cardData; // 보유 카드
    [SerializeField, Min(0)] private int ownedCount = 1; // 보유 수량

    public CardData CardData => cardData; // 보유 카드 반환
    public int OwnedCount => ownedCount; // 보유 수량 반환

    public CardRarity Rarity =>
        cardData == null
            ? CardRarity.Common
            : cardData.Rarity; // 카드 희귀도 반환

    public int MaxOwnedCount =>
        CardRarityRules.GetMaxCopies(Rarity); // 희귀도별 보유 한도

    public int EssenceReward =>
        CardRarityRules.GetEssenceReward(Rarity); // 초과 변환량

    public bool IsFull =>
        ownedCount >= MaxOwnedCount; // 보유 한도 도달 여부

    public OwnedCardData(CardData data)
    {
        cardData = data; // 카드 저장
        ownedCount = 1; // 최초 보유 1장
    }

    public bool TryAddCopy() // 보유 한도 내 추가
    {
        if (IsFull) { return false; } // 한도 초과 차단

        ownedCount += 1; // 보유 수량 증가

        return true;
    }
}
