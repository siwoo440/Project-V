using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 그리모어 강화 화면 연결 (기획서 6.6 / 11.14)
// 왼쪽에서 분기를 고르고, 가운데에서 노드를 고르고, 오른쪽에서 강화한다.
// 화면의 패널과 버튼은 씬 구성 도구(SceneUIBuilderGrimoire.cs)가 만든다.
public partial class GrimoireUpgradeFlow : MonoBehaviour
{
    [Header("화면 이동")]
    [SerializeField] private Button backButton; // 돌아가기

    [Header("목록")]
    [SerializeField] private Transform branchListContent; // 분기 줄 배치 영역
    [SerializeField] private Transform nodeListContent;   // 노드 줄 배치 영역

    [Header("분기 정보")]
    [SerializeField] private Image branchIconImage;    // 선택한 분기 문양
    [SerializeField] private TMP_Text branchTitleText; // 선택한 분기 이름과 진행도
    [SerializeField] private TMP_Text branchGuideText; // 선택한 분기의 강화 방향

    [Header("노드 정보")]
    [SerializeField] private TMP_Text detailTitleText; // 선택한 노드 이름
    [SerializeField] private TMP_Text detailText;      // 단계별 효과와 비용
    [SerializeField] private Button upgradeButton;     // 해금 또는 강화

    [Header("화면 텍스트")]
    [SerializeField] private TMP_Text shardText;   // 보유 파편과 전체 진행도
    [SerializeField] private TMP_Text messageText; // 안내 문구

    private readonly List<GameObject> generatedRows =
        new List<GameObject>(); // 생성한 목록 줄

    private GrimoireBranch selectedBranch = GrimoireBranch.Contract; // 선택한 분기
    private GrimoireNodeData selectedNode; // 선택한 노드
    private GrimoireNodeData confirmNode;  // 강화 확인을 기다리는 노드

    private void Awake()
    {
        backButton = SceneUIBinder.Bind(backButton, "BackButton");

        branchListContent =
            SceneUIBinder.Bind(branchListContent, "BranchListContent");

        nodeListContent =
            SceneUIBinder.Bind(nodeListContent, "NodeListContent");

        branchIconImage =
            SceneUIBinder.Bind(branchIconImage, "BranchIconImage");

        branchTitleText =
            SceneUIBinder.Bind(branchTitleText, "BranchTitleText");

        branchGuideText =
            SceneUIBinder.Bind(branchGuideText, "BranchGuideText");

        detailTitleText =
            SceneUIBinder.Bind(detailTitleText, "DetailTitleText");

        detailText = SceneUIBinder.Bind(detailText, "DetailText");

        upgradeButton =
            SceneUIBinder.Bind(upgradeButton, "UpgradeButton");

        shardText = SceneUIBinder.Bind(shardText, "ShardText");
        messageText = SceneUIBinder.Bind(messageText, "MessageText");
    }

    private void Start()
    {
        AddClickListener(backButton, SceneFlow.ReturnToPreviousScene);
        AddClickListener(upgradeButton, OnUpgradeButton);

        ShowMessage("분기를 고르고 강화할 노드를 선택하세요.");
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

    private void SelectBranch(GrimoireBranch branch) // 분기 선택
    {
        selectedBranch = branch;
        selectedNode = null; // 분기를 바꾸면 그 분기의 기본 노드를 고른다.
        confirmNode = null;
        Refresh();
    }

    private void SelectNode(GrimoireNodeData node) // 노드 선택
    {
        selectedNode = node;
        confirmNode = null;
        Refresh();
    }

    // 강화는 되돌릴 수 없으므로(기획서 6.6.7) 한 번 더 눌러야 실행한다.
    private void OnUpgradeButton()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        if (!progress.CanUpgradeGrimoireNode(selectedNode, out string reason))
        {
            confirmNode = null;
            ShowMessage(reason);
            Refresh();
            return;
        }

        if (confirmNode != selectedNode)
        {
            int cost = GrimoireRules.GetUpgradeCost(
                progress.GetGrimoireLevel(selectedNode)
            );

            confirmNode = selectedNode;

            ShowMessage(
                $"{selectedNode.DisplayName}: 한 번 더 누르면 욕망의 파편 {cost}개를 사용합니다. " +
                "되돌릴 수 없습니다."
            );

            Refresh();
            return;
        }

        confirmNode = null;

        progress.TryUpgradeGrimoireNode(selectedNode, out string message);

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

        if (shardText != null)
        {
            shardText.text =
                $"{UISkin.IconOr(UIIcons.Shard, "욕망의 파편")} {progress.DesireShards}" +
                $"      강화 {progress.TotalGrimoireLevel} / {progress.MaxTotalGrimoireLevel}";
        }

        BuildBranchList(progress);
        BuildNodeList(progress);
        UpdateBranchHeader(progress);
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
