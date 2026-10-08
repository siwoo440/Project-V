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
