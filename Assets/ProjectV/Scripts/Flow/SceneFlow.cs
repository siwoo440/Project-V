using UnityEngine; // Unity 기본 기능
using UnityEngine.SceneManagement; // 씬 관리 기능

public static class SceneFlow // 씬 전환 관리
{
    public static string PreviousSceneName { get; private set; } // 이전 씬 이름
    public static bool SaveScreenAllowsSave { get; private set; } // 저장 화면에서 저장도 할 수 있는지 여부

    public static void LoadScene(string sceneName) // 씬 전환
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("씬 이름이 비어 있습니다."); // 빈 씬 이름 차단
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene(); // 현재 씬 확인

        if (currentScene.name != SceneNames.Bootstrap)
        {
            PreviousSceneName = currentScene.name; // 돌아가기 대상 저장
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

    public static void LoadStory()
    {
        LoadScene(SceneNames.Story);
    }

    public static void LoadBattle()
    {
        LoadScene(SceneNames.Battle);
    }

    public static void ReturnToPreviousScene() // 이전 씬으로 복귀
    {
        if (string.IsNullOrEmpty(PreviousSceneName) ||
            PreviousSceneName == SceneManager.GetActiveScene().name)
        {
            LoadMainMenu(); // 기록이 없으면 메인 메뉴
            return;
        }

        LoadScene(PreviousSceneName);
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
