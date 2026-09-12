using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 씬 UI 생성 및 연결 도구
// 기획서 11.2 해상도, 11.4.1 기본 색상, 11.4.2 희귀도 색상을 기준으로 구성한다.
public static class SceneUIBuilder
{
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;

    private static readonly Color BackgroundColor = new Color(0.05f, 0.05f, 0.07f, 1f);
    private static readonly Color PanelColor = new Color(0.13f, 0.10f, 0.20f, 0.96f);
    private static readonly Color PanelDeepColor = new Color(0.08f, 0.07f, 0.12f, 0.96f);
    private static readonly Color ButtonColor = new Color(0.20f, 0.16f, 0.31f, 1f);
    private static readonly Color AccentColor = new Color(1f, 0.82f, 0.36f, 1f);
    private static readonly Color TextColor = new Color(0.93f, 0.92f, 0.96f, 1f);
    private static readonly Color SubTextColor = new Color(0.62f, 0.60f, 0.70f, 1f);
    private static readonly Color WarningColor = new Color(0.45f, 0.14f, 0.18f, 1f);

    private const string SceneFolder = "Assets/ProjectV/Scenes/";
    private const string MonsterUnitPrefabPath =
        "Assets/ProjectV/Prefabs/UI/MonsterUnit.prefab";

    private const string CardButtonPrefabPath =
        "Assets/ProjectV/Prefabs/UI/CardButton.prefab";

    // 기획서 6.8.2 시작 덱 구성
    private static readonly string[] StartingCardIds =
    {
        "CRD-GOB-01", "CRD-GOB-02", "CRD-GOB-04", "CRD-SLM-01",
        "CRD-SLM-02", "CRD-DEM-01", "CRD-SPI-01", "CRD-SPI-02",
        "CRD-MEC-01", "CRD-UND-01", "CRD-ANG-01",
    };

    private static readonly int[] StartingCardCounts =
    {
        3, 3, 2, 3, 3, 3, 3, 3, 3, 3, 1,
    };

    [MenuItem("Project V/씬 UI 다시 구성", false, 10)]
    public static void RebuildAllScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        string originalScenePath =
            SceneManager.GetActiveScene().path;

        BuildMonsterUnitPrefab();
        BuildCardButtonPrefab();

        BuildScene("00_Bootstrap", BuildBootstrapScene);
        BuildScene("01_MainMenu", BuildMainMenuScene);
        BuildScene("02_DeckBuilder", BuildDeckBuilderScene);
        BuildScene("03_StageSelect", BuildStageSelectScene);
        BuildScene("04_Story", BuildStoryScene);
        BuildScene("05_Enhance", BuildEnhanceScene);
        BuildScene("BattleScene", BuildBattleSceneExtras);

        if (!string.IsNullOrEmpty(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        Debug.Log("씬 UI 구성을 완료했습니다.");
    }

    private static void BuildScene(string sceneName, System.Action buildAction)
    {
        string scenePath = SceneFolder + sceneName + ".unity";

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        if (!scene.IsValid())
        {
            Debug.LogWarning("씬을 찾지 못했습니다: " + scenePath);
            return;
        }

        buildAction();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("씬 UI를 구성했습니다: " + sceneName);
    }

    // ---------- 씬별 구성 ----------

    private static void BuildBootstrapScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();
        EnsureBackground(canvas.transform);

        TextMeshProUGUI loadingText = EnsureText(
            "LoadingText", canvas.transform, "프로젝트 V",
            72f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(loadingText.gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(900f, 120f));

        TextMeshProUGUI hintText = EnsureText(
            "LoadingHintText", canvas.transform, "불러오는 중...",
            28f, SubTextColor, TextAlignmentOptions.Center);

        SetAnchored(hintText.gameObject,
            new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(900f, 60f));

        GameObject controller = EnsureObject("BootstrapController", null);
        controller.AddComponentIfMissing<BootstrapFlow>();

        GameObject progressObject = EnsureObject("PlayerProgressManager", null);
        PlayerProgressManager progress =
            progressObject.AddComponentIfMissing<PlayerProgressManager>();

        ApplyStartingCards(progress);
    }

    private static void BuildMainMenuScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();
        EnsureBackground(canvas.transform);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "프로젝트 V",
            96f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(title.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(1200f, 140f));

        TextMeshProUGUI subtitle = EnsureText(
            "SubtitleText", canvas.transform, "소환사와 마물 카드 전투",
            30f, SubTextColor, TextAlignmentOptions.Center);

        SetAnchored(subtitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(1200f, 60f));

        GameObject panel = EnsurePanel("MainMenuPanel", canvas.transform, PanelColor);
        SetAnchored(panel, new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(560f, 620f));
        ApplyVerticalLayout(panel, 24f, 48);

        Button storyButton = EnsureButton("StoryButton", panel.transform, "스토리", ButtonColor);
        Button stageButton = EnsureButton("StageSelectButton", panel.transform, "지역 선택", ButtonColor);
        Button deckButton = EnsureButton("DeckBuilderButton", panel.transform, "덱 편성", ButtonColor);
        Button enhanceButton = EnsureButton("EnhanceButton", panel.transform, "마물 강화", ButtonColor);
        Button quitButton = EnsureButton("QuitButton", panel.transform, "게임 종료", WarningColor);

        storyButton.transform.SetSiblingIndex(0);
        stageButton.transform.SetSiblingIndex(1);
        deckButton.transform.SetSiblingIndex(2);
        enhanceButton.transform.SetSiblingIndex(3);
        quitButton.transform.SetSiblingIndex(4);

        TextMeshProUGUI progressText = EnsureText(
            "ProgressText", canvas.transform, "골드 0    정수 0    보유 카드 0    덱 0",
            26f, SubTextColor, TextAlignmentOptions.Center);

        SetAnchored(progressText.gameObject,
            new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(1400f, 50f));

        GameObject controller = EnsureObject("MainMenuController", null);
        MainMenuFlow flow = controller.AddComponentIfMissing<MainMenuFlow>();

        AssignReference(flow, "storyButton", storyButton);
        AssignReference(flow, "stageSelectButton", stageButton);
        AssignReference(flow, "deckBuilderButton", deckButton);
        AssignReference(flow, "enhanceButton", enhanceButton);
        AssignReference(flow, "quitButton", quitButton);
        AssignReference(flow, "progressText", progressText);
    }

    private static void BuildDeckBuilderScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();
        EnsureBackground(canvas.transform);

        SetActiveByName("DeckBuilderPanel", false); // 비어 있는 껍데기 패널 숨김

        // 강화는 05_Enhance 씬으로 옮겼으므로 이전 구성의 잔재를 지운다.
        DestroyByName("EnhancePanel");
        DestroyByName("ModeButton");
        DestroyByName("ResourceText");

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "덱 편성",
            48f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(title.gameObject,
            new Vector2(0f, 1f), new Vector2(260f, -72f), new Vector2(400f, 60f));

        Button enhanceSceneButton = EnsureButton(
            "EnhanceSceneButton", canvas.transform, "마물 강화", AccentColor);

        SetAnchored(enhanceSceneButton.gameObject,
            new Vector2(0f, 1f), new Vector2(660f, -72f), new Vector2(240f, 56f));

        StyleButtonByName("EnhanceSceneButton", AccentColor, 21f);

        TextMeshProUGUI deckCountText = EnsureText(
            "DeckCountText", canvas.transform, "덱 0 / 30      평균 마나 0.00",
            28f, TextColor, TextAlignmentOptions.Right);

        SetAnchored(deckCountText.gameObject,
            new Vector2(1f, 1f), new Vector2(-320f, -72f), new Vector2(560f, 60f));

        // 프리셋 줄
        for (int i = 0; i < 5; i++)
        {
            Button presetButton = EnsureButton(
                $"PresetButton{i + 1}", canvas.transform, $"덱 {i + 1}", ButtonColor);

            SetAnchored(presetButton.gameObject,
                new Vector2(0f, 1f), new Vector2(180f + i * 150f, -150f), new Vector2(140f, 56f));

            StyleButtonByName($"PresetButton{i + 1}", ButtonColor, 20f);
        }

        TMP_InputField presetNameInput = EnsureInputField(
            "PresetNameInput", canvas.transform, "프리셋 이름");

        SetAnchored(presetNameInput.gameObject,
            new Vector2(0f, 1f), new Vector2(1010f, -150f), new Vector2(300f, 56f));

        Button copyPresetButton = EnsureButton(
            "CopyPresetButton", canvas.transform, "덱 복사", ButtonColor);

        SetAnchored(copyPresetButton.gameObject,
            new Vector2(0f, 1f), new Vector2(1320f, -150f), new Vector2(220f, 56f));

        StyleButtonByName("CopyPresetButton", ButtonColor, 20f);

        // 필터와 정렬 줄
        Button typeFilterButton = EnsureButton(
            "TypeFilterButton", canvas.transform, "계열: 전체", ButtonColor);

        SetAnchored(typeFilterButton.gameObject,
            new Vector2(0f, 1f), new Vector2(200f, -216f), new Vector2(220f, 52f));

        Button rarityFilterButton = EnsureButton(
            "RarityFilterButton", canvas.transform, "희귀도: 전체", ButtonColor);

        SetAnchored(rarityFilterButton.gameObject,
            new Vector2(0f, 1f), new Vector2(430f, -216f), new Vector2(220f, 52f));

        Button manaFilterButton = EnsureButton(
            "ManaFilterButton", canvas.transform, "마나: 전체", ButtonColor);

        SetAnchored(manaFilterButton.gameObject,
            new Vector2(0f, 1f), new Vector2(660f, -216f), new Vector2(200f, 52f));

        TMP_InputField searchInput = EnsureInputField(
            "SearchInput", canvas.transform, "카드 이름 검색");

        SetAnchored(searchInput.gameObject,
            new Vector2(0f, 1f), new Vector2(900f, -216f), new Vector2(300f, 52f));

        Button sortButton = EnsureButton(
            "SortButton", canvas.transform, "정렬: 이름순", ButtonColor);

        SetAnchored(sortButton.gameObject,
            new Vector2(0f, 1f), new Vector2(1230f, -216f), new Vector2(260f, 52f));

        StyleButtonByName("TypeFilterButton", ButtonColor, 19f);
        StyleButtonByName("RarityFilterButton", ButtonColor, 19f);
        StyleButtonByName("ManaFilterButton", ButtonColor, 19f);
        StyleButtonByName("SortButton", ButtonColor, 19f);

        // 좌측 보유 카드
        GameObject ownedPanel = EnsurePanel("OwnedCardsPanel", canvas.transform, PanelColor);
        SetAnchored(ownedPanel, new Vector2(0f, 0.5f), new Vector2(500f, -70f), new Vector2(860f, 620f));

        TextMeshProUGUI ownedTitle = EnsureText(
            "OwnedCardsTitleText", ownedPanel.transform, "보유 카드",
            30f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(ownedTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(800f, 40f));

        GameObject ownedContent = EnsureScrollList(
            "OwnedCardsView", "OwnedCardsContent", ownedPanel.transform,
            new Vector4(22f, 74f, 22f, 22f));

        // 우측 현재 덱
        GameObject deckPanel = EnsurePanel("CurrentDeckPanel", canvas.transform, PanelDeepColor);
        SetAnchored(deckPanel, new Vector2(1f, 0.5f), new Vector2(-500f, -70f), new Vector2(860f, 620f));

        TextMeshProUGUI deckTitle = EnsureText(
            "CurrentDeckTitleText", deckPanel.transform, "현재 덱",
            30f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(deckTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(800f, 40f));

        GameObject deckContent = EnsureScrollList(
            "CurrentDeckView", "CurrentDeckContent", deckPanel.transform,
            new Vector4(22f, 74f, 22f, 22f));

        // 좌측 하단 통계와 안내
        TextMeshProUGUI deckStatsText = EnsureText(
            "DeckStatsText", canvas.transform, "",
            20f, SubTextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(deckStatsText.gameObject,
            new Vector2(0f, 0f), new Vector2(500f, 108f), new Vector2(860f, 110f));

        TextMeshProUGUI messageText = EnsureText(
            "MessageText", canvas.transform, "",
            20f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(messageText.gameObject,
            new Vector2(0f, 0f), new Vector2(500f, 34f), new Vector2(860f, 36f));

        // 우측 하단 조작 버튼
        Button fillDeckButton = EnsureButton("FillDeckButton", canvas.transform, "보유 카드로 채우기", ButtonColor);
        SetAnchored(fillDeckButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-700f, 80f), new Vector2(300f, 64f));

        Button clearDeckButton = EnsureButton("ClearDeckButton", canvas.transform, "덱 비우기", ButtonColor);
        SetAnchored(clearDeckButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-400f, 80f), new Vector2(240f, 64f));

        Button backButton = EnsureButton("BackButton", canvas.transform, "돌아가기", ButtonColor);
        SetAnchored(backButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-160f, 80f), new Vector2(200f, 64f));

        StyleButtonByName("FillDeckButton", ButtonColor, 22f);
        StyleButtonByName("ClearDeckButton", ButtonColor, 22f);
        StyleButtonByName("BackButton", ButtonColor, 22f);

        GameObject controller = EnsureObject("DeckBuilderController", null);
        DeckBuilderFlow flow = controller.AddComponentIfMissing<DeckBuilderFlow>();

        AssignReference(flow, "backButton", backButton);
        AssignReference(flow, "clearDeckButton", clearDeckButton);
        AssignReference(flow, "fillDeckButton", fillDeckButton);
        AssignReference(flow, "deckStatsText", deckStatsText);
        AssignReference(flow, "ownedCardsContent", ownedContent.transform);
        AssignReference(flow, "currentDeckContent", deckContent.transform);
        AssignReference(flow, "deckCountText", deckCountText);
        AssignReference(flow, "messageText", messageText);
        AssignReference(flow, "presetNameInput", presetNameInput);
        AssignReference(flow, "copyPresetButton", copyPresetButton);
        AssignReference(flow, "typeFilterButton", typeFilterButton);
        AssignReference(flow, "rarityFilterButton", rarityFilterButton);
        AssignReference(flow, "manaFilterButton", manaFilterButton);
        AssignReference(flow, "sortButton", sortButton);
        AssignReference(flow, "enhanceSceneButton", enhanceSceneButton);
        AssignReference(flow, "searchInput", searchInput);
        ApplyPresetButtonList(flow);
    }

    // 마물 카드 강화 화면 (기획서 6.9 / 11.13)
    // 왼쪽은 덱 편성과 같은 보유 카드 목록, 오른쪽은 카드를 올려 강화하는 강화대.
    private static void BuildEnhanceScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();
        EnsureBackground(canvas.transform);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "마물 강화",
            48f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(title.gameObject,
            new Vector2(0f, 1f), new Vector2(260f, -72f), new Vector2(400f, 60f));

        Button deckBuilderButton = EnsureButton(
            "DeckBuilderButton", canvas.transform, "덱 편성", ButtonColor);

        SetAnchored(deckBuilderButton.gameObject,
            new Vector2(0f, 1f), new Vector2(640f, -72f), new Vector2(220f, 56f));

        StyleButtonByName("DeckBuilderButton", ButtonColor, 21f);

        TextMeshProUGUI resourceText = EnsureText(
            "ResourceText", canvas.transform, "골드 0      마물의 정수 0",
            28f, AccentColor, TextAlignmentOptions.Right);

        SetAnchored(resourceText.gameObject,
            new Vector2(1f, 1f), new Vector2(-320f, -72f), new Vector2(560f, 60f));

        // 필터와 정렬 줄
        Button typeFilterButton = EnsureButton(
            "TypeFilterButton", canvas.transform, "계열: 전체", ButtonColor);

        SetAnchored(typeFilterButton.gameObject,
            new Vector2(0f, 1f), new Vector2(200f, -150f), new Vector2(220f, 52f));

        Button rarityFilterButton = EnsureButton(
            "RarityFilterButton", canvas.transform, "희귀도: 전체", ButtonColor);

        SetAnchored(rarityFilterButton.gameObject,
            new Vector2(0f, 1f), new Vector2(430f, -150f), new Vector2(220f, 52f));

        Button manaFilterButton = EnsureButton(
            "ManaFilterButton", canvas.transform, "마나: 전체", ButtonColor);

        SetAnchored(manaFilterButton.gameObject,
            new Vector2(0f, 1f), new Vector2(660f, -150f), new Vector2(200f, 52f));

        TMP_InputField searchInput = EnsureInputField(
            "SearchInput", canvas.transform, "카드 이름 검색");

        SetAnchored(searchInput.gameObject,
            new Vector2(0f, 1f), new Vector2(900f, -150f), new Vector2(300f, 52f));

        Button sortButton = EnsureButton(
            "SortButton", canvas.transform, "정렬: 이름순", ButtonColor);

        SetAnchored(sortButton.gameObject,
            new Vector2(0f, 1f), new Vector2(1230f, -150f), new Vector2(260f, 52f));

        StyleButtonByName("TypeFilterButton", ButtonColor, 19f);
        StyleButtonByName("RarityFilterButton", ButtonColor, 19f);
        StyleButtonByName("ManaFilterButton", ButtonColor, 19f);
        StyleButtonByName("SortButton", ButtonColor, 19f);

        // 좌측 보유 카드 (덱 편성 화면과 같은 구성)
        GameObject ownedPanel = EnsurePanel("OwnedCardsPanel", canvas.transform, PanelColor);
        SetAnchored(ownedPanel, new Vector2(0f, 0.5f), new Vector2(500f, -40f), new Vector2(940f, 680f));

        TextMeshProUGUI ownedTitle = EnsureText(
            "OwnedCardsTitleText", ownedPanel.transform, "보유 카드",
            30f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(ownedTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(880f, 40f));

        GameObject ownedContent = EnsureScrollList(
            "OwnedCardsView", "OwnedCardsContent", ownedPanel.transform,
            new Vector4(22f, 74f, 22f, 22f));

        // 우측 강화대
        GameObject enhancePanel = EnsurePanel("EnhancePanel", canvas.transform, PanelDeepColor);
        SetAnchored(enhancePanel, new Vector2(1f, 0.5f), new Vector2(-470f, -40f), new Vector2(900f, 680f));

        TextMeshProUGUI enhanceTitle = EnsureText(
            "EnhanceTitleText", enhancePanel.transform, "강화대",
            30f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(enhanceTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(840f, 40f));

        // 카드를 올려 두는 자리
        GameObject slotObject = EnsurePanel("EnhanceSlot", enhancePanel.transform, ButtonColor);
        SetAnchored(slotObject, new Vector2(0f, 1f), new Vector2(160f, -250f), new Vector2(240f, 330f));

        GameObject slotHeader = EnsurePanel(
            "EnhanceSlotHeader", slotObject.transform, SubTextColor);

        SetAnchored(slotHeader, new Vector2(0.5f, 1f), new Vector2(0f, -7f), new Vector2(240f, 14f));

        TextMeshProUGUI slotNameText = EnsureText(
            "EnhanceSlotNameText", slotObject.transform, "카드를 올리세요",
            22f, TextColor, TextAlignmentOptions.Center);

        SetAnchored(slotNameText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(220f, 64f));

        TextMeshProUGUI slotInfoText = EnsureText(
            "EnhanceSlotInfoText", slotObject.transform, "왼쪽 보유 카드를\n눌러 주세요",
            18f, SubTextColor, TextAlignmentOptions.Center);

        SetAnchored(slotInfoText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(220f, 150f));

        TextMeshProUGUI slotLevelText = EnsureText(
            "EnhanceSlotLevelText", slotObject.transform, "",
            26f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(slotLevelText.gameObject,
            new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(220f, 44f));

        // 강화 전후 능력치와 비용
        TextMeshProUGUI enhanceStatText = EnsureText(
            "EnhanceStatText", enhancePanel.transform, "",
            21f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(enhanceStatText.gameObject,
            new Vector2(0f, 1f), new Vector2(600f, -200f), new Vector2(560f, 230f));

        TextMeshProUGUI enhanceCostText = EnsureText(
            "EnhanceCostText", enhancePanel.transform, "",
            20f, SubTextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(enhanceCostText.gameObject,
            new Vector2(0f, 1f), new Vector2(600f, -380f), new Vector2(560f, 120f));

        // 사본 목록
        GameObject copyContent = EnsureVerticalScrollList(
            "EnhanceCopyView", "EnhanceCopyContent", enhancePanel.transform,
            new Vector4(40f, 450f, 40f, 90f));

        Button enhanceButton = EnsureButton(
            "EnhanceButton", enhancePanel.transform, "강화", AccentColor);

        SetAnchored(enhanceButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(-140f, 42f), new Vector2(300f, 60f));

        Button clearSlotButton = EnsureButton(
            "ClearSlotButton", enhancePanel.transform, "내려놓기", ButtonColor);

        SetAnchored(clearSlotButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(180f, 42f), new Vector2(220f, 60f));

        StyleButtonByName("EnhanceButton", AccentColor, 24f);
        StyleButtonByName("ClearSlotButton", ButtonColor, 22f);

        // 하단 안내와 돌아가기
        TextMeshProUGUI messageText = EnsureText(
            "MessageText", canvas.transform, "강화할 카드를 왼쪽에서 고르세요.",
            22f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(messageText.gameObject,
            new Vector2(0f, 0f), new Vector2(500f, 46f), new Vector2(940f, 40f));

        Button backButton = EnsureButton(
            "BackButton", canvas.transform, "돌아가기", ButtonColor);

        SetAnchored(backButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-160f, 50f), new Vector2(200f, 64f));

        StyleButtonByName("BackButton", ButtonColor, 22f);

        GameObject controller = EnsureObject("EnhanceController", null);
        EnhanceFlow flow = controller.AddComponentIfMissing<EnhanceFlow>();

        AssignReference(flow, "backButton", backButton);
        AssignReference(flow, "deckBuilderButton", deckBuilderButton);
        AssignReference(flow, "typeFilterButton", typeFilterButton);
        AssignReference(flow, "rarityFilterButton", rarityFilterButton);
        AssignReference(flow, "manaFilterButton", manaFilterButton);
        AssignReference(flow, "sortButton", sortButton);
        AssignReference(flow, "searchInput", searchInput);
        AssignReference(flow, "ownedCardsContent", ownedContent.transform);
        AssignReference(flow, "slotHeaderImage", slotHeader.GetComponent<Image>());
        AssignReference(flow, "slotNameText", slotNameText);
        AssignReference(flow, "slotInfoText", slotInfoText);
        AssignReference(flow, "slotLevelText", slotLevelText);
        AssignReference(flow, "copyListContent", copyContent.transform);
        AssignReference(flow, "enhanceStatText", enhanceStatText);
        AssignReference(flow, "enhanceCostText", enhanceCostText);
        AssignReference(flow, "enhanceButton", enhanceButton);
        AssignReference(flow, "clearSlotButton", clearSlotButton);
        AssignReference(flow, "resourceText", resourceText);
        AssignReference(flow, "messageText", messageText);
    }

    private static void BuildStageSelectScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();
        EnsureBackground(canvas.transform);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "지역 선택",
            56f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(title.gameObject,
            new Vector2(0f, 1f), new Vector2(400f, -90f), new Vector2(700f, 80f));

        SendToBack("Panel");

        GameObject listPanel = EnsurePanel("StageListPanel", canvas.transform, PanelColor);
        SetAnchored(listPanel, new Vector2(0f, 0.5f), new Vector2(480f, -20f), new Vector2(760f, 660f));

        GameObject listContent = EnsureObject("StageListContent", listPanel.transform);
        SetStretch(listContent, new Vector4(30f, 30f, 30f, 30f));
        ApplyVerticalLayout(listContent, 12f, 0, TextAnchor.UpperLeft);

        GameObject detailPanel = EnsurePanel("SelectedStagePanel", canvas.transform, PanelDeepColor);
        SetAnchored(detailPanel, new Vector2(1f, 0.5f), new Vector2(-520f, -20f), new Vector2(880f, 660f));

        TextMeshProUGUI stageNameText = EnsureText(
            "StageNameText", detailPanel.transform, "스테이지 데이터 없음",
            40f, AccentColor, TextAlignmentOptions.TopLeft);

        SetAnchored(stageNameText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(780f, 70f));

        TextMeshProUGUI stageDescriptionText = EnsureText(
            "StageDescriptionText", detailPanel.transform, "",
            24f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(stageDescriptionText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -260f), new Vector2(780f, 320f));

        Button startButton = EnsureButton("StartBattleButton", detailPanel.transform, "전투 시작", AccentColor);
        SetAnchored(startButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(400f, 80f));

        SetButtonLabelColor(startButton, new Color(0.10f, 0.08f, 0.05f, 1f));

        Button deckButton = EnsureButton("DeckBuilderButton", canvas.transform, "덱 편성", ButtonColor);
        SetAnchored(deckButton.gameObject,
            new Vector2(0f, 0f), new Vector2(560f, 100f), new Vector2(300f, 68f));

        Button backButton = EnsureButton("BackButton", canvas.transform, "돌아가기", ButtonColor);
        SetAnchored(backButton.gameObject,
            new Vector2(0f, 0f), new Vector2(880f, 100f), new Vector2(280f, 68f));

        GameObject controller = EnsureObject("StageSelectController", null);
        StageSelectFlow flow = controller.AddComponentIfMissing<StageSelectFlow>();

        AssignReference(flow, "startBattleButton", startButton);
        AssignReference(flow, "deckBuilderButton", deckButton);
        AssignReference(flow, "backButton", backButton);
        AssignReference(flow, "stageListContent", listContent.transform);
        AssignReference(flow, "stageNameText", stageNameText);
        AssignReference(flow, "stageDescriptionText", stageDescriptionText);

        ApplyDefaultStages(flow);
    }

    private static void BuildStoryScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();

        GameObject background = EnsurePanel("BackgroundImage", canvas.transform,
            new Color(0.07f, 0.06f, 0.10f, 1f));
        SetStretch(background, Vector4.zero);
        background.transform.SetSiblingIndex(0);

        SendToBack("StoryPanel");

        GameObject character = EnsurePanel("CharacterImage", canvas.transform,
            new Color(0.20f, 0.16f, 0.28f, 0.55f));
        SetAnchored(character, new Vector2(0.5f, 0f), new Vector2(0f, 340f), new Vector2(520f, 720f));

        GameObject dialoguePanel = EnsurePanel("DialoguePanel", canvas.transform, PanelDeepColor);
        SetAnchored(dialoguePanel, new Vector2(0.5f, 0f), new Vector2(0f, 190f), new Vector2(1560f, 320f));

        TextMeshProUGUI speakerNameText = EnsureText(
            "SpeakerNameText", dialoguePanel.transform, "화자",
            34f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(speakerNameText.gameObject,
            new Vector2(0f, 1f), new Vector2(260f, -46f), new Vector2(460f, 56f));

        TextMeshProUGUI dialogueText = EnsureText(
            "DialogueText", dialoguePanel.transform, "",
            28f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(dialogueText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(1440f, 170f));

        Button nextButton = EnsureButton("NextButton", canvas.transform, "다음", ButtonColor);
        SetAnchored(nextButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-260f, 100f), new Vector2(260f, 68f));

        Button skipButton = EnsureButton("SkipButton", canvas.transform, "건너뛰기", ButtonColor);
        SetAnchored(skipButton.gameObject,
            new Vector2(1f, 1f), new Vector2(-160f, -70f), new Vector2(220f, 60f));

        GameObject controller = EnsureObject("StoryController", null);
        StoryFlow flow = controller.AddComponentIfMissing<StoryFlow>();

        AssignReference(flow, "nextButton", nextButton);
        AssignReference(flow, "skipButton", skipButton);
        AssignReference(flow, "speakerNameText", speakerNameText);
        AssignReference(flow, "dialogueText", dialogueText);

        ApplyDefaultStoryLines(flow);
    }

    private static void BuildBattleSceneExtras()
    {
        Canvas canvas = FindCanvas();

        if (canvas == null)
        {
            Debug.LogWarning("전투 캔버스를 찾지 못했습니다.");
            return;
        }

        BuildBattleLayout(canvas);

        Button returnButton = EnsureButton("ReturnButton", canvas.transform, "지역 선택", ButtonColor);
        SetAnchored(returnButton.gameObject,
            new Vector2(0f, 0f), new Vector2(560f, 62f), new Vector2(200f, 56f));

        TextMeshProUGUI progressText = EnsureText(
            "BattleProgressText", canvas.transform, "",
            20f, SubTextColor, TextAlignmentOptions.Left);

        SetAnchored(progressText.gameObject,
            new Vector2(0f, 0f), new Vector2(530f, 22f), new Vector2(840f, 32f));

        GameObject controller = EnsureObject("BattleReturnController", null);
        BattleReturnFlow flow = controller.AddComponentIfMissing<BattleReturnFlow>();

        AssignReference(flow, "returnButton", returnButton);
        AssignReference(flow, "progressText", progressText);
    }

    // 전투 화면 전체 배치 (기획서 11.7 기준)
    private static void BuildBattleLayout(Canvas canvas)
    {
        GameObject background = Locate("Background");

        if (background != null)
        {
            SetStretch(background, Vector4.zero);
            background.transform.SetSiblingIndex(0);

            Image backgroundImage = background.GetComponent<Image>();

            if (backgroundImage != null)
            {
                backgroundImage.color = BackgroundColor;
            }
        }

        // 좌상단 플레이어 정보
        PlaceByName("PlayerPanel", new Vector2(0f, 1f), new Vector2(190f, -100f), new Vector2(340f, 160f));
        StylePanelByName("PlayerPanel", PanelColor);
        LayoutByName("PlayerPanel", 4f, 16, TextAnchor.UpperCenter);
        StyleRow("PlayerTitleText", 26f, AccentColor, TextAlignmentOptions.Center, 32f);
        StyleRow("PlayerHPText", 22f, TextColor, TextAlignmentOptions.Center, 28f);
        StyleRow("PlayerShieldText", 22f, TextColor, TextAlignmentOptions.Center, 28f);
        StyleRow("ManaText", 22f, TextColor, TextAlignmentOptions.Center, 28f);

        // 상단 중앙 턴 정보
        PlaceByName("TurnPanel", new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(360f, 100f));
        StylePanelByName("TurnPanel", PanelColor);
        LayoutByName("TurnPanel", 2f, 12, TextAnchor.UpperCenter);
        StyleRow("TurnText", 28f, AccentColor, TextAlignmentOptions.Center, 34f);
        StyleRow("TurnNumberText", 22f, TextColor, TextAlignmentOptions.Center, 28f);

        // 우상단 히로인 정보
        PlaceByName("HeroinePanel", new Vector2(1f, 1f), new Vector2(-180f, -185f), new Vector2(320f, 330f));
        StylePanelByName("HeroinePanel", PanelColor);
        LayoutByName("HeroinePanel", 4f, 16, TextAnchor.UpperCenter);
        StyleRow("HeroineNameText", 28f, AccentColor, TextAlignmentOptions.Center, 34f);
        StyleRow("HeroineHPText", 22f, TextColor, TextAlignmentOptions.Center, 28f);
        StyleRow("HeroineDefenseText", 22f, TextColor, TextAlignmentOptions.Center, 28f);
        StyleRow("HeroineShieldText", 22f, TextColor, TextAlignmentOptions.Center, 28f);
        StyleRow("LustText", 22f, TextColor, TextAlignmentOptions.Center, 28f);
        SetLayoutHeight("HeroineLustSlider", 22f);
        StyleRow("HeroineStatusText", 20f, SubTextColor, TextAlignmentOptions.Center, 26f);
        SetLayoutHeight("HeroineStatusIconContainer", 44f);

        // 우측 히로인 행동 예고
        PlaceByName("HeroineIntentPanel", new Vector2(1f, 1f), new Vector2(-180f, -430f), new Vector2(320f, 150f));
        StylePanelByName("HeroineIntentPanel", PanelDeepColor);
        StretchByName("HeroineIntentText", new Vector4(20f, 16f, 20f, 16f));
        StyleTextByName("HeroineIntentText", 21f, TextColor, TextAlignmentOptions.TopLeft);

        // 중앙 마물 필드
        PlaceByName("MonsterFieldPanel", new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), new Vector2(1200f, 300f));
        StylePanelByName("MonsterFieldPanel", PanelDeepColor);
        PlaceByName("FieldGuideText", new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(1140f, 32f));
        StyleTextByName("FieldGuideText", 22f, AccentColor, TextAlignmentOptions.Left);
        StretchByName("MonsterFieldContainer", new Vector4(24f, 50f, 24f, 20f));
        HorizontalLayoutByName("MonsterFieldContainer", 10f, 0, TextAnchor.MiddleCenter);

        // 전투 안내 문구
        PlaceByName("ResultText", new Vector2(0.5f, 0f), new Vector2(0f, 330f), new Vector2(1300f, 44f));
        StyleTextByName("ResultText", 24f, AccentColor, TextAlignmentOptions.Center);

        // 손패
        GameObject handPanel = Locate("HandPanel");

        if (handPanel != null)
        {
            SetAnchored(handPanel, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1500f, 210f));
            HorizontalLayoutByName("HandPanel", 10f, 0, TextAnchor.MiddleCenter);
        }

        // 손패 레이아웃에 섞여 있던 안내 요소 분리
        GameObject handGuide = Locate("HandGuideText");

        if (handGuide != null && canvas != null)
        {
            handGuide.transform.SetParent(canvas.transform, false);
            SetAnchored(handGuide, new Vector2(0f, 0f), new Vector2(370f, 274f), new Vector2(320f, 34f));
            StyleTextByName("HandGuideText", 22f, AccentColor, TextAlignmentOptions.Left);
        }

        GameObject deckStatus = Locate("DeckStatusText");

        if (deckStatus != null && canvas != null)
        {
            deckStatus.transform.SetParent(canvas.transform, false);
            SetAnchored(deckStatus, new Vector2(1f, 0f), new Vector2(-230f, 274f), new Vector2(400f, 34f));
            StyleTextByName("DeckStatusText", 22f, TextColor, TextAlignmentOptions.Right);
        }

        SetActiveByName("CardPanel", false); // 비어 있는 손패 컨테이너 숨김

        // 우측 행동 버튼
        PlaceByName("HpAttackButton", new Vector2(1f, 0.5f), new Vector2(-140f, -30f), new Vector2(220f, 70f));
        PlaceByName("LustAttackButton", new Vector2(1f, 0.5f), new Vector2(-140f, -110f), new Vector2(220f, 70f));
        PlaceByName("EndTurnButton", new Vector2(1f, 0.5f), new Vector2(-140f, -270f), new Vector2(220f, 70f));

        Button skillButton = EnsureButton("SkillButton", canvas.transform, "스킬 사용", ButtonColor);
        SetAnchored(skillButton.gameObject,
            new Vector2(1f, 0.5f), new Vector2(-140f, -190f), new Vector2(220f, 70f));
        StyleButtonByName("SkillButton", ButtonColor, 26f);

        // 좌측 시너지 표시 (기획서 7.9)
        GameObject synergyPanel = EnsurePanel("SynergyPanel", canvas.transform, PanelDeepColor);
        SetAnchored(synergyPanel, new Vector2(0f, 0.5f), new Vector2(150f, -60f), new Vector2(260f, 300f));

        TextMeshProUGUI synergyTitle = EnsureText(
            "SynergyTitleText", synergyPanel.transform, "시너지",
            22f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(synergyTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(220f, 30f));

        TextMeshProUGUI synergyText = EnsureText(
            "SynergyText", synergyPanel.transform, "활성 시너지 없음",
            18f, TextColor, TextAlignmentOptions.TopLeft);

        SetStretch(synergyText.gameObject, new Vector4(16f, 48f, 16f, 16f));

        GameObject battleManagerObject = FindInScene("BattleManager");

        if (battleManagerObject != null)
        {
            BattleManager battleManager =
                battleManagerObject.GetComponent<BattleManager>();

            if (battleManager != null)
            {
                AssignReference(battleManager, "skillButton", skillButton);
                AssignReference(battleManager, "synergyText", synergyText);
                ApplySynergyDataList(battleManager);
            }
        }
        StyleButtonByName("HpAttackButton", ButtonColor, 26f);
        StyleButtonByName("LustAttackButton", ButtonColor, 26f);
        StyleButtonByName("EndTurnButton", AccentColor, 26f);
        SetButtonLabelColorByName("EndTurnButton", new Color(0.10f, 0.08f, 0.05f, 1f));

        // 좌하단 보조 버튼
        PlaceByName("BattleLogOpenButton", new Vector2(0f, 0f), new Vector2(110f, 212f), new Vector2(180f, 56f));
        PlaceByName("OpenCollectionButton", new Vector2(0f, 0f), new Vector2(110f, 146f), new Vector2(180f, 56f));
        StyleButtonByName("BattleLogOpenButton", ButtonColor, 22f);
        StyleButtonByName("OpenCollectionButton", ButtonColor, 22f);

        // 전투 로그 패널
        PlaceByName("BattleLogPanel", new Vector2(0f, 0.5f), new Vector2(330f, 40f), new Vector2(600f, 720f));
        StylePanelByName("BattleLogPanel", PanelDeepColor);
        PlaceByName("BattleLogTitleText", new Vector2(0.5f, 1f), new Vector2(-30f, -34f), new Vector2(500f, 40f));
        StyleTextByName("BattleLogTitleText", 26f, AccentColor, TextAlignmentOptions.Left);
        PlaceByName("BattleLogCloseButton", new Vector2(1f, 1f), new Vector2(-44f, -34f), new Vector2(56f, 44f));
        StyleButtonByName("BattleLogCloseButton", ButtonColor, 22f);
        PlaceByName("BattleLogFilterNameText", new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(560f, 32f));
        StyleTextByName("BattleLogFilterNameText", 20f, SubTextColor, TextAlignmentOptions.Left);
        PlaceByName("BattleLogFilterButtons", new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(560f, 52f));
        HorizontalLayoutByName("BattleLogFilterButtons", 8f, 0, TextAnchor.MiddleCenter);
        StyleFilterButton("AllLogButton");
        StyleFilterButton("SystemLogButton");
        StyleFilterButton("PlayerLogButton");
        StyleFilterButton("HeroineLogButton");
        StretchByName("BattleLogScrollView", new Vector4(24f, 160f, 24f, 24f));
        StyleTextByName("BattleLogText", 20f, TextColor, TextAlignmentOptions.TopLeft);

        // 전투 결과 패널
        PlaceByName("BattleResultPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 480f));
        StylePanelByName("BattleResultPanel", PanelColor);
        LayoutByName("BattleResultPanel", 18f, 44, TextAnchor.UpperCenter);
        StyleRow("OutcomeText", 38f, AccentColor, TextAlignmentOptions.Center, 52f);
        StyleRow("RewardText", 24f, TextColor, TextAlignmentOptions.Center, 90f);
        StyleRow("CaptureText", 24f, TextColor, TextAlignmentOptions.Center, 130f);
        SetLayoutHeight("ContinueButton", 68f);
        StyleButtonByName("ContinueButton", AccentColor, 26f);
        SetButtonLabelColorByName("ContinueButton", new Color(0.10f, 0.08f, 0.05f, 1f));

        // 마물 도감 패널
        PlaceByName("MonsterCollectionPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 720f));
        StylePanelByName("MonsterCollectionPanel", PanelColor);
        PlaceByName("CollectionSummaryText", new Vector2(0.5f, 1f), new Vector2(-40f, -44f), new Vector2(1040f, 44f));
        StyleTextByName("CollectionSummaryText", 24f, AccentColor, TextAlignmentOptions.Left);
        PlaceByName("CollectionCloseButton", new Vector2(1f, 1f), new Vector2(-52f, -44f), new Vector2(64f, 48f));
        StyleButtonByName("CollectionCloseButton", ButtonColor, 22f);
        SetButtonLabelByName("CollectionCloseButton", "닫기");
        SetButtonLabelByName("BattleLogCloseButton", "닫기");
        PlaceByName("MonsterListScrollView", new Vector2(0f, 0.5f), new Vector2(310f, -34f), new Vector2(560f, 560f));
        PlaceByName("MonsterDetailText", new Vector2(1f, 0.5f), new Vector2(-320f, -34f), new Vector2(560f, 560f));
        StyleTextByName("MonsterDetailText", 21f, TextColor, TextAlignmentOptions.TopLeft);

        // 상태 효과 툴팁
        PlaceByName("StatusEffectTooltip", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 230f));
        StylePanelByName("StatusEffectTooltip", PanelDeepColor);
        LayoutByName("StatusEffectTooltip", 6f, 18, TextAnchor.UpperLeft);
        StyleRow("StatusNameText", 24f, AccentColor, TextAlignmentOptions.Left, 32f);
        StyleRow("CategoryText", 20f, SubTextColor, TextAlignmentOptions.Left, 26f);
        StyleRow("EffectText", 20f, TextColor, TextAlignmentOptions.Left, 76f);
        StyleRow("RemainingTurnsText", 20f, SubTextColor, TextAlignmentOptions.Left, 26f);

        // 겹침 방지를 위한 표시 순서 정리
        SendToFront("BattleLogPanel");
        SendToFront("MonsterCollectionController");
        SendToFront("BattleResultController");
        SendToFront("StatusEffectTooltip");

        // 전투 시작 시 열려 있으면 안 되는 패널은 꺼 둔다.
        SetActiveByName("BattleLogPanel", false);
        SetActiveByName("MonsterCollectionPanel", false);
        SetActiveByName("BattleResultPanel", false);
        SetActiveByName("StatusEffectTooltip", false);
    }

    // 마물 카드 프리팹 정리 (텍스트가 카드 밖으로 넘치는 문제 해결)
    private static void BuildMonsterUnitPrefab()
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents(MonsterUnitPrefabPath);

        if (prefabRoot == null)
        {
            Debug.LogWarning(
                "마물 카드 프리팹을 찾지 못했습니다: " + MonsterUnitPrefabPath);
            return;
        }

        RectTransform rootRect = prefabRoot.GetComponent<RectTransform>();

        if (rootRect != null)
        {
            rootRect.sizeDelta = new Vector2(130f, 220f);
        }

        Image cardImage = prefabRoot.GetComponent<Image>();

        if (cardImage != null)
        {
            cardImage.color = new Color(0.18f, 0.15f, 0.27f, 1f);
        }

        VerticalLayoutGroup layout =
            prefabRoot.AddComponentIfMissing<VerticalLayoutGroup>();

        layout.spacing = 2f;
        layout.padding = new RectOffset(6, 6, 8, 8);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        EnsureMonsterSkillRow(prefabRoot); // 스킬 표시 행 준비

        string[] rowNames =
        {
            "MonsterNameText",
            "MonsterHPText",
            "MonsterAttackText",
            "MonsterLustDamageText",
            "MonsterDefenseText",
            "MonsterShieldText",
            "MonsterSkillText",
            "MonsterStateText",
            "MonsterStatusIconContainer",
        };

        float[] rowSizes = { 17f, 15f, 15f, 15f, 15f, 15f, 14f, 14f, 0f };
        float[] rowHeights = { 24f, 20f, 20f, 20f, 20f, 20f, 20f, 20f, 24f };

        for (int i = 0; i < rowNames.Length; i++)
        {
            Transform row = FindRecursive(prefabRoot.transform, rowNames[i]);

            if (row == null)
            {
                Debug.LogWarning("마물 카드 항목을 찾지 못했습니다: " + rowNames[i]);
                continue;
            }

            row.SetSiblingIndex(i);

            RectTransform rowRect = row.GetComponent<RectTransform>();

            if (rowRect != null)
            {
                rowRect.sizeDelta = new Vector2(0f, rowHeights[i]);
            }

            TextMeshProUGUI rowText = row.GetComponent<TextMeshProUGUI>();

            if (rowText != null)
            {
                rowText.fontSize = rowSizes[i];
                rowText.alignment = TextAlignmentOptions.Center;
                rowText.raycastTarget = false;
                rowText.color = i == 0 ? AccentColor : TextColor;
                rowText.overflowMode = TextOverflowModes.Truncate;
            }

            LayoutElement rowLayout =
                row.gameObject.AddComponentIfMissing<LayoutElement>();

            rowLayout.minHeight = rowHeights[i];
            rowLayout.preferredHeight = rowHeights[i];
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, MonsterUnitPrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("마물 카드 프리팹을 정리했습니다.");
    }

    // 마물 카드에 스킬 표시 행을 만들고 컴포넌트에 연결한다.
    private static void EnsureMonsterSkillRow(GameObject prefabRoot)
    {
        Transform skillRow =
            FindRecursive(prefabRoot.transform, "MonsterSkillText");

        if (skillRow == null)
        {
            GameObject skillObject =
                new GameObject("MonsterSkillText", typeof(RectTransform));

            skillObject.transform.SetParent(prefabRoot.transform, false);
            skillObject.AddComponent<TextMeshProUGUI>();
            skillRow = skillObject.transform;
        }

        TextMeshProUGUI skillText =
            skillRow.GetComponent<TextMeshProUGUI>();

        if (skillText != null)
        {
            skillText.text = string.Empty;
        }

        MonsterUnit monsterUnit = prefabRoot.GetComponent<MonsterUnit>();

        if (monsterUnit != null && skillText != null)
        {
            AssignReference(monsterUnit, "monsterSkillText", skillText);
        }
    }

    // 손패 카드 프리팹 정리 (카드가 서로 겹치지 않도록 크기 조정)
    private static void BuildCardButtonPrefab()
    {
        GameObject prefabRoot =
            PrefabUtility.LoadPrefabContents(CardButtonPrefabPath);

        if (prefabRoot == null)
        {
            Debug.LogWarning(
                "손패 카드 프리팹을 찾지 못했습니다: " + CardButtonPrefabPath);
            return;
        }

        RectTransform rootRect = prefabRoot.GetComponent<RectTransform>();

        if (rootRect != null)
        {
            rootRect.sizeDelta = new Vector2(140f, 200f);
        }

        LayoutElement rootLayout =
            prefabRoot.AddComponentIfMissing<LayoutElement>();

        rootLayout.minWidth = 140f;
        rootLayout.preferredWidth = 140f;
        rootLayout.minHeight = 200f;
        rootLayout.preferredHeight = 200f;

        Image cardImage = prefabRoot.GetComponent<Image>();

        if (cardImage != null)
        {
            cardImage.color = new Color(0.22f, 0.18f, 0.34f, 1f);
        }

        Transform cardText = FindRecursive(prefabRoot.transform, "CardText");

        if (cardText != null)
        {
            RectTransform textRect = cardText.GetComponent<RectTransform>();

            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(8f, 8f);
                textRect.offsetMax = new Vector2(-8f, -8f);
            }

            TextMeshProUGUI label = cardText.GetComponent<TextMeshProUGUI>();

            if (label != null)
            {
                label.fontSize = 16f;
                label.color = TextColor;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                label.overflowMode = TextOverflowModes.Truncate;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, CardButtonPrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("손패 카드 프리팹을 정리했습니다.");
    }

    // 프리셋 버튼 5개를 덱 편성 컨트롤러에 연결한다.
    private static void ApplyPresetButtonList(DeckBuilderFlow flow)
    {
        SerializedObject serializedFlow = new SerializedObject(flow);

        SerializedProperty listProperty =
            serializedFlow.FindProperty("presetButtons");

        if (listProperty == null)
        {
            Debug.LogWarning("presetButtons 항목을 찾지 못했습니다.");
            return;
        }

        listProperty.arraySize = 5;

        for (int i = 0; i < 5; i++)
        {
            GameObject presetObject = FindInScene($"PresetButton{i + 1}");

            listProperty.GetArrayElementAtIndex(i).objectReferenceValue =
                presetObject == null ? null : presetObject.GetComponent<Button>();
        }

        serializedFlow.ApplyModifiedPropertiesWithoutUndo();
    }

    // 한 줄 입력 필드를 만든다.
    private static TMP_InputField EnsureInputField(
        string objectName,
        Transform parent,
        string placeholderText)
    {
        GameObject fieldObject = EnsureObject(objectName, parent);

        Image fieldImage = fieldObject.AddComponentIfMissing<Image>();
        fieldImage.color = new Color(0.10f, 0.09f, 0.16f, 1f);

        GameObject textArea = EnsureObject(objectName + "TextArea", fieldObject.transform);
        SetStretch(textArea, new Vector4(12f, 8f, 12f, 8f));
        textArea.AddComponentIfMissing<RectMask2D>();

        TextMeshProUGUI placeholder = EnsureText(
            objectName + "Placeholder", textArea.transform, placeholderText,
            19f, SubTextColor, TextAlignmentOptions.MidlineLeft);

        SetStretch(placeholder.gameObject, Vector4.zero);

        TextMeshProUGUI inputText = EnsureText(
            objectName + "Text", textArea.transform, string.Empty,
            19f, TextColor, TextAlignmentOptions.MidlineLeft);

        SetStretch(inputText.gameObject, Vector4.zero);

        TMP_InputField inputField =
            fieldObject.AddComponentIfMissing<TMP_InputField>();

        inputField.textViewport = textArea.GetComponent<RectTransform>();
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;
        inputField.lineType = TMP_InputField.LineType.SingleLine;
        inputField.targetGraphic = fieldImage;
        inputField.caretColor = TextColor;
        inputField.customCaretColor = true;

        return inputField;
    }

    // 세로 스크롤 목록을 만들고 항목이 들어갈 Content 오브젝트를 반환한다.
    // 세로로 쌓이는 목록을 만든다. (사본 선택처럼 행 단위 목록에 쓴다)
    private static GameObject EnsureVerticalScrollList(
        string viewName,
        string contentName,
        Transform parent,
        Vector4 offsets)
    {
        GameObject viewObject = EnsureObject(viewName, parent);
        SetStretch(viewObject, offsets);

        Image viewImage = viewObject.AddComponentIfMissing<Image>();
        viewImage.color = new Color(0.06f, 0.05f, 0.09f, 0.6f);
        viewImage.raycastTarget = true;

        viewObject.AddComponentIfMissing<RectMask2D>();

        ScrollRect scrollRect = viewObject.AddComponentIfMissing<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 24f;

        GameObject contentObject = EnsureObject(contentName, viewObject.transform);

        RectTransform contentRect =
            contentObject.GetComponent<RectTransform>();

        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = new Vector2(0f, contentRect.offsetMin.y);
        contentRect.offsetMax = new Vector2(0f, contentRect.offsetMax.y);
        contentRect.sizeDelta = new Vector2(0f, 0f);
        contentRect.anchoredPosition = Vector2.zero;

        GridLayoutGroup gridLayout =
            contentObject.GetComponent<GridLayoutGroup>();

        if (gridLayout != null)
        {
            Object.DestroyImmediate(gridLayout); // 격자 배치 제거
        }

        VerticalLayoutGroup listLayout =
            contentObject.AddComponentIfMissing<VerticalLayoutGroup>();

        listLayout.spacing = 8f;
        listLayout.padding = new RectOffset(10, 10, 10, 10);
        listLayout.childAlignment = TextAnchor.UpperLeft;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;

        ContentSizeFitter contentFitter =
            contentObject.AddComponentIfMissing<ContentSizeFitter>();

        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;
        scrollRect.viewport = viewObject.GetComponent<RectTransform>();

        return contentObject;
    }

    private static GameObject EnsureScrollList(
        string viewName,
        string contentName,
        Transform parent,
        Vector4 offsets)
    {
        GameObject viewObject = EnsureObject(viewName, parent);
        SetStretch(viewObject, offsets);

        Image viewImage = viewObject.AddComponentIfMissing<Image>();
        viewImage.color = new Color(0.06f, 0.05f, 0.09f, 0.6f);
        viewImage.raycastTarget = true;

        viewObject.AddComponentIfMissing<RectMask2D>();

        ScrollRect scrollRect = viewObject.AddComponentIfMissing<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 24f;

        GameObject contentObject = EnsureObject(contentName, viewObject.transform);

        RectTransform contentRect =
            contentObject.GetComponent<RectTransform>();

        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = new Vector2(0f, contentRect.offsetMin.y);
        contentRect.offsetMax = new Vector2(0f, contentRect.offsetMax.y);
        contentRect.sizeDelta = new Vector2(0f, 0f);
        contentRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup listLayout =
            contentObject.GetComponent<VerticalLayoutGroup>();

        if (listLayout != null)
        {
            Object.DestroyImmediate(listLayout); // 세로 목록 배치 제거
        }

        GridLayoutGroup contentLayout =
            contentObject.AddComponentIfMissing<GridLayoutGroup>();

        contentLayout.cellSize = new Vector2(190f, 230f);
        contentLayout.spacing = new Vector2(10f, 10f);
        contentLayout.padding = new RectOffset(8, 8, 8, 8);
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        contentLayout.constraintCount = 4; // 가로 4장 배치

        ContentSizeFitter contentFitter =
            contentObject.AddComponentIfMissing<ContentSizeFitter>();

        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;
        scrollRect.viewport = viewObject.GetComponent<RectTransform>();

        return contentObject;
    }

    // ---------- 기존 오브젝트 배치 도우미 ----------

    private static GameObject Locate(string objectName)
    {
        GameObject target = FindInScene(objectName);

        if (target == null)
        {
            Debug.LogWarning("배치할 오브젝트를 찾지 못했습니다: " + objectName);
        }

        return target;
    }

    private static void PlaceByName(
        string objectName,
        Vector2 anchor,
        Vector2 position,
        Vector2 size)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        SetAnchored(target, anchor, position, size);
    }

    private static void StretchByName(string objectName, Vector4 offsets)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        SetStretch(target, offsets);
    }

    private static void StyleTextByName(
        string objectName,
        float fontSize,
        Color textColor,
        TextAlignmentOptions alignment)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();

        if (text == null) { return; }

        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private static void StyleRow(
        string objectName,
        float fontSize,
        Color textColor,
        TextAlignmentOptions alignment,
        float rowHeight)
    {
        StyleTextByName(objectName, fontSize, textColor, alignment);
        SetLayoutHeight(objectName, rowHeight);
    }

    private static void StylePanelByName(string objectName, Color panelColor)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        Image panelImage = target.AddComponentIfMissing<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = false;
    }

    private static void StyleButtonByName(
        string objectName,
        Color buttonColor,
        float labelSize)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        Image buttonImage = target.AddComponentIfMissing<Image>();
        buttonImage.color = buttonColor;
        buttonImage.raycastTarget = true;

        TextMeshProUGUI label =
            target.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
        {
            label.fontSize = labelSize;
            label.color = TextColor;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }
    }

    private static void SetButtonLabelByName(string objectName, string label)
    {
        GameObject target = FindInScene(objectName);

        if (target == null) { return; }

        TextMeshProUGUI buttonLabel =
            target.GetComponentInChildren<TextMeshProUGUI>(true);

        if (buttonLabel != null)
        {
            buttonLabel.text = label;
        }
    }

    private static void SetButtonLabelColorByName(string objectName, Color labelColor)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        TextMeshProUGUI label =
            target.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
        {
            label.color = labelColor;
        }
    }

    private static void StyleFilterButton(string objectName)
    {
        StyleButtonByName(objectName, ButtonColor, 18f);
        SetLayoutHeight(objectName, 44f);
    }

    private static void LayoutByName(
        string objectName,
        float spacing,
        int padding,
        TextAnchor alignment)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        ApplyVerticalLayout(target, spacing, padding, alignment);
    }

    private static void HorizontalLayoutByName(
        string objectName,
        float spacing,
        int padding,
        TextAnchor alignment)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        HorizontalLayoutGroup layout =
            target.AddComponentIfMissing<HorizontalLayoutGroup>();

        layout.spacing = spacing;
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.childAlignment = alignment;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private static void SetLayoutHeight(string objectName, float height)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        LayoutElement layout = target.AddComponentIfMissing<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
    }

    // 이전 구성에서 만든 오브젝트를 씬에서 지운다.
    private static void DestroyByName(string objectName)
    {
        GameObject target = FindInScene(objectName);

        if (target == null) { return; }

        Object.DestroyImmediate(target);
    }

    private static void SetActiveByName(string objectName, bool isActive)
    {
        GameObject target = FindInScene(objectName);

        if (target == null) { return; }

        target.SetActive(isActive);
    }

    private static void SendToFront(string objectName)
    {
        GameObject target = FindInScene(objectName);

        if (target == null) { return; }

        target.transform.SetAsLastSibling();
    }

    // 프로젝트의 시너지 데이터를 전투 관리자에 연결한다.
    private static void ApplySynergyDataList(BattleManager battleManager)
    {
        SerializedObject serializedManager = new SerializedObject(battleManager);

        SerializedProperty listProperty =
            serializedManager.FindProperty("synergyDataList");

        if (listProperty == null)
        {
            Debug.LogWarning("synergyDataList 항목을 찾지 못했습니다.");
            return;
        }

        string[] assetGuids = AssetDatabase.FindAssets("t:SynergyData");
        List<SynergyData> synergyAssets = new List<SynergyData>();

        foreach (string assetGuid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

            SynergyData synergyData =
                AssetDatabase.LoadAssetAtPath<SynergyData>(assetPath);

            if (synergyData == null) { continue; }

            synergyAssets.Add(synergyData);
        }

        synergyAssets.Sort(
            (left, right) => left.MonsterType.CompareTo(right.MonsterType)
        ); // 타입 순서로 정렬

        listProperty.arraySize = synergyAssets.Count;

        for (int i = 0; i < synergyAssets.Count; i++)
        {
            listProperty.GetArrayElementAtIndex(i).objectReferenceValue =
                synergyAssets[i];
        }

        serializedManager.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"시너지 데이터 {synergyAssets.Count}종을 연결했습니다.");
    }

    // ---------- 데이터 기본값 ----------

    private static void ApplyStartingCards(PlayerProgressManager progress)
    {
        SerializedObject serializedProgress = new SerializedObject(progress);

        SerializedProperty deckSizeProperty =
            serializedProgress.FindProperty("requiredDeckSize");

        if (deckSizeProperty != null)
        {
            deckSizeProperty.intValue = 30;
        }

        SerializedProperty cardsProperty =
            serializedProgress.FindProperty("startingCards");

        if (cardsProperty == null)
        {
            Debug.LogWarning("startingCards 항목을 찾지 못했습니다.");
            return;
        }

        if (cardsProperty.arraySize > 0)
        {
            serializedProgress.ApplyModifiedPropertiesWithoutUndo();
            return; // 이미 설정된 목록 유지
        }

        List<CardData> startingCards = new List<CardData>();
        List<int> startingCounts = new List<int>();

        for (int i = 0; i < StartingCardIds.Length; i++)
        {
            CardData cardData = FindCardById(StartingCardIds[i]);

            if (cardData == null)
            {
                Debug.LogWarning("시작 카드를 찾지 못했습니다: " + StartingCardIds[i]);
                continue;
            }

            startingCards.Add(cardData);
            startingCounts.Add(StartingCardCounts[i]);
        }

        cardsProperty.arraySize = startingCards.Count;

        for (int i = 0; i < startingCards.Count; i++)
        {
            SerializedProperty entry = cardsProperty.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("cardData").objectReferenceValue = startingCards[i];
            entry.FindPropertyRelative("count").intValue = startingCounts[i];
        }

        serializedProgress.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ApplyDefaultStages(StageSelectFlow flow)
    {
        SerializedObject serializedFlow = new SerializedObject(flow);
        SerializedProperty stages = serializedFlow.FindProperty("stages");

        if (stages == null || stages.arraySize > 0) { return; }

        string[] names = { "지역 1 - 일반전", "지역 1 - 포획전", "지역 1 - 히로인전" };
        string[] types = { "일반전", "포획전", "히로인전" };
        int[] levels = { 1, 2, 3 };
        bool[] unlocked = { true, true, false };
        string[] descriptions =
        {
            "서브 히로인과의 기본 전투다. 현재 플레이어 덱을 사용한다.",
            "포획전이다. 대상 마물을 쓰러뜨리면 해당 카드를 보유 목록에 추가한다.",
            "메인 히로인전이다. 지역 해금 조건을 충족하면 열린다.",
        };

        stages.arraySize = names.Length;

        for (int i = 0; i < names.Length; i++)
        {
            SerializedProperty entry = stages.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("stageName").stringValue = names[i];
            entry.FindPropertyRelative("stageType").stringValue = types[i];
            entry.FindPropertyRelative("recommendedLevel").intValue = levels[i];
            entry.FindPropertyRelative("description").stringValue = descriptions[i];
            entry.FindPropertyRelative("isUnlocked").boolValue = unlocked[i];
        }

        serializedFlow.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ApplyDefaultStoryLines(StoryFlow flow)
    {
        SerializedObject serializedFlow = new SerializedObject(flow);
        SerializedProperty lines = serializedFlow.FindProperty("storyLines");

        if (lines == null || lines.arraySize > 0) { return; }

        string[] speakers = { "그리모어", "도윤", "그리모어" };
        string[] dialogues =
        {
            "너는 이 세계의 소환사로 불려왔다.",
            "내가 포획한 마물이 그대로 내 전투 카드가 되는 거군.",
            "지역을 골라 시작해라. 덱 구성이 모든 것을 결정한다.",
        };

        lines.arraySize = speakers.Length;

        for (int i = 0; i < speakers.Length; i++)
        {
            SerializedProperty entry = lines.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("speakerName").stringValue = speakers[i];
            entry.FindPropertyRelative("dialogue").stringValue = dialogues[i];
        }

        serializedFlow.ApplyModifiedPropertiesWithoutUndo();
    }

    private static CardData FindCardById(string cardId)
    {
        string[] assetGuids = AssetDatabase.FindAssets("t:CardData");

        foreach (string assetGuid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

            CardData cardData =
                AssetDatabase.LoadAssetAtPath<CardData>(assetPath);

            if (cardData == null) { continue; }
            if (cardData.CardId != cardId) { continue; }

            return cardData;
        }

        return null;
    }

    // ---------- UI 생성 도우미 ----------

    private static Canvas FindCanvas()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Canvas canvas = root.GetComponentInChildren<Canvas>(true);

            if (canvas != null) { return canvas; }
        }

        return null;
    }

    // UI 전용 씬이라도 카메라가 없으면 화면이 출력되지 않는다.
    private static void EnsureMainCamera()
    {
        Camera sceneCamera = Object.FindFirstObjectByType<Camera>();

        if (sceneCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");

            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            sceneCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        sceneCamera.clearFlags = CameraClearFlags.SolidColor;
        sceneCamera.backgroundColor = BackgroundColor;
    }

    private static Canvas EnsureCanvas()
    {
        Canvas canvas = FindCanvas();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvas.gameObject.AddComponentIfMissing<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f; // 16:9 안전 영역 유지

        canvas.gameObject.AddComponentIfMissing<GraphicRaycaster>();

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = null;

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            eventSystem = root.GetComponentInChildren<EventSystem>(true);

            if (eventSystem != null) { break; }
        }

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        ApplyInputModule(eventSystem.gameObject);
    }

    // 프로젝트의 입력 처리 설정에 맞는 입력 모듈을 적용한다.
    private static void ApplyInputModule(GameObject target)
    {
#if ENABLE_INPUT_SYSTEM
        StandaloneInputModule legacyModule =
            target.GetComponent<StandaloneInputModule>();

        if (legacyModule != null)
        {
            Object.DestroyImmediate(legacyModule); // 레거시 입력 모듈 제거
        }

        if (target.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
        {
            target.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
#else
        if (target.GetComponent<StandaloneInputModule>() == null)
        {
            target.AddComponent<StandaloneInputModule>();
        }
#endif
    }

    private static void EnsureBackground(Transform canvasTransform)
    {
        GameObject background = EnsurePanel("Background", canvasTransform, BackgroundColor);
        SetStretch(background, Vector4.zero);
        background.transform.SetSiblingIndex(0);
    }

    private static void SendToBack(string objectName)
    {
        GameObject target = FindInScene(objectName);

        if (target == null) { return; }

        target.transform.SetSiblingIndex(1); // 배경 바로 위로 이동
    }

    private static GameObject FindInScene(string objectName)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == objectName) { return root; }

            Transform found = FindRecursive(root.transform, objectName);

            if (found != null) { return found.gameObject; }
        }

        return null;
    }

    private static Transform FindRecursive(Transform parent, string objectName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == objectName) { return child; }

            Transform found = FindRecursive(child, objectName);

            if (found != null) { return found; }
        }

        return null;
    }

    private static GameObject EnsureObject(string objectName, Transform parent)
    {
        GameObject target = FindInScene(objectName);

        if (target == null)
        {
            target = new GameObject(objectName, typeof(RectTransform));
        }

        if (parent != null && target.transform.parent != parent)
        {
            target.transform.SetParent(parent, false);
        }

        if (parent != null && target.GetComponent<RectTransform>() == null)
        {
            target.AddComponent<RectTransform>();
        }

        return target;
    }

    private static GameObject EnsurePanel(string objectName, Transform parent, Color panelColor)
    {
        GameObject panel = EnsureObject(objectName, parent);

        Image panelImage = panel.AddComponentIfMissing<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = false;

        return panel;
    }

    private static TextMeshProUGUI EnsureText(
        string objectName,
        Transform parent,
        string content,
        float fontSize,
        Color textColor,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = EnsureObject(objectName, parent);

        TextMeshProUGUI text =
            textObject.AddComponentIfMissing<TextMeshProUGUI>();

        if (string.IsNullOrEmpty(text.text) || !string.IsNullOrEmpty(content))
        {
            text.text = content;
        }

        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = alignment;
        text.raycastTarget = false;

        return text;
    }

    private static Button EnsureButton(
        string objectName,
        Transform parent,
        string label,
        Color buttonColor)
    {
        GameObject buttonObject = EnsureObject(objectName, parent);

        Image buttonImage = buttonObject.AddComponentIfMissing<Image>();
        buttonImage.color = buttonColor;
        buttonImage.raycastTarget = true;

        Button button = buttonObject.AddComponentIfMissing<Button>();
        button.targetGraphic = buttonImage;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.96f, 0.82f, 1f);
        colors.pressedColor = new Color(0.80f, 0.76f, 0.66f, 1f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.48f, 1f);
        button.colors = colors;

        LayoutElement layout = buttonObject.AddComponentIfMissing<LayoutElement>();
        layout.minHeight = 68f;
        layout.preferredHeight = 68f;

        TextMeshProUGUI buttonLabel = EnsureButtonLabel(buttonObject, label);
        buttonLabel.fontSize = 30f;
        buttonLabel.alignment = TextAlignmentOptions.Center;

        return button;
    }

    private static TextMeshProUGUI EnsureButtonLabel(GameObject buttonObject, string label)
    {
        TextMeshProUGUI existingLabel =
            buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);

        if (existingLabel == null)
        {
            GameObject labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(buttonObject.transform, false);
            existingLabel = labelObject.AddComponent<TextMeshProUGUI>();
        }

        existingLabel.text = label;
        existingLabel.color = TextColor;
        existingLabel.raycastTarget = false;

        RectTransform labelRect =
            existingLabel.GetComponent<RectTransform>();

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return existingLabel;
    }

    private static void SetButtonLabelColor(Button button, Color labelColor)
    {
        TextMeshProUGUI label =
            button.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
        {
            label.color = labelColor;
        }
    }

    private static void ApplyVerticalLayout(
        GameObject target,
        float spacing,
        int padding,
        TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        VerticalLayoutGroup layout =
            target.AddComponentIfMissing<VerticalLayoutGroup>();

        layout.spacing = spacing;
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.childAlignment = alignment;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private static RectTransform SetAnchored(
        GameObject target,
        Vector2 anchor,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rect = target.GetComponent<RectTransform>();

        if (rect == null) { rect = target.AddComponent<RectTransform>(); }

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return rect;
    }

    private static RectTransform SetStretch(GameObject target, Vector4 offsets)
    {
        RectTransform rect = target.GetComponent<RectTransform>();

        if (rect == null) { rect = target.AddComponent<RectTransform>(); }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(offsets.x, offsets.w);
        rect.offsetMax = new Vector2(-offsets.z, -offsets.y);

        return rect;
    }

    private static void AssignReference(
        Component target,
        string fieldName,
        Object value)
    {
        SerializedObject serializedTarget = new SerializedObject(target);
        SerializedProperty property = serializedTarget.FindProperty(fieldName);

        if (property == null)
        {
            Debug.LogWarning(
                $"{target.GetType().Name}에 {fieldName} 항목이 없습니다.");
            return;
        }

        property.objectReferenceValue = value;
        serializedTarget.ApplyModifiedPropertiesWithoutUndo();
    }
}

public static class SceneUIBuilderExtensions
{
    public static T AddComponentIfMissing<T>(this GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();

        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
    }
}
