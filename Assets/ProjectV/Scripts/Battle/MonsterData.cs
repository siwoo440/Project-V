using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Project V/Monster Data")] // 마물 데이터 생성 메뉴
public class MonsterData : ScriptableObject // 마물 데이터 정의
{
    [Header("Monster Information")] // 마물 정보 구분
    [SerializeField] private string monsterId = "M000"; // 마물 고유 ID
    [SerializeField] private string monsterName = "New Monster"; // 마물 표시 이름
    [SerializeField] private int maxHp = 5; // 마물 최대 체력
    [SerializeField] private int attack = 1; // 마물 공격력
    [SerializeField, Min(0)] private int lustDamage = 5; // 성욕 피해
    [SerializeField] private int defense = 0; // 마물 방어력
    [SerializeField] private int startingShield = 0; // 마물 시작 보호막

    [Header("Level Growth")]
    [SerializeField, Min(0)] private int hpGrowthPerLevel = 1;
    [SerializeField, Min(0)] private int attackGrowthPerLevel = 1;
    [SerializeField, Min(0)] private int lustGrowthPerLevel = 1;
    [SerializeField, Min(0)] private int defenseGrowthPerLevel = 0;

    [Header("Target Rules")]
    [SerializeField] private bool isTaunting;

    [Header("Attack Status Effect")] // 공격 상태 효과 구분
    [SerializeField] private StatusEffectData attackStatusEffect; // 공격 시 부여 상태 효과

    public string MonsterId => monsterId; // 마물 ID 반환
    public string MonsterName => monsterName; // 마물 이름 반환
    public int MaxHp => maxHp; // 마물 최대 체력 반환
    public int Attack => attack; // 마물 공격력 반환
    public int LustDamage => lustDamage; // 성욕 피해 반환
    public int Defense => defense; // 마물 방어력 반환
    public int StartingShield => startingShield; // 마물 시작 보호막 반환
    public bool IsTaunting => isTaunting; // 도발 상태 반환
    public StatusEffectData AttackStatusEffect => attackStatusEffect; // 공격 상태 효과 반환
    public int HpGrowthPerLevel => hpGrowthPerLevel;
    public int AttackGrowthPerLevel => attackGrowthPerLevel;
    public int LustGrowthPerLevel => lustGrowthPerLevel;
    public int DefenseGrowthPerLevel => defenseGrowthPerLevel;

} 