using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

public partial class StageSelectFlow // 지역 화면의 지역 표시와 스테이지 목록 구성 (기획서 4.5 / 8.10 / F.3.1)
{
    private const string NormalBattleType = "일반전"; // 적 마물과 싸우는 스테이지의 종류 이름

    [Header("적 편성")]
    [SerializeField]
    private List<EnemyFormationData> formations =
        new List<EnemyFormationData>(); // 일반전 적 편성 전체 (지역 ID와 단계로 찾는다)

    private readonly Dictionary<StageEntry, EnemyFormationData> stageFormations =
        new Dictionary<StageEntry, EnemyFormationData>(); // 스테이지별 적 편성

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

    private bool IsDataStage(StageEntry stage) // 데이터로 만든 줄인지 여부 (씬에 저장된 시험 항목이 아닌 줄)
    {
        return stageFormations.ContainsKey(stage) ||
               stageHeroines.ContainsKey(stage) ||
               captureSlots.ContainsKey(stage);
    }

    // 목록과 상세에 적는 스테이지 이름. 지역 이름은 화면 제목에 있으므로 붙이지 않는다.
    private string GetStageTitle(StageEntry stage)
    {
        if (stage == null) { return string.Empty; }

        return region == null || IsDataStage(stage)
            ? stage.StageName // 아리아 1차전, 일반전 1단계, 실리아, 포획전 2
            : $"{region.DisplayName} {stage.StageType}";
    }

    private string GetBattleTitle(StageEntry stage) // 전투 기록에 남길 이름. 지역 이름을 앞에 붙인다.
    {
        if (stage == null) { return string.Empty; }
        if (region == null) { return stage.StageName; }

        if (stageHeroines.TryGetValue(stage, out HeroineBattleData battle))
        {
            return $"{region.DisplayName} {battle.BattleTitle}";
        }

        return IsDataStage(stage)
            ? $"{region.DisplayName} {stage.StageName}"
            : $"{region.DisplayName} {stage.StageType}";
    }

    // 승리 기록에 쓰는 스테이지 ID. 기록을 남기지 않는 시험 항목은 빈 문자열이다.
    private string GetStageId(StageEntry stage)
    {
        if (stage == null) { return string.Empty; }

        if (stageFormations.TryGetValue(stage, out EnemyFormationData formation))
        {
            return formation.FormationId;
        }

        if (stageHeroines.TryGetValue(stage, out HeroineBattleData battle))
        {
            return battle.BattleId;
        }

        return captureSlots.TryGetValue(stage, out int captureSlot) && region != null
            ? RegionRules.GetCaptureStageId(region.RegionId, captureSlot)
            : string.Empty;
    }

    private bool IsStageEntryCleared(StageEntry stage) // 어느 난이도로든 이긴 적이 있는 줄인지 여부
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        return progress != null && progress.IsStageCleared(GetStageId(stage));
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

    private bool AreNormalStagesCleared(List<EnemyFormationData> regionFormations) // 일반전을 모두 이겼는지 여부
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return false; }

        foreach (EnemyFormationData formation in regionFormations)
        {
            if (!progress.IsStageCleared(formation.FormationId)) { return false; }
        }

        return true; // 일반전 편성이 없는 지역은 1차전만 이기면 2차전이 열린다.
    }

    // 고른 탭의 스테이지 목록. 메인 진행은 기획서 F.3.1의 순서대로 놓는다.
    // 주요 히로인 1차전 → 일반전 1, 2, 3단계 → 2차전 → 3차전. 서브 콘텐츠는 서브 히로인전과 포획전이다.
    private List<StageEntry> GetShownStages()
    {
        stageFormations.Clear();
        stageHeroines.Clear();
        captureSlots.Clear();
        lastNormalStage = 0;

        List<EnemyFormationData> regionFormations = GetRegionFormations();
        List<StageEntry> shownStages = new List<StageEntry>();

        if (!HasTabs)
        {
            AddTestRegionStages(shownStages, regionFormations);
            return shownStages;
        }

        if (showSubTab)
        {
            AddSubHeroineStages(shownStages);
            AddCaptureStages(shownStages);
            return shownStages;
        }

        bool areNormalStagesCleared = AreNormalStagesCleared(regionFormations);

        AddMainHeroineStage(shownStages, 1, areNormalStagesCleared);
        AddNormalStages(shownStages, regionFormations);
        AddMainHeroineStage(shownStages, 2, areNormalStagesCleared);
        AddMainHeroineStage(shownStages, 3, areNormalStagesCleared);

        return shownStages;
    }

    // 일반전 줄을 더한다. 주요 히로인 1차전을 이기면 1단계가 열리고, 앞 단계를 이기면 다음 단계가 열린다. (기획서 8.10)
    private void AddNormalStages(List<StageEntry> shownStages, List<EnemyFormationData> regionFormations)
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        bool isOpen = progress == null || region == null || progress.IsNormalBattleUnlocked(region);
        EnemyFormationData previousFormation = null;

        foreach (EnemyFormationData formation in regionFormations)
        {
            bool isCleared = progress != null && progress.IsStageCleared(formation.FormationId);

            bool isUnlocked =
                isCleared || // 이미 이긴 단계는 언제든 다시 할 수 있다.
                (isOpen &&
                 (previousFormation == null ||
                  (progress != null && progress.IsStageCleared(previousFormation.FormationId))));

            StageEntry normalStage = new StageEntry(
                formation.DisplayName,
                NormalBattleType,
                formation.Stage,
                formation.Description,
                isUnlocked,
                isOpen ? "앞 단계를 이기면 열립니다." : "주요 히로인 1차전을 이기면 열립니다."
            );

            stageFormations[normalStage] = formation;
            shownStages.Add(normalStage);

            lastNormalStage = Mathf.Max(lastNormalStage, formation.Stage);
            previousFormation = formation;
        }
    }

    // 시험 규칙: 히로인전 데이터가 없는 지역은 일반전과 포획전, 씬에 저장된 시험 항목을 한 목록에 보여 준다.
    private void AddTestRegionStages(List<StageEntry> shownStages, List<EnemyFormationData> regionFormations)
    {
        AddNormalStages(shownStages, regionFormations);
        AddCaptureStages(shownStages);

        bool hasCaptureStages = captureSlots.Count > 0;

        foreach (StageEntry stage in stages)
        {
            if (stage == null) { continue; }
            if (regionFormations.Count > 0 && stage.StageType == NormalBattleType) { continue; } // 편성으로 대체
            if (hasCaptureStages && stage.StageType == CaptureBattleType) { continue; } // 포획 목록으로 대체

            shownStages.Add(stage);
        }
    }

    // 처음에 골라 둘 줄: 열려 있고 아직 이기지 못한 첫 스테이지. 다음에 진행할 전투를 바로 보여 준다.
    private StageEntry GetDefaultStage(List<StageEntry> shownStages)
    {
        foreach (StageEntry stage in shownStages)
        {
            if (stage == null || !stage.IsUnlocked) { continue; }
            if (captureSlots.ContainsKey(stage)) { continue; } // 포획전은 반복 콘텐츠다.
            if (IsStageEntryCleared(stage)) { continue; }

            return stage;
        }

        return shownStages[0];
    }

    // 스테이지 이름 아래에 적는 한 줄. 일반전은 적의 수와 승리 여부를, 시험 항목은 기존 표기를 쓴다.
    private string GetStageSubtitle(StageEntry stage)
    {
        if (stage == null) { return string.Empty; }

        if (captureSlots.ContainsKey(stage))
        {
            return GetCaptureSubtitle(stage);
        }

        if (stageHeroines.TryGetValue(stage, out HeroineBattleData battle))
        {
            return GetHeroineSubtitle(stage, battle);
        }

        if (!stageFormations.TryGetValue(stage, out EnemyFormationData formation))
        {
            return $"{stage.StageType}   권장 레벨 {stage.RecommendedLevel}";
        }

        return
            $"{stage.StageType}   적 마물 {formation.Enemies.Count}체" +
            (IsStageEntryCleared(stage) ? "   클리어" : string.Empty);
    }

    private void PrepareBattleSetup(StageEntry stage) // 전투 씬이 읽을 전투 종류, 상대, 난이도
    {
        string stageTitle = GetBattleTitle(stage);
        int regionOrder = region == null ? 1 : region.Order;

        if (stageHeroines.TryGetValue(stage, out HeroineBattleData battle))
        {
            BattleSetup.SetHeroineBattle(
                battle, regionOrder, stageTitle, selectedDifficulty,
                ClearsRegion(battle) // 주요 히로인 3차전을 이기면 지역 클리어
            );

            return;
        }

        if (stageFormations.TryGetValue(stage, out EnemyFormationData formation))
        {
            BattleSetup.SetNormalBattle(
                formation, regionOrder, stageTitle, selectedDifficulty,
                !HasTabs && formation.Stage >= lastNormalStage // 시험 규칙: 히로인전 데이터가 없는 지역은 일반전 마지막 단계로 클리어
            );

            return;
        }

        if (captureSlots.TryGetValue(stage, out int captureSlot))
        {
            BattleSetup.SetCaptureBattle(
                RegionRules.GetCaptureStageId(region.RegionId, captureSlot),
                GetCaptureMonsters(stage), regionOrder, stageTitle
            );

            return;
        }

        BattleSetup.SetTestHeroineBattle(
            stageTitle, regionOrder,
            lastNormalStage == 0 // 시험 규칙: 일반전도 없는 지역은 어느 전투든 이기면 지역 클리어
        );
    }
}
