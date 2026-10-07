using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(
    fileName = "NewSummonerSkillData",
    menuName = "Project V/소환사 액티브 스킬 데이터"
)] // 소환사 액티브 스킬 데이터 생성 메뉴
public class SummonerSkillData : ScriptableObject // 소환사 액티브 스킬 정의 (기획서 6.4)
{
    [Header("스킬 정보")]
    [SerializeField] private string skillId = "SSK-00";      // 스킬 고유 ID
    [SerializeField] private string displayName = "새 스킬"; // 스킬 표시 이름

    [SerializeField]
    private SummonerSkillType skillType = SummonerSkillType.FocusCommand; // 스킬 종류

    [Header("사용 조건")]
    [SerializeField, Min(1)] private int unlockLevel = 1; // 해금 플레이어 레벨
    [SerializeField, Min(0)] private int manaCost = 1;    // 마나 비용

    [Header("효과")]
    [SerializeField, Min(0)] private int amount = 1; // 효과 수치

    public string SkillId => skillId;               // 스킬 ID 반환
    public string DisplayName => displayName;       // 스킬 이름 반환
    public SummonerSkillType SkillType => skillType; // 스킬 종류 반환
    public int UnlockLevel => Mathf.Max(1, unlockLevel); // 해금 레벨 반환
    public int ManaCost => Mathf.Max(0, manaCost);  // 마나 비용 반환
    public int Amount => Mathf.Max(0, amount);      // 효과 수치 반환

    public bool NeedsTarget => // 대상 마물 선택이 필요한지 여부
        skillType == SummonerSkillType.FocusCommand ||
        skillType == SummonerSkillType.EmergencyReturn ||
        skillType == SummonerSkillType.AbsoluteCommand;

    public string EffectText => GetEffectText(0); // 수치를 반영한 효과 설명

    // bonusAmount: 그리모어 강화로 더해지는 수치
    public string GetEffectText(int bonusAmount)
    {
        int shownAmount = Amount + Mathf.Max(0, bonusAmount);

        switch (skillType)
        {
            case SummonerSkillType.FocusCommand:
                return $"행동 가능한 마물 하나의 이번 턴 공격 +{shownAmount}";

            case SummonerSkillType.EmergencyDraw:
                return $"카드 {shownAmount}장 드로우";

            case SummonerSkillType.ManaCycle:
                return $"이번 턴 임시 마나 {shownAmount} 획득";

            case SummonerSkillType.ContractShield:
                return $"플레이어 보호막 +{shownAmount}";

            case SummonerSkillType.EmergencyReturn:
                return "아군 마물 하나를 손패로 반환";

            case SummonerSkillType.LustResonance:
                return $"이번 턴 모든 아군의 성욕 부여량 +{shownAmount}";

            case SummonerSkillType.AbsoluteCommand:
                return "행동을 마친 마물 하나가 다시 행동";

            default:
                return string.Empty;
        }
    }
}
