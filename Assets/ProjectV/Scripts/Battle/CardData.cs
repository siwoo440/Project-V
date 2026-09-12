using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(fileName = "NewCardData", menuName = "Project V/카드 데이터")] // 카드 데이터 생성 메뉴
public class CardData : ScriptableObject // 카드 데이터 정의
{ // 클래스 시작
    [Header("카드 정보")] // 카드 정보 구분
    [SerializeField] private string cardId = "C000"; // 카드 고유 ID
    [SerializeField] private string cardName = "새 카드"; // 카드 표시 이름
    [SerializeField] private int manaCost = 1; // 카드 마나 비용
    [SerializeField] private MonsterData summonMonster; // 소환 마물 데이터

    public string CardId => cardId; // 카드 ID 반환
    public string CardName => cardName; // 카드 이름 반환
    public int ManaCost => manaCost; // 카드 마나 비용 반환
    public MonsterData SummonMonster => summonMonster; // 소환 마물 반환

    public CardRarity Rarity =>
        summonMonster == null
            ? CardRarity.Common
            : summonMonster.Rarity; // 소환 마물 희귀도 반환

    public MonsterType MainType =>
        summonMonster == null
            ? MonsterType.None
            : summonMonster.MainType; // 소환 마물 주 타입 반환

    public MonsterType SubType =>
        summonMonster == null
            ? MonsterType.None
            : summonMonster.SubType; // 소환 마물 보조 타입 반환

    public bool IsBossOrigin =>
        summonMonster != null &&
        summonMonster.IsBossOrigin; // 보스 출신 여부 반환

    public int MaxCopies =>
        CardRarityRules.GetMaxCopies(Rarity); // 희귀도별 보유 및 편성 한도

    public int EssenceReward =>
        CardRarityRules.GetEssenceReward(Rarity); // 초과 변환량
} // 클래스 끝