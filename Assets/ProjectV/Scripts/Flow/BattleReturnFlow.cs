using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class BattleReturnFlow : MonoBehaviour // 전투 씬 복귀 처리
{
    [Header("화면 이동")]
    [SerializeField] private Button returnButton; // 지역 선택 복귀

    [Header("진행 정보 텍스트")]
    [SerializeField] private TMP_Text progressText; // 진행 정보 표시

    private void Awake()
    {
        returnButton =
            SceneUIBinder.Bind(returnButton, "ReturnButton");

        progressText =
            SceneUIBinder.Bind(progressText, "BattleProgressText");
    }

    private void Start()
    {
        if (returnButton != null)
        {
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(ReturnToStageSelect);
        }

        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.ProgressChanged +=
                RefreshProgressText;
        }

        RefreshProgressText(); // 진행 정보 갱신
    }

    private void OnDestroy()
    {
        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.ProgressChanged -=
                RefreshProgressText;
        }
    }

    public void ReturnToStageSelect()
    {
        SceneFlow.LoadStageSelect(); // 지역 선택 복귀
    }

    private void RefreshProgressText()
    {
        if (progressText == null) { return; } // 빈 텍스트 차단

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null)
        {
            progressText.text = "단독 전투";
            return;
        }

        progressText.text =
            $"{progress.PlayerLevelText}    " +
            $"{UISkin.IconOr(UIIcons.Gold, "골드")} {progress.Gold}    " +
            $"{UISkin.IconOr(UIIcons.Essence, "정수")} {progress.MonsterEssence}    " +
            $"보유 카드 {progress.TotalOwnedCardCount}"; // 진행 요약 표시
    }
}
