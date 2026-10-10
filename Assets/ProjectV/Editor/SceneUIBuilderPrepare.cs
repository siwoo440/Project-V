using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 씬 UI 구성 도구의 전투 준비 화면과 전투 편의 버튼 (기획서 6.14 / 10.18 / 10.19 / 11.11)
public static partial class SceneUIBuilder
{
    // 씬 파일이 없으면 빈 씬으로 만들고 빌드 목록에 넣는다. 새 화면을 더할 때 씬 파일을 손으로 만들지 않아도 된다.
    // beforeSceneName: 빌드 목록에서 이 씬 앞에 끼워 넣는다.
    private static void EnsureSceneFile(string sceneName, string beforeSceneName)
    {
        string scenePath = SceneFolder + sceneName + ".unity";

        if (!System.IO.File.Exists(scenePath))
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.Refresh();

            Debug.Log("새 씬을 만들었습니다: " + scenePath);
        }

        List<EditorBuildSettingsScene> buildScenes =
            new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        if (buildScenes.Exists(buildScene => buildScene.path == scenePath)) { return; }

        string beforePath = SceneFolder + beforeSceneName + ".unity";
        int insertIndex = buildScenes.FindIndex(buildScene => buildScene.path == beforePath);

        EditorBuildSettingsScene entry = new EditorBuildSettingsScene(scenePath, true);

        if (insertIndex < 0) { buildScenes.Add(entry); }
        else { buildScenes.Insert(insertIndex, entry); }

        EditorBuildSettings.scenes = buildScenes.ToArray();

        Debug.Log("빌드 목록에 씬을 넣었습니다: " + scenePath);
    }

    // 전투 준비 화면: 왼쪽은 상대, 오른쪽은 내 준비 상태, 아래는 난이도와 보상, 전투 시작.
    private static void BuildBattlePrepareScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();
        EnsureBackground(canvas.transform, UIKeys.BgDeckBuilder);

        EnsureTopBar(canvas.transform, 72f, 104f);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "전투 준비",
            48f, AccentColor, TextAlignmentOptions.Left);

        PlaceSceneTitle(title, new Vector2(300f, -72f), new Vector2(480f, 60f), 48f);

        TextMeshProUGUI stageText = EnsureText(
            "StageText", canvas.transform, "",
            30f, TextColor, TextAlignmentOptions.Left);

        SetAnchored(stageText.gameObject,
            new Vector2(0f, 1f), new Vector2(1220f, -72f), new Vector2(1200f, 50f));

        // 왼쪽: 상대의 이름, 능력치, 행동. 히로인은 그림도 보여 준다.
        GameObject opponentPanel = EnsurePanel("OpponentPanel", canvas.transform, PanelDeepColor);
        SetAnchored(opponentPanel, new Vector2(0f, 0.5f), new Vector2(490f, 60f), new Vector2(900f, 680f));

        TextMeshProUGUI opponentTitle = EnsureText(
            "OpponentTitleText", opponentPanel.transform, "",
            34f, AccentColor, TextAlignmentOptions.TopLeft);

        SetAnchored(opponentTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(-125f, -62f), new Vector2(590f, 56f));

        TextMeshProUGUI opponentText = EnsureText(
            "OpponentText", opponentPanel.transform, "",
            21f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(opponentText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(-125f, -375f), new Vector2(590f, 560f));

        opponentText.enableAutoSizing = true; // 행동이 많은 히로인은 글자를 줄여 칸에 맞춘다.
        opponentText.fontSizeMin = 15f;
        opponentText.fontSizeMax = 21f;

        GameObject portraitFrame = EnsurePanel("PreparePortraitFrame", opponentPanel.transform, PanelColor);

        SetAnchored(portraitFrame,
            new Vector2(1f, 1f), new Vector2(-140f, -200f), new Vector2(230f, 330f));

        portraitFrame.AddComponentIfMissing<RectMask2D>();

        GameObject portraitObject = EnsureObject("PreparePortraitImage", portraitFrame.transform);
        Image portraitImage = portraitObject.AddComponentIfMissing<Image>();

        portraitImage.sprite = null; // 실행 중에 상대 히로인의 그림을 넣는다.
        portraitImage.color = Color.white;
        portraitImage.preserveAspect = true;
        portraitImage.raycastTarget = false;
        portraitImage.enabled = false;

        RectTransform portraitRect = portraitObject.GetComponent<RectTransform>();

        portraitRect.anchorMin = new Vector2(0.5f, 1f);
        portraitRect.anchorMax = new Vector2(0.5f, 1f);
        portraitRect.pivot = new Vector2(0.5f, 1f);
        portraitRect.anchoredPosition = new Vector2(-9f, -4f);
        portraitRect.sizeDelta = new Vector2(310f, 465f); // 2:3 그림. 칸보다 길어 허리 아래는 가려진다.

        portraitFrame.SetActive(false);

        GameObject controller = EnsureObject("BattlePrepareController", null);
        BattlePrepareFlow flow = controller.AddComponentIfMissing<BattlePrepareFlow>();

        AssignReference(flow, "stageText", stageText);
        AssignReference(flow, "opponentTitleText", opponentTitle);
        AssignReference(flow, "opponentText", opponentText);
        AssignReference(flow, "portraitFrame", portraitFrame);
        AssignReference(flow, "portraitImage", portraitImage);

        BuildPrepareLoadout(canvas.transform, flow);
        BuildPrepareBottom(canvas.transform, flow);
    }

    // 오른쪽: 내 준비 상태. 덱 프리셋, 액티브 스킬, 패시브, 소모품을 이 자리에서 바꾼다.
    private static void BuildPrepareLoadout(Transform canvasTransform, BattlePrepareFlow flow)
    {
        GameObject loadoutPanel = EnsurePanel("LoadoutPanel", canvasTransform, PanelDeepColor);
        SetAnchored(loadoutPanel, new Vector2(1f, 0.5f), new Vector2(-490f, 60f), new Vector2(900f, 680f));

        Vector2 top = new Vector2(0.5f, 1f);

        TextMeshProUGUI playerTitle = EnsureText(
            "PlayerTitleText", loadoutPanel.transform, "내 준비",
            32f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(playerTitle.gameObject, top, new Vector2(0f, -58f), new Vector2(820f, 52f));

        TextMeshProUGUI presetText = EnsureText(
            "PresetText", loadoutPanel.transform, "",
            26f, TextColor, TextAlignmentOptions.Center);

        SetAnchored(presetText.gameObject, top, new Vector2(0f, -132f), new Vector2(620f, 48f));

        Button presetPrev = EnsureButton("PresetPrevButton", loadoutPanel.transform, "<", ButtonColor);
        SetAnchored(presetPrev.gameObject, top, new Vector2(-370f, -132f), new Vector2(64f, 56f));
        ApplyIconButton("PresetPrevButton", UIKeys.ArrowLeft, top, new Vector2(-370f, -132f), 60f);

        Button presetNext = EnsureButton("PresetNextButton", loadoutPanel.transform, ">", ButtonColor);
        SetAnchored(presetNext.gameObject, top, new Vector2(370f, -132f), new Vector2(64f, 56f));
        ApplyIconButton("PresetNextButton", UIKeys.ArrowRight, top, new Vector2(370f, -132f), 60f);

        TextMeshProUGUI deckText = EnsureText(
            "DeckText", loadoutPanel.transform, "",
            21f, TextColor, TextAlignmentOptions.Center);

        SetAnchored(deckText.gameObject, top, new Vector2(0f, -204f), new Vector2(820f, 72f));

        Button deckBuilderButton = EnsurePrepareButton(
            "DeckBuilderButton", loadoutPanel.transform, "덱 편성", -282f, 360f, 22f, UIKeys.IconDeck);

        Button skillButton = EnsurePrepareButton(
            "SkillButton", loadoutPanel.transform, "액티브 스킬", -366f, 820f, 20f, UIKeys.IconSummoner);

        Button passiveButton = EnsurePrepareButton(
            "PassiveButton", loadoutPanel.transform, "패시브", -440f, 820f, 20f, string.Empty);

        Button itemSlotButton = EnsurePrepareButton(
            "ItemSlotButton", loadoutPanel.transform, "소모품: 없음", -514f, 820f, 20f, UIKeys.ItemEmpty);

        Button summonerButton = EnsurePrepareButton(
            "SummonerButton", loadoutPanel.transform, "소환사 화면 (스킬과 패시브 관리)", -600f, 520f, 21f, string.Empty);

        AssignReference(flow, "playerTitleText", playerTitle);
        AssignReference(flow, "presetText", presetText);
        AssignReference(flow, "deckText", deckText);
        AssignReference(flow, "presetPrevButton", presetPrev);
        AssignReference(flow, "presetNextButton", presetNext);
        AssignReference(flow, "deckBuilderButton", deckBuilderButton);
        AssignReference(flow, "skillButton", skillButton);
        AssignReference(flow, "passiveButton", passiveButton);
        AssignReference(flow, "itemSlotButton", itemSlotButton);
        AssignReference(flow, "summonerButton", summonerButton);
    }

    // 준비 화면 오른쪽 칸의 가로로 긴 버튼 하나. 위에서부터의 거리로 놓는다.
    private static Button EnsurePrepareButton(
        string objectName,
        Transform parent,
        string label,
        float topY,
        float width,
        float labelSize,
        string iconKey)
    {
        Button button = EnsureButton(objectName, parent, label, ButtonColor);

        SetAnchored(button.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, topY), new Vector2(width, 60f));

        StyleButtonByName(objectName, ButtonColor, labelSize);

        if (!string.IsNullOrEmpty(iconKey)) { EnsureButtonIcon(button, iconKey); }

        return button;
    }

    // 아래: 난이도와 보상, 안내 줄, 지역으로 돌아가기와 전투 시작.
    private static void BuildPrepareBottom(Transform canvasTransform, BattlePrepareFlow flow)
    {
        GameObject rewardPanel = EnsurePanel("RewardPanel", canvasTransform, PanelDeepColor);
        SetAnchored(rewardPanel, new Vector2(0f, 0f), new Vector2(490f, 128f), new Vector2(900f, 204f));

        string[] objectNames = { "DifficultyEasyButton", "DifficultyNormalButton", "DifficultyHardButton" };
        string[] labels = { "쉬움", "보통", "어려움" };
        string[] fieldNames = { "easyButton", "normalButton", "hardButton" };
        string[] iconKeys = { UIKeys.DifficultyEasy, UIKeys.DifficultyNormal, UIKeys.DifficultyHard };

        for (int i = 0; i < objectNames.Length; i++)
        {
            Button difficultyButton = EnsureButton(objectNames[i], rewardPanel.transform, labels[i], ButtonColor);

            SetAnchored(difficultyButton.gameObject,
                new Vector2(0.5f, 1f), new Vector2((i - 1) * 270f - 40f, -42f), new Vector2(250f, 50f));

            StyleButtonByName(objectNames[i], ButtonColor, 21f);
            EnsureButtonIcon(difficultyButton, iconKeys[i]);

            AssignReference(flow, fieldNames[i], difficultyButton);
        }

        TextMeshProUGUI rewardText = EnsureText(
            "RewardText", rewardPanel.transform, "",
            20f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(rewardText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(-40f, -134f), new Vector2(760f, 116f));

        Image rewardIcon = EnsureIcon(
            "RewardIcon", rewardPanel.transform, UIKeys.RewardChestClosed,
            new Vector2(1f, 1f), new Vector2(-58f, -48f), 68f);

        TextMeshProUGUI messageText = EnsureText(
            "MessageText", canvasTransform, "",
            22f, AccentColor, TextAlignmentOptions.Center);

        SetAnchored(messageText.gameObject,
            new Vector2(1f, 0f), new Vector2(-490f, 204f), new Vector2(880f, 40f));

        EnsureTextPlateByName("MessageText", new Vector2(14f, 3f));

        Button backButton = EnsureButton("BackButton", canvasTransform, "지역으로", ButtonColor);
        SetAnchored(backButton.gameObject, new Vector2(1f, 0f), new Vector2(-780f, 108f), new Vector2(260f, 64f));
        StyleButtonByName("BackButton", ButtonColor, 22f);
        EnsureButtonIcon(backButton, UIKeys.IconWorldMap);

        Button startButton = EnsureButton("StartBattleButton", canvasTransform, "전투 시작", AccentColor);
        SetAnchored(startButton.gameObject, new Vector2(1f, 0f), new Vector2(-300f, 108f), new Vector2(460f, 92f));
        StyleButtonByName("StartBattleButton", AccentColor, 30f);
        SetButtonLabelColor(startButton, new Color(0.10f, 0.08f, 0.05f, 1f));

        AssignReference(flow, "rewardText", rewardText);
        AssignReference(flow, "rewardIconImage", rewardIcon);
        AssignReference(flow, "messageText", messageText);
        AssignReference(flow, "backButton", backButton);
        AssignReference(flow, "startButton", startButton);
    }

    // 전투 화면의 편의 버튼: 전투 속도와 전투 포기는 오른쪽 위에, 재도전은 결과 창 안에 둔다.
    private static void BuildBattleComfortButtons(Canvas canvas, BattleManager battleManager)
    {
        Vector2 topRight = new Vector2(1f, 1f);

        Button speedButton = EnsureButton("BattleSpeedButton", canvas.transform, "x1", ButtonColor);
        SetAnchored(speedButton.gameObject, topRight, new Vector2(-414f, -46f), new Vector2(128f, 50f));
        StyleButtonByName("BattleSpeedButton", ButtonColor, 22f);
        EnsureButtonIcon(speedButton, UIKeys.ArrowSkip);

        Button forfeitButton = EnsureButton("ForfeitButton", canvas.transform, "포기", WarningColor);
        SetAnchored(forfeitButton.gameObject, topRight, new Vector2(-414f, -104f), new Vector2(128f, 50f));
        StyleButtonByName("ForfeitButton", WarningColor, 22f);
        EnsureButtonIcon(forfeitButton, UIKeys.IconQuit);

        GameObject turnPanel = Locate("TurnPanel");

        if (turnPanel != null)
        {
            int backIndex = turnPanel.transform.GetSiblingIndex();

            speedButton.transform.SetSiblingIndex(backIndex); // 결과 창과 전투 로그보다 뒤에 그린다.
            forfeitButton.transform.SetSiblingIndex(backIndex);
        }

        AssignReference(battleManager, "speedButton", speedButton);
        AssignReference(battleManager, "forfeitButton", forfeitButton);

        GameObject resultPanel = FindInScene("BattleResultPanel");

        BattleResultUI resultUI = Object.FindFirstObjectByType<BattleResultUI>(
            FindObjectsInactive.Include
        );

        if (resultPanel == null || resultUI == null) { return; }

        Button retryButton = EnsureButton("RetryButton", resultPanel.transform, "재도전", ButtonColor);

        SetLayoutHeight("RetryButton", 60f);
        StyleButtonByName("RetryButton", ButtonColor, 24f);

        AssignReference(resultUI, "retryButton", retryButton);
    }
}
