public enum HeroineArtState // 전투 화면에 보여 주는 히로인 그림의 상태 (기획서 12.4 / 12.5)
{
    Normal,   // 기본 자세
    Hit,      // 가벼운 피격: HP 피해를 조금 받음
    HeavyHit, // 강한 피격: HP 피해를 크게 받음
    Guard,    // 방어 성공: 보호막이 피해를 모두 막음
    LustHit,  // 성욕 공격 피격
    Shaken,   // 동요 상태 (성욕 50 이상)의 평소 모습
    ShakenLustHit, // 동요 상태에서 성욕 공격 피격
    Defeat,   // HP가 0이 되어 패배
    Climax,   // 성욕이 최대가 되어 패배
    Attack    // 공격 행동
}
