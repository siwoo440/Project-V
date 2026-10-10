using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 메인 지역 하나의 데이터 (기획서 F.2)
[CreateAssetMenu(
    fileName = "NewRegion",
    menuName = "Project V/지역 데이터"
)]
public class RegionData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string regionId;       // 지역 ID (R01 ~ R09)
    [SerializeField] private string displayName;    // 지역 이름
    [SerializeField, Min(1)] private int order = 1; // 해금 순서. 챕터 번호와 같고 9는 최종장이다.
    [SerializeField] private string heroineName;    // 주요 히로인
    [SerializeField] private string battleTheme;    // 지역 전투 주제
    [SerializeField] private string stageLabel;     // 진행 단계 (초반, 중반, 후반 등)

    [SerializeField, TextArea]
    private string description; // 지역 소개

    [Header("표시")]
    [SerializeField] private string iconKey;       // 지역 문양 이미지 이름
    [SerializeField] private string backgroundKey; // 지역 화면 배경 이미지 이름 (비어 있으면 지도 그림)

    [SerializeField]
    private Vector2 mapPosition = new Vector2(0.5f, 0.5f); // 지도 위 위치 (왼쪽 아래 0,0 ~ 오른쪽 위 1,1)

    [Header("포획")]
    [SerializeField]
    private List<MonsterData> captureMonsters =
        new List<MonsterData>(); // 이 지역의 포획전에 나올 수 있는 마물 (기획서 7.14의 지역 출현표)

    public IReadOnlyList<MonsterData> CaptureMonsters => captureMonsters; // 포획전 출현 마물 반환

    [Header("히로인전")]
    [SerializeField]
    private List<HeroineBattleData> heroineBattles =
        new List<HeroineBattleData>(); // 주요 히로인 1, 2, 3차전과 서브 히로인전 (기획서 F.3.1)

    public IReadOnlyList<HeroineBattleData> HeroineBattles => heroineBattles; // 히로인 전투 전체 반환

    public bool HasMainBattles => GetMainBattle(1) != null; // 주요 히로인전 데이터가 있는 지역인지 여부

    public HeroineBattleData GetMainBattle(int stage) // 주요 히로인 n차전 (없으면 null)
    {
        foreach (HeroineBattleData battle in heroineBattles)
        {
            if (battle != null && battle.IsMain && battle.Stage == stage) { return battle; }
        }

        return null;
    }

    public List<HeroineBattleData> GetSubBattles() // 서브 히로인전 (목록 번호 순)
    {
        List<HeroineBattleData> subBattles = new List<HeroineBattleData>();

        foreach (HeroineBattleData battle in heroineBattles)
        {
            if (battle != null && !battle.IsMain) { subBattles.Add(battle); }
        }

        subBattles.Sort((left, right) => left.Stage.CompareTo(right.Stage));

        return subBattles;
    }

    public string RegionId => regionId;           // 지역 ID 반환
    public string DisplayName => displayName;     // 지역 이름 반환
    public int Order => Mathf.Max(1, order);      // 해금 순서 반환
    public string HeroineName => heroineName;     // 주요 히로인 반환
    public string BattleTheme => battleTheme;     // 지역 전투 주제 반환
    public string StageLabel => stageLabel;       // 진행 단계 반환
    public string Description => description;     // 지역 소개 반환
    public string IconKey => iconKey;             // 지역 문양 이미지 이름 반환
    public string BackgroundKey => backgroundKey; // 지역 화면 배경 이미지 이름 반환

    public Vector2 MapPosition => new Vector2(
        Mathf.Clamp01(mapPosition.x),
        Mathf.Clamp01(mapPosition.y)
    ); // 지도 위 위치 반환

    public string ChapterName =>
        RegionRules.GetChapterName(Order); // 챕터 표시 이름
}
