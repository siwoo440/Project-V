using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class ShopFlow // 상점 화면의 탭과 상품 목록
{
    private const float ProductRowHeight = 104f; // 상품 한 줄의 높이
    private const float RowInset = 30f;          // 줄 양 끝 장식을 피하는 여백
    private const float ProductIconSize = 72f;   // 상품 아이콘 크기

    private static readonly Color TabColor = new Color(0.20f, 0.16f, 0.31f, 1f);         // 탭 이미지가 없을 때 기본 색
    private static readonly Color TabSelectedColor = new Color(0.42f, 0.34f, 0.16f, 1f); // 탭 이미지가 없을 때 강조 색

    private void UpdateTabs() // 선택한 탭을 금색 버튼으로 표시
    {
        ApplyTabState(essenceTabButton, ShopRewardType.MonsterEssence);
        ApplyTabState(shardTabButton, ShopRewardType.DesireShard);
        ApplyTabState(itemTabButton, ShopRewardType.BattleItem);
    }

    private void ApplyTabState(Button tabButton, ShopRewardType tab)
    {
        if (tabButton == null) { return; }

        Image tabImage = tabButton.GetComponent<Image>();

        if (tabImage == null) { return; }

        bool isSelected = tab == selectedTab;

        bool hasSkin = UISkin.ApplySelectable(
            tabImage, isSelected,
            UIKeys.ButtonBlue, UIKeys.ButtonGold,
            TabColor, TabSelectedColor
        );

        TMP_Text tabLabel = tabButton.GetComponentInChildren<TMP_Text>(true);

        if (hasSkin && tabLabel != null)
        {
            tabLabel.color = isSelected ? UISkin.ButtonInk : UISkin.Cream; // 금색 버튼 위에는 어두운 글자
        }
    }

    private void BuildProductList(PlayerProgressManager progress)
    {
        List<ShopItemData> tabItems = GetTabItems(progress);

        if (selectedItem == null || selectedItem.RewardType != selectedTab)
        {
            selectedItem = tabItems.Count > 0 ? tabItems[0] : null;
        }

        foreach (ShopItemData item in tabItems)
        {
            ShopItemData targetItem = item;

            CreateProductRow(
                progress,
                item,
                item == selectedItem,
                () => SelectItem(targetItem)
            );
        }
    }

    private List<ShopItemData> GetTabItems(PlayerProgressManager progress) // 선택한 탭의 상품 (순서대로)
    {
        List<ShopItemData> tabItems = new List<ShopItemData>();

        foreach (ShopItemData item in progress.ShopItems)
        {
            if (item == null || item.RewardType != selectedTab) { continue; }

            tabItems.Add(item);
        }

        tabItems.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));

        return tabItems;
    }

    // 상품 한 줄: 아이콘, 이름, 효과, 가격, 보유 수량 또는 해금 조건 (기획서 11.14)
    private void CreateProductRow(
        PlayerProgressManager progress,
        ShopItemData item,
        bool isSelected,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        GameObject rowObject = UIListRow.Create(
            productListContent, "ProductRow", ProductRowHeight, isSelected, clickAction
        );

        if (rowObject == null) { return; }

        generatedRows.Add(rowObject);

        bool isUnlocked = progress.IsShopItemUnlocked(item);
        Vector2 leftCenter = new Vector2(0f, 0.5f);

        Image icon = CardEntryFactory.CreateImage(
            rowObject.transform, "ProductIcon", Color.white,
            leftCenter, leftCenter,
            new Vector2(RowInset - 4f, -ProductIconSize * 0.5f),
            new Vector2(RowInset - 4f + ProductIconSize, ProductIconSize * 0.5f)
        );

        bool hasIcon = UISkin.ApplySimple(icon, ShopRules.GetIconKey(item), true);

        icon.enabled = hasIcon; // 아이콘 이미지가 없으면 글자만 보여준다.

        if (hasIcon && !isUnlocked)
        {
            icon.color = new Color(0.55f, 0.55f, 0.6f, 1f); // 잠긴 상품은 어둡게
        }

        float textLeft = hasIcon ? RowInset + ProductIconSize + 8f : RowInset;
        Color titleColor = isUnlocked ? UISkin.Cream : UIListRow.LockedTextColor;
        Color bodyColor = isUnlocked ? UISkin.CreamSub : UIListRow.LockedTextColor;

        string stateText = isUnlocked
            ? $"보유 {progress.GetShopOwnedCount(item)}"
            : $"{UISkin.IconOr(UIIcons.Lock, "잠김")} Lv.{item.UnlockLevel}";

        CardEntryFactory.CreateLabel(
            rowObject.transform, "TitleText", item.DisplayName,
            23f, titleColor, TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0.55f), new Vector2(0.62f, 1f),
            new Vector2(textLeft, 0f), new Vector2(0f, -10f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "PriceText",
            $"{UISkin.IconOr(UIIcons.Gold, "골드")} {progress.GetShopPrice(item)}",
            22f, isUnlocked ? UISkin.Gold : UIListRow.LockedTextColor,
            TextAlignmentOptions.BottomRight,
            new Vector2(0.62f, 0.55f), new Vector2(1f, 1f),
            Vector2.zero, new Vector2(-RowInset, -10f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "BodyText", item.EffectText,
            16f, bodyColor, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0f), new Vector2(0.76f, 0.55f),
            new Vector2(textLeft, 10f), new Vector2(0f, -4f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "StateText", stateText,
            17f, bodyColor, TextAlignmentOptions.TopRight,
            new Vector2(0.76f, 0f), new Vector2(1f, 0.55f),
            new Vector2(0f, 10f), new Vector2(-RowInset, -4f)
        );
    }
}
