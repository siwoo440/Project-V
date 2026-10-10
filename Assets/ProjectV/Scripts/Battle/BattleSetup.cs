using System.Collections.Generic; // 리스트 기능

// 다음 전투의 종류와 상대. 지역 화면이 전투 씬으로 넘어가기 전에 정하고, 전투가 시작될 때 읽는다.
// 아무것도 정하지 않았으면 히로인 전투다. (전투 씬만 따로 실행했을 때 포함)
public static class BattleSetup
{
    private static readonly List<MonsterData> enemies = new List<MonsterData>(); // 적 마물 (왼쪽부터)

    public static BattleKind Kind { get; private set; } = BattleKind.Heroine; // 전투 종류
    public static IReadOnlyList<MonsterData> Enemies => enemies;              // 적 마물 목록 반환

    public static string StageId { get; private set; } = string.Empty;    // 승리 기록에 쓰는 스테이지 ID (R01-G1, R01-CAP1)
    public static int StageNumber { get; private set; } = 1;              // 일반전 단계 (보상 계산용)
    public static int HpPercent { get; private set; } = 100;              // 단계 HP 계수
    public static int AttackPercent { get; private set; } = 100;          // 단계 ATK 계수
    public static int DefenseBonus { get; private set; }                  // 단계 DEF 보정

    public static int RegionOrder { get; private set; } = 1;              // 전투가 벌어지는 지역의 순서
    public static string StageTitle { get; private set; } = string.Empty; // 전투 기록에 남길 스테이지 이름
    public static BattleDifficulty Difficulty { get; private set; } = BattleDifficulty.Normal; // 고른 난이도
    public static bool ClearsRegion { get; private set; }                 // 이 전투에서 이기면 지역을 클리어하는지 여부

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

    public static void SetHeroineBattle(string stageTitle, int regionOrder, bool clearsRegion)
    {
        SetCommon(BattleKind.Heroine, regionOrder, stageTitle, BattleDifficulty.Normal, clearsRegion); // 히로인 전투의 난이도는 히로인전 일차에 넣는다.
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
