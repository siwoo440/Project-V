using UnityEngine; // Unity 기본 기능

public class BootstrapFlow : MonoBehaviour // 진입 씬 처리
{
    [SerializeField] private string nextSceneName = SceneNames.MainMenu; // 다음 씬
    [SerializeField, Min(0f)] private float loadDelay = 0.1f; // 로드 대기 시간

    private void Start()
    {
        if (PlayerProgressManager.Instance == null)
        {
            Debug.LogWarning(
                "진입 씬에 진행 데이터 관리자가 없습니다."
            ); // 진행 데이터 누락 경고
        }

        Invoke(nameof(LoadNextScene), loadDelay); // 다음 씬 예약
    }

    private void LoadNextScene()
    {
        SceneFlow.LoadScene(nextSceneName); // 다음 씬 로드
    }
}
