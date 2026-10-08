using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultUI : MonoBehaviour
{
    [Header("결과 패널")]
    [SerializeField] private GameObject resultPanel;

    [Header("결과 텍스트")]
    [SerializeField] private TMP_Text outcomeText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text captureText;
    [SerializeField] private TMP_Text levelText; // 플레이어 레벨 변화
    [SerializeField] private Image emblemImage;  // 승패 문장

    [Header("결과 버튼")]
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        Hide();
    }

    public void Show(BattleResultData resultData)
    {
        if (resultData == null)
        {
            Hide(); // 빈 결과 숨김
            return;
        }
        if (resultPanel != null){ resultPanel.SetActive(true);} // 결과 패널 표시
        Refresh(resultData); // 결과 문구 갱신
        if (continueButton != null) { continueButton.interactable = true; } // 계속 버튼 활성화  
    }
    public void Refresh(BattleResultData resultData)
    {
        if (resultData == null) { return; } // 빈 결과 차단

        if (outcomeText != null)
        {
            outcomeText.text =
                GetOutcomeDisplayName(resultData.Outcome); // 승패 표시
        }

        if (emblemImage != null)
        {
            Sprite emblem = UISkin.Get(
                resultData.IsVictory ? UIKeys.EmblemVictory : UIKeys.EmblemDefeat
            ); // 승패 문장

            emblemImage.sprite = emblem;
            emblemImage.enabled = emblem != null;
        }

        if (rewardText != null)
        {
            rewardText.text = resultData.IsVictory
                ? $"{UISkin.IconOr(UIIcons.Gold, "골드")} +{resultData.GoldReward}    " +
                  $"{UISkin.IconOr(UIIcons.Exp, "경험치")} +{GetShownExperience(resultData)}" +
                  GetExtraRewardText(resultData) +
                  GetLimitLossText(resultData)
                : "보상 없음"; // 전투 보상 표시
        }

        string levelMessage = GetLevelDisplayText(resultData); // 레벨 변화 문구

        if (levelText != null)
        {
            levelText.text = levelMessage;
        }
        else if (rewardText != null && !string.IsNullOrEmpty(levelMessage))
        {
            rewardText.text += $"\n{levelMessage}"; // 전용 칸이 없으면 보상 칸에 이어 쓴다.
        }

        if (captureText != null)
        {
            captureText.text =
                GetCaptureDisplayText(resultData); // 포획 결과 표시
        }
    }
    // 욕망의 파편과 마물의 정수는 받은 것이 있을 때만 둘째 줄에 표시한다.
    private string GetExtraRewardText(BattleResultData resultData)
    {
        int shardCount = resultData.ShownDesireShards;
        int essenceCount =
            resultData.EssenceReward + resultData.BonusEssenceReward; // 초과 포획 변환 + 그리모어 강화

        string extraText = string.Empty;

        if (shardCount > 0)
        {
            extraText +=
                $"{UISkin.IconOr(UIIcons.Shard, "욕망의 파편")} +{shardCount}";
        }

        if (essenceCount > 0)
        {
            if (extraText.Length > 0) { extraText += "    "; }

            extraText +=
                $"{UISkin.IconOr(UIIcons.Essence, "마물의 정수")} +{essenceCount}";
        }

        return extraText.Length > 0 ? "\n" + extraText : string.Empty;
    }

    // 보유 한도 때문에 받지 못하는 수량 안내 (기획서 9.3 / A.47). 수령 전에는 예상 값을 보여준다.
    private string GetLimitLossText(BattleResultData resultData)
    {
        int lostGold = resultData.LostGold;
        int lostEssence = resultData.LostEssence;
        int lostShards = resultData.LostShards;

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (!resultData.RewardsApplied && progress != null)
        {
            lostGold = resultData.GoldReward -
                       progress.PreviewGoldGain(resultData.GoldReward);

            lostEssence = resultData.BonusEssenceReward -
                          progress.PreviewEssenceGain(resultData.BonusEssenceReward);

            lostShards = resultData.DesireShardReward -
                         progress.PreviewShardGain(resultData.DesireShardReward);
        }

        if (lostGold <= 0 && lostEssence <= 0 && lostShards <= 0)
        {
            return string.Empty;
        }

        string lossText = "보유 한도 초과로 받지 못함:";

        if (lostGold > 0) { lossText += $" 골드 {lostGold}"; }
        if (lostEssence > 0) { lossText += $" 정수 {lostEssence}"; }
        if (lostShards > 0) { lossText += $" 파편 {lostShards}"; }

        return "\n" + lossText;
    }

    public void Hide()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    public void OnContinueButtonClicked()
    {
        Hide();
    }

    // 최대 레벨에서는 경험치를 받지 않으므로(기획서 6.17.1) 실제로 반영되는 양을 보여준다.
    private int GetShownExperience(BattleResultData resultData)
    {
        if (resultData.RewardsApplied)
        {
            return resultData.ExperienceGained; // 수령 후에는 반영된 양
        }

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        return progress == null
            ? resultData.ExperienceReward
            : progress.PreviewExperienceGain(resultData.ExperienceReward);
    }

    // 계속 버튼을 누르면 패널이 닫히므로, 수령 전에도 예상 레벨 변화를 미리 보여준다.
    private string GetLevelDisplayText(BattleResultData resultData)
    {
        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return string.Empty; } // 단독 전투

        int levelBefore = progress.PlayerLevel;
        int totalExperienceAfter = progress.TotalExperience;

        if (resultData.RewardsApplied)
        {
            levelBefore = resultData.PlayerLevelBefore; // 수령 후에는 기록된 값 사용
        }
        else if (resultData.IsVictory)
        {
            totalExperienceAfter += progress.PreviewExperienceGain(
                resultData.ExperienceReward
            ); // 수령 전에는 예상 값 계산
        }

        int levelAfter = PlayerLevelRules.GetLevel(totalExperienceAfter);

        if (levelAfter <= levelBefore)
        {
            return $"플레이어 {PlayerLevelRules.GetLevelText(totalExperienceAfter)}";
        }

        string unlockSummary = PlayerLevelRules.GetUnlockSummary(
            levelBefore,
            levelAfter
        ); // 오른 구간의 해금 내용

        string levelUpText =
            $"레벨 업! Lv.{levelBefore} → Lv.{levelAfter}";

        if (!string.IsNullOrEmpty(unlockSummary))
        {
            levelUpText += $"\n{unlockSummary}";
        }

        string summonerSummary = progress.GetSummonerUnlockSummary(
            levelBefore,
            levelAfter
        ); // 새로 열린 소환사 스킬과 패시브

        if (!string.IsNullOrEmpty(summonerSummary))
        {
            levelUpText += $"\n{summonerSummary}";
        }

        return levelUpText;
    }

    private string GetOutcomeDisplayName(
        BattleOutcome outcome
    )
    {
        switch (outcome)
        {
            case BattleOutcome.VictoryHp:
                return "승리 - HP 소진";

            case BattleOutcome.VictoryLust:
                return "승리 - 성욕 최대";

            case BattleOutcome.Defeat:
                return "패배";

            default:
                return "알 수 없는 결과";
        }
    }

    private string GetCaptureDisplayText(
        BattleResultData resultData
    )
    {
        if (!resultData.IsVictory)
        {
            return "포획: 시도 없음";
        }

        if (!resultData.CaptureAttempted)
        {
            return "포획: 대상 없음";
        }

        if (!resultData.CaptureSucceeded)
        {
            return "포획 실패";
        }

        if (resultData.CapturedMonster == null)
        {
            return "포획 실패";
        }

        string rarityText =
            CardRarityRules.GetDisplayName(
                resultData.CapturedMonster.Rarity
            ); // 희귀도 표시

        if (resultData.DuplicateConverted)
        {
            return
                $"포획 성공\n" +
                $"{resultData.CapturedMonster.MonsterName} ({rarityText})\n" +
                $"보유 한도 초과\n" +
                $"마물의 정수 +{resultData.EssenceReward}"; // 초과 변환 표시
        }

        return
            $"포획 성공\n" +
            $"{resultData.CapturedMonster.MonsterName} ({rarityText})\n" +
            $"카드 획득"; // 카드 획득 표시
    }
}
