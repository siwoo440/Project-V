using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 시작 손패 교환 창 (기획서 5.4.2)
public static partial class SceneUIBuilder
{
    // 전투 화면의 시작 손패 교환 창. 마물 필드와 손패 사이에 두고, 전투가 시작될 때만 보인다.
    private static void BuildMulliganPanel(Canvas canvas, BattleManager battleManager)
    {
        GameObject panel = EnsurePanel("MulliganPanel", canvas.transform, PanelDeepColor);

        SetAnchored(panel,
            new Vector2(0.5f, 0f), new Vector2(0f, 452f), new Vector2(860f, 160f));

        TextMeshProUGUI mulliganText = EnsureText(
            "MulliganText", panel.transform, "시작 손패 교환",
            22f, TextColor, TextAlignmentOptions.Center);

        SetAnchored(mulliganText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(800f, 64f));

        Button confirmButton = EnsureButton("MulliganConfirmButton", panel.transform, "교환", AccentColor);

        SetAnchored(confirmButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(-170f, 42f), new Vector2(300f, 52f));

        StyleButtonByName("MulliganConfirmButton", AccentColor, 22f);
        SetButtonLabelColor(confirmButton, new Color(0.10f, 0.08f, 0.05f, 1f));

        Button skipButton = EnsureButton("MulliganSkipButton", panel.transform, "그대로 시작", ButtonColor);

        SetAnchored(skipButton.gameObject,
            new Vector2(0.5f, 0f), new Vector2(170f, 42f), new Vector2(300f, 52f));

        StyleButtonByName("MulliganSkipButton", ButtonColor, 22f);

        GameObject handPanel = Locate("HandPanel");

        if (handPanel != null)
        {
            panel.transform.SetSiblingIndex(handPanel.transform.GetSiblingIndex() + 1); // 결과 창과 전투 로그보다 뒤에 그린다.
        }

        AssignReference(battleManager, "mulliganPanel", panel);
        AssignReference(battleManager, "mulliganText", mulliganText);
        AssignReference(battleManager, "mulliganConfirmButton", confirmButton);
        AssignReference(battleManager, "mulliganSkipButton", skipButton);

        panel.SetActive(false); // 시작 손패를 받은 뒤에 켠다.
    }
}
