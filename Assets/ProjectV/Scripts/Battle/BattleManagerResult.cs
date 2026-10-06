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
            case BattleOutcome.VictoryHp: return "승리 - HP 소진";
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
            $"경험치 +{resultData.ExperienceReward}"
        );

        if (!resultData.CaptureAttempted)
        {
            AddBattleLog(
                BattleLogCategory.System,
                "포획을 시도하지 않았습니다."
            );

            return;
        }

        if (!resultData.CaptureSucceeded)
        {
            AddBattleLog(
                BattleLogCategory.System,
                "포획에 실패했습니다."
            );

            return;
        }

        AddBattleLog(
            BattleLogCategory.System,
            $"{resultData.CapturedMonster.MonsterName}을 포획했습니다."
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

        ClearHeroineTargetPreview();
        ClearMonsterSelection();

        lastBattleResult =
            CreateBattleResult(outcome);

        string resultMessage =
            GetBattleOutcomeDisplayName(outcome);

        if (turnText != null)
        {
            turnText.text = "전투 종료";
        }

        if (resultText != null)
        {
            resultText.text = resultMessage;
        }

        if (heroineIntentText != null)
        {
            heroineIntentText.text = "다음 행동: 없음";
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