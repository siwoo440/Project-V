using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 상점 화면, 전투 화면의 소모성 아이템 버튼, 상품 데이터 연결 (기획서 9.12 / 11.14)
public static partial class SceneUIBuilder
{
    // 상점 화면: 왼쪽은 탭과 상품 목록, 오른쪽은 선택한 상품의 상세와 구매.
    private static void BuildShopScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();

        EnsureBackground(
            canvas.transform,
            UISkin.Has(UIKeys.BgShop) ? UIKeys.BgShop : UIKeys.BgDeckBuilder
        ); // 전용 배경이 없으면 서재 배경을 쓴다.

        EnsureTopBar(canvas.transform, 72f, 104f);
        EnsureBottomBar(canvas.transform, 92f);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "상점",
            48f, AccentColor, TextAlignmentOptions.Left);

        PlaceSceneTitle(title, new Vector2(260f, -72f), new Vector2(400f, 60f), 48f);

        TextMeshProUGUI resourceText = EnsureText(
            "ResourceText", canvas.transform, "골드 0    정수 0    파편 0",
            26f, AccentColor, TextAlignmentOptions.Right);

        SetAnchored(resourceText.gameObject,
            new Vector2(1f, 1f), new Vector2(-420f, -72f), new Vector2(760f, 60f));

        // 왼쪽: 탭과 상품 목록
        GameObject productPanel = EnsurePanel("ProductPanel", canvas.transform, PanelColor);
        SetAnchored(productPanel, new Vector2(0f, 0.5f), new Vector2(590f, -30f), new Vector2(1120f, 790f));

        Button essenceTab = EnsureShopTab(
            "EssenceTabButton", productPanel.transform, "마물의 정수", 200f, UIKeys.StatEssence);

        Button shardTab = EnsureShopTab(
            "ShardTabButton", productPanel.transform, "욕망의 파편", 560f, UIKeys.StatShard);

        Button itemTab = EnsureShopTab(
            "ItemTabButton", productPanel.transform, "소모성 아이템", 920f, UIKeys.ItemBag);

        GameObject productContent = EnsureVerticalScrollList(
            "ProductListView", "ProductListContent", productPanel.transform,
            new Vector4(24f, 104f, 24f, 24f));

        // 오른쪽: 선택한 상품의 효과, 가격, 보유 수량
        GameObject detailPanel = EnsurePanel("DetailPanel", canvas.transform, PanelDeepColor);
        SetAnchored(detailPanel, new Vector2(1f, 0.5f), new Vector2(-385f, -30f), new Vector2(710f, 790f));

        Image detailIcon = EnsureIcon(
            "DetailIconImage", detailPanel.transform, UIKeys.StatEssence,
            new Vector2(0.5f, 1f), new Vector2(0f, -124f), 150f);

        TextMeshProUGUI detailTitle = EnsureText(
            "DetailTitleText", detailPanel.transform, "상품을 선택하세요",
            34f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(detailTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -236f), new Vector2(620f, 50f));

        TextMeshProUGUI detailText = EnsureText(
            "DetailText", detailPanel.transform, "",
            21f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(detailText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -436f), new Vector2(620f, 320f));

        Button buyButton = EnsureButton(
            "BuyButton", detailPanel.transform, "구매", AccentColor);

        SetAnchored(buyButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(0f, 74f), new Vector2(400f, 68f));

        StyleButtonByName("BuyButton", AccentColor, 24f);

        // 하단 안내와 돌아가기
        TextMeshProUGUI messageText = EnsureText(
            "MessageText", canvas.transform, "탭을 고르고 구매할 상품을 선택하세요.",
            22f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(messageText.gameObject,
            new Vector2(0f, 0f), new Vector2(660f, 56f), new Vector2(1220f, 40f));

        Button backButton = EnsureButton(
            "BackButton", canvas.transform, "돌아가기", ButtonColor);

        SetAnchored(backButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-160f, 56f), new Vector2(200f, 60f));

        StyleButtonByName("BackButton", ButtonColor, 22f);

        GameObject controller = EnsureObject("ShopController", null);
        ShopFlow flow = controller.AddComponentIfMissing<ShopFlow>();

        AssignReference(flow, "backButton", backButton);
        AssignReference(flow, "essenceTabButton", essenceTab);
        AssignReference(flow, "shardTabButton", shardTab);
        AssignReference(flow, "itemTabButton", itemTab);
        AssignReference(flow, "productListContent", productContent.transform);
        AssignReference(flow, "detailIconImage", detailIcon);
        AssignReference(flow, "detailTitleText", detailTitle);
        AssignReference(flow, "detailText", detailText);
        AssignReference(flow, "buyButton", buyButton);
        AssignReference(flow, "resourceText", resourceText);
        AssignReference(flow, "messageText", messageText);
    }

    // 상품 목록 위의 탭 버튼 하나를 만든다.
    private static Button EnsureShopTab(
        string objectName,
        Transform parent,
        string label,
        float centerX,
        string iconKey)
    {
        Button tabButton = EnsureButton(objectName, parent, label, ButtonColor);

        SetAnchored(tabButton.gameObject,
            new Vector2(0f, 1f), new Vector2(centerX, -58f), new Vector2(340f, 58f));

        StyleButtonByName(objectName, ButtonColor, 22f);
        EnsureButtonIcon(tabButton, iconKey);

        return tabButton;
    }

    // 전투 화면의 소모성 아이템 버튼. 왼쪽 아래 전투 로그 버튼 위에 둔다. (기획서 9.12.3)
    private static void BuildBattleItemButton(Canvas canvas, BattleManager battleManager)
    {
        Button itemButton = EnsureButton(
            "BattleItemButton", canvas.transform, "소모품 없음", ButtonColor);

        SetAnchored(itemButton.gameObject,
            new Vector2(0f, 0f), new Vector2(110f, 278f), new Vector2(180f, 56f));

        StyleButtonByName("BattleItemButton", ButtonColor, 18f);

        AssignReference(battleManager, "battleItemButton", itemButton);

        // 전투 씬만 따로 실행해도 아이템을 쓸 수 있도록 이 씬의 진행 데이터에도 상품 목록을 넣는다.
        GameObject progressObject = FindInScene("PlayerProgressManager");

        if (progressObject != null)
        {
            ApplyShopItemList(progressObject.GetComponent<PlayerProgressManager>());
            ApplyCardCatalog(progressObject.GetComponent<PlayerProgressManager>()); // 진입 씬과 같은 목록을 넣어 둔다.
            ApplyRegionList(progressObject.GetComponent<PlayerProgressManager>());
        }
    }

    // 프로젝트의 상점 상품 데이터를 탭과 순서대로 진행 데이터에 연결한다.
    private static void ApplyShopItemList(PlayerProgressManager progress)
    {
        if (progress == null) { return; }

        List<ShopItemData> items = LoadAssets<ShopItemData>();

        items.Sort((left, right) =>
            left.RewardType != right.RewardType
                ? left.RewardType.CompareTo(right.RewardType)
                : left.SortOrder != right.SortOrder
                    ? left.SortOrder.CompareTo(right.SortOrder)
                    : string.CompareOrdinal(left.ItemId, right.ItemId));

        SerializedObject serializedProgress = new SerializedObject(progress);

        AssignList(serializedProgress, "shopItems", items);

        serializedProgress.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"상점 상품 {items.Count}종을 연결했습니다.");
    }
}
