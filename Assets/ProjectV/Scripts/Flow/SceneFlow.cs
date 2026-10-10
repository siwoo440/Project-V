using UnityEngine; // Unity 기본 기능
using UnityEngine.SceneManagement; // 씬 관리 기능

public static class SceneFlow // 씬 전환 관리
{
    public static string HubSceneName { get; private set; } // 마지막으로 머문 중심 화면 (메인 메뉴, 월드맵, 지역 화면)
    public static bool SaveScreenAllowsSave { get; private set; } // 저장 화면에서 저장도 할 수 있는지 여부

    public static void LoadScene(string sceneName) // 씬 전환
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("씬 이름이 비어 있습니다."); // 빈 씬 이름 차단
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene(); // 현재 씬 확인

        if (IsHubScene(currentScene.name))
        {
            HubSceneName = currentScene.name; // 돌아가기 대상 저장
        }

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress != null)
        {
            if (sceneName == SceneNames.Battle)
            {
                progress.AutoSave("전투 시작"); // 기획서 15.6: 전투 시작 직전
            }
            else
            {
                progress.AutoSaveIfChanged("화면 이동"); // 덱 편성, 장착 변경처럼 그때그때 저장하지 않은 변경
            }
        }

        Debug.Log($"씬 전환: {currentScene.name} -> {sceneName}"); // 전환 기록

        SceneManager.LoadScene(sceneName); // 씬 로드
    }

    public static void LoadMainMenu()
    {
        LoadScene(SceneNames.MainMenu);
    }

    public static void LoadWorldMap()
    {
        LoadScene(SceneNames.WorldMap);
    }

    public static void LoadDeckBuilder()
    {
        LoadScene(SceneNames.DeckBuilder);
    }

    public static void LoadStageSelect()
    {
        LoadScene(SceneNames.StageSelect);
    }

    public static void LoadEnhance()
    {
        LoadScene(SceneNames.Enhance);
    }

    public static void LoadSummoner()
    {
        LoadScene(SceneNames.Summoner);
    }

    public static void LoadGrimoire()
    {
        LoadScene(SceneNames.Grimoire);
    }

    public static void LoadShop()
    {
        LoadScene(SceneNames.Shop);
    }

    // 저장 화면으로 이동한다. 저장은 전투 밖의 지역 화면에서만 할 수 있다. (기획서 15.5)
    public static void LoadSaveScreen(bool allowSave)
    {
        SaveScreenAllowsSave = allowSave;
        LoadScene(SceneNames.SaveLoad);
    }

    public static void LoadStory() // 스토리 씬에 적힌 시험 대사를 본다. (메인 메뉴의 스토리 버튼)
    {
        StorySetup.Clear();
        LoadScene(SceneNames.Story);
    }

    public static void LoadStory(StorySceneData scene, string returnSceneName) // 스토리 장면 하나를 보고 정한 화면으로 간다.
    {
        StorySetup.Set(scene, returnSceneName);
        LoadScene(SceneNames.Story);
    }

    public static void LoadBattlePrepare() // 전투 준비 화면. 지역 화면이 전투를 정한 뒤에 부른다.
    {
        LoadScene(SceneNames.BattlePrepare);
    }

    public static void LoadBattle()
    {
        LoadScene(SceneNames.Battle);
    }

    private static bool IsHubScene(string sceneName) // 다른 화면으로 나가는 출발점이 되는 화면인지 여부
    {
        return sceneName == SceneNames.MainMenu ||
               sceneName == SceneNames.WorldMap ||
               sceneName == SceneNames.StageSelect ||
               sceneName == SceneNames.BattlePrepare; // 덱 편성과 소환사 화면에서 준비 화면으로 돌아온다.
    }

    // 덱 편성, 강화, 상점 같은 화면의 돌아가기. 들어올 때 거친 중심 화면으로 돌아간다.
    // 바로 앞 화면으로 돌아가면 덱 편성과 강화처럼 서로 오가는 화면에서 돌아가기가 둘 사이만 왕복한다.
    public static void ReturnToPreviousScene()
    {
        if (string.IsNullOrEmpty(HubSceneName) ||
            HubSceneName == SceneManager.GetActiveScene().name)
        {
            LoadMainMenu(); // 기록이 없으면 메인 메뉴
            return;
        }

        LoadScene(HubSceneName);
    }

    public static void QuitGame() // 게임 종료
    {
        Debug.Log("게임 종료를 요청했습니다."); // 종료 요청 기록

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 재생 종료
#else
        Application.Quit(); // 빌드 종료
#endif
    }
}
