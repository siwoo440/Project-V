using System.Text; // 문자열 조립 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class GrimoireUpgradeFlow // 그리모어 강화 화면의 노드 상세와 강화 버튼
{
    private static readonly Color WarningTextColor = new Color(1f, 0.58f, 0.52f, 1f); // 강화할 수 없는 사유

    // 선택한 노드의 단계별 효과, 비용, 선행 노드를 보여준다. (기획서 11.14: 현재 단계와 다음 단계 비교)
    private void UpdateDetail(PlayerProgressManager progress)
    {
        if (selectedNode == null)
        {
            SetText(detailTitleText, "노드를 선택하세요");
            SetText(detailText, string.Empty);
            SetUpgradeButton("강화", false);
            return;
        }

        int level = progress.GetGrimoireLevel(selectedNode);
        int cost = GrimoireRules.GetUpgradeCost(level);
        bool isMax = level >= GrimoireRules.MaxLevel;
        bool canUpgrade = progress.CanUpgradeGrimoireNode(selectedNode, out string reason);

        string goldHex = ColorUtility.ToHtmlStringRGB(UISkin.Gold);
        string dimHex = ColorUtility.ToHtmlStringRGB(UISkin.CreamSub);
        string warningHex = ColorUtility.ToHtmlStringRGB(WarningTextColor);

        StringBuilder builder = new StringBuilder();

        builder.Append(
            $"{GrimoireRules.GetBranchName(selectedNode.Branch)} 분기 {selectedNode.Order}번 노드\n" +
            $"현재 단계  Lv.{level} / {GrimoireRules.MaxLevel}\n\n"
        );

        for (int step = 1; step <= GrimoireRules.MaxLevel; step++)
        {
            string effectLine = $"Lv.{step}  {selectedNode.GetEffectText(step)}";

            if (step == level)
            {
                builder.Append($"{effectLine} (현재)\n");
            }
            else if (step == level + 1)
            {
                builder.Append($"<color=#{goldHex}>{effectLine} (다음)</color>\n");
            }
            else if (step < level)
            {
                builder.Append($"{effectLine}\n");
            }
            else
            {
                builder.Append($"<color=#{dimHex}>{effectLine}</color>\n");
            }
        }

        builder.Append('\n');

        if (isMax)
        {
            builder.Append("최대 단계입니다.");
        }
        else
        {
            builder.Append(
                $"다음 단계 비용  {UISkin.IconOr(UIIcons.Shard, "욕망의 파편")} {cost}" +
                $"  (보유 {progress.DesireShards})\n"
            );

            GrimoireNodeData prerequisite =
                progress.GetGrimoirePrerequisite(selectedNode);

            if (prerequisite != null)
            {
                string prerequisiteState =
                    progress.GetGrimoireLevel(prerequisite) > 0 ? "해금됨" : "미해금";

                builder.Append(
                    $"선행 노드  {prerequisite.DisplayName} ({prerequisiteState})\n"
                );
            }

            builder.Append(
                canUpgrade
                    ? "강화하면 되돌릴 수 없습니다."
                    : $"<color=#{warningHex}>{reason}</color>"
            );
        }

        SetText(detailTitleText, selectedNode.DisplayName);
        SetText(detailText, builder.ToString());

        string buttonLabel = isMax
            ? "최대 단계"
            : confirmNode == selectedNode
                ? "한 번 더 눌러 확정"
                : level > 0 ? $"강화 (파편 {cost})" : $"해금 (파편 {cost})";

        SetUpgradeButton(buttonLabel, canUpgrade);
    }

    private void SetText(TMP_Text targetText, string content)
    {
        if (targetText == null) { return; }

        targetText.text = content;
    }

    private void SetUpgradeButton(string label, bool isInteractable)
    {
        if (upgradeButton == null) { return; }

        upgradeButton.interactable = isInteractable;

        TMP_Text buttonLabel =
            upgradeButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel != null)
        {
            buttonLabel.text = label;
        }
    }
}
