using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 스토리 회상 화면 (기획서 4.16.4)
// 왼쪽 목록에서 장면을 고르고 오른쪽에서 다시 본다. 본 장면만 다시 볼 수 있고, 보지 않은 장면은 나오는 곳만 알려 준다.
// 다시 본 장면은 진행과 보상에 영향을 주지 않는다.
public partial class StoryRecallFlow : MonoBehaviour
{
    [Header("화면 이동")]
    [SerializeField] private Button backButton; // 돌아가기
    [SerializeField] private Button playButton; // 다시 보기

    [Header("목록")]
    [SerializeField] private Transform listContent; // 장면 줄 배치 영역
    [SerializeField] private TMP_Text countText;    // 본 장면 수

    [Header("상세")]
    [SerializeField] private TMP_Text detailTitleText; // 고른 장면의 제목
    [SerializeField] private TMP_Text detailText;      // 지역, 나오는 곳, 줄 수
    [SerializeField] private TMP_Text messageText;     // 안내 문구

    private static string selectedSceneId = string.Empty; // 고른 장면. 다시 보고 돌아왔을 때 같은 줄을 고른 채로 둔다.

    private readonly List<GameObject> generatedRows = new List<GameObject>(); // 생성한 줄

    private List<StoryCatalogEntry> entries = new List<StoryCatalogEntry>(); // 진행 순서대로 모은 장면

    private void Awake()
    {
        backButton = SceneUIBinder.Bind(backButton, "BackButton");
        playButton = SceneUIBinder.Bind(playButton, "PlayButton");
        listContent = SceneUIBinder.Bind(listContent, "RecallListContent");
        countText = SceneUIBinder.Bind(countText, "CountText");
        detailTitleText = SceneUIBinder.Bind(detailTitleText, "DetailTitleText");
        detailText = SceneUIBinder.Bind(detailText, "DetailText");
        messageText = SceneUIBinder.Bind(messageText, "MessageText");
    }

    private void Start()
    {
        AddClickListener(backButton, SceneFlow.ReturnToPreviousScene);
        AddClickListener(playButton, PlaySelected);

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        entries = progress == null
            ? new List<StoryCatalogEntry>()
            : StoryCatalog.Build(progress.Regions);

        Refresh();
    }

    private static void AddClickListener(Button targetButton, UnityEngine.Events.UnityAction clickAction)
    {
        if (targetButton == null) { return; }

        targetButton.onClick.RemoveAllListeners();
        targetButton.onClick.AddListener(clickAction);
    }

    private static bool IsSeen(StoryCatalogEntry entry) // 본 장면인지 여부
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        return progress != null && entry != null && entry.Scene != null &&
               progress.IsStorySeen(entry.Scene.SceneId);
    }

    // 고른 장면을 찾는다. 고른 적이 없으면 본 장면 가운데 첫 장면, 그것도 없으면 목록의 첫 줄이다.
    private StoryCatalogEntry GetSelectedEntry()
    {
        StoryCatalogEntry firstSeen = null;

        foreach (StoryCatalogEntry entry in entries)
        {
            if (entry.Scene.SceneId == selectedSceneId) { return entry; }
            if (firstSeen == null && IsSeen(entry)) { firstSeen = entry; }
        }

        if (firstSeen != null) { return firstSeen; }

        return entries.Count > 0 ? entries[0] : null;
    }

    private void SelectEntry(StoryCatalogEntry entry)
    {
        selectedSceneId = entry == null ? string.Empty : entry.Scene.SceneId;

        Refresh();
    }

    private void PlaySelected() // 고른 장면을 다시 본다. 끝나면 이 화면으로 돌아온다.
    {
        StoryCatalogEntry entry = GetSelectedEntry();

        if (entry == null || !IsSeen(entry)) { return; }

        selectedSceneId = entry.Scene.SceneId;

        SceneFlow.LoadStoryAsRecall(entry.Scene);
    }

    private void ClearRows()
    {
        foreach (GameObject generatedRow in generatedRows)
        {
            if (generatedRow != null) { Destroy(generatedRow); }
        }

        generatedRows.Clear();
    }

    private static void SetText(TMP_Text target, string content)
    {
        if (target != null) { target.text = content; }
    }
}
