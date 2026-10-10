// 다음 전투의 종류와 상대. 지역 화면이 전투 씬으로 넘어가기 전에 정하고, 전투가 시작될 때 읽는다.
// 아무것도 정하지 않았으면 히로인 전투다. (전투 씬만 따로 실행했을 때 포함)
public static class BattleSetup
{
    public static EnemyFormationData Formation { get; private set; } // 일반전의 적 편성 (없으면 히로인 전투)
    public static int RegionOrder { get; private set; } = 1;          // 전투가 벌어지는 지역의 순서
    public static string StageTitle { get; private set; } = string.Empty; // 전투 기록에 남길 스테이지 이름

    public static bool IsEnemyBattle => Formation != null; // 적 마물과 싸우는 전투인지 여부

    public static void SetEnemyBattle(EnemyFormationData formation, int regionOrder, string stageTitle)
    {
        Formation = formation;
        RegionOrder = regionOrder < 1 ? 1 : regionOrder;
        StageTitle = stageTitle ?? string.Empty;
    }

    public static void SetHeroineBattle(string stageTitle)
    {
        Formation = null;
        RegionOrder = 1;
        StageTitle = stageTitle ?? string.Empty;
    }
}
