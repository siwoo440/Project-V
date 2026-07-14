using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(fileName = "NewHeroineActionData", menuName = "Project V/Heroine Action Data")] // 히로인 행동 데이터 생성 메뉴
public class HeroineActionData : ScriptableObject // 히로인 행동 데이터 정의
{ // 클래스 시작
    [Header("Action Information")] // 행동 기본 정보 구분
    [SerializeField] private string actionId = "HA000"; // 행동 고유 ID
    [SerializeField] private string displayName = "New Action"; // 행동 표시 이름
    [SerializeField] private HeroineActionType actionType = HeroineActionType.SingleAttack; // 행동 종류
    [SerializeField] private HeroineTargetType targetType = HeroineTargetType.FirstMonster; // 행동 대상 규칙
    [SerializeField] private int damage = 1; // 행동 피해량
    [SerializeField] private int shieldAmount = 0; // 행동 보호막 획득량
    [SerializeField] private int healAmount; // 체력 회복량
    [SerializeField] private int weight = 1; // 행동 선택 가중치

    [Header("AI Restrictions")] // AI 제약 조건 구분
    [Min(0)] // 쿨타임 최소값 제한
    [SerializeField] private int cooldownTurns = 0; // 행동 사용 후 쿨타임
    [Min(1)] // 연속 사용 최소값 제한
    [SerializeField] private int maxConsecutiveUses = 1; // 최대 연속 사용 횟수

    [Header("HP Condition")] // HP 조건 구분
    [Range(0f, 1f)] // 최소 HP 비율 범위
    [SerializeField] private float minimumHpRatio = 0f; // 최소 HP 비율
    [Range(0f, 1f)] // 최대 HP 비율 범위
    [SerializeField] private float maximumHpRatio = 1f; // 최대 HP 비율

    public string ActionId => actionId; // 행동 ID 반환
    public string DisplayName => displayName; // 행동 이름 반환
    public HeroineActionType ActionType => actionType; // 행동 종류 반환
    public HeroineTargetType TargetType => targetType; // 행동 대상 규칙 반환
    public int Damage => damage; // 행동 피해량 반환
    public int ShieldAmount => shieldAmount; // 보호막 획득량 반환
    public int HealAmount => healAmount; // 체력 회복량 반환
    public int Weight => weight; // 행동 가중치 반환
    public int CooldownTurns => cooldownTurns; // 행동 쿨타임 반환
    public int MaxConsecutiveUses => maxConsecutiveUses; // 최대 연속 사용 횟수 반환
    public bool IsAvailable(float currentHpRatio) // 현재 HP 조건 확인
    { // 메서드 시작
        return currentHpRatio > minimumHpRatio && currentHpRatio <= maximumHpRatio; // 행동 사용 가능 여부 반환
    } // 메서드 끝
} // 클래스 끝