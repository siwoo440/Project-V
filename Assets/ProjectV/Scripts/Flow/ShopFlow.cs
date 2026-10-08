using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 상점 화면 연결 (기획서 9.12 / 11.14)
// 위에서 탭을 고르고, 왼쪽에서 상품을 고르고, 오른쪽에서 구매한다.
// 화면의 패널과 버튼은 씬 구성 도구(SceneUIBuilderShop.cs)가 만든다.
public partial class ShopFlow : MonoBehaviour
{
    [Header("화면 이동")]
    [SerializeField] private Button backButton; // 돌아가기

    [Header("탭")]
    [SerializeField] private Button essenceTabButton; // 마물의 정수
    [SerializeField] private Button shardTabButton;   // 욕망의 파편
    [SerializeField] private Button itemTabButton;    // 소모성 아이템

    [Header("상품")]
    [SerializeField] private Transform productListContent; // 상품 줄 배치 영역
    [SerializeField] private Image detailIconImage;        // 선택한 상품 아이콘
    [SerializeField] private TMP_Text detailTitleText;     // 선택한 상품 이름
    [SerializeField] private TMP_Text detailText;          // 효과, 가격, 보유 수량
    [SerializeField] private Button buyButton;             // 구매

    [Header("화면 텍스트")]
    [SerializeField] private TMP_Text resourceText; // 보유 재화
    [SerializeField] private TMP_Text messageText;  // 안내 문구

    private readonly List<GameObject> generatedRows =
        new List<GameObject>(); // 생성한 상품 줄

    private ShopRewardType selectedTab = ShopRewardType.MonsterEssence; // 선택한 탭
    private ShopItemData selectedItem; // 선택한 상품
    private ShopItemData confirmItem;  // 구매 확인을 기다리는 상품

    private void Awake()
    {
        backButton = SceneUIBinder.Bind(backButton, "BackButton");

        essenceTabButton =
            SceneUIBinder.Bind(essenceTabButton, "EssenceTabButton");

        shardTabButton =
            SceneUIBinder.Bind(shardTabButton, "ShardTabButton");

        itemTabButton =
            SceneUIBinder.Bind(itemTabButton, "ItemTabButton");

        productListContent =
            SceneUIBinder.Bind(productListContent, "ProductListContent");

        detailIconImage =
            SceneUIBinder.Bind(detailIconImage, "DetailIconImage");

        detailTitleText =
            SceneUIBinder.Bind(detailTitleText, "DetailTitleText");

        detailText = SceneUIBinder.Bind(detailText, "DetailText");
        buyButton = SceneUIBinder.Bind(buyButton, "BuyButton");
        resourceText = SceneUIBinder.Bind(resourceText, "ResourceText");
        messageText = SceneUIBinder.Bind(messageText, "MessageText");
    }

    private void Start()
    {
        AddClickListener(backButton, SceneFlow.ReturnToPreviousScene);
        AddClickListener(buyButton, OnBuyButton);

        AddClickListener(
            essenceTabButton, () => SelectTab(ShopRewardType.MonsterEssence));

        AddClickListener(
            shardTabButton, () => SelectTab(ShopRewardType.DesireShard));

        AddClickListener(
            itemTabButton, () => SelectTab(ShopRewardType.BattleItem));

        ShowMessage("탭을 고르고 구매할 상품을 선택하세요.");
        Refresh();
    }

    private void AddClickListener(
        Button targetButton,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        if (targetButton == null) { return; }

        targetButton.onClick.RemoveAllListeners();
        targetButton.onClick.AddListener(clickAction);
    }

    // ---------- 조작 ----------

    private void SelectTab(ShopRewardType tab) // 탭 선택
    {
        selectedTab = tab;
        selectedItem = null; // 탭을 바꾸면 그 탭의 첫 상품을 고른다.
        confirmItem = null;
        Refresh();
    }

    private void SelectItem(ShopItemData item) // 상품 선택
    {
        selectedItem = item;
        confirmItem = null;
        Refresh();
    }

    // 욕망의 파편과 묶음 상품은 한 번 더 눌러야 구매한다. 나머지는 바로 구매한다. (기획서 11.14)
    private void OnBuyButton()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        if (!progress.CanBuyShopItem(selectedItem, out string reason))
        {
            confirmItem = null;
            ShowMessage(reason);
            Refresh();
            return;
        }

        if (selectedItem.NeedsConfirm && confirmItem != selectedItem)
        {
            confirmItem = selectedItem;

            ShowMessage(
                $"{selectedItem.DisplayName}: 한 번 더 누르면 구매합니다. " +
                $"(골드 {progress.GetShopPrice(selectedItem)})"
            );

            Refresh();
            return;
        }

        confirmItem = null;

        progress.TryBuyShopItem(selectedItem, out string message);

        ShowMessage(message);
        Refresh();
    }

    // ---------- 화면 갱신 ----------

    public void Refresh()
    {
        ClearRows();

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null)
        {
            ShowMessage("진행 데이터가 없습니다");
            return;
        }

        if (resourceText != null)
        {
            string discountText = progress.ShopDiscountPercent > 0
                ? $"      할인 {progress.ShopDiscountPercent}%"
                : string.Empty;

            resourceText.text =
                $"{UISkin.IconOr(UIIcons.Gold, "골드")} {progress.Gold}    " +
                $"{UISkin.IconOr(UIIcons.Essence, "정수")} {progress.MonsterEssence}    " +
                $"{UISkin.IconOr(UIIcons.Shard, "파편")} {progress.DesireShards}" +
                discountText;
        }

        UpdateTabs();
        BuildProductList(progress);
        UpdateDetail(progress);
    }

    private void ShowMessage(string message)
    {
        if (messageText == null) { return; }

        messageText.text = message;
    }

    private void ClearRows()
    {
        foreach (GameObject generatedRow in generatedRows)
        {
            if (generatedRow == null) { continue; }

            Destroy(generatedRow);
        }

        generatedRows.Clear();
    }
}
