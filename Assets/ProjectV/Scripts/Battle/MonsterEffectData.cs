using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(
    fileName = "NewMonsterEffectData",
    menuName = "Project V/마물 효과 데이터"
)] // 마물 효과 데이터 생성 메뉴
public class MonsterEffectData : ScriptableObject // 마물 고유 효과 정의
{
    [Header("효과 정보")]
    [SerializeField] private string effectId = "EF000";   // 효과 ID
    [SerializeField] private string displayName = "새 효과"; // 효과 표시 이름

    [TextArea(2, 4)]
    [SerializeField] private string description;          // 효과 설명

    [Header("발동 규칙")]
    [SerializeField]
    private MonsterEffectTrigger trigger = MonsterEffectTrigger.Summon; // 발동 시점

    [SerializeField]
    private MonsterEffectType effectType = MonsterEffectType.None; // 효과 종류

    [SerializeField, Min(0)] private int cooldownTurns; // 재사용 대기시간

    [Header("효과 수치")]
    [SerializeField, Min(0)] private int amount = 1;          // 기본 수치
    [SerializeField, Min(0)] private int secondaryAmount;     // 추가 수치
    [SerializeField, Min(0)] private int maximumAmount;       // 수치 상한, 0이면 제한 없음
    [SerializeField, Min(0)] private int playerHpCost;        // 플레이어 HP 소비량

    [Header("조건")]
    [SerializeField]
    private MonsterType requiredAllyType = MonsterType.None; // 조건 및 대상 타입

    [SerializeField] private bool requireOtherAlly; // 다른 아군 필요 여부

    [SerializeField]
    private bool scaleByAllyCount; // 조건 아군 수만큼 수치 증가

    [Header("참조 데이터")]
    [SerializeField] private StatusEffectData statusEffect; // 부여할 상태 효과
    [SerializeField] private MonsterData tokenMonster;      // 소환할 토큰 마물

    public string EffectId => effectId;                       // 효과 ID 반환
    public string DisplayName => displayName;                 // 효과 이름 반환
    public string Description => description;                 // 효과 설명 반환
    public MonsterEffectTrigger Trigger => trigger;           // 발동 시점 반환
    public MonsterEffectType EffectType => effectType;        // 효과 종류 반환
    public int CooldownTurns => Mathf.Max(0, cooldownTurns);  // 재사용 대기시간 반환
    public int Amount => Mathf.Max(0, amount);                // 기본 수치 반환
    public int SecondaryAmount => Mathf.Max(0, secondaryAmount); // 추가 수치 반환
    public int MaximumAmount => Mathf.Max(0, maximumAmount);  // 수치 상한 반환
    public int PlayerHpCost => Mathf.Max(0, playerHpCost);    // HP 소비량 반환
    public MonsterType RequiredAllyType => requiredAllyType;  // 조건 타입 반환
    public bool RequireOtherAlly => requireOtherAlly;         // 다른 아군 필요 여부 반환
    public bool ScaleByAllyCount => scaleByAllyCount;         // 아군 수 비례 여부 반환
    public StatusEffectData StatusEffect => statusEffect;     // 상태 효과 반환
    public MonsterData TokenMonster => tokenMonster;          // 토큰 마물 반환

    public int ResolveAmount(int allyCount) // 조건 아군 수를 반영한 최종 수치
    {
        int resolvedAmount = scaleByAllyCount
            ? Amount * Mathf.Max(0, allyCount)
            : Amount;

        if (MaximumAmount > 0)
        {
            resolvedAmount = Mathf.Min(resolvedAmount, MaximumAmount);
        }

        return resolvedAmount;
    }
}
