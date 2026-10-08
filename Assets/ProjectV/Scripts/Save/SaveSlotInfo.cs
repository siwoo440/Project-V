using System; // 날짜와 시간

// 저장 칸 하나를 읽은 결과. 저장 화면의 표시와 이어하기 대상 선택에 쓴다.
public class SaveSlotInfo
{
    public SaveSlot Slot { get; }     // 읽은 칸
    public bool Exists { get; }       // 저장 파일이 있는지 여부
    public bool IsValid { get; }      // 불러올 수 있는지 여부
    public bool UsedBackup { get; }   // 본 파일에 문제가 있어 백업을 읽었는지 여부
    public string Error { get; }      // 불러올 수 없는 이유
    public SaveData Data { get; }     // 읽은 진행 데이터
    public DateTime SavedAt { get; }  // 저장 일시 (가장 최근 칸을 고를 때 쓴다)

    public SaveSlotInfo(
        SaveSlot slot,
        bool exists,
        bool isValid,
        bool usedBackup,
        string error,
        SaveData data
    )
    {
        Slot = slot;
        Exists = exists;
        IsValid = isValid;
        UsedBackup = usedBackup;
        Error = error ?? string.Empty;
        Data = data;

        SavedAt =
            data != null && SaveRules.TryParseSavedAt(data.savedAt, out DateTime savedAt)
                ? savedAt
                : DateTime.MinValue;
    }
}
