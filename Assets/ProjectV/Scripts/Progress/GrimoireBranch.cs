// 그리모어 강화 분기 (기획서 6.6.4)
public enum GrimoireBranch
{
    Contract = 0, // 계약: 플레이어 HP와 보호막
    Summon = 1,   // 소환: 소환 마물의 HP와 생존력
    Command = 2,  // 지휘: 마물 공격과 행동 보조
    Mana = 3,     // 마나: 시작 마나와 임시 마나
    Memory = 4,   // 기억: 드로우와 손패 관리
    Desire = 5,   // 욕망: 성욕 부여와 절정 공략
    Lineage = 6,  // 계열: 마물 시너지 효과
    Capture = 7,  // 포획: 중복 포획과 재료 획득
}
