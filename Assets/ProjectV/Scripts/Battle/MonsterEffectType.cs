public enum MonsterEffectType // 마물 효과 종류
{
    None = 0,               // 효과 없음
    DrawCard = 1,           // 카드 드로우
    RestoreMana = 2,        // 마나 회복
    GainShield = 3,         // 보호막 획득
    CopyShieldFromLeft = 4, // 왼쪽 마물 보호막 복사
    BuffAllyAttack = 5,     // 아군 공격력 증가
    HealAlly = 6,           // 아군 회복
    CleanseAllyDebuff = 7,  // 아군 디버프 제거
    AddHeroineLust = 8,     // 히로인 성욕 부여
    HealPlayer = 9,         // 플레이어 회복
    SummonToken = 10,       // 토큰 마물 소환
}
