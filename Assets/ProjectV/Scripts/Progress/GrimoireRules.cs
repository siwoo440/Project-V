using UnityEngine; // Unity 기본 기능

// 그리모어 영구 강화 규칙 (기획서 6.6 / 9.9.5 / A.46 / A.47)
public static class GrimoireRules
{
    public const int MaxLevel = 3;           // 노드 최대 단계 (기획서 6.6.5)
    public const int NodesPerBranch = 5;     // 분기마다 노드 수 (기획서 9.9.5)
    public const int ShardLimit = 9999;      // 욕망의 파편 보유 한도 (기획서 A.47)
    public const int LegionMonsterCount = 4; // 군단 지휘가 발동하는 필드 마물 수

    private static readonly int[] UpgradeCosts = { 3, 6, 9 }; // 단계별 파편 비용 (기획서 A.46)

    private static readonly GrimoireBranch[] BranchOrder =
    {
        GrimoireBranch.Contract, GrimoireBranch.Summon,
        GrimoireBranch.Command, GrimoireBranch.Mana,
        GrimoireBranch.Memory, GrimoireBranch.Desire,
        GrimoireBranch.Lineage, GrimoireBranch.Capture,
    };

    public static GrimoireBranch[] Branches => BranchOrder; // 화면에 보여줄 분기 순서

    // 현재 단계에서 다음 단계로 올리는 비용. 최대 단계에서는 0.
    public static int GetUpgradeCost(int currentLevel)
    {
        if (currentLevel < 0 || currentLevel >= MaxLevel) { return 0; }

        return UpgradeCosts[currentLevel];
    }

    public static int ClampLevel(int level)
    {
        return Mathf.Clamp(level, 0, MaxLevel);
    }

    public static string GetBranchName(GrimoireBranch branch)
    {
        switch (branch)
        {
            case GrimoireBranch.Contract: return "계약";
            case GrimoireBranch.Summon: return "소환";
            case GrimoireBranch.Command: return "지휘";
            case GrimoireBranch.Mana: return "마나";
            case GrimoireBranch.Memory: return "기억";
            case GrimoireBranch.Desire: return "욕망";
            case GrimoireBranch.Lineage: return "계열";
            case GrimoireBranch.Capture: return "포획";
            default: return "알 수 없음";
        }
    }

    public static string GetBranchSummary(GrimoireBranch branch) // 분기의 강화 방향 (기획서 6.6.4)
    {
        switch (branch)
        {
            case GrimoireBranch.Contract: return "플레이어 HP와 보호막";
            case GrimoireBranch.Summon: return "소환 마물의 HP와 생존력";
            case GrimoireBranch.Command: return "마물 공격과 행동 보조";
            case GrimoireBranch.Mana: return "시작 마나와 임시 마나";
            case GrimoireBranch.Memory: return "드로우와 손패 관리";
            case GrimoireBranch.Desire: return "성욕 부여와 절정 공략";
            case GrimoireBranch.Lineage: return "마물 시너지 효과";
            case GrimoireBranch.Capture: return "중복 포획과 재료 획득";
            default: return string.Empty;
        }
    }

    public static string GetBranchIconKey(GrimoireBranch branch) // 분기 문양 이미지 이름
    {
        switch (branch)
        {
            case GrimoireBranch.Contract: return UIKeys.GrimoireContract;
            case GrimoireBranch.Summon: return UIKeys.GrimoireSummon;
            case GrimoireBranch.Command: return UIKeys.GrimoireCommand;
            case GrimoireBranch.Mana: return UIKeys.GrimoireMana;
            case GrimoireBranch.Memory: return UIKeys.GrimoireMemory;
            case GrimoireBranch.Desire: return UIKeys.GrimoireDesire;
            case GrimoireBranch.Lineage: return UIKeys.GrimoireLineage;
            case GrimoireBranch.Capture: return UIKeys.GrimoireCapture;
            default: return string.Empty;
        }
    }
}
