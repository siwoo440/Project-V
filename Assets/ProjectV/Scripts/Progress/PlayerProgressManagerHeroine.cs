// 지역 안의 진행 순서 (기획서 F.3.1)
// 주요 히로인 1차전 → 일반전 1, 2, 3단계 → 2차전 → 서브 히로인전, 포획전, 3차전 → 지역 클리어
public partial class PlayerProgressManager
{
    public bool IsMainBattleCleared(RegionData region, int stage) // 주요 히로인 n차전에서 이긴 적이 있는지 여부
    {
        HeroineBattleData battle = region == null ? null : region.GetMainBattle(stage);

        return battle != null && IsStageCleared(battle.BattleId);
    }

    // 일반전은 1차전을 이기면 열린다. 히로인전 데이터가 없는 지역은 처음부터 열려 있다. (시험 규칙)
    public bool IsNormalBattleUnlocked(RegionData region)
    {
        return region != null &&
               (!region.HasMainBattles || IsMainBattleCleared(region, 1));
    }

    // 주요 히로인전의 해금. 2차전은 일반전을 모두 이겨야 열린다.
    public bool IsMainBattleUnlocked(RegionData region, int stage, bool areNormalStagesCleared)
    {
        if (region == null || region.GetMainBattle(stage) == null) { return false; }
        if (IsMainBattleCleared(region, stage)) { return true; } // 이긴 전투는 언제든 다시 할 수 있다. (재전투)

        switch (stage)
        {
            case 1: return IsStorySeen(region.IntroStory); // 도입 스토리를 봐야 열린다. 스토리가 없는 지역은 바로 열린다.
            case 2: return IsMainBattleCleared(region, 1) && areNormalStagesCleared;
            default: return IsMainBattleCleared(region, 2);
        }
    }

    // 서브 히로인전과 포획 콘텐츠는 2차전을 이기면 함께 열린다. 메인 진행의 필수 조건은 아니다.
    public bool IsSideContentUnlocked(RegionData region)
    {
        if (region == null) { return false; }

        return region.HasMainBattles
            ? IsMainBattleCleared(region, 2)
            : IsStageCleared(
                RegionRules.GetNormalStageId(region.RegionId, RegionRules.TestSideUnlockNormalStage)
            ); // 시험 규칙 (RegionRules 참고)
    }
}
