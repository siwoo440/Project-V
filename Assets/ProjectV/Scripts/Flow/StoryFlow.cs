using System;
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class StoryFlow : MonoBehaviour // 스토리 화면 연결
{
    [Serializable]
    public class StoryLine // 스토리 대사
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
    private List<StoryLine> storyLines = new List<StoryLine>(); // 대사 목록

    [SerializeField]
    private string nextSceneName = SceneNames.StageSelect; // 종료 후 씬

    private int currentLineIndex; // 현재 대사 번호

    private void Awake()
    {
        nextButton =
            SceneUIBinder.Bind(nextButton, "NextButton");

        skipButton =
            SceneUIBinder.Bind(skipButton, "SkipButton");

        speakerNameText =
            SceneUIBinder.Bind(speakerNameText, "SpeakerNameText");

        dialogueText =
            SceneUIBinder.Bind(dialogueText, "DialogueText");
    }

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(ShowNextLine);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipStory);
        }

        currentLineIndex = 0;
        ShowCurrentLine(); // 첫 대사 표시
    }

    public void ShowNextLine()
    {
        currentLineIndex += 1;

        if (currentLineIndex >= storyLines.Count)
        {
            SkipStory(); // 마지막 대사 이후 종료
            return;
        }

        ShowCurrentLine();
    }

    public void SkipStory()
    {
        SceneFlow.LoadScene(nextSceneName); // 다음 씬 로드
    }

    private void ShowCurrentLine()
    {
        if (storyLines.Count == 0)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = "시스템";
            }

            if (dialogueText != null)
            {
                dialogueText.text =
                    "대사 데이터를 추가하세요."; // 데이터 없음 안내
            }

            return;
        }

        StoryLine currentLine = storyLines[
            Mathf.Clamp(currentLineIndex, 0, storyLines.Count - 1)
        ];

        if (speakerNameText != null)
        {
            speakerNameText.text = currentLine.SpeakerName;
        }

        if (dialogueText != null)
        {
            dialogueText.text = currentLine.Dialogue;
        }

        if (nextButton != null)
        {
            TMP_Text nextLabel =
                nextButton.GetComponentInChildren<TMP_Text>();

            if (nextLabel != null)
            {
                nextLabel.text =
                    currentLineIndex >= storyLines.Count - 1
                        ? "계속"
                        : "다음"; // 마지막 대사 표시 변경
            }
        }
    }
}
