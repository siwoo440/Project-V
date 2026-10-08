using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 저장 화면 (기획서 10.5 / 15.3)
public static partial class SceneUIBuilder
{
    // 저장 화면: 왼쪽은 저장 칸 4개, 오른쪽은 선택한 칸의 진행 정보와 저장, 불러오기, 삭제.
    private static void BuildSaveLoadScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();

        EnsureBackground(
            canvas.transform,
            UISkin.Has(UIKeys.BgSaveLoad) ? UIKeys.BgSaveLoad : UIKeys.BgDeckBuilder
        ); // 전용 배경이 없으면 서재 배경을 쓴다.

        EnsureTopBar(canvas.transform, 72f, 104f);
        EnsureBottomBar(canvas.transform, 92f);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "저장 / 불러오기",
            48f, AccentColor, TextAlignmentOptions.Left);

        PlaceSceneTitle(title, new Vector2(340f, -72f), new Vector2(560f, 60f), 48f);

        // 왼쪽: 저장 칸 목록 (자동 저장 1개, 수동 저장 3개)
        GameObject slotPanel = EnsurePanel("SlotPanel", canvas.transform, PanelColor);
        SetAnchored(slotPanel, new Vector2(0f, 0.5f), new Vector2(590f, -30f), new Vector2(1120f, 790f));

        GameObject slotContent = EnsureVerticalScrollList(
            "SlotListView", "SlotListContent", slotPanel.transform,
            new Vector4(24f, 24f, 24f, 24f));

        // 오른쪽: 선택한 칸의 진행 정보
        GameObject detailPanel = EnsurePanel("DetailPanel", canvas.transform, PanelDeepColor);
        SetAnchored(detailPanel, new Vector2(1f, 0.5f), new Vector2(-385f, -30f), new Vector2(710f, 790f));

        Image detailIcon = EnsureIcon(
            "DetailIconImage", detailPanel.transform, UIKeys.SaveEmpty,
            new Vector2(0.5f, 1f), new Vector2(0f, -92f), 104f);

        TextMeshProUGUI detailTitle = EnsureText(
            "DetailTitleText", detailPanel.transform, "저장 칸을 선택하세요",
            34f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(detailTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -176f), new Vector2(620f, 50f));

        TextMeshProUGUI detailText = EnsureText(
            "DetailText", detailPanel.transform, "",
            20f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(detailText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -370f), new Vector2(620f, 320f));

        Button saveButton = EnsureSaveActionButton(
            "SaveButton", detailPanel.transform, "저장", AccentColor, 196f, UIKeys.SaveWrite);

        Button loadButton = EnsureSaveActionButton(
            "LoadButton", detailPanel.transform, "불러오기", ButtonColor, 128f, UIKeys.SaveLoad);

        Button deleteButton = EnsureSaveActionButton(
            "DeleteButton", detailPanel.transform, "삭제", WarningColor, 60f, UIKeys.SaveDelete);

        // 하단 안내와 돌아가기
        TextMeshProUGUI messageText = EnsureText(
            "MessageText", canvas.transform, "칸을 고르세요.",
            22f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(messageText.gameObject,
            new Vector2(0f, 0f), new Vector2(660f, 56f), new Vector2(1220f, 40f));

        Button backButton = EnsureButton(
            "BackButton", canvas.transform, "돌아가기", ButtonColor);

        SetAnchored(backButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-160f, 56f), new Vector2(200f, 60f));

        StyleButtonByName("BackButton", ButtonColor, 22f);

        GameObject controller = EnsureObject("SaveLoadController", null);
        SaveLoadFlow flow = controller.AddComponentIfMissing<SaveLoadFlow>();

        AssignReference(flow, "backButton", backButton);
        AssignReference(flow, "slotListContent", slotContent.transform);
        AssignReference(flow, "detailIconImage", detailIcon);
        AssignReference(flow, "detailTitleText", detailTitle);
        AssignReference(flow, "detailText", detailText);
        AssignReference(flow, "saveButton", saveButton);
        AssignReference(flow, "loadButton", loadButton);
        AssignReference(flow, "deleteButton", deleteButton);
        AssignReference(flow, "titleText", title);
        AssignReference(flow, "messageText", messageText);
    }

    // 저장 화면 상세 패널 아래쪽의 동작 버튼 하나를 만든다.
    private static Button EnsureSaveActionButton(
        string objectName,
        Transform parent,
        string label,
        Color buttonColor,
        float bottomY,
        string iconKey)
    {
        Button button = EnsureButton(objectName, parent, label, buttonColor);

        SetAnchored(button.gameObject,
            new Vector2(0.5f, 0f), new Vector2(0f, bottomY), new Vector2(440f, 58f));

        StyleButtonByName(objectName, buttonColor, 23f);
        EnsureButtonIcon(button, iconKey);

        return button;
    }
}
