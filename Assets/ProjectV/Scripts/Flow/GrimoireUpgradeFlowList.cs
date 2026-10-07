using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

public partial class GrimoireUpgradeFlow // 그리모어 강화 화면의 분기 목록과 노드 목록
{
    private void BuildBranchList(PlayerProgressManager progress)
    {
        foreach (GrimoireBranch branch in GrimoireRules.Branches)
        {
            GrimoireBranch targetBranch = branch;

            CreateBranchRow(
                branch,
                branch == selectedBranch,
                progress.GetGrimoireBranchLevel(branch),
                progress.GetGrimoireBranchMaxLevel(branch),
                () => SelectBranch(targetBranch)
            );
        }
    }

    private void BuildNodeList(PlayerProgressManager progress)
    {
        List<GrimoireNodeData> branchNodes = GetBranchNodes(progress);

        if (selectedNode == null || selectedNode.Branch != selectedBranch)
        {
            selectedNode = FindDefaultNode(progress, branchNodes);
        }

        foreach (GrimoireNodeData node in branchNodes)
        {
            GrimoireNodeData targetNode = node;

            CreateNodeRow(
                progress,
                node,
                node == selectedNode,
                () => SelectNode(targetNode)
            );
        }
    }

    private List<GrimoireNodeData> GetBranchNodes(PlayerProgressManager progress) // 선택한 분기의 노드 (순서대로)
    {
        List<GrimoireNodeData> branchNodes = new List<GrimoireNodeData>();

        foreach (GrimoireNodeData node in progress.GrimoireNodes)
        {
            if (node == null || node.Branch != selectedBranch) { continue; }

            branchNodes.Add(node);
        }

        branchNodes.Sort((left, right) => left.Order.CompareTo(right.Order));

        return branchNodes;
    }

    // 분기를 열었을 때 고를 노드: 지금 올릴 수 있는 첫 노드, 없으면 첫 노드.
    private GrimoireNodeData FindDefaultNode(
        PlayerProgressManager progress,
        List<GrimoireNodeData> branchNodes
    )
    {
        foreach (GrimoireNodeData node in branchNodes)
        {
            bool isMax =
                progress.GetGrimoireLevel(node) >= GrimoireRules.MaxLevel;

            if (!isMax && progress.IsGrimoireNodeOpen(node)) { return node; }
        }

        return branchNodes.Count > 0 ? branchNodes[0] : null;
    }

    private void UpdateBranchHeader(PlayerProgressManager progress)
    {
        if (branchIconImage != null)
        {
            branchIconImage.enabled = UISkin.ApplySimple(
                branchIconImage,
                GrimoireRules.GetBranchIconKey(selectedBranch),
                true
            ); // 문양 이미지가 없으면 숨긴다.
        }

        if (branchTitleText != null)
        {
            branchTitleText.text =
                $"{GrimoireRules.GetBranchName(selectedBranch)} 분기   " +
                $"{progress.GetGrimoireBranchLevel(selectedBranch)} / " +
                $"{progress.GetGrimoireBranchMaxLevel(selectedBranch)}";
        }

        if (branchGuideText != null)
        {
            branchGuideText.text =
                $"{GrimoireRules.GetBranchSummary(selectedBranch)} " +
                "(위에서부터 차례로 열립니다)";
        }
    }
}
