using System.Collections.Generic; // 리스트 기능
using System.Text; // 문자열 조립 기능

public partial class StageSelectFlow // 지역 화면의 포획전 상세 (기획서 9.13.2)
{
    // 마물별 계열, 희귀도, 보유 수량과 받을 보상을 적는다.
    private string GetCaptureStageDetail(StageEntry stage)
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;
        IReadOnlyList<MonsterData> monsters = GetCaptureMonsters(stage);

        string stageId = RegionRules.GetCaptureStageId(region.RegionId, captureSlots[stage]);
        bool isFirstClear = !progress.IsStageCleared(stageId);

        StringBuilder builder = new StringBuilder();

        builder.Append($"{stage.StageType}   마물 {monsters.Count}체   난이도 고정\n");

        foreach (MonsterData monster in monsters)
        {
            if (monster == null) { continue; }

            CardData card = monster.CaptureRewardCard;
            int ownedCount = progress.GetOwnedCardCount(card);

            string ownedText = ownedCount <= 0
                ? "미포획"
                : $"보유 {ownedCount} / {card.MaxCopies}";

            builder.Append(
                $"{monster.MonsterName} ({MonsterTypeRules.GetDisplayName(monster.MainType)}, " +
                $"{CardRarityRules.GetDisplayName(monster.Rarity)})  {ownedText}\n"
            );
        }

        builder.Append(
            $"\n{(isFirstClear ? "최초 승리 보상" : "반복 승리 보상")}: " +
            $"{UISkin.IconOr(UIIcons.Exp, "경험치")} {StageRules.GetCaptureExperience(region.Order, isFirstClear)}    " +
            $"{UISkin.IconOr(UIIcons.Essence, "정수")} {StageRules.GetCaptureEssence(monsters.Count, isFirstClear)}    골드 없음\n"
        );

        builder.Append("이기면 쓰러뜨린 마물을 모두 얻고, 지면 얻지 못합니다. 전투가 끝나면 목록이 새로 바뀝니다.\n");

        int rerollsLeft = progress.GetRerollsUntilLegend(region);

        if (rerollsLeft >= 0)
        {
            builder.Append($"전설 마물 확정까지 남은 다시 뽑기: {rerollsLeft}회\n");
        }

        if (!string.IsNullOrEmpty(captureMessage))
        {
            builder.Append($"{captureMessage}\n");
        }

        return builder.ToString();
    }
}
