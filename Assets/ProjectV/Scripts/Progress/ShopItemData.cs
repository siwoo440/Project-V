using UnityEngine; // Unity 기본 기능

// 상점 상품 데이터 (기획서 9.12.2)
[CreateAssetMenu(
    fileName = "NewShopItem",
    menuName = "Project V/상점 상품 데이터"
)]
public class ShopItemData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string itemId;      // 상품 ID
    [SerializeField] private string displayName; // 상품 이름
    [SerializeField, Min(0)] private int sortOrder; // 탭 안에서의 순서

    [Header("지급 내용")]
    [SerializeField] private ShopRewardType rewardType;    // 주는 것
    [SerializeField, Min(1)] private int amount = 1;       // 주는 수량
    [SerializeField] private BattleItemData battleItem;    // 소모성 아이템 상품일 때의 아이템

    [Header("가격과 해금")]
    [SerializeField, Min(0)] private int price = 100;      // 골드 가격
    [SerializeField, Min(0)] private int unlockChapter;    // 해금에 필요한 클리어 챕터 (0이면 상점 해금 시, 기획서 9.12.2)
    [SerializeField, Min(1)] private int unlockLevel = 1;  // 챕터 진행이 생기기 전까지 쓰는 해금 레벨 (기획서 6.3.5)
    [SerializeField] private bool needsConfirm;            // 구매 전에 한 번 더 확인할지 (기획서 11.14)

    public string ItemId => itemId;                 // 상품 ID 반환
    public string DisplayName => displayName;       // 상품 이름 반환
    public int SortOrder => sortOrder;              // 순서 반환
    public ShopRewardType RewardType => rewardType; // 주는 것 반환
    public int Amount => Mathf.Max(1, amount);      // 주는 수량 반환
    public BattleItemData BattleItem => battleItem; // 소모성 아이템 반환
    public int Price => Mathf.Max(0, price);        // 정가 반환
    public int UnlockChapter => Mathf.Max(0, unlockChapter); // 해금 챕터 반환
    public int UnlockLevel => Mathf.Max(1, unlockLevel);     // 해금 레벨 반환
    public bool NeedsConfirm => needsConfirm;       // 구매 확인 필요 여부 반환

    public string EffectText // 상품 설명
    {
        get
        {
            switch (rewardType)
            {
                case ShopRewardType.MonsterEssence:
                    return $"마물의 정수 {Amount}개. 마물 카드 강화에 사용합니다.";

                case ShopRewardType.DesireShard:
                    return $"욕망의 파편 {Amount}개. 그리모어 강화에 사용합니다.";

                case ShopRewardType.BattleItem:
                    return battleItem == null
                        ? string.Empty
                        : $"{battleItem.EffectText}. 전투마다 한 번 사용합니다.";

                default:
                    return string.Empty;
            }
        }
    }
}
