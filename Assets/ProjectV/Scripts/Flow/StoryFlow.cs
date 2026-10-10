using System;
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 스토리 화면 (기획서 10.21 / 12.12)
// 배경, 인물 그림 최대 세 명, 아래 대화창으로 대사를 차례로 보여 준다.
// 보여 줄 장면은 StorySetup에서 받는다. 받은 장면이 없으면 씬에 적힌 시험 대사를 보여 준다.
public partial class StoryFlow : MonoBehaviour
{
    [Serializable]
    public class StoryLine // 씬에 적어 둔 시험 대사
    {
        [SerializeField] private string speakerName = "";          // 화자 이름
        [SerializeField, TextArea] private string dialogue = "";   // 대사 내용

        public string SpeakerName => speakerName;
        public string Dialogue => dialogue;
    }

    [Header("화면 이동")]
    [SerializeField] private Button nextButton; // 다음 대사
    [SerializeField] private Button skipButton; // 건너뛰기

    [Header("스토리 텍스트")]
    [SerializeField] private TMP_Text speakerNameText; // 화자 이름 표시
    [SerializeField] private TMP_Text dialogueText;    // 대사 표시

    [Header("스토리 데이터")]
    [SerializeField]
    private List<StoryLine> storyLines = new List<StoryLine>(); // 시험 대사 목록

    [SerializeField]
    private string nextSceneName = SceneNames.WorldMap; // 시험 대사가 끝난 뒤 갈 씬 (기획서 4.3.1: 스토리를 마치면 월드맵으로)

    private readonly List<StorySceneData.Line> lines = new List<StorySceneData.Line>(); // 이번에 보여 줄 대사

    private StorySceneData storyScene; // 보여 주는 장면 (시험 대사면 null)
    private int currentLineIndex;      // 현재 대사 번호
    private bool isFinished;           // 장면을 끝내고 화면을 떠나는 중인지 여부

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
        ResetStage(); // 인물 그림 자리 비우기

        currentLineIndex = 0;
        ShowCurrentLine(); // 첫 대사 표시
    }

    private void LoadLines() // 받은 장면의 대사를 읽는다. 없으면 씬에 적힌 시험 대사를 쓴다.
    {
        lines.Clear();
        storyScene = StorySetup.Scene;

        if (storyScene != null)
        {
            foreach (StorySceneData.Line line in storyScene.Lines)
            {
                if (line != null) { lines.Add(line); }
            }

            return;
        }

        foreach (StoryLine legacyLine in storyLines)
        {
            if (legacyLine != null)
            {
                lines.Add(new StorySceneData.Line(legacyLine.SpeakerName, legacyLine.Dialogue));
            }
        }
    }

    public void ShowNextLine()
    {
        if (isFinished || IsConfirmingSkip) { return; }

        currentLineIndex += 1;

        if (currentLineIndex >= lines.Count)
        {
            FinishStory(); // 마지막 대사 이후 종료
            return;
        }

        ShowCurrentLine();
    }

    // 장면을 끝낸다. 끝까지 봤든 건너뛰었든 본 장면으로 기록하고, 그 장면에 걸린 해금을 처리한 뒤 다음 화면으로 간다.
    private void FinishStory()
    {
        if (isFinished) { return; }

        isFinished = true;

        string targetScene = storyScene != null && !string.IsNullOrEmpty(StorySetup.ReturnSceneName)
            ? StorySetup.ReturnSceneName
            : nextSceneName;

        if (storyScene != null && PlayerProgressManager.Instance != null)
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
            if (dialogueText != null) { dialogueText.text = "대사 데이터를 추가하세요."; } // 데이터 없음 안내

            return;
        }

        StorySceneData.Line currentLine = lines[Mathf.Clamp(currentLineIndex, 0, lines.Count - 1)];

        if (speakerNameText != null) { speakerNameText.text = currentLine.SpeakerName; }
        if (dialogueText != null) { dialogueText.text = currentLine.Text; }

        ApplyStage(currentLine); // 배경과 인물 그림, 화자 강조

        if (nextButton == null) { return; }

        TMP_Text nextLabel = nextButton.GetComponentInChildren<TMP_Text>();

        if (nextLabel != null)
        {
            string nextWord = currentLineIndex >= lines.Count - 1 ? "계속" : "다음"; // 마지막 대사 표시 변경

            nextLabel.text = $"{nextWord} {UISkin.Icon(UIIcons.Next)}".Trim();
        }
    }
}
