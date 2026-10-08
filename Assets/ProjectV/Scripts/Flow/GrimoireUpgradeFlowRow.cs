using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class GrimoireUpgradeFlow // 그리모어 강화 화면의 목록 줄 생성
{
    private const float BranchRowHeight = 82f; // 분기 한 줄의 높이
    private const float NodeRowHeight = 112f;  // 노드 한 줄의 높이
    private const float RowInset = 30f;        // 줄 양 끝 장식을 피하는 여백
    private const float BranchIconSize = 54f;  // 분기 문양 크기
    private const float NodeMedalSize = 80f;   // 노드 받침 크기

    private static readonly Color LockedTextColor = UIListRow.LockedTextColor; // 잠긴 항목 글자색

    // 누를 수 있는 목록 한 줄의 바탕을 만든다.
    private GameObject CreateRowBase(
        Transform parent,
        float height,
        bool isHighlighted,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        GameObject rowObject = UIListRow.Create(
            parent, "GrimoireRow", height, isHighlighted, clickAction
        );

        if (rowObject != null)
        {
            generatedRows.Add(rowObject);
        }

        return rowObject;
    }

    // 분기 한 줄: 문양, 이름, 진행도, 강화 방향
    private void CreateBranchRow(
        GrimoireBranch branch,
        bool isSelected,
        int level,
        int maxLevel,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        GameObject rowObject =
            CreateRowBase(branchListContent, BranchRowHeight, isSelected, clickAction);

        if (rowObject == null) { return; }

        Vector2 leftCenter = new Vector2(0f, 0.5f);

        Image icon = CardEntryFactory.CreateImage(
            rowObject.transform, "BranchIcon", Color.white,
            leftCenter, leftCenter,
            new Vector2(RowInset - 4f, -BranchIconSize * 0.5f),
            new Vector2(RowInset - 4f + BranchIconSize, BranchIconSize * 0.5f)
        );

        bool hasIcon = UISkin.ApplySimple(
            icon, GrimoireRules.GetBranchIconKey(branch), true
        );

        icon.enabled = hasIcon; // 문양 이미지가 없으면 글자만 보여준다.

        float textLeft = hasIcon ? RowInset + BranchIconSize + 6f : RowInset;
        bool isComplete = maxLevel > 0 && level >= maxLevel;

        CardEntryFactory.CreateLabel(
            rowObject.transform, "TitleText", GrimoireRules.GetBranchName(branch),
            23f, UISkin.Cream, TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0.5f), new Vector2(0.62f, 1f),
            new Vector2(textLeft, 0f), new Vector2(0f, -8f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "StateText", $"{level} / {maxLevel}",
            18f, isComplete || isSelected ? UISkin.Gold : UISkin.CreamSub,
            TextAlignmentOptions.BottomRight,
            new Vector2(0.62f, 0.5f), new Vector2(1f, 1f),
            Vector2.zero, new Vector2(-RowInset, -8f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "BodyText", GrimoireRules.GetBranchSummary(branch),
            16f, UISkin.CreamSub, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0f), new Vector2(1f, 0.5f),
            new Vector2(textLeft, 8f), new Vector2(-RowInset, -2f)
        );
    }

    // 노드 한 줄: 단계 받침, 이름, 상태, 효과. 잠긴 노드도 효과를 보여준다. (기획서 11.14)
    private void CreateNodeRow(
        PlayerProgressManager progress,
        GrimoireNodeData node,
        bool isSelected,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        GameObject rowObject =
            CreateRowBase(nodeListContent, NodeRowHeight, isSelected, clickAction);

        if (rowObject == null) { return; }

        int level = progress.GetGrimoireLevel(node);
        bool isOpen = progress.IsGrimoireNodeOpen(node);
        bool isMax = level >= GrimoireRules.MaxLevel;
        bool canUpgrade = progress.CanUpgradeGrimoireNode(node, out string _);

        Vector2 leftCenter = new Vector2(0f, 0.5f);
        Vector2 medalMin = new Vector2(RowInset - 6f, -NodeMedalSize * 0.5f);
        Vector2 medalMax = new Vector2(RowInset - 6f + NodeMedalSize, NodeMedalSize * 0.5f);

        Image medal = CardEntryFactory.CreateImage(
            rowObject.transform, "NodeMedal", Color.white,
            leftCenter, leftCenter, medalMin, medalMax
        );

        string medalKey = isMax
            ? UIKeys.NodeDone
            : isOpen ? UIKeys.NodeOpen : UIKeys.NodeLocked;

        medal.enabled = UISkin.ApplySimple(medal, medalKey, true); // 받침 이미지가 없으면 단계 글자만 보인다.

        TextMeshProUGUI levelLabel = CardEntryFactory.CreateLabel(
            rowObject.transform, "LevelText",
            isOpen ? $"Lv.{level}" : UISkin.IconOr(UIIcons.Lock, "잠김"),
            22f,
            isMax ? UISkin.Gold : isOpen ? UISkin.Cream : LockedTextColor,
            TextAlignmentOptions.Center,
            leftCenter, leftCenter, medalMin, medalMax
        );

        levelLabel.fontStyle = FontStyles.Bold;

        string stateText = isMax
            ? "최대 단계"
            : !isOpen
                ? "선행 노드 필요"
                : canUpgrade
                    ? (level > 0 ? "강화 가능" : "해금 가능")
                    : "파편 부족";

        Color stateColor = isMax || canUpgrade
            ? UISkin.Gold
            : isOpen ? UISkin.CreamSub : LockedTextColor;

        float textLeft = RowInset + NodeMedalSize + 4f;

        CardEntryFactory.CreateLabel(
            rowObject.transform, "TitleText", node.DisplayName,
            23f, isOpen ? UISkin.Cream : LockedTextColor,
            TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0.58f), new Vector2(0.64f, 1f),
            new Vector2(textLeft, 0f), new Vector2(0f, -12f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "StateText", stateText,
            17f, stateColor, TextAlignmentOptions.BottomRight,
            new Vector2(0.64f, 0.58f), new Vector2(1f, 1f),
            Vector2.zero, new Vector2(-RowInset, -12f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "BodyText", node.GetEffectText(Mathf.Max(1, level)),
            16f, isOpen ? UISkin.CreamSub : LockedTextColor,
            TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0f), new Vector2(1f, 0.58f),
            new Vector2(textLeft, 10f), new Vector2(-RowInset, -4f)
        );
    }
}
