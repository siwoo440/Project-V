using System.Collections.Generic; // 리스트 기능
using System.Text; // 문자열 조립 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 지역 화면의 히로인전 (기획서 F.3.1 / F.17 / 9.7.2 / 9.9)
// 주요 히로인 1, 2, 3차전은 메인 진행 목록에, 서브 히로인전 3개는 서브 콘텐츠 목록에 놓는다.
public partial class StageSelectFlow
{
    private const string MainHeroineType = "주요 히로인전"; // 주요 히로인전의 종류 이름
    private const string SubHeroineType = "서브 히로인전";  // 서브 히로인전의 종류 이름

    private readonly Dictionary<StageEntry, HeroineBattleData> stageHeroines =
        new Dictionary<StageEntry, HeroineBattleData>(); // 스테이지별 히로인 전투 데이터

    [Header("히로인 그림")]
    [SerializeField] private GameObject heroinePortraitFrame; // 상세 칸의 얼굴 그림 칸
    [SerializeField] private Image heroinePortraitImage;      // 얼굴 그림 (전투 그림의 얼굴 부분만 보인다)

    // 히로인전을 골랐고 그 히로인의 그림이 있으면 상세 칸에 얼굴을 보여 준다. 잠긴 전투도 보여 준다.
    private void RefreshHeroinePortrait()
    {
        if (heroinePortraitFrame == null || heroinePortraitImage == null) { return; }

        Sprite portrait = null;

        if (selectedStage != null &&
            stageHeroines.TryGetValue(selectedStage, out HeroineBattleData battle))
        {
            portrait = HeroineArtLibrary.Get(battle.ArtKey, HeroineArtState.Normal);
        }

        heroinePortraitFrame.SetActive(portrait != null);

        if (portrait == null) { return; }

        heroinePortraitImage.sprite = portrait;
        heroinePortraitImage.preserveAspect = true;
        heroinePortraitImage.enabled = true;
    }

    // 주요 히로인 n차전 줄을 더한다. 2차전은 일반전을 모두 이겨야 열리고 3차전은 2차전을 이겨야 열린다.
    private void AddMainHeroineStage(List<StageEntry> shownStages, int stage, bool areNormalStagesCleared)
    {
        HeroineBattleData battle = region.GetMainBattle(stage);
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (battle == null || progress == null) { return; }

        string lockHint = stage == 2
            ? progress.IsMainBattleCleared(region, 1)
                ? "일반전을 모두 이기면 열립니다."
                : "1차전과 일반전을 모두 이기면 열립니다."
            : "2차전을 이기면 열립니다.";

        StageEntry heroineStage = new StageEntry(
            battle.DisplayName, MainHeroineType, 1, battle.Description,
            progress.IsMainBattleUnlocked(region, stage, areNormalStagesCleared), lockHint
        );

        stageHeroines[heroineStage] = battle;
        shownStages.Add(heroineStage);
    }

    // 서브 히로인전 줄을 더한다. 주요 히로인 2차전을 이기면 세 전투가 함께 열리고 순서는 자유다.
    private void AddSubHeroineStages(List<StageEntry> shownStages)
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        bool isUnlocked = progress.IsSideContentUnlocked(region);

        foreach (HeroineBattleData battle in region.GetSubBattles())
        {
            StageEntry subStage = new StageEntry(
                battle.DisplayName, SubHeroineType, 1, battle.Description,
                isUnlocked, "주요 히로인 2차전을 이기면 열립니다."
            );

            stageHeroines[subStage] = battle;
            shownStages.Add(subStage);
        }
    }

    private string GetHeroineSubtitle(StageEntry stage, HeroineBattleData battle) // 줄에 적는 한 줄: 종류, 직위, 승리 여부
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        bool isCleared = progress != null && progress.IsStageCleared(battle.BattleId);

        return
            $"{stage.StageType}   {battle.HeroineTitle}" +
            (isCleared ? "   클리어" : string.Empty);
    }

    // 히로인전 상세: 고른 난이도의 능력치, 행동, 받을 보상, 난이도별 승리 기록
    private string GetHeroineStageDetail(StageEntry stage, HeroineBattleData battle)
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        int regionOrder = region == null ? 1 : region.Order;
        bool isFirstClear = progress == null || !progress.IsStageCleared(battle.BattleId);

        StringBuilder builder = new StringBuilder();

        builder.Append($"{GetHeroineSubtitle(stage, battle)}\n");

        if (!string.IsNullOrEmpty(battle.Description))
        {
            builder.Append($"{battle.Description}\n");
        }

        builder.Append(
            $"난이도 {StageRules.GetDifficultyName(selectedDifficulty)}: " +
            $"HP {HeroineBattleRules.GetMaxHp(battle, selectedDifficulty)}, " +
            $"공격 {HeroineBattleRules.GetAttack(battle, selectedDifficulty)}, " +
            $"방어 {battle.Defense}, " +
            $"성욕 저항 {HeroineBattleRules.GetLustResistanceName(battle.LustResistance)}\n"
        );

        builder.Append(
            $"행동 ({HeroineBattleRules.GetActionCountText(battle)}): {battle.ActionListText}\n"
        );

        builder.Append(
            $"{(isFirstClear ? "최초 승리 보상" : "재전투 보상")}: " +
            $"{UISkin.IconOr(UIIcons.Gold, "골드")} {HeroineBattleRules.GetGold(battle, regionOrder, selectedDifficulty, isFirstClear)}    " +
            $"{UISkin.IconOr(UIIcons.Exp, "경험치")} {HeroineBattleRules.GetExperience(battle, regionOrder, selectedDifficulty, isFirstClear)}    " +
            $"{UISkin.IconOr(UIIcons.Shard, "파편")} {HeroineBattleRules.GetShards(battle, selectedDifficulty, isFirstClear)}\n"
        );

        if (ClearsRegion(battle) && progress != null && !progress.IsRegionCleared(region))
        {
            builder.Append(
                $"지역 클리어 보상: 골드 {HeroineBattleRules.GetRegionClearGold(regionOrder)}, " +
                $"욕망의 파편 {HeroineBattleRules.GetRegionClearShards(regionOrder)}\n"
            );
        }

        builder.Append("승리 기록:");

        foreach (BattleDifficulty difficulty in StageRules.Difficulties)
        {
            bool isCleared =
                progress != null && progress.IsStageCleared(battle.BattleId, difficulty);

            builder.Append(
                $"  {StageRules.GetDifficultyName(difficulty)} {(isCleared ? "승리" : "없음")}"
            );
        }

        builder.Append("\n");

        return builder.ToString();
    }

    private static bool ClearsRegion(HeroineBattleData battle) // 주요 히로인 3차전을 이기면 지역을 클리어한다. (기획서 4.14.1)
    {
        return battle != null && battle.IsMain && battle.Stage >= HeroineBattleRules.MainStageCount;
    }
}
