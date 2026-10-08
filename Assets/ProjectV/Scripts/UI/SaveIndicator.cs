using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 저장했을 때 화면 오른쪽 위에 잠깐 나타나는 표시 (기획서 15.6)
// 진행 데이터 관리자에 붙어 씬이 바뀌어도 유지된다. 표시용 캔버스는 실행 중에 만든다.
public class SaveIndicator : MonoBehaviour
{
    private const float ShowSeconds = 1.4f; // 또렷하게 보이는 시간
    private const float FadeSeconds = 0.5f; // 사라지는 시간

    private static readonly Color PlateColor = new Color(0.06f, 0.07f, 0.14f, 0.82f); // 받침 이미지가 없을 때 색
    private static readonly Color FailTextColor = new Color(1f, 0.58f, 0.52f, 1f);    // 저장 실패 글자색

    private PlayerProgressManager progress; // 살펴볼 진행 데이터
    private CanvasGroup canvasGroup;        // 표시 전체의 투명도
    private Image iconImage;                // 저장 아이콘
    private TMP_Text labelText;             // 저장 문구
    private float remainingSeconds;         // 남은 표시 시간

    private void Start()
    {
        progress = GetComponent<PlayerProgressManager>();

        if (progress == null) { return; }

        BuildView();
        progress.SaveFinished += HandleSaveFinished;
    }

    private void OnDestroy()
    {
        if (progress != null)
        {
            progress.SaveFinished -= HandleSaveFinished;
        }
    }

    private void Update()
    {
        if (canvasGroup == null || remainingSeconds <= 0f) { return; }

        remainingSeconds -= Time.unscaledDeltaTime;
        canvasGroup.alpha = Mathf.Clamp01(remainingSeconds / FadeSeconds); // 끝날 때 서서히 사라진다.
    }

    private void HandleSaveFinished(SaveSlot slot, bool succeeded)
    {
        if (canvasGroup == null) { return; }

        string iconKey = !succeeded
            ? UIKeys.SaveWarning
            : slot == SaveSlot.Auto ? UIKeys.SaveAuto : UIKeys.SaveWrite;

        iconImage.enabled = UISkin.ApplySimple(iconImage, iconKey, true); // 아이콘 이미지가 없으면 글자만 보여준다.

        labelText.text = !succeeded
            ? "저장 실패"
            : slot == SaveSlot.Auto ? "자동 저장" : "저장 완료";

        labelText.color = succeeded ? UISkin.Cream : FailTextColor;

        remainingSeconds = ShowSeconds + FadeSeconds;
        canvasGroup.alpha = 1f;
    }

    private void BuildView() // 다른 화면 위에 그려지는 표시용 캔버스
    {
        GameObject canvasObject = new GameObject("SaveIndicatorCanvas", typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500; // 모든 화면 위에 그린다.

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        canvasGroup = canvasObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false; // 아래 화면의 버튼을 가리지 않는다.

        Vector2 topRight = new Vector2(1f, 1f);
        Vector2 leftCenter = new Vector2(0f, 0.5f);

        Image plate = CardEntryFactory.CreateImage(
            canvasObject.transform, "SaveIndicatorPlate", Color.white,
            topRight, topRight,
            new Vector2(-226f, -166f), new Vector2(-16f, -116f)
        ); // 상단 띠 바로 아래

        if (!UISkin.ApplyBar(plate, UIKeys.PlateLabel)) { plate.color = PlateColor; }

        iconImage = CardEntryFactory.CreateImage(
            plate.transform, "SaveIndicatorIcon", Color.white,
            leftCenter, leftCenter,
            new Vector2(16f, -17f), new Vector2(50f, 17f)
        );

        labelText = CardEntryFactory.CreateLabel(
            plate.transform, "SaveIndicatorText", "자동 저장",
            20f, UISkin.Cream, TextAlignmentOptions.Left,
            Vector2.zero, Vector2.one,
            new Vector2(60f, 0f), new Vector2(-14f, 0f)
        );
    }
}
