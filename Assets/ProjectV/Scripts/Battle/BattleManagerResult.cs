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

        if (!isVictory || battleRewardData == null)
        {
            return new BattleResultData(
                outcome,
                0,
                0,
                false,
                false,
                null
            );
        }

        int goldReward =
            battleRewardData.RollGold();

        int experienceReward =
            battleRewardData.RollExperience();

        bool captureAttempted =
            battleRewardData.HasCaptureCandidate &&
            battleRewardData.CaptureChance > 0f;

        bool captureSucceeded =
            captureAttempted &&
            battleRewardData.RollCapture();

        MonsterData capturedMonster = captureSucceeded
            ? battleRewardData.GetRandomCaptureCandidate()
            : null;

        if (capturedMonster == null)
        {
            captureSucceeded = false;
        }

        return new BattleResultData(
            outcome,
            goldReward,
            experienceReward,
            captureAttempted,
            captureSucceeded,
            capturedMonster
        );
    }
    private string GetBattleOutcomeDisplayName( BattleOutcome outcome)
    {
        switch (outcome)
        {
            case BattleOutcome.VictoryHp: return "Victory - HP Depleted";
            case BattleOutcome.VictoryLust: return "Victory - Lust MAX";
            case BattleOutcome.Defeat: return "Defeat";
            default: return "Unknown Result";
        }
    }
    private void AddBattleResultLogs(BattleResultData resultData)
    {
        if (resultData == null) { return; }

        string outcomeName =
            GetBattleOutcomeDisplayName(resultData.Outcome);

        AddBattleLog(
            BattleLogCategory.System,
            $"Battle ended: {outcomeName}."
        );

        if (!resultData.IsVictory) { return; }

        AddBattleLog(
            BattleLogCategory.System,
            $"Rewards: Gold +{resultData.GoldReward}, " +
            $"EXP +{resultData.ExperienceReward}."
        );

        if (!resultData.CaptureAttempted)
        {
            AddBattleLog(
                BattleLogCategory.System,
                "Capture was not attempted."
            );

            return;
        }

        if (!resultData.CaptureSucceeded)
        {
            AddBattleLog(
                BattleLogCategory.System,
                "Capture failed."
            );

            return;
        }

        AddBattleLog(
            BattleLogCategory.System,
            $"Captured {resultData.CapturedMonster.MonsterName}."
        );
    }
    public void ClaimBattleRewards()
    {
        if (lastBattleResult == null)
        {
            if (resultText != null)
            {
                resultText.text = "Missing Battle Result";
            }

            return;
        }

        if (PlayerProgressManager.Instance == null)
        {
            if (resultText != null)
            {
                resultText.text = "Missing Player Progress Manager";
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
                resultText.text = "Rewards Already Claimed";
            }

            return;
        }

        string claimMessage = lastBattleResult.IsVictory
    ? "Rewards Added to Player Progress"
    : "Battle Result Confirmed"; // 기본 수령 문구

        if (lastBattleResult.DuplicateMaterialReward > 0)
        {
            claimMessage =
                $"Duplicate Converted: Enhancement Material +" +
                $"{lastBattleResult.DuplicateMaterialReward}"; // 중복 변환 문구
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

        ClearHeroineTargetPreview();
        ClearMonsterSelection();

        lastBattleResult =
            CreateBattleResult(outcome);

        string resultMessage =
            GetBattleOutcomeDisplayName(outcome);

        if (turnText != null)
        {
            turnText.text = "Battle End";
        }

        if (resultText != null)
        {
            resultText.text = resultMessage;
        }

        if (heroineIntentText != null)
        {
            heroineIntentText.text = "Next Action: None";
        }

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