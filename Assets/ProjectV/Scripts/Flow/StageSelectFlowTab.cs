using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 지역 화면의 목록 탭 (기획서 8.7 / F.3.1)
// 메인 진행: 주요 히로인 1차전, 일반전, 2차전, 3차전. 서브 콘텐츠: 서브 히로인전, 포획전.
// 서브 콘텐츠는 처음부터 볼 수 있고, 주요 히로인 2차전을 이긴 뒤에 들어갈 수 있다.
public partial class StageSelectFlow
{
    [Header("목록 탭")]
    [SerializeField] private Button mainTabButton; // 메인 진행
    [SerializeField] private Button subTabButton;  // 서브 콘텐츠

    private static bool showSubTab;                   // 서브 콘텐츠 목록을 보고 있는지 여부. 전투에서 돌아와도 유지한다.
    private static string tabRegionId = string.Empty; // 탭을 기억하고 있는 지역

    private bool HasTabs => region != null && region.HasMainBattles; // 히로인전 데이터가 있는 지역만 탭으로 나눈다.

    private void StartTabButtons() // 탭 버튼 연결
    {
        mainTabButton = SceneUIBinder.Bind(mainTabButton, "MainTabButton");
        subTabButton = SceneUIBinder.Bind(subTabButton, "SubTabButton");

        string regionId = region == null ? string.Empty : region.RegionId;

        if (tabRegionId != regionId)
        {
            tabRegionId = regionId;
            showSubTab = false; // 다른 지역에 들어오면 메인 진행부터 보여 준다.
        }

        AddTabListener(mainTabButton, false);
        AddTabListener(subTabButton, true);
    }

    private void AddTabListener(Button button, bool isSubTab)
    {
        if (button == null) { return; }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectTab(isSubTab));
    }

    private void SelectTab(bool isSubTab)
    {
        if (showSubTab == isSubTab) { return; }

        showSubTab = isSubTab;
        captureMessage = string.Empty;

        BuildStageList(); // 고른 탭의 목록으로 다시 만든다.
    }

    private void RefreshTabButtons() // 고른 탭은 금색 버튼으로 표시한다.
    {
        ApplyTabButton(mainTabButton, !showSubTab);
        ApplyTabButton(subTabButton, showSubTab);
    }

    private void ApplyTabButton(Button button, bool isSelected)
    {
        if (button == null) { return; }

        button.gameObject.SetActive(HasTabs);

        if (!HasTabs) { return; }

        Image buttonImage = button.GetComponent<Image>();

        bool hasSkin =
            buttonImage != null &&
            UISkin.ApplySelectable(
                buttonImage, isSelected,
                UIKeys.ButtonBlue, UIKeys.ButtonGold,
                DifficultyColor, DifficultySelectedColor
            );

        TMP_Text buttonLabel = button.GetComponentInChildren<TMP_Text>(true);

        if (hasSkin && buttonLabel != null)
        {
            buttonLabel.color = isSelected ? UISkin.ButtonInk : UISkin.Cream; // 금색 버튼 위에는 어두운 글자
        }
    }
}
