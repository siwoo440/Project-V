using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Project V/마물 데이터")] // 마물 데이터 생성 메뉴
public class MonsterData : ScriptableObject // 마물 데이터 정의
{
    [Header("마물 정보")] // 마물 정보 구분
    [SerializeField] private string monsterId = "M000"; // 마물 고유 ID
    [SerializeField] private string monsterName = "새 마물"; // 마물 표시 이름
    [SerializeField] private CardRarity rarity = CardRarity.Common; // 마물 희귀도
    [SerializeField] private MonsterType mainType = MonsterType.None; // 마물 주 타입
    [SerializeField] private MonsterType subType = MonsterType.None; // 마물 보조 타입
    [SerializeField] private bool isBossOrigin; // 보스 출신 여부
    [SerializeField] private bool isToken; // 토큰 마물 여부
    [SerializeField] private int maxHp = 5; // 마물 최대 체력
    [SerializeField] private int attack = 1; // 마물 공격력
    [SerializeField, Min(0)] private int lustDamage = 5; // 성욕 피해
    [SerializeField] private int defense = 0; // 마물 방어력
    [SerializeField] private int startingShield = 0; // 마물 시작 보호막

    [Header("레벨 성장")]
    [SerializeField, Min(0)] private int hpGrowthPerLevel = 1;
    [SerializeField, Min(0)] private int attackGrowthPerLevel = 1;
    [SerializeField, Min(0)] private int lustGrowthPerLevel = 1;
    [SerializeField, Min(0)] private int defenseGrowthPerLevel = 0;

    [Header("대상 규칙")]
    [SerializeField] private bool isTaunting;

    [Header("고유 효과")] // 마물 고유 효과 구분
    [SerializeField]
    private List<MonsterEffectData> effects =
        new List<MonsterEffectData>(); // 마물 고유 효과 목록

    [Header("카드 연결")] // 카드 연결 구분
    [SerializeField] private CardData captureRewardCard; // 포획 보상 카드

    [Header("공격 상태 효과")] // 공격 상태 효과 구분
    [SerializeField] private StatusEffectData attackStatusEffect; // 공격 시 부여 상태 효과

    public string MonsterId => monsterId; // 마물 ID 반환
    public string MonsterName => monsterName; // 마물 이름 반환
    public CardRarity Rarity => rarity; // 마물 희귀도 반환
    public MonsterType MainType => mainType; // 주 타입 반환
    public MonsterType SubType => subType; // 보조 타입 반환
    public bool IsBossOrigin => isBossOrigin; // 보스 출신 여부 반환
    public bool IsToken => isToken; // 토큰 마물 여부 반환

    public IReadOnlyList<MonsterEffectData> Effects => effects; // 고유 효과 목록 반환

    public MonsterEffectData GetEffect(MonsterEffectTrigger trigger) // 발동 시점별 효과 반환
    {
        foreach (MonsterEffectData effect in effects)
        {
            if (effect == null) { continue; }
            if (effect.Trigger != trigger) { continue; }

            return effect;
        }

        return null;
    }

    public bool HasType(MonsterType monsterType) // 타입 보유 여부
    {
        if (monsterType == MonsterType.None) { return false; }

        return mainType == monsterType || subType == monsterType;
    }
    public CardData CaptureRewardCard => captureRewardCard; // 포획 보상 카드 반환

    public int MaxOwnedCopies =>
        CardRarityRules.GetMaxCopies(rarity); // 희귀도별 보유 한도

    public int DuplicateEssenceReward =>
        CardRarityRules.GetEssenceReward(rarity); // 초과 변환량
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