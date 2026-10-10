using System.Collections.Generic; // 리스트 기능
using System.Text; // 문자열 조립 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 지역 화면의 포획 콘텐츠 (기획서 8.12 / 9.13 / 9.14)
// 포획 목록 3개를 스테이지 줄로 보여 주고, 고른 목록의 마물과 싸워 이기면 쓰러뜨린 마물을 모두 얻는다.
public partial class StageSelectFlow
{
    private const string CaptureBattleType = "포획전"; // 포획 스테이지의 종류 이름

    [Header("포획")]
    [SerializeField] private Button rerollButton; // 포획 목록 다시 뽑기

    private readonly Dictionary<StageEntry, int> captureSlots =
        new Dictionary<StageEntry, int>(); // 스테이지별 포획 목록 번호

    private string captureMessage = string.Empty; // 리롤 결과 안내

    private void StartRerollButton() // 리롤 버튼 연결
    {
        rerollButton = SceneUIBinder.Bind(rerollButton, "RerollButton");

        if (rerollButton == null) { return; }

        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(OnRerollButton);
    }

    private bool HasCaptureStages() // 이 지역에 포획 콘텐츠가 있는지 여부
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        return progress != null && region != null && progress.HasCaptureContent(region);
    }

    // 포획 목록 3개를 스테이지 줄로 더한다. 열리기 전에는 잠긴 줄로 보인다.
    private void AddCaptureStages(List<StageEntry> shownStages)
    {
        captureSlots.Clear();

        if (!HasCaptureStages()) { return; }

        bool isUnlocked = PlayerProgressManager.Instance.IsCaptureUnlocked(region);

        for (int slot = 0; slot < CaptureRules.SlotCount; slot++)
        {
            StageEntry captureStage = new StageEntry(
                $"포획전 {slot + 1}", CaptureBattleType, 1, string.Empty, isUnlocked
            );

            captureSlots[captureStage] = slot;
            shownStages.Add(captureStage);
        }
    }

    private IReadOnlyList<MonsterData> GetCaptureMonsters(StageEntry stage) // 그 줄의 포획 목록
    {
        return PlayerProgressManager.Instance.GetCaptureSlot(region, captureSlots[stage]);
    }

    private string GetCaptureSubtitle(StageEntry stage) // 줄에 적는 한 줄: 나오는 마물 이름
    {
        if (!stage.IsUnlocked)
        {
            return $"{stage.StageType}   일반전 {RegionRules.CaptureUnlockNormalStage}단계를 이기면 열립니다"; // 임시 조건 (RegionRules 참고)
        }

        List<string> names = new List<string>();

        foreach (MonsterData monster in GetCaptureMonsters(stage))
        {
            if (monster != null) { names.Add(monster.MonsterName); }
        }

        return $"{stage.StageType}   {string.Join(", ", names)}";
    }

    private void RefreshRerollButton() // 포획 목록을 골랐을 때만 다시 뽑기 버튼을 보여 준다.
    {
        if (rerollButton == null) { return; }

        bool isCaptureStage =
            selectedStage != null &&
            selectedStage.IsUnlocked &&
            captureSlots.ContainsKey(selectedStage);

        rerollButton.gameObject.SetActive(isCaptureStage);

        if (!isCaptureStage) { return; }

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        rerollButton.interactable = progress != null && progress.CanRerollCapture(region, out _);

        TMP_Text buttonLabel = rerollButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel != null)
        {
            buttonLabel.text = $"목록 다시 뽑기 (골드 {CaptureRules.RerollCost})";
        }
    }

    // 골드를 내고 목록 3개를 모두 바꾼다. 같은 번호의 목록을 다시 골라 둔다.
    private void OnRerollButton()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null || selectedStage == null) { return; }
        if (!captureSlots.TryGetValue(selectedStage, out int slot)) { return; }

        progress.TryRerollCapture(region, out captureMessage);

        BuildStageList(); // 줄에 적힌 마물 이름을 새 목록으로 바꾼다.

        foreach (KeyValuePair<StageEntry, int> pair in captureSlots)
        {
            if (pair.Value != slot) { continue; }

            SelectStage(pair.Key);
            break;
        }
    }
}
