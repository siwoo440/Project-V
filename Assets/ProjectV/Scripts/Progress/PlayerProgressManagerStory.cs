using System.Collections.Generic; // 리스트 기능

// 스토리 진행 기록 (기획서 10.21 / F.3.1 / F.3.2)
// 본 장면을 기록한다. 건너뛴 장면도 본 장면으로 친다.
// 지역 도입 스토리를 봐야 주요 히로인 1차전이 열리고, 3차전을 이긴 뒤 마무리 스토리를 봐야 지역이 클리어된다.
// 히로인전의 승리 뒤 장면은 이긴 뒤 지역 화면으로 돌아올 때 나온다.
public partial class PlayerProgressManager
{
    private readonly HashSet<string> seenStories = new HashSet<string>(); // 본 장면의 ID

    private string regionClearNotice = string.Empty; // 방금 클리어한 지역의 안내. 월드맵이 한 번 보여 주고 지운다. (저장하지 않는다)

    private void InitializeStoryProgress()
    {
        seenStories.Clear();
        regionClearNotice = string.Empty;
    }

    public bool IsStorySeen(string sceneId) // 본 장면인지 여부
    {
        return !string.IsNullOrEmpty(sceneId) && seenStories.Contains(sceneId);
    }

    public bool IsStorySeen(StorySceneData scene) // 장면이 없으면 본 것으로 친다. (스토리 데이터가 없는 지역)
    {
        return scene == null || IsStorySeen(scene.SceneId);
    }

    // 지역 화면에 들어갈 때 먼저 봐야 하는 장면. 도입 스토리, 이긴 히로인전의 승리 뒤 장면, 마무리 스토리 순서로 찾는다.
    public StorySceneData GetPendingRegionStory(RegionData region)
    {
        if (region == null) { return null; }

        if (!IsStorySeen(region.IntroStory)) { return region.IntroStory; }

        foreach (HeroineBattleData battle in region.HeroineBattles)
        {
            if (battle == null || IsStorySeen(battle.AfterStory)) { continue; }

            if (IsStageCleared(battle.BattleId)) { return battle.AfterStory; } // 이긴 전투의 승리 뒤 장면
        }

        if (!IsStorySeen(region.EndStory) && IsMainBattleCleared(region, HeroineBattleRules.MainStageCount))
        {
            return region.EndStory;
        }

        return null;
    }

    // 장면을 끝까지 봤거나 건너뛰었을 때 부른다. 마무리 스토리였다면 지역을 클리어한다.
    public void CompleteStory(StorySceneData scene)
    {
        if (scene == null || string.IsNullOrEmpty(scene.SceneId)) { return; }

        bool isFirstTime = seenStories.Add(scene.SceneId);

        foreach (RegionData region in regions)
        {
            if (region == null || region.EndStory != scene) { continue; }
            if (!IsMainBattleCleared(region, HeroineBattleRules.MainStageCount)) { continue; }

            CompleteRegionByStory(region);
        }

        if (!isFirstTime) { return; }

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림
        AutoSave("스토리 완료"); // 기획서 10.21.1: 건너뛰어도 해금과 자동 저장은 정상 처리한다.
    }

    // 마무리 스토리까지 본 지역을 클리어하고 지역 클리어 보상을 준다. 보상은 한 번만 준다. (기획서 C.28)
    private void CompleteRegionByStory(RegionData region)
    {
        if (!MarkRegionCleared(region)) { return; }

        int gold = AddGold(HeroineBattleRules.GetRegionClearGold(region.Order));
        int shards = AddDesireShards(HeroineBattleRules.GetRegionClearShards(region.Order));

        regionClearNotice = $"지역 클리어: {region.DisplayName} (골드 +{gold}, 욕망의 파편 +{shards})";
    }

    public string TakeRegionClearNotice() // 방금 클리어한 지역의 안내를 한 번만 돌려준다.
    {
        string notice = regionClearNotice;

        regionClearNotice = string.Empty;

        return notice;
    }

    private List<string> CreateSeenStoryList() // 저장용 목록
    {
        return new List<string>(seenStories);
    }

    private void RestoreSeenStories(List<string> savedStories) // 저장 파일에서 복원
    {
        InitializeStoryProgress();

        if (savedStories == null) { return; }

        foreach (string sceneId in savedStories)
        {
            if (!string.IsNullOrEmpty(sceneId)) { seenStories.Add(sceneId); }
        }
    }
}
