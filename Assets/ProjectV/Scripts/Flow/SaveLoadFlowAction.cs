using UnityEngine; // Unity 기본 기능

public partial class SaveLoadFlow // 저장 화면의 저장, 불러오기, 삭제
{
    // 기존 데이터가 있는 칸에 저장할 때는 한 번 더 눌러야 덮어쓴다. (기획서 15.5)
    private void OnSaveButton()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        if (!CanSaveSelected(out string reason))
        {
            pendingAction = PendingAction.None;
            ShowMessage(reason);
            Refresh();
            return;
        }

        SaveSlotInfo info = GetInfo(selectedSlot);

        if (info != null && info.Exists && pendingAction != PendingAction.Save)
        {
            pendingAction = PendingAction.Save;
            ShowMessage("기존 저장 데이터를 덮어쓰시겠습니까? 한 번 더 누르면 저장합니다.");
            Refresh();
            return;
        }

        pendingAction = PendingAction.None;

        progress.SaveToSlot(selectedSlot, out string message);

        ShowMessage(message);
        ReadSlots();
        Refresh();
    }

    private bool CanSaveSelected(out string reason) // 저장 가능 여부와 사유
    {
        if (!allowSave)
        {
            reason = "저장은 지역 선택 화면에서 들어왔을 때만 할 수 있습니다.";
            return false;
        }

        if (!SaveRules.IsManual(selectedSlot))
        {
            reason = "자동 저장 칸에는 직접 저장할 수 없습니다.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    // 게임을 진행하던 중이면 지금 진행이 바뀌므로 한 번 더 눌러야 불러온다.
    private void OnLoadButton()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        SaveSlotInfo info = GetInfo(selectedSlot);

        if (info == null || !info.IsValid)
        {
            pendingAction = PendingAction.None;

            ShowMessage(info != null && info.Exists
                ? info.Error
                : "불러올 저장 데이터가 없습니다.");

            Refresh();
            return;
        }

        if (progress.IsSessionActive && pendingAction != PendingAction.Load)
        {
            pendingAction = PendingAction.Load;
            ShowMessage("지금 진행을 이 저장 데이터로 바꿉니다. 한 번 더 누르면 불러옵니다.");
            Refresh();
            return;
        }

        pendingAction = PendingAction.None;

        progress.LoadFromSlot(selectedSlot, out string message);

        ShowMessage(message);
        ReadSlots();
        Refresh();
    }

    // 삭제는 두 단계로 확인한다. 백업 파일도 함께 지운다. (기획서 15.28)
    private void OnDeleteButton()
    {
        SaveSlotInfo info = GetInfo(selectedSlot);

        if (info == null || !info.Exists)
        {
            pendingAction = PendingAction.None;
            ShowMessage("삭제할 저장 데이터가 없습니다.");
            Refresh();
            return;
        }

        if (pendingAction != PendingAction.Delete &&
            pendingAction != PendingAction.DeleteFinal)
        {
            pendingAction = PendingAction.Delete;
            ShowMessage("선택한 저장 데이터를 삭제하시겠습니까? 삭제 버튼을 다시 누르세요.");
            Refresh();
            return;
        }

        if (pendingAction == PendingAction.Delete)
        {
            pendingAction = PendingAction.DeleteFinal;
            ShowMessage("삭제한 저장 데이터는 복구할 수 없습니다. 한 번 더 누르면 삭제합니다.");
            Refresh();
            return;
        }

        pendingAction = PendingAction.None;

        bool deleted = SaveFileSystem.Delete(selectedSlot, out string error);

        ShowMessage(deleted
            ? $"{SaveRules.GetSlotName(selectedSlot)} 삭제 완료"
            : error);

        if (deleted)
        {
            Debug.Log($"저장 삭제: {SaveRules.GetSlotName(selectedSlot)}"); // 삭제 기록
        }

        ReadSlots();
        Refresh();
    }
}
