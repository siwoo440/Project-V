using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(fileName = "NewStatusEffectData", menuName = "Project V/Status Effect Data")] // 상태 효과 데이터 생성 메뉴
public class StatusEffectData : ScriptableObject // 상태 효과 데이터 정의
{
    [Header("Status Information")] // 상태 효과 기본 정보
    [SerializeField] private string statusId = "ST000"; // 상태 효과 ID
    [SerializeField] private string displayName = "New Status"; // 상태 효과 표시 이름
    [SerializeField] private StatusEffectType statusType = StatusEffectType.DefenseUp; // 상태 효과 종류
    [SerializeField] private int amount = 1; // 상태 효과 적용량
    [SerializeField] private bool isNegative; // 해로운 상태 효과 여부


    [Header("Duration")] // 지속시간 정보
    [Min(1)] // 최소 지속시간 제한
    [SerializeField] private int durationTurns = 1; // 상태 효과 지속 턴
    [SerializeField] private StatusDurationTiming durationTiming = StatusDurationTiming.AfterPlayerTurn; // 지속시간 감소 시점
  
    [Header("UI Display")] // 상태 효과 UI 표시 정보
    [SerializeField] private Sprite icon; // 상태 효과 아이콘
    [SerializeField] private Color displayColor = Color.white; // 상태 효과 표시 색상

    public string StatusId => statusId; // 상태 효과 ID 반환
    public string DisplayName => displayName; // 상태 효과 이름 반환
    public StatusEffectType StatusType => statusType; // 상태 효과 종류 반환
    public int Amount => amount; // 상태 효과 적용량 반환
    public bool IsNegative => isNegative; // 해로운 상태 효과 여부 반환
    public int DurationTurns => durationTurns; // 상태 효과 지속시간 반환

    public StatusDurationTiming DurationTiming => durationTiming; // 지속시간 감소 시점 반환
    public Sprite Icon => icon; // 상태 효과 아이콘 반환
    public Color DisplayColor => displayColor; // 상태 효과 표시 색상 반환
}