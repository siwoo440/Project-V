using System; // 직렬화 특성
using UnityEngine; // Unity 기본 기능

[Serializable]
public class CardCopy // 개별 마물 카드 사본 (기획서 6.9.1)
{
    [SerializeField] private CardData cardData; // 사본의 원본 카드
    [SerializeField, Min(1)] private int copyNumber = 1; // 같은 카드 내 사본 번호
    [SerializeField, Min(1)] private int enhanceLevel = 1; // 강화 단계

    public CardData CardData => cardData; // 원본 카드 반환
    public int CopyNumber => copyNumber; // 사본 번호 반환

    public int EnhanceLevel =>
        CardEnhanceRules.ClampLevel(enhanceLevel); // 강화 단계 반환

    public bool IsMaxLevel =>
        CardEnhanceRules.IsMaxLevel(EnhanceLevel); // 최대 단계 여부

    public CardRarity Rarity =>
        cardData == null ? CardRarity.Common : cardData.Rarity; // 희귀도 반환

    public string CardId =>
        cardData == null ? string.Empty : cardData.CardId; // 카드 ID 반환

    public string CardName =>
        cardData == null ? "빈 카드" : cardData.CardName; // 카드 이름 반환

    public int ManaCost =>
        cardData == null ? 0 : cardData.ManaCost; // 마나 비용 반환

    public MonsterData SummonMonster =>
        cardData == null ? null : cardData.SummonMonster; // 소환 마물 반환

    public bool IsBossOrigin =>
        cardData != null && cardData.IsBossOrigin; // 보스 출신 여부 반환

    public int MaxCopies =>
        cardData == null ? 1 : cardData.MaxCopies; // 희귀도별 보유 한도 반환

    public string DisplayName =>
        $"{CardName} Lv.{EnhanceLevel}"; // 사본 표시 이름

    public int NextEssenceCost =>
        IsMaxLevel
            ? 0
            : CardEnhanceRules.GetEssenceCost(Rarity, EnhanceLevel); // 다음 단계 정수 비용

    public int NextGoldCost =>
        IsMaxLevel
            ? 0
            : CardEnhanceRules.GetGoldCost(Rarity, EnhanceLevel); // 다음 단계 골드 비용

    public CardCopy(CardData data, int number)
    {
        cardData = data; // 원본 카드 저장
        copyNumber = Mathf.Max(1, number); // 사본 번호 저장
        enhanceLevel = CardEnhanceRules.MinLevel; // 새 사본은 Lv.1
    }

    public CardCopy(CardData data, int number, int level) // 저장 데이터에서 되살린 사본
    {
        cardData = data;
        copyNumber = Mathf.Max(1, number);
        enhanceLevel = CardEnhanceRules.ClampLevel(level);
    }

    public bool RaiseEnhanceLevel() // 강화 단계 1 상승 (기획서 6.9.7: 되돌리기 없음)
    {
        if (IsMaxLevel) { return false; } // 최대 단계 차단

        enhanceLevel = EnhanceLevel + 1;

        return true;
    }

    public bool IsSameCard(CardData other) // 같은 카드인지 확인
    {
        if (cardData == null || other == null) { return false; }
        if (cardData == other) { return true; }

        return cardData.CardId == other.CardId;
    }
}
