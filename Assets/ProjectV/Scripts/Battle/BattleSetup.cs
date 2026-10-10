// 다음 전투의 종류와 상대. 지역 화면이 전투 씬으로 넘어가기 전에 정하고, 전투가 시작될 때 읽는다.
// 아무것도 정하지 않았으면 히로인 전투다. (전투 씬만 따로 실행했을 때 포함)
public static class BattleSetup
{
    public static EnemyFormationData Formation { get; private set; } // 일반전의 적 편성 (없으면 히로인 전투)
    public static int RegionOrder { get; private set; } = 1;          // 전투가 벌어지는 지역의 순서
    public static string StageTitle { get; private set; } = string.Empty; // 전투 기록에 남길 스테이지 이름

    public static BattleDifficulty Difficulty { get; private set; } = BattleDifficulty.Normal; // 고른 난이도
    public static bool ClearsRegion { get; private set; } // 이 전투에서 이기면 지역을 클리어하는지 여부

    public static bool IsEnemyBattle => Formation != null; // 적 마물과 싸우는 전투인지 여부

    public static string StageId =>
        Formation == null ? string.Empty : Formation.FormationId; // 승리 기록에 쓰는 스테이지 ID (일반전만)

    public static void SetEnemyBattle(
        EnemyFormationData formation,
        int regionOrder,
        string stageTitle,
        BattleDifficulty difficulty,
        bool clearsRegion
    )
    {
        Formation = formation;
        RegionOrder = regionOrder < 1 ? 1 : regionOrder;
        StageTitle = stageTitle ?? string.Empty;
        Difficulty = difficulty;
        ClearsRegion = clearsRegion;
    }

    public static void SetHeroineBattle(string stageTitle, int regionOrder, bool clearsRegion)
    {
        Formation = null;
        RegionOrder = regionOrder < 1 ? 1 : regionOrder;
        StageTitle = stageTitle ?? string.Empty;
        Difficulty = BattleDifficulty.Normal; // 히로인 전투의 난이도는 히로인전 일차에 넣는다.
        ClearsRegion = clearsRegion;
    }
}
