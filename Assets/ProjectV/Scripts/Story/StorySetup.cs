// 다음에 보여 줄 스토리 장면. 스토리 화면으로 넘어가기 전에 정하고, 스토리 화면이 시작될 때 읽는다.
// 아무것도 정하지 않았으면 스토리 씬에 적힌 시험 대사를 보여 준다.
public static class StorySetup
{
    public static StorySceneData Scene { get; private set; }                    // 보여 줄 장면
    public static string ReturnSceneName { get; private set; } = string.Empty;  // 장면이 끝난 뒤 갈 화면

    public static void Set(StorySceneData scene, string returnSceneName)
    {
        Scene = scene;
        ReturnSceneName = returnSceneName ?? string.Empty;
    }

    public static void Clear()
    {
        Scene = null;
        ReturnSceneName = string.Empty;
    }
}
