using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 스토리 화면 (기획서 2.10 / 10.21 / 12.12)
// 배경, 인물 그림 최대 세 명, 아래 대화창으로 대사를 차례로 보여 준다.
// 보여 줄 장면은 StorySetup에서 받는다. 선택지에서 고른 답에 따라 그 갈래의 줄만 이어서 보여 준다.
public partial class StoryFlow : MonoBehaviour
{
    [Header("화면 이동")]
    [SerializeField] private Button nextButton; // 다음 대사
    [SerializeField] private Button skipButton; // 건너뛰기

    [Header("스토리 텍스트")]
    [SerializeField] private TMP_Text speakerNameText; // 화자 이름 표시
    [SerializeField] private TMP_Text dialogueText;    // 대사 표시

    private readonly List<StorySceneData.Line> lines = new List<StorySceneData.Line>(); // 이번에 보여 줄 대사

    private StorySceneData storyScene; // 보여 주는 장면
    private int currentLineIndex;      // 현재 대사 번호
    private bool isFinished;           // 장면을 끝내고 화면을 떠나는 중인지 여부

    private string selectedBranch = string.Empty; // 마지막 선택지에서 고른 갈래 이름

    private void Awake()
    {
        nextButton = SceneUIBinder.Bind(nextButton, "NextButton");
        skipButton = SceneUIBinder.Bind(skipButton, "SkipButton");
        speakerNameText = SceneUIBinder.Bind(speakerNameText, "SpeakerNameText");
        dialogueText = SceneUIBinder.Bind(dialogueText, "DialogueText");
    }

    private void Start()
    {
        LoadLines();

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(ShowNextLine);
        }

        StartControls(); // 건너뛰기 확인, 자동 진행
        StartChoices(); // 선택지 버튼
        ResetStage(); // 인물 그림 자리 비우기

        selectedBranch = string.Empty;
        currentLineIndex = FindVisibleLine(0);
        ShowCurrentLine(); // 첫 대사 표시
    }

    private void LoadLines() // 받은 장면의 대사를 읽는다.
    {
        lines.Clear();
        storyScene = StorySetup.Scene;

        if (storyScene == null) { return; }

        foreach (StorySceneData.Line line in storyScene.Lines)
        {
            if (line != null) { lines.Add(line); }
        }
    }

    // 그 번호부터 찾아, 지금 고른 갈래에서 보여 줄 첫 줄의 번호를 돌려준다. 없으면 줄 수를 돌려준다.
    // 갈래 이름이 없는 줄은 언제나 보여 주고, 갈래 이름이 있는 줄은 그 갈래를 골랐을 때만 보여 준다.
    private int FindVisibleLine(int startIndex)
    {
        for (int i = Mathf.Max(0, startIndex); i < lines.Count; i++)
        {
            string branch = lines[i].Branch;

            if (branch.Length == 0 || branch == selectedBranch) { return i; }
        }

        return lines.Count;
    }

    public void ShowNextLine()
    {
        if (isFinished || IsConfirmingSkip || IsChoosing) { return; } // 선택지가 떠 있으면 답을 골라야 넘어간다.

        AdvanceLine();
    }

    private void AdvanceLine()
    {
        currentLineIndex = FindVisibleLine(currentLineIndex + 1);

        if (currentLineIndex >= lines.Count)
        {
            FinishStory(); // 마지막 대사 이후 종료
            return;
        }

        ShowCurrentLine();
    }

    // 장면을 끝낸다. 끝까지 봤든 건너뛰었든 본 장면으로 기록하고, 그 장면에 걸린 해금을 처리한 뒤 다음 화면으로 간다.
    // 회상으로 다시 본 장면은 기록과 해금을 건드리지 않는다.
    private void FinishStory()
    {
        if (isFinished) { return; }

        isFinished = true;

        string targetScene = string.IsNullOrEmpty(StorySetup.ReturnSceneName)
            ? SceneNames.MainMenu
            : StorySetup.ReturnSceneName;

        if (storyScene != null && !StorySetup.IsRecall && PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.CompleteStory(storyScene);
        }

        StorySetup.Clear();
        SceneFlow.LoadScene(targetScene); // 다음 씬 로드
    }

    private void ShowCurrentLine()
    {
        ResetAutoTimer();

        if (lines.Count == 0)
        {
            if (speakerNameText != null) { speakerNameText.text = "시스템"; }
            if (dialogueText != null) { dialogueText.text = "보여 줄 장면이 없습니다."; } // 장면을 정하지 않고 화면을 열었을 때

            HideChoices();
            return;
        }

        StorySceneData.Line currentLine = lines[Mathf.Clamp(currentLineIndex, 0, lines.Count - 1)];

        if (speakerNameText != null) { speakerNameText.text = currentLine.SpeakerName; }
        if (dialogueText != null) { dialogueText.text = currentLine.Text; }

        ApplyStage(currentLine); // 배경과 인물 그림, 화자 강조
        ShowChoices(currentLine); // 선택지가 있는 줄이면 버튼을 띄운다.

        if (nextButton == null) { return; }

        TMP_Text nextLabel = nextButton.GetComponentInChildren<TMP_Text>();

        if (nextLabel != null)
        {
            bool isLastLine = FindVisibleLine(currentLineIndex + 1) >= lines.Count;
            string nextWord = isLastLine && !IsChoosing ? "계속" : "다음"; // 마지막 대사 표시 변경

            nextLabel.text = $"{nextWord} {UISkin.Icon(UIIcons.Next)}".Trim();
        }
    }
}
