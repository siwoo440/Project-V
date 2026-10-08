// 소모성 전투 아이템의 효과 종류 (기획서 9.12.3)
public enum BattleItemType
{
    HealPlayer = 0,    // 플레이어 HP 회복
    RestoreMana = 1,   // 현재 마나 회복 (최대 마나 이내)
    ShieldMonster = 2, // 선택한 아군 마물에게 보호막
    DrawCards = 3,     // 카드 드로우
    CleanseAllies = 4, // 아군 전체의 해로운 상태 효과 제거
}
