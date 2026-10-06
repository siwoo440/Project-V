using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 손패 카드 표시
{
    private const float HandCardWidth = 140f; // 손패 카드 너비 (CardButton 프리팹 크기와 같다)

    // 손패 카드 한 장의 표시 내용을 갱신한다. 덱 편성 화면과 같은 카드 표시를 쓴다.
    private void UpdateHandCardView(Button cardButton, CardCopy cardCopy)
    {
        if (cardButton == null) { return; }
        if (cardCopy == null || cardCopy.CardData == null) { return; }

        Transform legacyText = cardButton.transform.Find("CardText");

        if (legacyText != null)
        {
            legacyText.gameObject.SetActive(false); // 프리팹의 글자 표시는 숨긴다.
        }

        int playCost = GetCardPlayCost(cardCopy); // 패시브를 반영한 실제 비용

        CardEntryFactory.BuildFace(
            cardButton.transform,
            cardButton.GetComponent<Image>(),
            cardCopy.CardData,
            cardCopy.EnhanceLevel,
            string.Empty,
            false,
            HandCardWidth,
            false,
            playCost,
            playCost < cardCopy.ManaCost
        );
    }
}
