using UnityEngine; // Unity 기본 기능

// 소모성 전투 아이템 데이터 (기획서 9.12.3)
// 전투 전에 하나를 장착하고 전투마다 한 번 쓴다. 마나와 마물의 행동은 쓰지 않는다.
[CreateAssetMenu(
    fileName = "NewBattleItem",
    menuName = "Project V/소모성 전투 아이템 데이터"
)]
public class BattleItemData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string itemId;      // 아이템 ID
    [SerializeField] private string displayName; // 아이템 이름
    [SerializeField] private string iconKey;     // 아이콘 이미지 이름 (UIKeys)

    [Header("효과")]
    [SerializeField] private BattleItemType itemType;     // 효과 종류
    [SerializeField, Min(0)] private int amount = 1;      // 효과 수치

    public string ItemId => itemId;               // 아이템 ID 반환
    public string DisplayName => displayName;     // 아이템 이름 반환
    public string IconKey => iconKey;             // 아이콘 이미지 이름 반환
    public BattleItemType ItemType => itemType;   // 효과 종류 반환
    public int Amount => Mathf.Max(0, amount);    // 효과 수치 반환

    public bool NeedsTarget =>
        itemType == BattleItemType.ShieldMonster; // 대상 마물 선택이 필요한지 여부

    public string EffectText // 수치를 반영한 효과 설명
    {
        get
        {
            switch (itemType)
            {
                case BattleItemType.HealPlayer:
                    return $"플레이어 HP {Amount} 회복";

                case BattleItemType.RestoreMana:
                    return $"현재 마나 {Amount} 회복 (최대 마나를 넘지 않음)";

                case BattleItemType.ShieldMonster:
                    return $"아군 마물 하나에게 보호막 {Amount}";

                case BattleItemType.DrawCards:
                    return $"카드 {Amount}장 드로우";

                case BattleItemType.CleanseAllies:
                    return $"아군 전체의 해로운 상태 효과 {Amount}개씩 제거";

                default:
                    return string.Empty;
            }
        }
    }
}
