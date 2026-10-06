public enum SummonerPassiveType // 소환사 패시브 종류 (기획서 6.5.3)
{
    LifeContract = 0,     // 생명 계약: 플레이어 최대 HP 증가
    QuickStudy = 1,       // 빠른 이해: 첫 턴 추가 카드 드로우
    ThriftySummon = 2,    // 절약 소환: 전투당 첫 저비용 마물의 비용 감소
    SturdyContract = 3,   // 강인한 계약: 플레이어가 받는 피해 감소
    LustAmplify = 4,      // 욕망 증폭: 모든 마물의 성욕 부여량 증가
    LegionCommand = 5,    // 군단 지휘: 필드의 마물 수에 따라 효과 획득
    TypeFocus = 6,        // 계열 집중: 가장 많이 편성한 계열의 시너지 강화
    RecycleKnowledge = 7, // 재활용 지식: 덱을 다시 섞을 때 추가 카드 드로우
    MerchantSense = 8,    // 상인의 감각: 상점 구매 가격 감소 (상점 구현 후 연결)
    CaptureRecord = 9,    // 포획 기록: 중복 포획 시 획득하는 재료 증가
}
