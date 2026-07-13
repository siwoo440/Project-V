using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(fileName = "NewCardData", menuName = "Project V/Card Data")] // 카드 데이터 생성 메뉴
public class CardData : ScriptableObject // 카드 데이터 정의
{ // 클래스 시작
    [Header("Card Information")] // 카드 정보 구분
    [SerializeField] private string cardId = "C000"; // 카드 고유 ID
    [SerializeField] private string cardName = "New Card"; // 카드 표시 이름
    [SerializeField] private int manaCost = 1; // 카드 마나 비용
    [SerializeField] private MonsterData summonMonster; // 소환 마물 데이터

    public string CardId => cardId; // 카드 ID 반환
    public string CardName => cardName; // 카드 이름 반환
    public int ManaCost => manaCost; // 카드 마나 비용 반환
    public MonsterData SummonMonster => summonMonster; // 소환 마물 반환
} // 클래스 끝