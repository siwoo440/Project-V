using TMPro; // TextMeshPro 기능
using UnityEngine.UI; // Unity UI 기능

// 메인 메뉴의 저장 메뉴 (기획서 10.5 / 15.4)
// 저장 파일이 있으면 이어하기나 새 게임을 고른 뒤에 월드맵이 열린다.
// 저장 파일이 없으면 고를 것이 없으므로 바로 새 게임으로 시작한다.
// 이어하기와 새 게임은 곧바로 월드맵으로 간다.
public partial class MainMenuFlow
{
    private bool isNewGameConfirming; // 새 게임 확인을 기다리는 중인지 여부

    private void StartSaveMenu()
    {
        AddListener(continueButton, OnContinueButton);
        AddListener(newGameButton, OnNewGameButton);
        AddListener(loadButton, () => SceneFlow.LoadSaveScreen(false)); // 메인 메뉴에서는 불러오기만 한다.

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress != null &&
            !progress.BeginSessionIfNoSave() &&
            continueButton == null &&
            newGameButton == null)
        {
            // 씬을 다시 구성하기 전이라 저장 메뉴가 없으면 고를 수 없으므로 가장 최근 저장을 바로 불러온다.
            if (!progress.ContinueLatest(out _)) { progress.StartNewGame(); }
        }

        RefreshSaveMenu(string.Empty);
    }

    private void OnContinueButton() // 가장 최근 저장을 불러온다.
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        isNewGameConfirming = false;

        if (progress.ContinueLatest(out string message))
        {
            SceneFlow.LoadWorldMap(); // 불러온 진행으로 월드맵에서 이어 간다.
            return;
        }

        RefreshSaveMenu(message);
        RefreshProgressText();
    }

    // 진행 중이거나 저장 파일이 있으면 한 번 더 눌러야 새로 시작한다.
    private void OnNewGameButton()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        bool needsConfirm = progress.IsSessionActive || SaveFileSystem.HasAnyFile();

        if (needsConfirm && !isNewGameConfirming)
        {
            isNewGameConfirming = true;

            RefreshSaveMenu(
                "새 게임을 시작하면 자동 저장이 새 게임으로 바뀝니다. " +
                "수동 저장은 그대로 남습니다. 한 번 더 누르면 시작합니다."
            );

            return;
        }

        isNewGameConfirming = false;

        progress.StartNewGame();

        SceneFlow.LoadWorldMap(); // 프롤로그 흐름이 생기기 전까지는 바로 월드맵으로 간다.
    }

    private void RefreshSaveMenu(string message)
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        bool isActive = progress != null && progress.IsSessionActive;
        SaveSlotInfo latest = SaveFileSystem.FindLatest();

        // 게임을 시작하기 전에는 저장 메뉴와 게임 종료만 누를 수 있다.
        SetInteractable(worldMapButton, isActive);
        SetInteractable(storyButton, isActive);

        SetInteractable(continueButton, !isActive && latest != null);
        SetInteractable(loadButton, SaveFileSystem.HasAnyFile());
        SetLabel(continueButton, isActive ? "진행 중" : "이어하기");
        SetLabel(newGameButton, isNewGameConfirming ? "한 번 더 눌러 시작" : "새 게임");

        if (saveInfoText == null) { return; }

        string latestText = latest == null
            ? "저장 데이터 없음"
            : $"최근 저장: {SaveRules.GetSlotName(latest.Slot)}\n" +
              $"{PlayerLevelRules.GetLevelText(latest.Data.totalExperience)}\n" +
              $"플레이 {SaveRules.FormatPlayTime(latest.Data.playTimeSeconds)}\n" +
              $"{SaveRules.FormatSavedAt(latest.Data.savedAt)}";

        string guideText = !string.IsNullOrEmpty(message)
            ? message
            : isActive
                ? string.Empty
                : latest != null
                    ? "이어하기 또는 새 게임을 선택하세요."
                    : "불러올 수 있는 저장 데이터가 없습니다. 새 게임을 선택하세요.";

        saveInfoText.text = string.IsNullOrEmpty(guideText)
            ? latestText
            : $"{latestText}\n\n{guideText}";
    }

    private void SetInteractable(Button targetButton, bool isInteractable)
    {
        if (targetButton == null) { return; }

        targetButton.interactable = isInteractable;
    }

    private void SetLabel(Button targetButton, string label)
    {
        if (targetButton == null) { return; }

        TMP_Text buttonLabel = targetButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel != null)
        {
            buttonLabel.text = label;
        }
    }
}
