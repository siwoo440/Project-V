using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

public partial class StageSelectFlow // 지역 화면의 지역 표시와 일반전 스테이지 (기획서 4.5 / 8.10)
{
    private const string NormalBattleType = "일반전"; // 적 마물과 싸우는 스테이지의 종류 이름

    [Header("적 편성")]
    [SerializeField]
    private List<EnemyFormationData> formations =
        new List<EnemyFormationData>(); // 일반전 적 편성 전체 (지역 ID와 단계로 찾는다)

    private readonly Dictionary<StageEntry, EnemyFormationData> stageFormations =
        new Dictionary<StageEntry, EnemyFormationData>(); // 스테이지별 적 편성 (없으면 히로인 전투)

    private RegionData region;   // 이 화면이 보여 주는 지역
    private int lastNormalStage; // 이 지역 일반전의 마지막 단계 (일반전이 없으면 0)

    // 월드맵에서 고른 지역을 화면에 표시한다. 월드맵을 거치지 않고 들어왔으면 첫 지역을 쓴다.
    private void ShowRegion()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        region = progress.CurrentRegion;

        if (region == null)
        {
            RegionData firstRegion = progress.GetFirstRegion();

            if (progress.TryEnterRegion(firstRegion, out _))
            {
                region = firstRegion;
            }
        }

        if (region == null) { return; } // 지역 데이터가 없으면 기존 표시를 그대로 둔다.

        if (titleText != null)
        {
            titleText.text = region.DisplayName;
        }

        if (backgroundImage != null && UISkin.Has(region.BackgroundKey))
        {
            UISkin.ApplySimple(backgroundImage, region.BackgroundKey, false); // 지역 배경 그림이 없으면 지도 그림을 그대로 둔다.
        }
    }

    // 스테이지 이름. 일반전은 지역 이름과 단계로, 시험 항목은 지역 이름과 전투 종류로 만든다.
    private string GetStageTitle(StageEntry stage)
    {
        if (stage == null) { return string.Empty; }
        if (region == null) { return stage.StageName; }

        return stageFormations.ContainsKey(stage)
            ? $"{region.DisplayName} {stage.StageName}" // 일반전 1단계처럼 단계가 들어간 이름
            : $"{region.DisplayName} {stage.StageType}";
    }

    private List<EnemyFormationData> GetRegionFormations() // 이 지역의 적 편성 (단계 순)
    {
        List<EnemyFormationData> regionFormations = new List<EnemyFormationData>();

        foreach (EnemyFormationData formation in formations)
        {
            if (formation == null || region == null) { continue; }
            if (formation.RegionId != region.RegionId) { continue; }

            regionFormations.Add(formation);
        }

        regionFormations.Sort((left, right) => left.Stage.CompareTo(right.Stage));

        return regionFormations;
    }

    // 이 지역에서 고를 수 있는 스테이지. 지역에 적 편성이 있으면 일반전 단계를 편성으로 만들고,
    // 포획전과 히로인전은 씬에 저장된 시험 항목을 그대로 쓴다. 적 편성이 없는 지역은 시험 항목만 쓴다.
    private List<StageEntry> GetShownStages()
    {
        stageFormations.Clear();
        lastNormalStage = 0;

        PlayerProgressManager progress = PlayerProgressManager.Instance;
        List<EnemyFormationData> regionFormations = GetRegionFormations();
        List<StageEntry> shownStages = new List<StageEntry>();
        EnemyFormationData previousFormation = null;

        foreach (EnemyFormationData formation in regionFormations)
        {
            bool isUnlocked =
                previousFormation == null ||
                (progress != null && progress.IsStageCleared(previousFormation.FormationId)); // 앞 단계를 이기면 열린다. (기획서 8.10)

            StageEntry normalStage = new StageEntry(
                formation.DisplayName,
                NormalBattleType,
                formation.Stage,
                formation.Description,
                isUnlocked
            );

            stageFormations[normalStage] = formation;
            shownStages.Add(normalStage);

            lastNormalStage = Mathf.Max(lastNormalStage, formation.Stage);
            previousFormation = formation;
        }

        foreach (StageEntry stage in stages)
        {
            if (stage == null) { continue; }
            if (regionFormations.Count > 0 && stage.StageType == NormalBattleType) { continue; } // 편성으로 대체

            shownStages.Add(stage);
        }

        return shownStages;
    }

    // 스테이지 이름 아래에 적는 한 줄. 일반전은 적의 수와 승리 여부를, 시험 항목은 기존 표기를 쓴다.
    private string GetStageSubtitle(StageEntry stage)
    {
        if (stage == null) { return string.Empty; }

        if (!stageFormations.TryGetValue(stage, out EnemyFormationData formation))
        {
            return $"{stage.StageType}   권장 레벨 {stage.RecommendedLevel}";
        }

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        bool isCleared = progress != null && progress.IsStageCleared(formation.FormationId);

        return
            $"{stage.StageType}   적 마물 {formation.Enemies.Count}체" +
            (isCleared ? "   클리어" : string.Empty);
    }

    private void PrepareBattleSetup(StageEntry stage) // 전투 씬이 읽을 전투 종류, 상대, 난이도
    {
        string stageTitle = GetStageTitle(stage);
        int regionOrder = region == null ? 1 : region.Order;

        if (stageFormations.TryGetValue(stage, out EnemyFormationData formation))
        {
            BattleSetup.SetEnemyBattle(
                formation, regionOrder, stageTitle, selectedDifficulty,
                formation.Stage >= lastNormalStage // 임시 규칙: 일반전 마지막 단계를 이기면 지역 클리어
            );

            return;
        }

        BattleSetup.SetHeroineBattle(
            stageTitle, regionOrder,
            lastNormalStage == 0 // 임시 규칙: 일반전이 없는 지역은 어느 전투든 이기면 지역 클리어
        );
    }
}
