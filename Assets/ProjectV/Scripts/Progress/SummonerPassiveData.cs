using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(
    fileName = "NewSummonerPassiveData",
    menuName = "Project V/소환사 패시브 데이터"
)] // 소환사 패시브 데이터 생성 메뉴
public class SummonerPassiveData : ScriptableObject // 소환사 패시브 정의 (기획서 6.5)
{
    [Header("패시브 정보")]
    [SerializeField] private string passiveId = "SPV-00";      // 패시브 고유 ID
    [SerializeField] private string displayName = "새 패시브"; // 패시브 표시 이름

    [SerializeField]
    private SummonerPassiveType passiveType = SummonerPassiveType.LifeContract; // 패시브 종류

    [Header("해금과 성장")]
    [SerializeField, Min(1)] private int unlockLevel = 2;      // 목록에 나타나는 플레이어 레벨
    [SerializeField, Min(1)] private int pointCostPerRank = 1; // 단계마다 드는 패시브 포인트

    [SerializeField, Range(1, 3)] private int maxRank = 3;  // 최대 단계
    [SerializeField, Min(0)] private int rank1Amount = 1;   // 1단계 효과 수치
    [SerializeField, Min(0)] private int rank2Amount = 2;   // 2단계 효과 수치
    [SerializeField, Min(0)] private int rank3Amount = 3;   // 3단계 효과 수치

    [SerializeField, Min(0)]
    private int conditionValue; // 조건 수치 (절약 소환: 대상 비용 상한, 군단 지휘: 필요 마물 수)

    public string PassiveId => passiveId;                 // 패시브 ID 반환
    public string DisplayName => displayName;             // 패시브 이름 반환
    public SummonerPassiveType PassiveType => passiveType; // 패시브 종류 반환
    public int UnlockLevel => Mathf.Max(1, unlockLevel);  // 해금 레벨 반환
    public int PointCostPerRank => Mathf.Max(1, pointCostPerRank); // 단계당 포인트 반환
    public int ConditionValue => Mathf.Max(0, conditionValue);     // 조건 수치 반환
    public int MaxRank => Mathf.Clamp(maxRank, 1, 3);     // 최대 단계 반환

    public int GetAmount(int rank) // 해당 단계의 효과 수치 (0단계는 효과 없음)
    {
        if (rank <= 0) { return 0; }

        switch (Mathf.Min(rank, MaxRank))
        {
            case 1: return Mathf.Max(0, rank1Amount);
            case 2: return Mathf.Max(0, rank2Amount);
            default: return Mathf.Max(0, rank3Amount);
        }
    }

    public string GetEffectText(int rank) // 해당 단계의 수치를 반영한 효과 설명
    {
        int value = GetAmount(Mathf.Max(1, rank));

        switch (passiveType)
        {
            case SummonerPassiveType.LifeContract:
                return $"플레이어 최대 HP +{value}";

            case SummonerPassiveType.QuickStudy:
                return $"첫 턴에 카드 {value}장 추가 드로우";

            case SummonerPassiveType.ThriftySummon:
                return $"전투마다 처음 소환하는 비용 {ConditionValue} 이하 마물의 비용 -{value}";

            case SummonerPassiveType.SturdyContract:
                return $"플레이어가 받는 HP 피해 -{value} (최소 1)";

            case SummonerPassiveType.LustAmplify:
                return $"모든 마물의 성욕 부여량 +{value}";

            case SummonerPassiveType.LegionCommand:
                return $"필드의 마물이 {ConditionValue}체 이상이면 모든 마물 공격 +{value}";

            case SummonerPassiveType.TypeFocus:
                return $"덱에 가장 많은 계열의 시너지 계산 수 +{value}";

            case SummonerPassiveType.RecycleKnowledge:
                return $"덱을 다시 섞을 때 카드 {value}장 추가 드로우";

            case SummonerPassiveType.MerchantSense:
                return $"상점 구매 가격 -{value}%";

            case SummonerPassiveType.CaptureRecord:
                return $"중복 포획 시 마물의 정수 +{value}";

            default:
                return string.Empty;
        }
    }
}
