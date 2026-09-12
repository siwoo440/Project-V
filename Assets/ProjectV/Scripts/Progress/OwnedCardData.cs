using System; // 직렬화 특성
using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

[Serializable]
public class OwnedCardData // 보유 마물 카드 데이터 (사본 단위 보관)
{
    [SerializeField] private CardData cardData; // 보유 카드

    [SerializeField]
    private List<CardCopy> copies = new List<CardCopy>(); // 보유 사본 목록

    [SerializeField, Min(1)] private int nextCopyNumber = 1; // 다음 사본 번호

    public CardData CardData => cardData; // 보유 카드 반환

    public IReadOnlyList<CardCopy> Copies => copies; // 보유 사본 목록 반환

    public int OwnedCount => copies.Count; // 보유 수량 반환

    public CardRarity Rarity =>
        cardData == null
            ? CardRarity.Common
            : cardData.Rarity; // 카드 희귀도 반환

    public int MaxOwnedCount =>
        CardRarityRules.GetMaxCopies(Rarity); // 희귀도별 보유 한도

    public int EssenceReward =>
        CardRarityRules.GetEssenceReward(Rarity); // 초과 변환량

    public bool IsFull =>
        OwnedCount >= MaxOwnedCount; // 보유 한도 도달 여부

    public int HighestEnhanceLevel // 보유 사본 중 최고 강화 단계
    {
        get
        {
            int highestLevel = CardEnhanceRules.MinLevel;

            foreach (CardCopy copy in copies)
            {
                if (copy == null) { continue; }

                if (copy.EnhanceLevel > highestLevel)
                {
                    highestLevel = copy.EnhanceLevel;
                }
            }

            return highestLevel;
        }
    }

    public OwnedCardData(CardData data)
    {
        cardData = data; // 카드 저장
        copies = new List<CardCopy>(); // 사본 목록 준비
        nextCopyNumber = 1; // 사본 번호 초기화

        TryAddCopy(); // 최초 보유 1장
    }

    public bool TryAddCopy() // 보유 한도 내 사본 추가
    {
        return AddCopy() != null;
    }

    public CardCopy AddCopy() // 사본을 추가하고 새 사본을 반환
    {
        if (IsFull) { return null; } // 한도 초과 차단

        CardCopy newCopy = new CardCopy(cardData, nextCopyNumber);

        copies.Add(newCopy); // 새 사본 등록
        nextCopyNumber += 1; // 다음 사본 번호 준비

        return newCopy;
    }

    public CardCopy GetCopy(int copyNumber) // 사본 번호로 검색
    {
        foreach (CardCopy copy in copies)
        {
            if (copy == null) { continue; }
            if (copy.CopyNumber != copyNumber) { continue; }

            return copy;
        }

        return null;
    }

    public bool HasCopy(CardCopy targetCopy) // 해당 사본 보유 여부
    {
        if (targetCopy == null) { return false; }

        return copies.Contains(targetCopy);
    }
}
