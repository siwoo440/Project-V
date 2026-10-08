using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 재화 보유 한도, 상점, 소모성 전투 아이템 (기획서 9.3 / 9.12)
public partial class PlayerProgressManager
{
    [Header("상점")]
    [SerializeField]
    private List<ShopItemData> shopItems =
        new List<ShopItemData>(); // 상품 목록 (탭, 순서 순)

    private readonly Dictionary<BattleItemData, int> battleItemCounts =
        new Dictionary<BattleItemData, int>(); // 소모성 아이템 보유 수량

    private BattleItemData equippedBattleItem; // 전투에 가져갈 소모성 아이템

    public IReadOnlyList<ShopItemData> ShopItems =>
        shopItems; // 상품 목록 반환

    private void InitializeShopProgress() // 시작 진행 데이터 구성 (무료 상품은 지급하지 않는다. 기획서 10.13)
    {
        battleItemCounts.Clear();
        equippedBattleItem = null;
    }

    // ---------- 재화 보유 한도 ----------

    // 지금 받으면 실제로 반영될 양. 한도를 넘는 양은 받지 못한다. (기획서 9.3)
    public int PreviewGoldGain(int amount)
    {
        return CurrencyRules.GetAddable(gold, amount, CurrencyRules.GoldLimit);
    }

    public int PreviewEssenceGain(int amount)
    {
        return CurrencyRules.GetAddable(monsterEssence, amount, CurrencyRules.EssenceLimit);
    }

    public int PreviewShardGain(int amount)
    {
        return CurrencyRules.GetAddable(desireShards, amount, CurrencyRules.ShardLimit);
    }

    private int AddGold(int amount) // 골드를 더하고 실제로 반영된 양을 반환한다.
    {
        int addedAmount = PreviewGoldGain(amount);

        gold += addedAmount;

        return addedAmount;
    }

    private int AddMonsterEssence(int amount) // 마물의 정수를 더하고 실제로 반영된 양을 반환한다.
    {
        int addedAmount = PreviewEssenceGain(amount);

        monsterEssence += addedAmount;

        return addedAmount;
    }

    // ---------- 상점 ----------

    public int ShopDiscountPercent =>
        GetEquippedPassiveAmount(SummonerPassiveType.MerchantSense); // 상인의 감각 패시브의 할인율

    public int GetShopPrice(ShopItemData item) // 할인을 반영한 가격
    {
        return item == null
            ? 0
            : ShopRules.GetPrice(item.Price, ShopDiscountPercent);
    }

    public bool IsShopItemUnlocked(ShopItemData item) // 상품 해금 여부
    {
        if (item == null) { return false; }

        return item.UnlockChapter <= 0 || IsChapterCleared(item.UnlockChapter); // 기획서 9.12.2
    }

    public int GetShopOwnedCount(ShopItemData item) // 상품이 주는 것의 현재 보유량
    {
        if (item == null) { return 0; }

        switch (item.RewardType)
        {
            case ShopRewardType.MonsterEssence: return monsterEssence;
            case ShopRewardType.DesireShard: return desireShards;
            case ShopRewardType.BattleItem: return GetBattleItemCount(item.BattleItem);
            default: return 0;
        }
    }

    private int GetShopOwnedLimit(ShopItemData item) // 상품이 주는 것의 보유 한도
    {
        switch (item.RewardType)
        {
            case ShopRewardType.MonsterEssence: return CurrencyRules.EssenceLimit;
            case ShopRewardType.DesireShard: return CurrencyRules.ShardLimit;
            default: return ShopRules.MaxBattleItemCount;
        }
    }

    public bool CanBuyShopItem( // 구매 가능 여부와 사유
        ShopItemData item,
        out string reason
    )
    {
        if (item == null)
        {
            reason = "구매할 상품을 선택하세요.";
            return false;
        }

        if (!IsShopItemUnlocked(item))
        {
            reason = ShopRules.GetUnlockText(item);
            return false;
        }

        if (item.RewardType == ShopRewardType.BattleItem && item.BattleItem == null)
        {
            reason = "상품 데이터가 올바르지 않습니다.";
            return false;
        }

        int ownedLimit = GetShopOwnedLimit(item);

        if (GetShopOwnedCount(item) + item.Amount > ownedLimit)
        {
            reason = $"더 가질 수 없습니다. (보유 한도 {ownedLimit})";
            return false;
        }

        int price = GetShopPrice(item);

        if (gold < price)
        {
            reason = $"골드가 부족합니다. ({gold} / {price})";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public bool TryBuyShopItem( // 상품 구매 (구매 횟수 제한 없음, 되팔기 없음. 기획서 9.12.1)
        ShopItemData item,
        out string message
    )
    {
        if (!CanBuyShopItem(item, out message)) { return false; }

        int price = GetShopPrice(item);

        gold -= price;

        switch (item.RewardType)
        {
            case ShopRewardType.MonsterEssence:
                monsterEssence += item.Amount;
                break;

            case ShopRewardType.DesireShard:
                desireShards += item.Amount;
                break;

            case ShopRewardType.BattleItem:
                battleItemCounts[item.BattleItem] =
                    GetBattleItemCount(item.BattleItem) + item.Amount;
                break;
        }

        message = $"{item.DisplayName} 구매 (골드 -{price})";

        Debug.Log(message); // 구매 기록

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림
        AutoSave("상점 구매"); // 기획서 9.17

        return true;
    }

    // ---------- 소모성 전투 아이템 ----------

    public int GetBattleItemCount(BattleItemData item) // 아이템 보유 수량
    {
        if (item == null) { return 0; }

        return battleItemCounts.TryGetValue(item, out int count) ? count : 0;
    }

    public BattleItemData EquippedBattleItem => // 장착한 아이템 (다 쓰면 없음)
        GetBattleItemCount(equippedBattleItem) > 0 ? equippedBattleItem : null;

    public List<BattleItemData> GetOwnedBattleItems() // 한 개 이상 가진 아이템 (상점 순서)
    {
        List<BattleItemData> ownedItems = new List<BattleItemData>();

        foreach (ShopItemData shopItem in shopItems)
        {
            if (shopItem == null || shopItem.BattleItem == null) { continue; }
            if (shopItem.RewardType != ShopRewardType.BattleItem) { continue; }
            if (GetBattleItemCount(shopItem.BattleItem) <= 0) { continue; }
            if (ownedItems.Contains(shopItem.BattleItem)) { continue; }

            ownedItems.Add(shopItem.BattleItem);
        }

        return ownedItems;
    }

    // 장착 칸을 누를 때마다 없음 → 가진 아이템 순으로 바꾼다. (기획서 9.12.3: 전투 전에 1개 장착)
    public BattleItemData CycleEquippedBattleItem()
    {
        List<BattleItemData> ownedItems = GetOwnedBattleItems();
        int nextIndex = ownedItems.IndexOf(EquippedBattleItem) + 1; // 장착이 없으면 첫 아이템

        equippedBattleItem = nextIndex < ownedItems.Count
            ? ownedItems[nextIndex]
            : null;

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림

        return equippedBattleItem;
    }

    public bool ConsumeBattleItem(BattleItemData item) // 전투에서 사용한 아이템 1개 소모
    {
        int count = GetBattleItemCount(item);

        if (count <= 0) { return false; }

        battleItemCounts[item] = count - 1;

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림
        AutoSave("소모성 아이템 사용"); // 쓴 순간에 기록해 전투를 그만둬도 돌아오지 않는다. (기획서 15.7)

        return true;
    }
}
