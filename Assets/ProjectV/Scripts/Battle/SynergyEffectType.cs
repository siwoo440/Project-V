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
    TurnStartLustByCount = 11,   // 발동: 턴 시작 시 그 계열 마물 수만큼 히로인 성욕 증가. 수치는 동요 상태일 때의 추가량
    SummonReady = 12,            // 발동: 그 계열 마물은 소환된 턴에도 행동 가능
    TurnStartAllyBuff = 13,      // 발동: 턴 시작 시 모든 아군이 그 턴 동안 공격력과 성욕 부여량 증가
    SurviveLethal = 14,          // 발동: 전투당 1회 플레이어가 쓰러질 피해를 HP 1로 버티고, 그 계열 마물이 수치만큼 보호막 획득
    FirstSummonCostDown = 15,    // 발동: 매 턴 처음 소환하는 그 계열 마물의 마나 비용 감소
    ActedDrawCard = 16,          // 발동: 한 턴에 그 계열 마물 3체가 행동을 마치면 카드 드로우. 턴당 1회
}
