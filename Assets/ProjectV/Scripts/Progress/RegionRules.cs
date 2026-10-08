// 지역 규칙 (기획서 4.4 / 8.3 / F.2)
public static class RegionRules
{
    public const int FinalOrder = 9; // 최종장 지역의 순서 (마계와 심연)

    // 임시 규칙: 지역 안의 스테이지 구조가 생기기 전까지는 그 지역에서 한 번 승리하면 클리어로 본다.
    // 기획서 4.14.1의 조건은 주요 히로인 3단계 전투 승리다. 히로인전 3단계를 만드는 일차에 바꾼다.
    public static readonly bool ClearOnAnyVictory = true;

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
