using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 스토리 선택지 (기획서 2.10)
// 선택지는 도윤의 대답을 고르는 장치다. 고른 답의 갈래만 이어서 보여 주고 다시 같은 줄기로 돌아온다.
// 진행, 보상, 해금에는 영향을 주지 않으므로 고른 답은 저장하지 않는다.
public partial class StoryFlow
{
    [Header("선택지")]
    [SerializeField] private GameObject choicePanel; // 선택지 버튼 묶음
    [SerializeField] private Button[] choiceButtons; // 선택지 버튼 (최대 4개)

    private readonly List<StorySceneData.Choice> shownChoices = new List<StorySceneData.Choice>(); // 지금 떠 있는 선택지

    private bool IsChoosing => shownChoices.Count > 0; // 답을 기다리는 중인지 여부

    private void StartChoices()
    {
        if (choiceButtons != null)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                Button choiceButton = choiceButtons[i];

                if (choiceButton == null) { continue; }

                int choiceIndex = i;

                choiceButton.onClick.RemoveAllListeners();
                choiceButton.onClick.AddListener(() => SelectChoice(choiceIndex));
            }
        }

        HideChoices();
    }

    // 줄에 선택지가 있으면 버튼에 적어 띄운다. 선택지가 떠 있는 동안에는 다음 버튼, 자동 진행, 빠른 진행이 멈춘다.
    private void ShowChoices(StorySceneData.Line line)
    {
        HideChoices();

        if (line == null || !line.HasChoices || choicePanel == null || choiceButtons == null) { return; }

        foreach (StorySceneData.Choice choice in line.Choices)
        {
            if (choice == null || shownChoices.Count >= choiceButtons.Length) { continue; }

            Button choiceButton = choiceButtons[shownChoices.Count];

            if (choiceButton == null) { continue; }

            TMP_Text choiceLabel = choiceButton.GetComponentInChildren<TMP_Text>(true);

            if (choiceLabel != null)
            {
                choiceLabel.text =
                    $"<size=72%><color=#E2C078>{choice.TypeName}</color></size>   {choice.Label}"; // 앞에 답변의 분위기를 작게 적는다.
            }

            choiceButton.gameObject.SetActive(true);
            shownChoices.Add(choice);
        }

        choicePanel.SetActive(IsChoosing);

        if (nextButton != null) { nextButton.interactable = !IsChoosing; }
    }

    private void HideChoices()
    {
        shownChoices.Clear();

        if (choiceButtons != null)
        {
            foreach (Button choiceButton in choiceButtons)
            {
                if (choiceButton != null) { choiceButton.gameObject.SetActive(false); }
            }
        }

        if (choicePanel != null) { choicePanel.SetActive(false); }
        if (nextButton != null) { nextButton.interactable = true; }
    }

    private void SelectChoice(int choiceIndex) // 답을 고르면 그 갈래의 첫 줄로 넘어간다.
    {
        if (isFinished || IsConfirmingSkip) { return; }
        if (choiceIndex < 0 || choiceIndex >= shownChoices.Count) { return; }

        selectedBranch = shownChoices[choiceIndex].Branch;

        HideChoices();
        AdvanceLine();
    }
}
