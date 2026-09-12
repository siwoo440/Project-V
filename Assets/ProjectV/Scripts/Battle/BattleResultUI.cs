using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultUI : MonoBehaviour
{
    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;

    [Header("Result Text")]
    [SerializeField] private TMP_Text outcomeText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text captureText;

    [Header("Result Button")]
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

        if (rewardText != null)
        {
            string essenceRewardText =
                resultData.DuplicateConverted
                    ? $"\nMonster Essence +" +
                      $"{resultData.EssenceReward}"
                    : string.Empty; // 마물의 정수 문구

            rewardText.text = resultData.IsVictory
                ? $"Gold +{resultData.GoldReward}\n" +
                  $"EXP +{resultData.ExperienceReward}" +
                  essenceRewardText
                : "No Rewards"; // 전투 보상 표시
        }

        if (captureText != null)
        {
            captureText.text =
                GetCaptureDisplayText(resultData); // 포획 결과 표시
        }
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

    private string GetOutcomeDisplayName(
        BattleOutcome outcome
    )
    {
        switch (outcome)
        {
            case BattleOutcome.VictoryHp:
                return "Victory - HP Depleted";

            case BattleOutcome.VictoryLust:
                return "Victory - Lust MAX";

            case BattleOutcome.Defeat:
                return "Defeat";

            default:
                return "Unknown Result";
        }
    }

    private string GetCaptureDisplayText(
        BattleResultData resultData
    )
    {
        if (!resultData.IsVictory)
        {
            return "Capture: Not Attempted";
        }

        if (!resultData.CaptureAttempted)
        {
            return "Capture: No Candidate";
        }

        if (!resultData.CaptureSucceeded)
        {
            return "Capture Failed";
        }

        if (resultData.CapturedMonster == null)
        {
            return "Capture Failed";
        }

        string rarityText =
            CardRarityRules.GetDisplayName(
                resultData.CapturedMonster.Rarity
            ); // 희귀도 표시

        if (resultData.DuplicateConverted)
        {
            return
                $"Capture Success\n" +
                $"{resultData.CapturedMonster.MonsterName} ({rarityText})\n" +
                $"Ownership Limit Reached\n" +
                $"Monster Essence +{resultData.EssenceReward}"; // 초과 변환 표시
        }

        return
            $"Capture Success\n" +
            $"{resultData.CapturedMonster.MonsterName} ({rarityText})\n" +
            $"Card Added"; // 카드 획득 표시
    }
}