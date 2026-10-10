using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 시험용 버튼 묶음
public static partial class SceneUIBuilder
{
    // 전투 화면 오른쪽의 시험용 버튼 4개. 에디터에서 실행할 때만 보이고 빌드에서는 숨긴다.
    // 승리: 상대 HP를 0으로, 성욕: 절정 상태로, 패배: 플레이어 HP를 0으로, 마나: 이번 턴 마나 가득.
    private static void BuildBattleDebugPanel(Canvas canvas, BattleManager battleManager)
    {
        GameObject panel = EnsurePanel("DebugPanel", canvas.transform, PanelDeepColor);

        SetAnchored(panel,
            new Vector2(1f, 0.5f), new Vector2(-306f, -70f), new Vector2(96f, 196f));

        string[] objectNames = { "DebugWinButton", "DebugLustButton", "DebugLoseButton", "DebugManaButton" };
        string[] labels = { "승리", "성욕", "패배", "마나" };
        string[] fieldNames = { "debugWinButton", "debugLustButton", "debugLoseButton", "debugManaButton" };

        for (int i = 0; i < objectNames.Length; i++)
        {
            Button debugButton = EnsureButton(objectNames[i], panel.transform, labels[i], ButtonColor);

            SetAnchored(debugButton.gameObject,
                new Vector2(0.5f, 1f), new Vector2(0f, -30f - i * 45f), new Vector2(84f, 38f));

            StyleButtonByName(objectNames[i], ButtonColor, 18f);

            AssignReference(battleManager, fieldNames[i], debugButton);
        }

        GameObject turnPanel = Locate("TurnPanel");

        if (turnPanel != null)
        {
            panel.transform.SetSiblingIndex(turnPanel.transform.GetSiblingIndex()); // 결과 창과 전투 로그보다 뒤에 그린다.
        }

        AssignReference(battleManager, "debugPanel", panel);
    }
}
