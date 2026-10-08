using System; // 이벤트와 수학 기능
using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 저장과 불러오기 (기획서 4.19 / 9.17 / 15.2 ~ 15.9)
// 전투 중의 상태는 진행 데이터에 들어 있지 않으므로, 언제 저장해도 전투 시작 전의 진행만 남는다. (기획서 15.7)
public partial class PlayerProgressManager
{
    [Header("저장")]
    [SerializeField]
    private List<CardData> cardCatalog =
        new List<CardData>(); // 저장 데이터의 카드 ID를 카드로 되돌릴 때 찾는 전체 카드 목록

    private bool isSessionActive;   // 이어하기, 새 게임, 불러오기 가운데 하나로 게임을 시작했는지 여부
    private bool hasUnsavedChanges; // 마지막 저장 뒤에 진행 데이터가 바뀌었는지 여부
    private double playTimeSeconds; // 누적 플레이 시간 (초)

    public event Action<SaveSlot, bool> SaveFinished; // 저장을 마쳤을 때 알림 (칸, 성공 여부)

    public bool IsSessionActive => isSessionActive;  // 게임을 시작했는지 여부 (시작 전에는 자동 저장하지 않는다)
    public double PlayTimeSeconds => playTimeSeconds; // 누적 플레이 시간 반환

    private void Update()
    {
        if (!isSessionActive) { return; }

        playTimeSeconds += Time.unscaledDeltaTime; // 플레이 시간 누적
    }

    private void OnApplicationQuit()
    {
        AutoSaveIfChanged("게임 종료"); // 기획서 15.6
    }

    private void MarkUnsavedChanges() // 진행 데이터가 바뀔 때마다 표시해 둔다.
    {
        hasUnsavedChanges = true;
    }

    // 저장 파일이 하나도 없으면 지금 상태를 새 게임으로 삼아 바로 시작한다. (메인 메뉴가 부른다)
    public bool BeginSessionIfNoSave()
    {
        if (isSessionActive) { return true; }
        if (SaveFileSystem.HasAnyFile()) { return false; } // 이어하기나 새 게임을 고르게 한다.

        isSessionActive = true;
        AutoSave("새 게임 시작"); // 기획서 15.6

        return true;
    }

    public void StartNewGame() // 진행 데이터를 처음 상태로 되돌리고 새로 시작한다.
    {
        InitializeStartingProgress();

        playTimeSeconds = 0d;
        isSessionActive = true;

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림
        AutoSave("새 게임 시작"); // 기획서 15.6
    }

    // ---------- 저장 ----------

    public bool AutoSave(string reason) // 자동 저장 칸에 저장한다. 게임을 시작하기 전에는 저장하지 않는다.
    {
        if (!isSessionActive) { return false; }

        return WriteSlot(SaveSlot.Auto, reason, out _);
    }

    public void AutoSaveIfChanged(string reason) // 마지막 저장 뒤에 바뀐 것이 있을 때만 자동 저장한다.
    {
        if (!hasUnsavedChanges) { return; }

        AutoSave(reason);
    }

    public bool SaveToSlot(SaveSlot slot, out string message) // 수동 저장 (기획서 15.5)
    {
        if (!isSessionActive)
        {
            message = "이어하기 또는 새 게임으로 게임을 시작한 뒤에 저장할 수 있습니다.";
            return false;
        }

        if (!SaveRules.IsManual(slot))
        {
            message = "자동 저장 칸에는 직접 저장할 수 없습니다.";
            return false;
        }

        return WriteSlot(slot, "수동 저장", out message);
    }

    private bool WriteSlot(SaveSlot slot, string reason, out string message)
    {
        bool succeeded = SaveFileSystem.Write(slot, CreateSaveData(), out string error);

        if (succeeded)
        {
            hasUnsavedChanges = false;
            message = $"{SaveRules.GetSlotName(slot)} 완료";

            Debug.Log($"저장: {SaveRules.GetSlotName(slot)} ({reason})"); // 저장 기록
        }
        else
        {
            message = error;

            Debug.LogWarning($"저장 실패: {SaveRules.GetSlotName(slot)} ({reason}) {error}");
        }

        SaveFinished?.Invoke(slot, succeeded); // 저장 표시 갱신

        return succeeded;
    }

    // ---------- 불러오기 ----------

    public bool ContinueLatest(out string message) // 이어하기: 가장 최근 저장을 불러온다. (기획서 15.4)
    {
        return LoadFromInfo(SaveFileSystem.FindLatest(), out message);
    }

    public bool LoadFromSlot(SaveSlot slot, out string message) // 고른 칸을 불러온다.
    {
        return LoadFromInfo(SaveFileSystem.Read(slot), out message);
    }

    private bool LoadFromInfo(SaveSlotInfo info, out string message)
    {
        if (info == null || !info.Exists)
        {
            message = "불러올 저장 데이터가 없습니다.";
            return false;
        }

        if (!info.IsValid)
        {
            message = info.Error;
            return false;
        }

        ApplySaveData(info.Data);

        isSessionActive = true;

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림
        hasUnsavedChanges = false; // 방금 읽은 내용은 이미 파일에 있다.

        message = info.UsedBackup
            ? "최근 저장 데이터에 문제가 발생하여 백업 데이터를 불러왔습니다."
            : $"{SaveRules.GetSlotName(info.Slot)} 불러오기 완료";

        Debug.Log(
            $"불러오기: {SaveRules.GetSlotName(info.Slot)} " +
            $"({PlayerLevelText}, 플레이 {SaveRules.FormatPlayTime(playTimeSeconds)})"
        ); // 불러오기 기록

        return true;
    }
}
