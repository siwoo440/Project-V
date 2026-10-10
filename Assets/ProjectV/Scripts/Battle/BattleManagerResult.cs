using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 분리된 전투 기능
{
    private BattleResultData CreateBattleResult(BattleOutcome outcome)
    {
        bool isVictory =
            outcome == BattleOutcome.VictoryHp ||
            outcome == BattleOutcome.VictoryLust;

        bool hasStage = isEnemyBattle || heroineBattle != null; // 승리 기록을 남기는 전투인지 여부 (시험 히로인 전투는 남기지 않는다)
        bool isFirstClear = hasStage && IsFirstStageClear(); // 보상을 반영하기 전에 확인한다.

        BattleResultData resultData;

        if (isEnemyBattle)
        {
            resultData = BattleSetup.IsCaptureBattle
                ? CreateCaptureResult(outcome, isVictory, isFirstClear)
                : CreateNormalStageResult(outcome, isVictory, isFirstClear);
        }
        else
        {
            resultData = heroineBattle != null
                ? CreateHeroineStageResult(outcome, isVictory, isFirstClear)
                : CreateRewardDataResult(outcome, isVictory);
        }

        resultData.SetStageInfo(
            hasStage ? BattleSetup.StageId : string.Empty,
            BattleSetup.Difficulty,
            isFirstClear,
            isVictory && BattleSetup.ClearsRegion
        );

        SetRegionClearInfo(resultData);

        return resultData;
    }

    // 이 승리로 지역을 처음 클리어하면 지역 이름과 다음 지역, 지역 클리어 보상을 결과에 적는다. (기획서 4.14 / C.28)
    private void SetRegionClearInfo(BattleResultData resultData)
    {
        if (!resultData.IsVictory || !resultData.ClearsRegion) { return; }

        PlayerProgressManager progress = PlayerProgressManager.Instance;
        RegionData region = progress == null ? null : progress.CurrentRegion;

        if (region == null || progress.IsRegionCleared(region)) { return; } // 다시 이겨도 보상은 한 번뿐이다.

        RegionData nextRegion = progress.GetNextRegion(region);
        bool hasReward = region.HasMainBattles; // 시험 규칙으로 클리어하는 지역은 보상을 주지 않는다.

        resultData.SetRegionClear(
            region.DisplayName,
            nextRegion == null ? string.Empty : nextRegion.DisplayName,
            hasReward ? HeroineBattleRules.GetRegionClearGold(region.Order) : 0,
            hasReward ? HeroineBattleRules.GetRegionClearShards(region.Order) : 0
        );
    }

    // 히로인전 보상: 차수별 기준 보상에 지역 배율과 난이도 배율을 곱한다. 욕망의 파편은 표의 값을 쓴다.
    // 서브 히로인전은 같은 지역의 1차전과 같은 골드와 경험치를 준다. (기획서 9.7.2 / 9.7.3 / 9.8.2 / 9.9)
    private BattleResultData CreateHeroineStageResult(
        BattleOutcome outcome,
        bool isVictory,
        bool isFirstClear
    )
    {
        int regionOrder = BattleSetup.RegionOrder;
        BattleDifficulty difficulty = BattleSetup.Difficulty;

        if (!isVictory)
        {
            int defeatExperience = StageRules.GetDefeatExperience(
                HeroineBattleRules.GetExperience(heroineBattle, regionOrder, difficulty, true)
            ); // 승리 경험치의 25%. 반복 감소는 적용하지 않는다. (기획서 9.8.3)

            return new BattleResultData(outcome, 0, defeatExperience, false, false, null);
        }

        int goldReward = ApplyGrimoirePercent(
            HeroineBattleRules.GetGold(heroineBattle, regionOrder, difficulty, isFirstClear),
            GrimoireEffectType.LootAppraisal
        ); // 전리품 감정 반영

        int experienceReward = ApplyGrimoirePercent(
            HeroineBattleRules.GetExperience(heroineBattle, regionOrder, difficulty, isFirstClear),
            GrimoireEffectType.BattleRecord
        ); // 전투 기록 반영

        return new BattleResultData(
            outcome,
            goldReward,
            experienceReward,
            false,
            false,
            null,
            HeroineBattleRules.GetShards(heroineBattle, difficulty, isFirstClear),
            Grimoire(GrimoireEffectType.EssenceCondense)
        ); // 욕망의 파편과 정수 응축 포함
    }

    private bool IsFirstStageClear() // 이 스테이지에서 아직 승리한 적이 없는지 여부
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        return progress == null || !progress.IsStageCleared(BattleSetup.StageId);
    }

    // 일반전 보상: 단계별 기준 보상에 지역 배율과 난이도 배율을 곱한다. (기획서 9.4 ~ 9.8)
    // 일반전에서는 마물을 포획할 수 없고 욕망의 파편도 받지 않는다. (기획서 8.10 / 9.9)
    private BattleResultData CreateNormalStageResult(
        BattleOutcome outcome,
        bool isVictory,
        bool isFirstClear
    )
    {
        int stage = BattleSetup.StageNumber;
        int regionOrder = BattleSetup.RegionOrder;
        BattleDifficulty difficulty = BattleSetup.Difficulty;

        if (!isVictory)
        {
            int defeatExperience = StageRules.GetDefeatExperience(
                StageRules.GetNormalExperience(stage, regionOrder, difficulty, true)
            ); // 승리 경험치의 25%. 반복 감소는 적용하지 않는다. (기획서 9.8.3)

            return new BattleResultData(outcome, 0, defeatExperience, false, false, null);
        }

        int goldReward = ApplyGrimoirePercent(
            StageRules.GetNormalGold(stage, regionOrder, difficulty, isFirstClear),
            GrimoireEffectType.LootAppraisal
        ); // 전리품 감정 반영

        int experienceReward = ApplyGrimoirePercent(
            StageRules.GetNormalExperience(stage, regionOrder, difficulty, isFirstClear),
            GrimoireEffectType.BattleRecord
        ); // 전투 기록 반영

        int essenceReward =
            StageRules.GetNormalEssence(stage, regionOrder, difficulty, isFirstClear) +
            Grimoire(GrimoireEffectType.EssenceCondense); // 일반전 정수 보상 (기획서 9.10.2) + 정수 응축

        return new BattleResultData(
            outcome,
            goldReward,
            experienceReward,
            false,
            false,
            null,
            0,
            essenceReward
        );
    }

    // 포획전 보상: 골드는 없고 경험치와 정수를 준다. 이기면 쓰러뜨린 마물을 모두 얻는다. (기획서 9.7.4 / 9.8.2 / 9.10.3 / 9.13)
    private BattleResultData CreateCaptureResult(
        BattleOutcome outcome,
        bool isVictory,
        bool isFirstClear
    )
    {
        int regionOrder = BattleSetup.RegionOrder;
        BattleResultData resultData;

        if (!isVictory)
        {
            int defeatExperience = StageRules.GetDefeatExperience(
                StageRules.GetCaptureExperience(regionOrder, true)
            ); // 승리 경험치의 25% (기획서 9.8.3)

            resultData = new BattleResultData(outcome, 0, defeatExperience, false, false, null);
            resultData.SetCaptureResult(null); // 패배하면 포획 결과가 전부 취소된다.

            return resultData;
        }

        int experienceReward = ApplyGrimoirePercent(
            StageRules.GetCaptureExperience(regionOrder, isFirstClear),
            GrimoireEffectType.BattleRecord
        ); // 전투 기록 반영

        int essenceReward =
            StageRules.GetCaptureEssence(BattleSetup.Enemies.Count, isFirstClear) +
            Grimoire(GrimoireEffectType.EssenceCondense); // 기본 정수 + 쓰러뜨린 마물 수 + 정수 응축

        resultData = new BattleResultData(
            outcome, 0, experienceReward, false, false, null, 0, essenceReward
        );

        resultData.SetCaptureResult(BattleSetup.Enemies); // 적 전멸이 승리 조건이므로 편성의 마물 전부다.

        return resultData;
    }

    // 시험 히로인 전투 보상: 히로인전 데이터가 없는 전투는 보상 데이터의 값을 쓴다.
    private BattleResultData CreateRewardDataResult(BattleOutcome outcome, bool isVictory)
    {
        if (battleRewardData == null)
        {
            return new BattleResultData(outcome, 0, 0, false, false, null);
        }

        if (!isVictory)
        {
            int defeatExperience =
                StageRules.GetDefeatExperience(battleRewardData.RollExperience()); // 승리 경험치의 25% (기획서 9.8.3)

            return new BattleResultData(outcome, 0, defeatExperience, false, false, null);
        }

        int goldReward = ApplyGrimoirePercent(
            battleRewardData.RollGold(),
            GrimoireEffectType.LootAppraisal
        ); // 전리품 감정 반영

        int experienceReward = ApplyGrimoirePercent(
            battleRewardData.RollExperience(),
            GrimoireEffectType.BattleRecord
        ); // 전투 기록 반영

        return new BattleResultData(
            outcome,
            goldReward,
            experienceReward,
            false,
            false,
            null,
            battleRewardData.DesireShards,
            Grimoire(GrimoireEffectType.EssenceCondense)
        ); // 욕망의 파편과 정수 응축 포함. 포획은 포획전에서만 한다. (기획서 9.13.1)
    }

    private string GetBattleOutcomeDisplayName( BattleOutcome outcome)
    {
        switch (outcome)
        {
            case BattleOutcome.VictoryHp: return isEnemyBattle ? "승리 - 적 전멸" : "승리 - HP 소진";
            case BattleOutcome.VictoryLust: return "승리 - 성욕 최대";
            case BattleOutcome.Defeat: return "패배";
            default: return "알 수 없는 결과";
        }
    }
    private void AddBattleResultLogs(BattleResultData resultData)
    {
        if (resultData == null) { return; }

        string outcomeName =
            GetBattleOutcomeDisplayName(resultData.Outcome);

        AddBattleLog(
            BattleLogCategory.System,
            $"전투 종료: {outcomeName}"
        );

        if (!resultData.IsVictory) { return; }

        AddBattleLog(
            BattleLogCategory.System,
            $"보상: 골드 +{resultData.GoldReward}, " +
            $"경험치 +{resultData.ExperienceReward}" +
            (resultData.DesireShardReward > 0
                ? $", 욕망의 파편 +{resultData.DesireShardReward}"
                : string.Empty)
        );

        foreach (MonsterData capturedMonster in resultData.CapturedMonsters)
        {
            AddBattleLog(
                BattleLogCategory.System,
                $"포획: {capturedMonster.MonsterName}"
            ); // 포획전에서 쓰러뜨린 마물
        }

        if (!resultData.HasRegionClear) { return; }

        AddBattleLog(
            BattleLogCategory.System,
            $"지역 클리어: {resultData.ClearedRegionName}" +
            (resultData.RegionClearGold > 0 || resultData.RegionClearShards > 0
                ? $" (골드 +{resultData.RegionClearGold}, 욕망의 파편 +{resultData.RegionClearShards})"
                : string.Empty)
        );
    }
    public void ClaimBattleRewards()
    {
        if (lastBattleResult == null)
        {
            if (resultText != null)
            {
                resultText.text = "전투 결과가 없습니다";
            }

            return;
        }

        if (PlayerProgressManager.Instance == null)
        {
            if (resultText != null)
            {
                resultText.text = "진행 데이터가 없습니다";
            }

            return;
        }

        bool applied =
            PlayerProgressManager.Instance.ApplyBattleResult(
                lastBattleResult
            );

        if (!applied)
        {
            if (resultText != null)
            {
                resultText.text = "이미 보상을 수령했습니다";
            }

            return;
        }

        string claimMessage = lastBattleResult.IsVictory
    ? "보상을 진행 데이터에 반영했습니다"
    : "전투 결과를 확인했습니다"; // 기본 수령 문구

        if (lastBattleResult.DuplicateConverted)
        {
            claimMessage =
                $"보유 한도 초과: 마물의 정수 +" +
                $"{lastBattleResult.EssenceReward}"; // 초과 변환 문구
        }

        if (lastBattleResult.LeveledUp)
        {
            claimMessage +=
                $" / 레벨 업 Lv.{lastBattleResult.PlayerLevelBefore} → " +
                $"Lv.{lastBattleResult.PlayerLevelAfter}"; // 레벨 상승 문구
        }

        if (resultText != null)
        {
            resultText.text = claimMessage; // 전투 안내 갱신
        }

        if (battleResultUI != null)
        {
            battleResultUI.Refresh(lastBattleResult); // 결과 패널 갱신
        }

        AddBattleLog(
            BattleLogCategory.System,
            claimMessage
        ); // 보상 수령 로그
    }
    private void EndBattle(BattleOutcome outcome)
    {
        if (isBattleEnded) { return; }

        isBattleEnded = true;
        isPlayerTurn = false;

        CancelSummonerSkillTargeting(string.Empty); // 스킬 대상 선택 중이면 취소
        CancelBattleItemTargeting(string.Empty); // 아이템 대상 선택 중이면 취소
        UpdateSummonerUI(); // 스킬 버튼 비활성화
        UpdateBattleItemUI(); // 아이템 버튼 비활성화
        ClearHeroineTargetPreview();
        ClearMonsterSelection();

        lastBattleResult =
            CreateBattleResult(outcome);

        ShowHeroineResultArt(outcome); // 히로인이 졌으면 패배 그림을 남긴다.

        string resultMessage =
            GetBattleOutcomeDisplayName(outcome);

        lastBattleResult.SetOutcomeLabel(resultMessage); // 결과 화면에도 같은 문구를 쓴다.

        if (turnText != null)
        {
            turnText.text = "전투 종료";
        }

        if (resultText != null)
        {
            resultText.text = resultMessage;
        }

        UpdateHeroineIntentUI(); // 전투 종료 후에는 예고를 비운다.

        if (endTurnButton != null)
        {
            endTurnButton.interactable = false;
        }

        SetAttackButtonsInteractable(false);
        SetHandInteractable(false);
        SetMonsterInteractable(false);

        AddBattleResultLogs(lastBattleResult);

        if (battleResultUI != null)
        {
            battleResultUI.Show(lastBattleResult);
        }
    }

}