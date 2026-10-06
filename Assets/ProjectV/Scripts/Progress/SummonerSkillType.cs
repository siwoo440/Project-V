public enum SummonerSkillType // 소환사 액티브 스킬 종류 (기획서 6.4.2)
{
    FocusCommand = 0,    // 집중 명령: 선택한 마물의 이번 턴 ATK 증가
    EmergencyDraw = 1,   // 긴급 드로우: 카드 드로우
    ManaCycle = 2,       // 마나 순환: 이번 턴 임시 마나 획득
    ContractShield = 3,  // 계약 보호막: 플레이어에게 보호막 부여
    EmergencyReturn = 4, // 긴급 귀환: 아군 마물 하나를 손패로 반환
    LustResonance = 5,   // 욕망 공명: 이번 턴 아군의 성욕 부여량 증가
    AbsoluteCommand = 6, // 절대 명령: 행동을 마친 마물 하나가 다시 행동
}
