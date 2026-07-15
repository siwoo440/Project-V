public class BattleLogEntry // 전투 로그 한 항목
{
    public int TurnNumber { get; } // 기록된 턴 번호
    public BattleLogCategory Category { get; } // 로그 분류
    public string Message { get; } // 로그 원본 메시지

    public BattleLogEntry(int turnNumber, BattleLogCategory category, string message) // 전투 로그 항목 생성
    {
        TurnNumber = turnNumber; // 턴 번호 저장
        Category = category; // 로그 분류 저장
        Message = message; // 로그 메시지 저장
    }
}