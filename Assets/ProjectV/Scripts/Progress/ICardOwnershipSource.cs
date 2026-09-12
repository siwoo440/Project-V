public interface ICardOwnershipSource // 카드 보유 수량 조회 기능
{
    int GetOwnedCardCount(CardData cardData); // 보유 수량 반환
}
