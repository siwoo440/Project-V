using System.Collections.Generic; // 리스트 기능

// 스토리 회상 목록의 한 줄 (기획서 4.16.4)
public class StoryCatalogEntry
{
    public StorySceneData Scene { get; } // 장면
    public RegionData Region { get; }    // 장면이 나오는 지역
    public string Place { get; }         // 장면이 나오는 자리 (지역 도입, 아리아 1차전 시작 전)

    public StoryCatalogEntry(StorySceneData scene, RegionData region, string place)
    {
        Scene = scene;
        Region = region;
        Place = place;
    }

    public string RegionLabel => Region == null // 지역 1 왕국과 엘프의 숲
        ? string.Empty
        : $"지역 {Region.Order} {Region.DisplayName}";
}

// 게임의 스토리 장면을 진행 순서대로 모은다.
// 장면은 지역 데이터와 히로인전 데이터에 연결되어 있으므로 따로 목록을 관리하지 않고 거기서 읽는다.
public static class StoryCatalog
{
    public static List<StoryCatalogEntry> Build(IReadOnlyList<RegionData> regions)
    {
        List<StoryCatalogEntry> entries = new List<StoryCatalogEntry>();
        HashSet<StorySceneData> added = new HashSet<StorySceneData>(); // 같은 장면을 두 번 넣지 않는다.

        if (regions == null) { return entries; }

        foreach (RegionData region in regions)
        {
            if (region == null) { continue; }

            Add(entries, added, region.IntroStory, region, "지역 도입");

            for (int stage = 1; stage <= HeroineBattleRules.MainStageCount; stage++)
            {
                AddBattle(entries, added, region, region.GetMainBattle(stage));
            }

            foreach (HeroineBattleData subBattle in region.GetSubBattles())
            {
                AddBattle(entries, added, region, subBattle);
            }

            Add(entries, added, region.EndStory, region, "지역 마무리");
        }

        return entries;
    }

    private static void AddBattle(
        List<StoryCatalogEntry> entries,
        HashSet<StorySceneData> added,
        RegionData region,
        HeroineBattleData battle
    )
    {
        if (battle == null) { return; }

        Add(entries, added, battle.BeforeStory, region, $"{battle.DisplayName} 시작 전");
        Add(entries, added, battle.AfterStory, region, $"{battle.DisplayName} 승리 뒤");
    }

    private static void Add(
        List<StoryCatalogEntry> entries,
        HashSet<StorySceneData> added,
        StorySceneData scene,
        RegionData region,
        string place
    )
    {
        if (scene == null || !added.Add(scene)) { return; }

        entries.Add(new StoryCatalogEntry(scene, region, place));
    }
}
