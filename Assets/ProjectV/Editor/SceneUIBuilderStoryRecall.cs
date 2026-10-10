using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 스토리 선택지와 스토리 회상 화면 (기획서 2.10 / 4.16.4)
public static partial class SceneUIBuilder
{
    private const int StoryChoiceButtonCount = 4; // 한 번에 띄우는 선택지의 최대 수

    // 스토리 화면의 선택지 버튼 묶음. 대화창 위 가운데에 세로로 쌓는다. 쓰지 않는 버튼은 실행 중에 숨긴다.
    private static void BuildStoryChoices(Canvas canvas, StoryFlow flow)
    {
        GameObject choicePanel = EnsureObject("StoryChoicePanel", canvas.transform);

        SetAnchored(choicePanel, new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1040f, 330f));
        ApplyVerticalLayout(choicePanel, 14f, 0);

        Button[] choiceButtons = new Button[StoryChoiceButtonCount];

        for (int i = 0; i < StoryChoiceButtonCount; i++)
        {
            string buttonName = "StoryChoiceButton" + (i + 1);

            Button choiceButton = EnsureButton(buttonName, choicePanel.transform, "선택지", ButtonColor);

            StyleButtonByName(buttonName, ButtonColor, 24f);

            LayoutElement choiceLayout = choiceButton.GetComponent<LayoutElement>();

            choiceLayout.minHeight = 66f;
            choiceLayout.preferredHeight = 66f;

            TextMeshProUGUI choiceLabel = choiceButton.GetComponentInChildren<TextMeshProUGUI>(true);

            if (choiceLabel != null)
            {
                choiceLabel.richText = true; // 앞에 답변의 분위기를 작은 글자로 적는다.
                choiceLabel.enableAutoSizing = true; // 긴 대답은 글자를 줄여 한 줄에 맞춘다.
                choiceLabel.fontSizeMin = 17f;
                choiceLabel.fontSizeMax = 24f;

                RectTransform labelRect = choiceLabel.GetComponent<RectTransform>();

                labelRect.offsetMin = new Vector2(36f, 0f);
                labelRect.offsetMax = new Vector2(-36f, 0f);
            }

            choiceButton.transform.SetSiblingIndex(i);
            choiceButtons[i] = choiceButton;
        }

        AssignReference(flow, "choicePanel", choicePanel);
        AssignReferenceArray(flow, "choiceButtons", choiceButtons);

        choicePanel.SetActive(false);
    }

    // 스토리 회상 화면: 왼쪽은 장면 목록, 오른쪽은 고른 장면의 정보와 다시 보기.
    private static void BuildStoryRecallScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();
        EnsureBackground(canvas.transform, UIKeys.BgDeckBuilder);

        EnsureTopBar(canvas.transform, 72f, 104f);
        EnsureBottomBar(canvas.transform, 92f);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "스토리 회상",
            48f, AccentColor, TextAlignmentOptions.Left);

        PlaceSceneTitle(title, new Vector2(300f, -72f), new Vector2(480f, 60f), 48f);

        TextMeshProUGUI countText = EnsureText(
            "CountText", canvas.transform, "본 장면 0 / 0",
            26f, AccentColor, TextAlignmentOptions.Right);

        SetAnchored(countText.gameObject,
            new Vector2(1f, 1f), new Vector2(-420f, -72f), new Vector2(760f, 60f));

        // 왼쪽: 진행 순서대로 늘어놓은 장면 목록
        GameObject listPanel = EnsurePanel("RecallListPanel", canvas.transform, PanelColor);
        SetAnchored(listPanel, new Vector2(0f, 0.5f), new Vector2(590f, -30f), new Vector2(1120f, 790f));

        GameObject listContent = EnsureVerticalScrollList(
            "RecallListView", "RecallListContent", listPanel.transform,
            new Vector4(24f, 24f, 24f, 24f));

        // 오른쪽: 고른 장면의 지역, 나오는 곳, 줄 수
        GameObject detailPanel = EnsurePanel("DetailPanel", canvas.transform, PanelDeepColor);
        SetAnchored(detailPanel, new Vector2(1f, 0.5f), new Vector2(-385f, -30f), new Vector2(710f, 790f));

        TextMeshProUGUI detailTitle = EnsureText(
            "DetailTitleText", detailPanel.transform, "장면을 선택하세요",
            34f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(detailTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(620f, 56f));

        TextMeshProUGUI detailText = EnsureText(
            "DetailText", detailPanel.transform, "",
            23f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(detailText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -350f), new Vector2(600f, 400f));

        Button playButton = EnsureButton("PlayButton", detailPanel.transform, "다시 보기", AccentColor);

        SetAnchored(playButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(0f, 74f), new Vector2(400f, 68f));

        StyleButtonByName("PlayButton", AccentColor, 24f);

        // 하단 안내와 돌아가기
        TextMeshProUGUI messageText = EnsureText(
            "MessageText", canvas.transform, "장면을 고르세요.",
            22f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(messageText.gameObject,
            new Vector2(0f, 0f), new Vector2(660f, 56f), new Vector2(1220f, 40f));

        Button backButton = EnsureButton("BackButton", canvas.transform, "돌아가기", ButtonColor);

        SetAnchored(backButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-160f, 56f), new Vector2(200f, 60f));

        StyleButtonByName("BackButton", ButtonColor, 22f);

        GameObject controller = EnsureObject("StoryRecallController", null);
        StoryRecallFlow flow = controller.AddComponentIfMissing<StoryRecallFlow>();

        AssignReference(flow, "backButton", backButton);
        AssignReference(flow, "playButton", playButton);
        AssignReference(flow, "listContent", listContent.transform);
        AssignReference(flow, "countText", countText);
        AssignReference(flow, "detailTitleText", detailTitle);
        AssignReference(flow, "detailText", detailText);
        AssignReference(flow, "messageText", messageText);
    }

    // 배열로 된 참조 항목을 채운다. (선택지 버튼처럼 같은 것을 여러 개 연결할 때)
    private static void AssignReferenceArray(Component target, string fieldName, Object[] values)
    {
        SerializedObject serializedTarget = new SerializedObject(target);
        SerializedProperty property = serializedTarget.FindProperty(fieldName);

        if (property == null || !property.isArray)
        {
            Debug.LogWarning($"{target.GetType().Name}에 {fieldName} 배열 항목이 없습니다.");
            return;
        }

        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        serializedTarget.ApplyModifiedPropertiesWithoutUndo();
    }
}
