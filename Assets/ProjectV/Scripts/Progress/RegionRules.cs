// 지역 규칙 (기획서 4.4 / 8.3 / F.2)
public static class RegionRules
{
    public const int FinalOrder = 9; // 최종장 지역의 순서 (마계와 심연)

    // 임시 규칙: 기획서 4.14.1의 지역 클리어 조건은 주요 히로인 3단계 전투 승리다.
    // 히로인전 3단계가 생기기 전까지는 일반전 마지막 단계를 이기면 클리어로 본다.
    // 일반전 편성이 없는 지역은 어느 전투든 한 번 이기면 클리어로 본다. (지역 화면이 전투를 시작할 때 정한다)
    public const string TemporaryClearHint =
        "시험 규칙: 일반전 마지막 단계를 이기면 클리어됩니다. 일반전이 없는 지역은 한 번 승리하면 클리어됩니다.";

    public static string GetChapterName(int order) // 지역 순서에 해당하는 챕터 이름 (기획서 8.1)
    {
        return order >= FinalOrder
            ? "최종장"
            : $"챕터 {order}";
    }

    public static string GetStateName(RegionState state) // 지역 상태 표시 문구
    {
        switch (state)
        {
            case RegionState.Locked: return "잠김";
            case RegionState.New: return "새로 해금";
            case RegionState.InProgress: return "진행 중";
            case RegionState.Cleared: return "클리어";
            default: return "알 수 없음";
        }
    }
}
