using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 소환사 화면과 전투 소환사 패널 (기획서 6.4 / 6.5)
public static partial class SceneUIBuilder
{
    // 소환사 화면: 왼쪽은 액티브 스킬, 오른쪽은 패시브.
    private static void BuildSummonerScene()
    {
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        EnsureMainCamera();
        EnsureBackground(canvas.transform, UIKeys.BgEnhance);

        EnsureTopBar(canvas.transform, 72f, 104f);
        EnsureBottomBar(canvas.transform, 92f);

        TextMeshProUGUI title = EnsureText(
            "TitleText", canvas.transform, "소환사",
            48f, AccentColor, TextAlignmentOptions.Left);

        PlaceSceneTitle(title, new Vector2(260f, -72f), new Vector2(400f, 60f), 48f);

        TextMeshProUGUI levelText = EnsureText(
            "LevelText", canvas.transform, "플레이어 Lv.1 (0 / 100)",
            26f, AccentColor, TextAlignmentOptions.Right);

        SetAnchored(levelText.gameObject,
            new Vector2(1f, 1f), new Vector2(-400f, -72f), new Vector2(700f, 60f));

        // 왼쪽: 액티브 스킬
        GameObject skillPanel = EnsurePanel("SkillPanel", canvas.transform, PanelColor);
        SetAnchored(skillPanel, new Vector2(0f, 0.5f), new Vector2(490f, -40f), new Vector2(900f, 720f));

        TextMeshProUGUI skillTitle = EnsureText(
            "SkillTitleText", skillPanel.transform, "액티브 스킬",
            30f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(skillTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(790f, 40f));

        TextMeshProUGUI skillGuide = EnsureText(
            "SkillGuideText", skillPanel.transform,
            "턴마다 한 번, 마나를 써서 사용합니다. 하나만 장착합니다.",
            18f, SubTextColor, TextAlignmentOptions.Left);

        SetAnchored(skillGuide.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(790f, 30f));

        GameObject skillContent = EnsureVerticalScrollList(
            "SkillListView", "SkillListContent", skillPanel.transform,
            new Vector4(26f, 106f, 26f, 26f));

        // 오른쪽: 패시브
        GameObject passivePanel = EnsurePanel("PassivePanel", canvas.transform, PanelDeepColor);
        SetAnchored(passivePanel, new Vector2(1f, 0.5f), new Vector2(-490f, -40f), new Vector2(900f, 720f));

        TextMeshProUGUI passiveTitle = EnsureText(
            "PassiveTitleText", passivePanel.transform, "패시브",
            30f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(passiveTitle.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(790f, 40f));

        TextMeshProUGUI passiveGuide = EnsureText(
            "PassiveGuideText", passivePanel.transform,
            "패시브 포인트로 해금하고 강화합니다. 하나만 장착합니다.",
            18f, SubTextColor, TextAlignmentOptions.Left);

        SetAnchored(passiveGuide.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(790f, 30f));

        GameObject passiveContent = EnsureVerticalScrollList(
            "PassiveListView", "PassiveListContent", passivePanel.transform,
            new Vector4(26f, 106f, 26f, 236f));

        TextMeshProUGUI passiveDetail = EnsureText(
            "PassiveDetailText", passivePanel.transform, "",
            19f, TextColor, TextAlignmentOptions.TopLeft);

        SetAnchored(passiveDetail.gameObject,
            new Vector2(0.5f, 0f), new Vector2(0f, 160f), new Vector2(800f, 124f));

        Button equipButton = EnsureButton(
            "EquipPassiveButton", passivePanel.transform, "장착", ButtonColor);

        SetAnchored(equipButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(-150f, 58f), new Vector2(260f, 60f));

        Button upgradeButton = EnsureButton(
            "UpgradePassiveButton", passivePanel.transform, "해금", AccentColor);

        SetAnchored(upgradeButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(150f, 58f), new Vector2(260f, 60f));

        StyleButtonByName("EquipPassiveButton", ButtonColor, 24f);
        StyleButtonByName("UpgradePassiveButton", AccentColor, 24f);

        // 하단 안내와 돌아가기
        TextMeshProUGUI messageText = EnsureText(
            "MessageText", canvas.transform, "장착할 스킬과 패시브를 고르세요.",
            22f, AccentColor, TextAlignmentOptions.Left);

        SetAnchored(messageText.gameObject,
            new Vector2(0f, 0f), new Vector2(520f, 56f), new Vector2(940f, 40f));

        Button backButton = EnsureButton(
            "BackButton", canvas.transform, "돌아가기", ButtonColor);

        SetAnchored(backButton.gameObject,
            new Vector2(1f, 0f), new Vector2(-160f, 56f), new Vector2(200f, 60f));

        StyleButtonByName("BackButton", ButtonColor, 22f);

        GameObject controller = EnsureObject("SummonerController", null);
        SummonerFlow flow = controller.AddComponentIfMissing<SummonerFlow>();

        AssignReference(flow, "backButton", backButton);
        AssignReference(flow, "skillListContent", skillContent.transform);
        AssignReference(flow, "passiveListContent", passiveContent.transform);
        AssignReference(flow, "passiveDetailText", passiveDetail);
        AssignReference(flow, "equipPassiveButton", equipButton);
        AssignReference(flow, "upgradePassiveButton", upgradeButton);
        AssignReference(flow, "levelText", levelText);
        AssignReference(flow, "messageText", messageText);
    }
}
