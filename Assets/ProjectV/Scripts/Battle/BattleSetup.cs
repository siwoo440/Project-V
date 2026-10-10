using System.Collections.Generic; // 리스트 기능

// 다음 전투의 종류와 상대. 지역 화면이 전투 씬으로 넘어가기 전에 정하고, 전투가 시작될 때 읽는다.
// 아무것도 정하지 않았으면 전투 씬에 적힌 시험 히로인과 싸운다. (전투 씬만 따로 실행했을 때 포함)
public static class BattleSetup
{
    private static readonly List<MonsterData> enemies = new List<MonsterData>(); // 적 마물 (왼쪽부터)

    public static BattleKind Kind { get; private set; } = BattleKind.Heroine; // 전투 종류
    public static IReadOnlyList<MonsterData> Enemies => enemies;              // 적 마물 목록 반환
    public static HeroineBattleData Heroine { get; private set; }             // 히로인 전투 데이터 (없으면 전투 씬의 시험 히로인)

    public static string StageId { get; private set; } = string.Empty;    // 승리 기록에 쓰는 스테이지 ID (R01-G1, R01-CAP1, R01-H1)
    public static int StageNumber { get; private set; } = 1;              // 일반전 단계 (보상 계산용)
    public static int HpPercent { get; private set; } = 100;              // 단계 HP 계수
    public static int AttackPercent { get; private set; } = 100;          // 단계 ATK 계수
    public static int DefenseBonus { get; private set; }                  // 단계 DEF 보정

    public static int RegionOrder { get; private set; } = 1;              // 전투가 벌어지는 지역의 순서
    public static string StageTitle { get; private set; } = string.Empty; // 전투 기록에 남길 스테이지 이름
    public static BattleDifficulty Difficulty { get; private set; } = BattleDifficulty.Normal; // 고른 난이도
    public static bool ClearsRegion { get; private set; }                 // 이 전투에서 이기면 지역을 클리어하는지 여부

    public static bool HasBattle { get; private set; } // 지역 화면이 전투를 정해 두었는지 여부 (전투 준비 화면이 확인한다)

    public static BattleDifficulty PreferredDifficulty { get; private set; } =
        BattleDifficulty.Normal; // 마지막으로 고른 난이도. 다음 전투를 정할 때도 이 난이도로 시작한다.

    // 난이도를 고를 수 있는 전투인지 여부. 포획전은 지역마다 고정이고 시험 히로인 전투에는 난이도가 없다.
    public static bool CanChangeDifficulty =>
        Kind == BattleKind.Normal || (Kind == BattleKind.Heroine && Heroine != null);

    public static void SetDifficulty(BattleDifficulty difficulty) // 전투 준비 화면에서 난이도를 바꾼다.
    {
        PreferredDifficulty = difficulty;

        if (CanChangeDifficulty) { Difficulty = difficulty; }
    }

    public static bool IsEnemyBattle => Kind != BattleKind.Heroine;   // 적 마물과 싸우는 전투인지 여부
    public static bool IsCaptureBattle => Kind == BattleKind.Capture; // 포획전인지 여부

    public static string EnemyListText // 적 이름을 쉼표로 이은 문구
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

    public static void SetNormalBattle( // 일반전: 고정 편성과 고른 난이도
        EnemyFormationData formation,
        int regionOrder,
        string stageTitle,
        BattleDifficulty difficulty,
        bool clearsRegion
    )
    {
        SetCommon(BattleKind.Normal, regionOrder, stageTitle, difficulty, clearsRegion);

        if (formation == null) { return; }

        enemies.AddRange(formation.Enemies);

        StageId = formation.FormationId;
        StageNumber = formation.Stage;
        HpPercent = formation.HpPercent;
        AttackPercent = formation.AttackPercent;
        DefenseBonus = formation.DefenseBonus;
    }

    public static void SetCaptureBattle( // 포획전: 포획 목록의 마물, 난이도는 지역마다 고정 (기획서 8.12)
        string stageId,
        IReadOnlyList<MonsterData> captureEnemies,
        int regionOrder,
        string stageTitle
    )
    {
        SetCommon(BattleKind.Capture, regionOrder, stageTitle, BattleDifficulty.Normal, false);

        if (captureEnemies != null) { enemies.AddRange(captureEnemies); }

        StageId = stageId ?? string.Empty;
    }

    public static void SetHeroineBattle( // 히로인전: 전투 데이터와 고른 난이도 (기획서 4.17 / F.3.1)
        HeroineBattleData battle,
        int regionOrder,
        string stageTitle,
        BattleDifficulty difficulty,
        bool clearsRegion
    )
    {
        SetCommon(BattleKind.Heroine, regionOrder, stageTitle, difficulty, clearsRegion);

        if (battle == null) { return; }

        Heroine = battle;
        StageId = battle.BattleId ?? string.Empty;
        StageNumber = battle.Stage;
    }

    // 히로인전 데이터가 없는 지역의 시험 전투. 전투 씬에 적힌 히로인 수치와 행동을 쓴다.
    public static void SetTestHeroineBattle(string stageTitle, int regionOrder, bool clearsRegion)
    {
        SetCommon(BattleKind.Heroine, regionOrder, stageTitle, BattleDifficulty.Normal, clearsRegion);
    }

    private static void SetCommon(
        BattleKind kind,
        int regionOrder,
        string stageTitle,
        BattleDifficulty difficulty,
        bool clearsRegion
    )
    {
        enemies.Clear();
        Heroine = null;
        HasBattle = true;

        Kind = kind;
        RegionOrder = regionOrder < 1 ? 1 : regionOrder;
        StageTitle = stageTitle ?? string.Empty;
        Difficulty = difficulty;
        ClearsRegion = clearsRegion;

        StageId = string.Empty;
        StageNumber = 1;
        HpPercent = 100;
        AttackPercent = 100;
        DefenseBonus = 0;
    }
}
