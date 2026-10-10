using System.Text; // 문자열 조립 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 지역 화면의 일반전 정보와 최초 보상 표시 (기획서 4.17 / 8.11 / F.6)
// 난이도는 전투 준비 화면에서 고른다. 지역 화면은 마지막으로 고른 난이도의 수치를 보여 준다.
public partial class StageSelectFlow
{
    [Header("보상 표시")]
    [SerializeField] private Image rewardIconImage; // 최초 보상 표시 (닫힌 상자: 남아 있음, 열린 상자: 받음)

    private static BattleDifficulty selectedDifficulty =>
        BattleSetup.PreferredDifficulty; // 마지막으로 고른 난이도

    private static readonly Color DifficultyColor = new Color(0.20f, 0.16f, 0.31f, 1f);         // 버튼 이미지가 없을 때 기본 색
    private static readonly Color DifficultySelectedColor = new Color(0.42f, 0.34f, 0.16f, 1f); // 버튼 이미지가 없을 때 강조 색

    private void RefreshRewardIcon() // 일반전과 히로인전의 최초 보상이 남아 있는지 상자 그림으로 보여 준다.
    {
        if (rewardIconImage == null) { return; }

        bool hasReward =
            selectedStage != null &&
            selectedStage.IsUnlocked &&
            (stageFormations.ContainsKey(selectedStage) || stageHeroines.ContainsKey(selectedStage));

        if (!hasReward)
        {
            rewardIconImage.enabled = false;
            return;
        }

        rewardIconImage.enabled = UISkin.ApplySimple(
            rewardIconImage,
            IsStageEntryCleared(selectedStage) ? UIKeys.RewardChestOpen : UIKeys.RewardChestClosed,
            true
        ); // 상자 그림이 없으면 글자로만 알린다.
    }

    // 일반전 상세: 적, 고른 난이도의 효과, 받을 보상, 난이도별 승리 기록 (기획서 F.6: 적 정보와 보상 초회 여부를 전투 전에 표시)
    private string GetNormalStageDetail(StageEntry stage, EnemyFormationData formation)
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        int regionOrder = region == null ? 1 : region.Order;
        bool isFirstClear = progress == null || !progress.IsStageCleared(formation.FormationId);

        int gold = StageRules.GetNormalGold(formation.Stage, regionOrder, selectedDifficulty, isFirstClear);
        int experience = StageRules.GetNormalExperience(formation.Stage, regionOrder, selectedDifficulty, isFirstClear);

        StringBuilder builder = new StringBuilder();

        builder.Append($"{GetStageSubtitle(stage)}\n");
        builder.Append($"적 마물: {formation.EnemyListText}\n");

        if (!string.IsNullOrEmpty(formation.Description))
        {
            builder.Append($"{formation.Description}\n");
        }

        builder.Append(
            $"\n난이도 {StageRules.GetDifficultyName(selectedDifficulty)}: " +
            $"적 HP {StageRules.GetEnemyHpPercent(selectedDifficulty)}%, " +
            $"적 공격 {StageRules.GetEnemyAttackPercent(selectedDifficulty)}%, " +
            $"보상 {StageRules.GetRewardPercent(selectedDifficulty)}%\n"
        );

        builder.Append(
            $"{(isFirstClear ? "최초 승리 보상" : "반복 승리 보상")}: " +
            $"{UISkin.IconOr(UIIcons.Gold, "골드")} {gold}    " +
            $"{UISkin.IconOr(UIIcons.Exp, "경험치")} {experience}\n"
        );

        builder.Append("승리 기록:");

        foreach (BattleDifficulty difficulty in StageRules.Difficulties)
        {
            bool isCleared =
                progress != null && progress.IsStageCleared(formation.FormationId, difficulty);

            builder.Append(
                $"  {StageRules.GetDifficultyName(difficulty)} {(isCleared ? "승리" : "없음")}"
            );
        }

        builder.Append("\n");

        return builder.ToString();
    }
}
