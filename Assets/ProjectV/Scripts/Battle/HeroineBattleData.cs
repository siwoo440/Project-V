using System; // 직렬화 표시
using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 히로인 전투 하나의 데이터 (기획서 D.3 / D.14.2 / F.3.1)
// 주요 히로인은 1, 2, 3차전마다 하나씩, 서브 히로인은 한 명에 하나씩 만든다. 수치는 보통 난이도 기준이다.
[CreateAssetMenu(
    fileName = "NewHeroineBattle",
    menuName = "Project V/히로인 전투 데이터"
)]
public class HeroineBattleData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string battleId;  // 전투 ID (R01-H1, R01-SUB1). 승리 기록에 쓴다.
    [SerializeField] private string regionId;  // 지역 ID (R01 ~ R09)
    [SerializeField] private HeroineBattleType battleType = HeroineBattleType.Main; // 주요 / 서브
    [SerializeField, Min(1)] private int stage = 1; // 주요 히로인전은 1, 2, 3차전. 서브 히로인전은 목록 번호 1, 2, 3
    [SerializeField] private string heroineName;    // 히로인 이름
    [SerializeField] private string heroineTitle;   // 종족과 직위
    [SerializeField] private string artKey;         // 그림 이름의 앞부분 (Aria). Resources/Characters/<이름>에서 찾는다.

    [SerializeField, TextArea]
    private string description; // 전투 소개

    [Header("스토리 (기획서 F.3.1)")]
    [SerializeField] private StorySceneData beforeStory; // 전투 시작 전 장면. 처음 도전할 때 한 번 나온다.
    [SerializeField] private StorySceneData afterStory;  // 승리 뒤 장면. 처음 이기고 지역 화면으로 돌아올 때 한 번 나온다.

    [Header("능력치 (보통 난이도)")]
    [SerializeField, Min(1)] private int maxHp = 220;    // 최대 HP
    [SerializeField, Min(0)] private int attack = 8;     // 공격력. 행동의 피해 배율에 곱한다.
    [SerializeField, Min(0)] private int defense = 4;    // 방어력
    [SerializeField, Min(0)] private int startingShield; // 전투 시작 보호막
    [SerializeField, Min(0)] private int maxShield = 60; // 최대 보호막
    [SerializeField] private LustResistance lustResistance = LustResistance.Normal; // 성욕 저항

    [Header("행동")]
    [SerializeField]
    private List<HeroineActionData> actions = new List<HeroineActionData>(); // 이 전투에서 쓰는 행동

    [SerializeField, Min(1)] private int actionsPerTurn = 1;  // 한 턴의 기본 행동 횟수 (기획서 D.2.3)
    [SerializeField, Min(0)] private int extraActionInterval; // 이 턴 수마다 행동이 한 번 늘어난다. 0이면 늘지 않는다.

    [Serializable]
    public class PhaseActionChange // 페이즈 전환 뒤에 달라지는 행동 하나
    {
        [SerializeField] private HeroineActionData action;  // 대상 행동
        [SerializeField] private int weightBonus;           // 가중치 변화
        [SerializeField, Min(0)] private int attackPercent; // 바뀌는 피해 배율 (0이면 그대로)

        public HeroineActionData Action => action;               // 대상 행동 반환
        public int WeightBonus => weightBonus;                   // 가중치 변화 반환
        public int AttackPercent => Mathf.Max(0, attackPercent); // 바뀌는 피해 배율 반환
    }

    [Header("페이즈 전환 (기획서 D.2.6)")]
    [SerializeField] private string phaseName;                  // 페이즈 전환 이름 (기사단장의 각오)
    [SerializeField, Range(0, 100)] private int phaseHpPercent; // HP가 이 비율 이하가 되면 한 번 발동한다. 0이면 없다.
    [SerializeField, Min(0)] private int phaseDefenseBonus;     // 전투가 끝날 때까지 오르는 방어력

    [SerializeField]
    private List<PhaseActionChange> phaseActionChanges =
        new List<PhaseActionChange>(); // 페이즈 전환 뒤에 달라지는 행동

    public string PhaseName => phaseName;                           // 페이즈 전환 이름 반환
    public int PhaseHpPercent => Mathf.Clamp(phaseHpPercent, 0, 100); // 발동 HP 비율 반환
    public int PhaseDefenseBonus => Mathf.Max(0, phaseDefenseBonus); // 방어력 증가 반환
    public bool HasPhase => phaseHpPercent > 0;                     // 페이즈 전환이 있는 전투인지 여부

    private PhaseActionChange FindPhaseChange(HeroineActionData action)
    {
        foreach (PhaseActionChange change in phaseActionChanges)
        {
            if (change != null && change.Action == action) { return change; }
        }

        return null;
    }

    public int GetPhaseWeightBonus(HeroineActionData action) // 페이즈 전환 뒤의 가중치 변화
    {
        PhaseActionChange change = FindPhaseChange(action);

        return change == null ? 0 : change.WeightBonus;
    }

    public int GetPhaseAttackPercent(HeroineActionData action) // 페이즈 전환 뒤의 피해 배율 (0이면 그대로)
    {
        PhaseActionChange change = FindPhaseChange(action);

        return change == null ? 0 : change.AttackPercent;
    }

    public string BattleId => battleId;                // 전투 ID 반환
    public string RegionId => regionId;                // 지역 ID 반환
    public HeroineBattleType BattleType => battleType; // 전투 구분 반환
    public int Stage => Mathf.Max(1, stage);           // 차수 또는 목록 번호 반환
    public string HeroineName => heroineName;          // 히로인 이름 반환
    public string HeroineTitle => heroineTitle;        // 종족과 직위 반환
    public string ArtKey => artKey;                    // 그림 이름의 앞부분 반환
    public string Description => description;          // 전투 소개 반환

    public StorySceneData BeforeStory => beforeStory; // 전투 시작 전 장면 반환 (없으면 null)
    public StorySceneData AfterStory => afterStory;   // 승리 뒤 장면 반환 (없으면 null)

    public int MaxHp => Mathf.Max(1, maxHp);                   // 최대 HP 반환
    public int Attack => Mathf.Max(0, attack);                 // 공격력 반환
    public int Defense => Mathf.Max(0, defense);               // 방어력 반환
    public int StartingShield => Mathf.Max(0, startingShield); // 시작 보호막 반환
    public int MaxShield => Mathf.Max(0, maxShield);           // 최대 보호막 반환
    public LustResistance LustResistance => lustResistance;    // 성욕 저항 반환

    public IReadOnlyList<HeroineActionData> Actions => actions;          // 행동 목록 반환
    public int ActionsPerTurn => Mathf.Max(1, actionsPerTurn);           // 기본 행동 횟수 반환
    public int ExtraActionInterval => Mathf.Max(0, extraActionInterval); // 추가 행동 주기 반환

    public bool IsMain => battleType == HeroineBattleType.Main; // 주요 히로인전인지 여부

    public string DisplayName => IsMain // 목록에 적는 이름 (아리아 1차전 / 실리아). 전투 종류는 아랫줄에 적는다.
        ? $"{heroineName} {Stage}차전"
        : heroineName;

    public string BattleTitle => IsMain // 전투 기록에 남기는 이름 (아리아 1차전 / 서브 히로인전 실리아)
        ? DisplayName
        : $"서브 히로인전 {heroineName}";

    public string ActionListText // 행동 이름을 쉼표로 이은 문구
    {
        get
        {
            List<string> names = new List<string>();

            foreach (HeroineActionData action in actions)
            {
                if (action != null) { names.Add(action.DisplayName); }
            }

            return names.Count == 0 ? "없음" : string.Join(", ", names);
        }
    }
}
