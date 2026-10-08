using System; // 날짜와 시간
using System.Globalization; // 날짜 형식

// 저장 규칙 (기획서 10.5 / 15.2 / 15.9)
public static class SaveRules
{
    // 저장 데이터 버전. 저장 항목의 구조가 바뀌면 올리고 Upgrade에 변환을 추가한다.
    public const int CurrentVersion = 1;

    public const string FormatName = "ProjectV.Save"; // 저장 파일 구분 문구

    private const string SavedAtFormat = "yyyy-MM-ddTHH:mm:ss"; // 저장 일시 기록 형식

    // 저장 화면에 보여 주는 순서 (기획서 15.3)
    public static readonly SaveSlot[] Slots =
    {
        SaveSlot.Auto, SaveSlot.Manual1, SaveSlot.Manual2, SaveSlot.Manual3
    };

    public static bool IsManual(SaveSlot slot) // 플레이어가 직접 저장하는 칸인지 여부
    {
        return slot != SaveSlot.Auto;
    }

    public static string GetSlotName(SaveSlot slot) // 칸 표시 이름
    {
        return slot == SaveSlot.Auto
            ? "자동 저장"
            : $"수동 저장 {(int)slot}";
    }

    public static string GetFileName(SaveSlot slot) // 칸의 파일 이름 (확장자 제외)
    {
        return slot == SaveSlot.Auto
            ? "auto"
            : $"manual{(int)slot}";
    }

    public static string CreateSavedAt() // 지금 시각을 저장 일시 문자열로 만든다.
    {
        return DateTime.Now.ToString(SavedAtFormat, CultureInfo.InvariantCulture);
    }

    public static bool TryParseSavedAt(string text, out DateTime savedAt) // 저장 일시 읽기
    {
        return DateTime.TryParseExact(
            text, SavedAtFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out savedAt
        );
    }

    public static string FormatSavedAt(string text) // 저장 일시 표시 문구
    {
        return TryParseSavedAt(text, out DateTime savedAt)
            ? savedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
            : "저장 일시 알 수 없음";
    }

    public static string FormatPlayTime(double seconds) // 플레이 시간 표시 문구
    {
        int totalSeconds = seconds <= 0d ? 0 : (int)Math.Min(seconds, int.MaxValue);
        int hours = totalSeconds / 3600;
        int minutes = totalSeconds % 3600 / 60;

        return hours > 0
            ? $"{hours}시간 {minutes:00}분"
            : $"{minutes}분 {totalSeconds % 60:00}초";
    }

    // 예전 버전의 저장 데이터를 지금 구조로 바꾼다. (기획서 15.9)
    // 지금은 버전 1뿐이라 바꿀 내용이 없다. 버전을 올릴 때 여기에 단계별 변환을 넣는다.
    public static bool Upgrade(SaveData data, out string error)
    {
        if (data == null)
        {
            error = "저장 데이터가 비어 있습니다.";
            return false;
        }

        if (data.version > CurrentVersion)
        {
            error = $"더 새로운 버전에서 만든 저장 데이터입니다. (저장 버전 {data.version})";
            return false;
        }

        if (data.version < 1)
        {
            error = "저장 버전을 알 수 없습니다.";
            return false;
        }

        data.FillMissing(); // 파일에 없던 항목을 빈 값으로 채운다.
        data.version = CurrentVersion;
        error = string.Empty;

        return true;
    }
}
