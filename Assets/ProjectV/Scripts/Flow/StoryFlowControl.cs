using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.InputSystem; // 키보드 입력
using UnityEngine.UI; // Unity UI 기능

// 스토리 진행 기능: 건너뛰기, 자동 진행, 빠른 진행 (기획서 10.21)
public partial class StoryFlow
{
    private const float AutoBaseSeconds = 1.6f;       // 자동 진행의 기본 대기 시간
    private const float AutoSecondsPerLetter = 0.06f; // 글자 수에 따라 더하는 대기 시간
    private const float FastSeconds = 0.08f;          // 빠른 진행에서 대사를 넘기는 간격

    [Header("진행 기능")]
    [SerializeField] private Button autoButton;           // 자동 진행 켜고 끄기
    [SerializeField] private GameObject skipConfirmPanel; // 건너뛰기 확인 창
    [SerializeField] private Button skipConfirmYesButton; // 건너뛴다
    [SerializeField] private Button skipConfirmNoButton;  // 계속 본다

    private bool isAuto;     // 자동 진행 여부
    private float lineTimer; // 지금 대사를 보여 준 시간

    private bool IsConfirmingSkip => skipConfirmPanel != null && skipConfirmPanel.activeSelf; // 확인 창이 떠 있는지 여부

    private void StartControls()
    {
        AddControlListener(skipButton, OpenSkipConfirm);
        AddControlListener(autoButton, ToggleAuto);
        AddControlListener(skipConfirmYesButton, FinishStory); // 건너뛴 장면도 본 장면으로 기록한다.
        AddControlListener(skipConfirmNoButton, CloseSkipConfirm);

        if (skipButton != null)
        {
            TMP_Text skipLabel = skipButton.GetComponentInChildren<TMP_Text>();

            if (skipLabel != null)
            {
                skipLabel.text = $"건너뛰기 {UISkin.Icon(UIIcons.Skip)}".Trim();
            }
        }

        if (skipConfirmPanel != null) { skipConfirmPanel.SetActive(false); }

        RefreshAutoButton();
    }

    private static void AddControlListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) { return; }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void OpenSkipConfirm() // 처음 보는 장면도 건너뛸 수 있다. 확인 창을 먼저 띄운다.
    {
        if (isFinished) { return; }

        if (skipConfirmPanel == null)
        {
            FinishStory(); // 확인 창이 없는 씬이면 바로 건너뛴다.
            return;
        }

        skipConfirmPanel.SetActive(true);
    }

    private void CloseSkipConfirm()
    {
        if (skipConfirmPanel != null) { skipConfirmPanel.SetActive(false); }

        ResetAutoTimer();
    }

    private void ToggleAuto()
    {
        isAuto = !isAuto;

        ResetAutoTimer();
        RefreshAutoButton();
    }

    private void RefreshAutoButton()
    {
        if (autoButton == null) { return; }

        TMP_Text autoLabel = autoButton.GetComponentInChildren<TMP_Text>();

        if (autoLabel != null) { autoLabel.text = isAuto ? "자동 켜짐" : "자동"; }
    }

    private void ResetAutoTimer()
    {
        lineTimer = 0f;
    }

    private void Update()
    {
        if (isFinished || IsConfirmingSkip || lines.Count == 0) { return; }

        lineTimer += Time.unscaledDeltaTime;

        if (IsFastForwardHeld())
        {
            if (lineTimer >= FastSeconds) { ShowNextLine(); } // Ctrl을 누르고 있는 동안 빠르게 넘긴다.
            return;
        }

        if (!isAuto) { return; }

        StorySceneData.Line currentLine = lines[Mathf.Clamp(currentLineIndex, 0, lines.Count - 1)];
        float waitSeconds = AutoBaseSeconds + AutoSecondsPerLetter * currentLine.Text.Length;

        if (lineTimer >= waitSeconds) { ShowNextLine(); }
    }

    private static bool IsFastForwardHeld()
    {
        Keyboard keyboard = Keyboard.current;

        return keyboard != null &&
               (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed);
    }
}
