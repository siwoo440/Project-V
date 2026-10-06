using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 소환사 화면 연결 (기획서 6.4 / 6.5)
// 왼쪽에서 액티브 스킬을 장착하고, 오른쪽에서 패시브를 해금·강화·장착한다.
public partial class SummonerFlow : MonoBehaviour
{
    [Header("화면 이동")]
    [SerializeField] private Button backButton; // 돌아가기

    [Header("목록")]
    [SerializeField] private Transform skillListContent;   // 액티브 스킬 줄 배치 영역
    [SerializeField] private Transform passiveListContent; // 패시브 줄 배치 영역

    [Header("패시브 조작")]
    [SerializeField] private TMP_Text passiveDetailText;   // 선택한 패시브 설명
    [SerializeField] private Button equipPassiveButton;    // 패시브 장착
    [SerializeField] private Button upgradePassiveButton;  // 패시브 해금 또는 강화

    [Header("화면 텍스트")]
    [SerializeField] private TMP_Text levelText;   // 플레이어 레벨과 패시브 포인트
    [SerializeField] private TMP_Text messageText; // 안내 문구

    private readonly List<GameObject> generatedRows =
        new List<GameObject>(); // 생성한 목록 줄

    private SummonerPassiveData selectedPassive; // 선택한 패시브

    private void Awake()
    {
        backButton =
            SceneUIBinder.Bind(backButton, "BackButton");

        skillListContent =
            SceneUIBinder.Bind(skillListContent, "SkillListContent");

        passiveListContent =
            SceneUIBinder.Bind(passiveListContent, "PassiveListContent");

        passiveDetailText =
            SceneUIBinder.Bind(passiveDetailText, "PassiveDetailText");

        equipPassiveButton =
            SceneUIBinder.Bind(equipPassiveButton, "EquipPassiveButton");

        upgradePassiveButton =
            SceneUIBinder.Bind(upgradePassiveButton, "UpgradePassiveButton");

        levelText =
            SceneUIBinder.Bind(levelText, "LevelText");

        messageText =
            SceneUIBinder.Bind(messageText, "MessageText");
    }

    private void Start()
    {
        AddClickListener(backButton, SceneFlow.ReturnToPreviousScene);
        AddClickListener(equipPassiveButton, EquipSelectedPassive);
        AddClickListener(upgradePassiveButton, UpgradeSelectedPassive);

        ShowMessage("장착할 스킬과 패시브를 고르세요.");
        Refresh();
    }

    private void AddClickListener(
        Button targetButton,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        if (targetButton == null) { return; }

        targetButton.onClick.RemoveAllListeners();
        targetButton.onClick.AddListener(clickAction);
    }

    // ---------- 조작 ----------

    private void EquipSkill(SummonerSkillData skill) // 액티브 스킬 장착
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        progress.TryEquipSkill(skill, out string message);

        ShowMessage(message);
        Refresh();
    }

    private void SelectPassive(SummonerPassiveData passive) // 패시브 선택
    {
        selectedPassive = passive;
        Refresh();
    }

    private void EquipSelectedPassive() // 선택한 패시브 장착
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        progress.TryEquipPassive(selectedPassive, out string message);

        ShowMessage(message);
        Refresh();
    }

    private void UpgradeSelectedPassive() // 선택한 패시브 해금 또는 강화
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        progress.TryUpgradePassive(selectedPassive, out string message);

        ShowMessage(message);
        Refresh();
    }

    // ---------- 화면 갱신 ----------

    public void Refresh()
    {
        ClearRows();

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null)
        {
            ShowMessage("진행 데이터가 없습니다");
            return;
        }

        if (levelText != null)
        {
            levelText.text = progress.IsPassiveSystemUnlocked
                ? $"플레이어 {progress.PlayerLevelText}      패시브 포인트 {progress.PassivePoints}"
                : $"플레이어 {progress.PlayerLevelText}";
        }

        BuildSkillList(progress);
        BuildPassiveList(progress);
        UpdatePassiveDetail(progress);
    }

    private void ShowMessage(string message)
    {
        if (messageText == null) { return; }

        messageText.text = message;
    }

    private void ClearRows()
    {
        foreach (GameObject generatedRow in generatedRows)
        {
            if (generatedRow == null) { continue; }

            Destroy(generatedRow);
        }

        generatedRows.Clear();
    }
}
