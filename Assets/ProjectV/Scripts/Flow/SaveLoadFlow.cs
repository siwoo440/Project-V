using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 저장 화면 연결 (기획서 10.5 / 15.3 / 15.5 / 15.28)
// 왼쪽에서 칸을 고르고, 오른쪽에서 저장, 불러오기, 삭제를 한다.
// 메인 메뉴에서 들어오면 불러오기만, 월드맵에서 들어오면 저장도 할 수 있다.
// 화면의 패널과 버튼은 씬 구성 도구(SceneUIBuilderSave.cs)가 만든다.
public partial class SaveLoadFlow : MonoBehaviour
{
    private enum PendingAction // 한 번 더 눌러야 실행되는 동작
    {
        None,
        Save,         // 기존 저장 덮어쓰기 확인
        Load,         // 지금 진행을 버리고 불러오기 확인
        Delete,       // 삭제 1차 확인
        DeleteFinal   // 삭제 2차 확인
    }

    [Header("화면 이동")]
    [SerializeField] private Button backButton; // 돌아가기

    [Header("저장 칸")]
    [SerializeField] private Transform slotListContent; // 칸 줄 배치 영역
    [SerializeField] private Image detailIconImage;     // 선택한 칸 아이콘
    [SerializeField] private TMP_Text detailTitleText;  // 선택한 칸 이름
    [SerializeField] private TMP_Text detailText;       // 선택한 칸의 진행 정보
    [SerializeField] private Button saveButton;         // 저장
    [SerializeField] private Button loadButton;         // 불러오기
    [SerializeField] private Button deleteButton;       // 삭제

    [Header("화면 텍스트")]
    [SerializeField] private TMP_Text titleText;   // 화면 제목
    [SerializeField] private TMP_Text messageText; // 안내 문구

    private readonly List<GameObject> generatedRows =
        new List<GameObject>(); // 생성한 칸 줄

    private readonly Dictionary<SaveSlot, SaveSlotInfo> slotInfos =
        new Dictionary<SaveSlot, SaveSlotInfo>(); // 칸별로 읽은 결과

    private SaveSlot selectedSlot = SaveSlot.Auto;          // 선택한 칸
    private PendingAction pendingAction = PendingAction.None; // 확인을 기다리는 동작
    private bool allowSave; // 이 화면에서 저장할 수 있는지 여부

    private void Awake()
    {
        backButton = SceneUIBinder.Bind(backButton, "BackButton");

        slotListContent =
            SceneUIBinder.Bind(slotListContent, "SlotListContent");

        detailIconImage =
            SceneUIBinder.Bind(detailIconImage, "DetailIconImage");

        detailTitleText =
            SceneUIBinder.Bind(detailTitleText, "DetailTitleText");

        detailText = SceneUIBinder.Bind(detailText, "DetailText");
        saveButton = SceneUIBinder.Bind(saveButton, "SaveButton");
        loadButton = SceneUIBinder.Bind(loadButton, "LoadButton");
        deleteButton = SceneUIBinder.Bind(deleteButton, "DeleteButton");
        titleText = SceneUIBinder.Bind(titleText, "TitleText");
        messageText = SceneUIBinder.Bind(messageText, "MessageText");
    }

    private void Start()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        allowSave =
            SceneFlow.SaveScreenAllowsSave &&
            progress != null &&
            progress.IsSessionActive;

        AddClickListener(backButton, SceneFlow.ReturnToPreviousScene);
        AddClickListener(saveButton, OnSaveButton);
        AddClickListener(loadButton, OnLoadButton);
        AddClickListener(deleteButton, OnDeleteButton);

        if (titleText != null)
        {
            titleText.text = allowSave ? "저장 / 불러오기" : "불러오기";
        }

        ReadSlots();

        selectedSlot = allowSave ? SaveSlot.Manual1 : FindLatestSlot(); // 저장하러 왔으면 수동 칸부터 보여 준다.

        ShowMessage(allowSave
            ? "칸을 고르고 저장하거나 불러오세요. 자동 저장 칸에는 직접 저장할 수 없습니다."
            : "불러올 칸을 고르세요.");

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

    private void ReadSlots() // 저장 파일을 다시 읽는다.
    {
        slotInfos.Clear();

        foreach (SaveSlot slot in SaveRules.Slots)
        {
            slotInfos[slot] = SaveFileSystem.Read(slot);
        }
    }

    private SaveSlotInfo GetInfo(SaveSlot slot)
    {
        return slotInfos.TryGetValue(slot, out SaveSlotInfo info) ? info : null;
    }

    private SaveSlot FindLatestSlot() // 불러올 수 있는 칸 가운데 가장 최근 칸 (없으면 자동 저장 칸)
    {
        SaveSlotInfo latest = null;

        foreach (SaveSlot slot in SaveRules.Slots)
        {
            SaveSlotInfo info = GetInfo(slot);

            if (info == null || !info.IsValid) { continue; }

            if (latest == null || info.SavedAt > latest.SavedAt)
            {
                latest = info;
            }
        }

        return latest == null ? SaveSlot.Auto : latest.Slot;
    }

    private void SelectSlot(SaveSlot slot) // 칸 선택
    {
        selectedSlot = slot;
        pendingAction = PendingAction.None;
        Refresh();
    }

    public void Refresh() // 화면 갱신
    {
        ClearRows();
        BuildSlotList();
        UpdateDetail();
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
