using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 그리모어 강화 화면과 노드 데이터 연결 (기획서 6.6 / 11.14)
public static partial class SceneUIBuilder
{
    // 그리모어 강화 화면: 왼쪽은 분기, 가운데는 노드, 오른쪽은 선택한 노드의 상세.
    private static void BuildGrimoireScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();

        EnsureBackground(
            canvas.transform,
            UISkin.Has(UIKeys.BgGrimoire) ? UIKeys.BgGrimoire : UIKeys.BgDeckBuilder
        ); // 전용 배경이 없으면 서재 배경을 쓴다.

        EnsureTopBar(canvas.transform, 72f, 104f);
        EnsureBottomBar(canvas.transform, 92f);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "그리모어 강화",
            48f, AccentColor, TextAlignmentOptions.Left);

        PlaceSceneTitle(title, new Vector2(300f, -72f), new Vector2(480f, 60f), 48f);

        TextMeshProUGUI shardText = EnsureText(
            "ShardText", canvas.transform, "욕망의 파편 0      강화 0 / 120",
            26f, AccentColor, TextAlignmentOptions.Right);

        SetAnchored(shardText.gameObject,
            new Vector2(1f, 1f), new Vector2(-400f, -72f), new Vector2(700f, 60f));

        // 왼쪽: 분기 목록
        GameObject branchPanel = EnsurePanel("BranchPanel", canvas.transform, PanelDeepColor);
        SetAnchored(branchPanel, new Vector2(0f, 0.5f), new Vector2(250f, -30f), new Vector2(440f, 790f));

        GameObject branchContent = EnsureVerticalScrollList(
            "BranchListView", "BranchListContent", branchPanel.transform,
            new Vector4(22f, 24f, 22f, 24f));

        // 가운데: 선택한 분기의 노드
        GameObject nodePanel = EnsurePanel("NodePanel", canvas.transform, PanelColor);
        SetAnchored(nodePanel, new Vector2(0f, 0.5f), new Vector2(830f, -30f), new Vector2(680f, 790f));

        Image branchIcon = EnsureIcon(
            "BranchIconImage", nodePanel.transform, UIKeys.GrimoireContract,
            new Vector2(0f, 1f), new Vector2(68f, -62f), 70f);

        TextMeshProUGUI branchTitle = EnsureText(
            "BranchTitleText", nodePanel.transform, "계약 분기   0 / 15",
            30f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(branchTitle.gameObject,
            new Vector2(0f, 1f), new Vector2(378f, -44f), new Vector2(530f, 40f));

        TextMeshProUGUI branchGuide = EnsureText(
            "BranchGuideText", nodePanel.transform, "",
            17f, SubTextColor, TextAlignmentOptions.Left);

        SetAnchored(branchGuide.gameObject,
            new Vector2(0f, 1f), new Vector2(378f, -84f), new Vector2(530f, 30f));

        GameObject nodeContent = EnsureVerticalScrollList(
            "NodeListView", "NodeListContent", nodePanel.transform,
            new Vector4(24f, 118f, 24f, 24f));

        // 오른쪽: 선택한 노드의 단계별 효과와 강화
        GameObject detailPanel = EnsurePanel("DetailPanel", canvas.transform, PanelDeepColor);
        SetAnchored(detailPanel, new Vector2(1f, 0.5f), new Vector2(-380f, -30f), new Vector2(700f, 790f));

        TextMeshProUGUI detailTitle = EnsureText(
            "DetailTitleText", detailPanel.transform, "노드를 선택하세요",
            34f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(detailTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(610f, 50f));

        TextMeshProUGUI detailText = EnsureText(
            "DetailText", detailPanel.transform, "",
            20f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(detailText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -332f), new Vector2(610f, 480f));

        Button upgradeButton = EnsureButton(
            "UpgradeButton", detailPanel.transform, "강화", AccentColor);

        SetAnchored(upgradeButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(0f, 74f), new Vector2(380f, 68f));

        StyleButtonByName("UpgradeButton", AccentColor, 24f);

        // 하단 안내와 돌아가기
        TextMeshProUGUI messageText = EnsureText(
            "MessageText", canvas.transform, "분기를 고르고 강화할 노드를 선택하세요.",
            22f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(messageText.gameObject,
            new Vector2(0f, 0f), new Vector2(660f, 56f), new Vector2(1220f, 40f));

        Button backButton = EnsureButton(
            "BackButton", canvas.transform, "돌아가기", ButtonColor);

        SetAnchored(backButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-160f, 56f), new Vector2(200f, 60f));

        StyleButtonByName("BackButton", ButtonColor, 22f);

        GameObject controller = EnsureObject("GrimoireController", null);
        GrimoireUpgradeFlow flow = controller.AddComponentIfMissing<GrimoireUpgradeFlow>();

        AssignReference(flow, "backButton", backButton);
        AssignReference(flow, "branchListContent", branchContent.transform);
        AssignReference(flow, "nodeListContent", nodeContent.transform);
        AssignReference(flow, "branchIconImage", branchIcon);
        AssignReference(flow, "branchTitleText", branchTitle);
        AssignReference(flow, "branchGuideText", branchGuide);
        AssignReference(flow, "detailTitleText", detailTitle);
        AssignReference(flow, "detailText", detailText);
        AssignReference(flow, "upgradeButton", upgradeButton);
        AssignReference(flow, "shardText", shardText);
        AssignReference(flow, "messageText", messageText);
    }

    // 프로젝트의 그리모어 노드 데이터를 분기와 순서대로 진행 데이터에 연결한다.
    private static void ApplyGrimoireNodeList(PlayerProgressManager progress)
    {
        if (progress == null) { return; }

        List<GrimoireNodeData> nodes = LoadAssets<GrimoireNodeData>();

        nodes.Sort((left, right) =>
            left.Branch != right.Branch
                ? left.Branch.CompareTo(right.Branch)
                : left.Order != right.Order
                    ? left.Order.CompareTo(right.Order)
                    : string.CompareOrdinal(left.NodeId, right.NodeId));

        SerializedObject serializedProgress = new SerializedObject(progress);

        AssignList(serializedProgress, "grimoireNodes", nodes);

        serializedProgress.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"그리모어 노드 {nodes.Count}개를 연결했습니다.");
    }
}
