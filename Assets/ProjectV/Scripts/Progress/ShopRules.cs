using UnityEngine; // Unity 기본 기능

// 상점 규칙 (기획서 9.12 / 11.14)
public static class ShopRules
{
    public const int MaxBattleItemCount = 99; // 소모성 아이템 한 종류의 보유 한도

    private static readonly ShopRewardType[] TabOrder =
    {
        ShopRewardType.MonsterEssence,
        ShopRewardType.DesireShard,
        ShopRewardType.BattleItem,
    };

    public static ShopRewardType[] Tabs => TabOrder; // 화면에 보여줄 탭 순서 (기획서 11.14)

    public static string GetTabName(ShopRewardType rewardType)
    {
        switch (rewardType)
        {
            case ShopRewardType.MonsterEssence: return "마물의 정수";
            case ShopRewardType.DesireShard: return "욕망의 파편";
            case ShopRewardType.BattleItem: return "소모성 아이템";
            default: return "알 수 없음";
        }
    }

    // 할인율을 반영한 가격. 소수점은 버리고 1골드 아래로는 내려가지 않는다.
    public static int GetPrice(int basePrice, int discountPercent)
    {
        if (basePrice <= 0) { return 0; }

        int percent = Mathf.Clamp(100 - discountPercent, 0, 100);

        return Mathf.Max(1, basePrice * percent / 100);
    }

    // 기획서 9.12.2는 챕터 클리어로 상품이 열린다. 챕터 진행이 생기기 전까지는
    // 기획서 6.3.5의 레벨 해금을 기준으로 판정한다.
    public static bool IsUnlocked(ShopItemData item, int playerLevel)
    {
        return item != null && playerLevel >= item.UnlockLevel;
    }

    public static string GetUnlockText(ShopItemData item) // 잠긴 상품의 해금 조건 안내
    {
        return item == null
            ? string.Empty
            : $"플레이어 Lv.{item.UnlockLevel}에 해금됩니다.";
    }

    public static string GetIconKey(ShopItemData item) // 상품 아이콘 이미지 이름
    {
        if (item == null) { return string.Empty; }

        switch (item.RewardType)
        {
            case ShopRewardType.MonsterEssence:
                return item.Amount > 1 && UISkin.Has(UIKeys.ItemEssenceBundle)
                    ? UIKeys.ItemEssenceBundle
                    : UIKeys.StatEssence; // 묶음 그림이 없으면 정수 아이콘

            case ShopRewardType.DesireShard:
                return UIKeys.StatShard;

            case ShopRewardType.BattleItem:
                return item.BattleItem == null ? string.Empty : item.BattleItem.IconKey;

            default:
                return string.Empty;
        }
    }
}
