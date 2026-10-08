using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

public partial class PlayerProgressManager // 그리모어 영구 강화 (기획서 6.6)
{
    [Header("그리모어")]
    [SerializeField, Min(0)]
    private int startingDesireShards = 30; // 시험용 시작 욕망의 파편 (시험값을 켰을 때만 쓴다)

    [SerializeField]
    private List<GrimoireNodeData> grimoireNodes =
        new List<GrimoireNodeData>(); // 강화 노드 목록 (분기, 순서 순)

    private readonly Dictionary<GrimoireNodeData, int> grimoireLevels =
        new Dictionary<GrimoireNodeData, int>(); // 노드별 현재 단계 (없으면 0)

    private int desireShards; // 욕망의 파편

    public int DesireShards => desireShards; // 보유한 욕망의 파편

    public IReadOnlyList<GrimoireNodeData> GrimoireNodes =>
        grimoireNodes; // 강화 노드 목록 반환

    public int TotalGrimoireLevel // 모든 노드에 올린 단계의 합
    {
        get
        {
            int totalLevel = 0;

            foreach (GrimoireNodeData node in grimoireNodes)
            {
                totalLevel += GetGrimoireLevel(node);
            }

            return totalLevel;
        }
    }

    public int MaxTotalGrimoireLevel // 모든 노드를 최대로 올렸을 때의 단계 합
    {
        get
        {
            int nodeCount = 0;

            foreach (GrimoireNodeData node in grimoireNodes)
            {
                if (node != null) { nodeCount += 1; }
            }

            return nodeCount * GrimoireRules.MaxLevel;
        }
    }

    private void InitializeGrimoireProgress() // 시작 진행 데이터 구성
    {
        desireShards = Mathf.Clamp(
            useTestStartingResources ? startingDesireShards : CurrencyRules.StartingShards,
            0, GrimoireRules.ShardLimit
        ); // 시험값을 끄면 기획서 9.2의 시작 파편 3개

        grimoireLevels.Clear();
    }

    // 욕망의 파편을 더하고 실제로 반영된 양을 반환한다. 한도를 넘는 양은 버린다. (기획서 A.47)
    public int AddDesireShards(int amount)
    {
        if (amount <= 0) { return 0; }

        int shardsBefore = desireShards;

        desireShards = Mathf.Min(
            GrimoireRules.ShardLimit, desireShards + amount
        );

        return desireShards - shardsBefore;
    }

    public int GetGrimoireLevel(GrimoireNodeData node) // 노드 현재 단계 (0이면 미해금)
    {
        if (node == null) { return 0; }

        return grimoireLevels.TryGetValue(node, out int level) ? level : 0;
    }

    // 같은 분기에서 순서가 바로 앞인 노드. 첫 노드는 선행 노드가 없다.
    public GrimoireNodeData GetGrimoirePrerequisite(GrimoireNodeData node)
    {
        if (node == null) { return null; }

        GrimoireNodeData prerequisite = null;

        foreach (GrimoireNodeData other in grimoireNodes)
        {
            if (other == null || other == node) { continue; }
            if (other.Branch != node.Branch) { continue; }
            if (other.Order >= node.Order) { continue; }

            if (prerequisite == null || other.Order > prerequisite.Order)
            {
                prerequisite = other;
            }
        }

        return prerequisite;
    }

    public bool IsGrimoireNodeOpen(GrimoireNodeData node) // 선행 노드를 찍었는지 여부
    {
        GrimoireNodeData prerequisite = GetGrimoirePrerequisite(node);

        return prerequisite == null || GetGrimoireLevel(prerequisite) > 0;
    }

    public bool CanUpgradeGrimoireNode( // 해금 또는 강화 가능 여부와 사유
        GrimoireNodeData node,
        out string reason
    )
    {
        if (node == null)
        {
            reason = "강화할 노드를 선택하세요.";
            return false;
        }

        int level = GetGrimoireLevel(node);

        if (level >= GrimoireRules.MaxLevel)
        {
            reason = "이미 최대 단계입니다.";
            return false;
        }

        GrimoireNodeData prerequisite = GetGrimoirePrerequisite(node);

        if (prerequisite != null && GetGrimoireLevel(prerequisite) <= 0)
        {
            reason = $"선행 노드가 필요합니다: {prerequisite.DisplayName}";
            return false;
        }

        int cost = GrimoireRules.GetUpgradeCost(level);

        if (desireShards < cost)
        {
            reason = $"욕망의 파편이 부족합니다. ({desireShards} / {cost})";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public bool TryUpgradeGrimoireNode( // 파편을 써서 한 단계 올린다. 되돌릴 수 없다. (기획서 6.6.7)
        GrimoireNodeData node,
        out string message
    )
    {
        if (!CanUpgradeGrimoireNode(node, out message)) { return false; }

        int levelBefore = GetGrimoireLevel(node);
        int cost = GrimoireRules.GetUpgradeCost(levelBefore);

        desireShards -= cost;
        grimoireLevels[node] = levelBefore + 1;

        message = levelBefore <= 0
            ? $"{node.DisplayName} 해금 (욕망의 파편 -{cost})"
            : $"{node.DisplayName} Lv.{levelBefore} → Lv.{levelBefore + 1} (욕망의 파편 -{cost})";

        Debug.Log(message); // 그리모어 강화 기록

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림

        return true;
    }

    // 지정한 효과를 가진 노드들의 현재 단계 수치를 합한다. 찍지 않았으면 0.
    public int GetGrimoireAmount(GrimoireEffectType effectType)
    {
        int totalAmount = 0;

        foreach (GrimoireNodeData node in grimoireNodes)
        {
            if (node == null || node.EffectType != effectType) { continue; }

            totalAmount += node.GetAmount(GetGrimoireLevel(node));
        }

        return totalAmount;
    }

    public int GetGrimoireBranchLevel(GrimoireBranch branch) // 분기에 올린 단계의 합
    {
        int branchLevel = 0;

        foreach (GrimoireNodeData node in grimoireNodes)
        {
            if (node == null || node.Branch != branch) { continue; }

            branchLevel += GetGrimoireLevel(node);
        }

        return branchLevel;
    }

    public int GetGrimoireBranchMaxLevel(GrimoireBranch branch) // 분기를 모두 올렸을 때의 단계 합
    {
        int nodeCount = 0;

        foreach (GrimoireNodeData node in grimoireNodes)
        {
            if (node != null && node.Branch == branch) { nodeCount += 1; }
        }

        return nodeCount * GrimoireRules.MaxLevel;
    }
}
