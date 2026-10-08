using System.Text; // 문자열 조립 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class ShopFlow // 상점 화면의 상품 상세와 구매 버튼
{
    private static readonly Color WarningTextColor = new Color(1f, 0.58f, 0.52f, 1f); // 구매할 수 없는 사유

    private void UpdateDetail(PlayerProgressManager progress)
    {
        if (detailIconImage != null)
        {
            detailIconImage.enabled =
                selectedItem != null &&
                UISkin.ApplySimple(detailIconImage, ShopRules.GetIconKey(selectedItem), true);
        }

        if (selectedItem == null)
        {
            SetText(detailTitleText, "상품이 없습니다");
            SetText(detailText, string.Empty);
            SetBuyButton("구매", false);
            return;
        }

        int price = progress.GetShopPrice(selectedItem);
        bool isUnlocked = progress.IsShopItemUnlocked(selectedItem);
        bool canBuy = progress.CanBuyShopItem(selectedItem, out string reason);
        string warningHex = ColorUtility.ToHtmlStringRGB(WarningTextColor);

        StringBuilder builder = new StringBuilder();

        builder.Append($"{selectedItem.EffectText}\n\n");

        builder.Append($"가격  {UISkin.IconOr(UIIcons.Gold, "골드")} {price}");

        if (price < selectedItem.Price)
        {
            builder.Append($"  (정가 {selectedItem.Price}, 할인 {progress.ShopDiscountPercent}%)");
        }

        builder.Append($"\n보유  {progress.GetShopOwnedCount(selectedItem)}\n");

        if (selectedItem.RewardType == ShopRewardType.BattleItem)
        {
            builder.Append("\n전투에 가져갈 아이템은 지역 화면에서 고릅니다.\n");
        }

        if (!canBuy)
        {
            builder.Append($"\n<color=#{warningHex}>{reason}</color>");
        }

        SetText(detailTitleText, selectedItem.DisplayName);
        SetText(detailText, builder.ToString());

        string buttonLabel = !isUnlocked
            ? "잠김"
            : confirmItem == selectedItem
                ? "한 번 더 눌러 확정"
                : $"구매 (골드 {price})";

        SetBuyButton(buttonLabel, canBuy);
    }

    private void SetText(TMP_Text targetText, string content)
    {
        if (targetText == null) { return; }

        targetText.text = content;
    }

    private void SetBuyButton(string label, bool isInteractable)
    {
        if (buyButton == null) { return; }

        buyButton.interactable = isInteractable;

        TMP_Text buttonLabel = buyButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel != null)
        {
            buttonLabel.text = label;
        }
    }
}
