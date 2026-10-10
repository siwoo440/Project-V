using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 일반전 한 단계의 적 편성 (기획서 8.10 / F.5 / F.7)
// 일반전은 고정된 적 마물과 싸운다. 다시 싸워도 편성은 바뀌지 않는다.
[CreateAssetMenu(
    fileName = "NewEnemyFormation",
    menuName = "Project V/적 편성 데이터"
)]
public class EnemyFormationData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string formationId;     // 편성 ID (R01-G1)
    [SerializeField] private string regionId;        // 소속 지역 ID
    [SerializeField, Min(1)] private int stage = 1;  // 일반전 단계 (1 ~ 3)

    [SerializeField, TextArea]
    private string description; // 전투 전에 보여 주는 안내

    [Header("적 편성")]
    [SerializeField]
    private List<MonsterData> enemies =
        new List<MonsterData>(); // 왼쪽부터 놓이는 적 마물 (최대 5체)

    [Header("단계 계수 (기획서 F.5.1)")]
    [SerializeField, Min(1)] private int hpPercent = 100;     // HP 계수
    [SerializeField, Min(1)] private int attackPercent = 100; // ATK 계수
    [SerializeField] private int defenseBonus;                // DEF 보정

    public string FormationId => formationId;         // 편성 ID 반환
    public string RegionId => regionId;               // 소속 지역 ID 반환
    public int Stage => Mathf.Max(1, stage);          // 일반전 단계 반환
    public string Description => description;         // 안내 문구 반환
    public IReadOnlyList<MonsterData> Enemies => enemies; // 적 마물 목록 반환
    public int HpPercent => Mathf.Max(1, hpPercent);         // HP 계수 반환
    public int AttackPercent => Mathf.Max(1, attackPercent); // ATK 계수 반환
    public int DefenseBonus => defenseBonus;                 // DEF 보정 반환

    public string DisplayName => $"일반전 {Stage}단계"; // 표시 이름

    public string EnemyListText // 적 이름을 쉼표로 이은 문구
    {
        get
        {
            List<string> names = new List<string>();

            foreach (MonsterData enemy in enemies)
            {
                if (enemy != null) { names.Add(enemy.MonsterName); }
            }

            return names.Count == 0 ? "없음" : string.Join(", ", names);
        }
    }
}
