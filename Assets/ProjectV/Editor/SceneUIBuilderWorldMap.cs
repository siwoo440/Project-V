using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 월드맵 화면 (기획서 4.3 / 11.6)
public static partial class SceneUIBuilder
{
    // 월드맵: 위쪽은 상단 메뉴, 왼쪽은 대륙 지도와 지역 표시, 오른쪽은 고른 지역의 정보.
    // 지도 그림은 틀 안에 따로 놓는다. 화면 전체에 깔면 메뉴와 정보 창이 지역을 가리기 때문이다.
    private static void BuildWorldMapScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();
        EnsureBackground(canvas.transform, UIKeys.BgDeckBuilder);

        EnsureTopBar(canvas.transform, 72f, 104f);
        EnsureBottomBar(canvas.transform, 92f);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "월드맵",
            48f, AccentColor, TextAlignmentOptions.Left);

        PlaceSceneTitle(title, new Vector2(260f, -72f), new Vector2(400f, 60f), 48f);

        TextMeshProUGUI resourceText = EnsureText(
            "ResourceText", canvas.transform, "Lv.1    골드 0    정수 0    파편 0",
            26f, AccentColor, TextAlignmentOptions.Right);

        SetAnchored(resourceText.gameObject,
            new Vector2(1f, 1f), new Vector2(-460f, -72f), new Vector2(840f, 60f));

        // 상단 메뉴 (기획서 11.6.2). 도감, 갤러리, 설정은 해당 기능을 만드는 일차에 더한다.
        Button deckButton = EnsureWorldMapMenuButton("DeckBuilderButton", canvas.transform, "덱 편성", 0, UIKeys.IconDeck);
        Button enhanceButton = EnsureWorldMapMenuButton("EnhanceButton", canvas.transform, "마물 강화", 1, UIKeys.IconEnhance);
        Button summonerButton = EnsureWorldMapMenuButton("SummonerButton", canvas.transform, "소환사", 2, UIKeys.IconSummoner);
        Button grimoireButton = EnsureWorldMapMenuButton("GrimoireButton", canvas.transform, "그리모어 강화", 3, UIKeys.IconGrimoire);
        Button shopButton = EnsureWorldMapMenuButton("ShopButton", canvas.transform, "상점", 4, UIKeys.IconShop);
        Button saveButton = EnsureWorldMapMenuButton("SaveButton", canvas.transform, "저장", 5, UIKeys.SaveWrite);

        // 지도 틀과 대륙 그림. 지역 표시는 실행 중에 MapArea 안에 만든다.
        GameObject mapFrame = EnsurePanel("MapFrame", canvas.transform, PanelDeepColor);
        SetAnchored(mapFrame, new Vector2(0f, 1f), new Vector2(720f, -584f), new Vector2(1360f, 776f));

        GameObject mapImageObject = EnsureObject("MapImage", mapFrame.transform);
        SetStretch(mapImageObject, new Vector4(12f, 12f, 12f, 12f));

        Image mapImage = mapImageObject.AddComponentIfMissing<Image>();
        mapImage.raycastTarget = false;

        if (!UISkin.ApplySimple(mapImage, UIKeys.BgStageSelect, false))
        {
            mapImage.color = BackgroundColor; // 지도 그림이 없으면 단색
        }

        GameObject mapArea = EnsureObject("MapArea", mapImageObject.transform);
        SetStretch(mapArea, Vector4.zero);

        // 오른쪽: 고른 지역의 정보 (기획서 11.6.3)
        GameObject regionPanel = EnsurePanel("RegionPanel", canvas.transform, PanelColor);
        SetAnchored(regionPanel, new Vector2(1f, 1f), new Vector2(-270f, -584f), new Vector2(460f, 776f));

        Image regionIcon = EnsureIcon(
            "RegionIconImage", regionPanel.transform, UIKeys.IconWorldMap,
            new Vector2(0.5f, 1f), new Vector2(0f, -100f), 128f);

        TextMeshProUGUI regionName = EnsureText(
            "RegionNameText", regionPanel.transform, "지역을 선택하세요",
            32f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(regionName.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(400f, 50f));

        TextMeshProUGUI regionInfo = EnsureText(
            "RegionInfoText", regionPanel.transform, "",
            20f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(regionInfo.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -430f), new Vector2(390f, 390f));

        Button enterButton = EnsureButton("EnterButton", regionPanel.transform, "지역 진입", AccentColor);

        SetAnchored(enterButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(0f, 78f), new Vector2(360f, 70f));

        StyleButtonByName("EnterButton", AccentColor, 26f);

        // 하단 안내와 메인 메뉴로 돌아가기
        TextMeshProUGUI messageText = EnsureText(
            "MessageText", canvas.transform, "지역을 고르고 들어가세요.",
            22f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(messageText.gameObject,
            new Vector2(0f, 0f), new Vector2(660f, 56f), new Vector2(1220f, 40f));

        Button titleButton = EnsureButton("TitleButton", canvas.transform, "메인 메뉴", ButtonColor);

        SetAnchored(titleButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-160f, 56f), new Vector2(200f, 60f));

        StyleButtonByName("TitleButton", ButtonColor, 22f);

        GameObject controller = EnsureObject("WorldMapController", null);
        WorldMapFlow flow = controller.AddComponentIfMissing<WorldMapFlow>();

        AssignReference(flow, "deckButton", deckButton);
        AssignReference(flow, "enhanceButton", enhanceButton);
        AssignReference(flow, "summonerButton", summonerButton);
        AssignReference(flow, "grimoireButton", grimoireButton);
        AssignReference(flow, "shopButton", shopButton);
        AssignReference(flow, "saveButton", saveButton);
        AssignReference(flow, "titleButton", titleButton);
        AssignReference(flow, "mapArea", mapArea.GetComponent<RectTransform>());
        AssignReference(flow, "regionIconImage", regionIcon);
        AssignReference(flow, "regionNameText", regionName);
        AssignReference(flow, "regionInfoText", regionInfo);
        AssignReference(flow, "enterButton", enterButton);
        AssignReference(flow, "resourceText", resourceText);
        AssignReference(flow, "messageText", messageText);
    }
}
