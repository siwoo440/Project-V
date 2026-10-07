// 그리모어 노드의 효과 종류. 숫자는 데이터 에셋에 저장되므로 바꾸지 않는다.
// "주력 계열"은 덱에 가장 많은 계열을 뜻한다.
public enum GrimoireEffectType
{
    // 계약
    PlayerMaxHp = 0,     // 플레이어 최대 HP 증가
    StartingShield = 1,  // 전투 시작 보호막 증가
    RegenContract = 2,   // 전투마다 한 번, HP가 절반 이하가 되면 회복
    TurnShield = 3,      // 턴 시작 시 보호막이 없으면 보호막 획득
    LastStand = 4,       // 전투마다 한 번, 쓰러질 피해를 받으면 버틴다

    // 소환
    SummonMaxHp = 10,     // 소환한 마물 최대 HP 증가
    SummonShield = 11,    // 소환한 마물 보호막 획득
    DeathShield = 12,     // 마물이 쓰러지면 플레이어 보호막 (턴마다 한 번)
    TurnHealMonster = 13, // 턴 시작 시 가장 많이 다친 마물 회복
    VanguardHp = 14,      // 전투마다 처음 소환한 마물 최대 HP 증가

    // 지휘
    AttackOrder = 20,  // 모든 마물 공격 증가
    SwiftOrder = 21,   // 수치형 소환사 스킬 효과 증가
    LegionOrder = 22,  // 필드 마물이 4체 이상이면 공격 증가
    StandbyOrder = 23, // 턴 종료 시 행동하지 않은 마물 수만큼 보호막 (상한)
    FinishOrder = 24,  // 히로인 HP가 절반 이하이면 공격 증가

    // 마나
    ManaVessel = 30,    // 전투 첫 턴 임시 마나
    ManaRecovery = 31,  // 마물이 쓰러지면 다음 턴 임시 마나 (턴마다 상한)
    CheapContract = 32, // 전투마다 처음 내는 카드 비용 감소
    ManaReserve = 33,   // 남은 마나를 다음 턴으로 넘김 (상한)
    SpareMana = 34,     // 턴 시작 시 손패가 적으면 임시 마나 (손패 기준)

    // 기억
    StartingHand = 40, // 시작 손패 증가
    Rewrite = 41,      // 몇 턴마다 추가 드로우 (턴 간격)
    CycleRecord = 42,  // 덱을 다시 섞을 때 추가 드로우
    FarewellDraw = 43, // 마물이 쓰러지면 드로우 (전투마다 횟수)
    SkillDraw = 44,    // 소환사 스킬을 쓰면 드로우 (전투마다 횟수)

    // 욕망
    LustResonance = 50,    // 모든 마물 성욕 부여량 증가
    ClimaxChase = 51,      // 히로인이 동요 상태이면 성욕 부여량 증가
    TemptationRecord = 52, // 주력 계열 마물 성욕 부여량 증가
    FirstTemptation = 53,  // 턴마다 처음 하는 성욕 공격 강화
    Afterglow = 54,        // 턴 종료 시 히로인 성욕 증가

    // 계열
    MainTypeAttack = 60, // 주력 계열 마물 공격 증가
    MainTypeHp = 61,     // 주력 계열 마물 최대 HP 증가
    SynergyDraw = 62,    // 시너지 단계가 새로 켜지면 드로우 (전투마다 횟수)
    MixedHeal = 63,      // 계열이 3종 이상이면 턴 시작 시 플레이어 회복
    TypeFocus = 64,      // 주력 계열 마물이 일정 수 이상이면 시너지 계산 수 증가 (필요 수)

    // 포획
    EssenceExtract = 70,  // 중복 포획 정수 증가
    CaptureSkill = 71,    // 포획 확률 증가 (%)
    LootAppraisal = 72,   // 승리 골드 증가 (%)
    BattleRecord = 73,    // 승리 경험치 증가 (%)
    EssenceCondense = 74, // 승리 시 정수 획득
}
