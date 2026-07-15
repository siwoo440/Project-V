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
            Hide();
            return;
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (outcomeText != null)
        {
            outcomeText.text =
                GetOutcomeDisplayName(resultData.Outcome);
        }

        if (rewardText != null)
        {
            rewardText.text = resultData.IsVictory
                ? $"Gold +{resultData.GoldReward}\n" +
                  $"EXP +{resultData.ExperienceReward}"
                : "No Rewards";
        }

        if (captureText != null)
        {
            captureText.text =
                GetCaptureDisplayText(resultData);
        }

        if (continueButton != null)
        {
            continueButton.interactable = true;
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

        return
            $"Capture Success\n" +
            $"{resultData.CapturedMonster.MonsterName}";
    }
}