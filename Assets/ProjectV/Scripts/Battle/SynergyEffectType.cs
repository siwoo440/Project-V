public enum SynergyEffectType // 시너지 효과 종류
{
    None = 0,                    // 효과 없음
    ModifyMaxHp = 1,             // 지속: 최대 체력 증가
    ModifyAttack = 2,            // 지속: 공격력 증가
    ModifyDefense = 3,           // 지속: 방어력 증가
    ModifyLustDamage = 4,        // 지속: 욕정 부여량 증가
    SummonGainShield = 5,        // 발동: 소환 시 보호막 획득
    TurnStartHealPlayer = 6,     // 발동: 턴 시작 시 플레이어 회복
    TurnStartReduceCooldown = 7, // 발동: 턴 시작 시 대기시간 추가 감소
    TurnEndReduceCooldown = 8,   // 발동: 턴 종료 시 대기시간 추가 감소
    FirstSummonRestoreMana = 9,  // 발동: 턴당 1회 소환 시 마나 회복
    FirstDeathDrawCard = 10,     // 발동: 턴당 1회 사망 시 카드 드로우
}
