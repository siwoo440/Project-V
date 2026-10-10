// 다음에 보여 줄 스토리 장면. 스토리 화면으로 넘어가기 전에 정하고, 스토리 화면이 시작될 때 읽는다.
public static class StorySetup
{
    public static StorySceneData Scene { get; private set; }                    // 보여 줄 장면
    public static string ReturnSceneName { get; private set; } = string.Empty;  // 장면이 끝난 뒤 갈 화면
    public static bool IsRecall { get; private set; } // 회상으로 다시 보는 중인지 여부. 진행과 보상에 영향을 주지 않는다.

    public static void Set(StorySceneData scene, string returnSceneName)
    {
        Scene = scene;
        ReturnSceneName = returnSceneName ?? string.Empty;
        IsRecall = false;
    }

    public static void SetRecall(StorySceneData scene, string returnSceneName) // 회상 목록에서 고른 장면
    {
        Set(scene, returnSceneName);
        IsRecall = true;
    }

    public static void Clear()
    {
        Scene = null;
        ReturnSceneName = string.Empty;
        IsRecall = false;
    }
}
