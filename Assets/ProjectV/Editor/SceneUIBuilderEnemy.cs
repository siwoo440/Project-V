using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 적 마물 필드와 적 편성 목록 연결 (기획서 8.10)
public static partial class SceneUIBuilder
{
    // 전투 화면의 적 마물 필드. 위쪽 가운데, 아군 마물 필드 바로 위에 둔다.
    // 적 마물 전투에서만 보이고, 그때 턴 표시는 오른쪽 위(히로인 정보 자리)로 옮겨진다.
    private static void BuildEnemyField(Canvas canvas, BattleManager battleManager)
    {
        GameObject enemyPanel = EnsurePanel("EnemyFieldPanel", canvas.transform, PanelDeepColor);

        SetAnchored(enemyPanel,
            new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(780f, 236f));

        GameObject enemyContainer = EnsureObject("EnemyFieldContainer", enemyPanel.transform);

        SetStretch(enemyContainer, new Vector4(16f, 8f, 16f, 8f));
        HorizontalLayoutByName("EnemyFieldContainer", 10f, 0, TextAnchor.MiddleCenter);

        enemyPanel.SetActive(false); // 전투가 시작될 때 전투 종류에 따라 켠다.

        AssignReference(battleManager, "enemyFieldPanel", enemyPanel);
        AssignReference(battleManager, "enemyFieldContainer", enemyContainer.transform);

        GameObject heroinePanel = Locate("HeroinePanel");

        if (heroinePanel != null)
        {
            AssignReference(battleManager, "heroinePanel", heroinePanel);
        }

        GameObject turnPanel = Locate("TurnPanel");

        if (turnPanel != null)
        {
            AssignReference(battleManager, "turnPanel", turnPanel.GetComponent<RectTransform>());
        }
    }

    // 지역 화면의 난이도 버튼. 소모품 칸 위에 한 줄로 놓고, 일반전을 골랐을 때만 보인다. (기획서 8.11)
    private static void BuildDifficultyButtons(Transform detailPanel, StageSelectFlow flow)
    {
        string[] objectNames = { "DifficultyEasyButton", "DifficultyNormalButton", "DifficultyHardButton" };
        string[] labels = { "쉬움", "보통", "어려움" };
        string[] fieldNames = { "easyButton", "normalButton", "hardButton" };

        string[] iconKeys =
        {
            UIKeys.DifficultyEasy, UIKeys.DifficultyNormal, UIKeys.DifficultyHard
        };

        for (int i = 0; i < objectNames.Length; i++)
        {
            Button difficultyButton = EnsureButton(objectNames[i], detailPanel, labels[i], ButtonColor);

            SetAnchored(difficultyButton.gameObject,
                new Vector2(0.5f, 0f), new Vector2((i - 1) * 264f, 232f), new Vector2(250f, 50f));

            StyleButtonByName(objectNames[i], ButtonColor, 21f);
            EnsureButtonIcon(difficultyButton, iconKeys[i]);

            AssignReference(flow, fieldNames[i], difficultyButton);
        }

        // 최초 보상 표시: 스테이지 이름 오른쪽의 상자 그림
        Image rewardIcon = EnsureIcon(
            "StageRewardIcon", detailPanel, UIKeys.RewardChestClosed,
            new Vector2(0.5f, 1f), new Vector2(340f, -62f), 76f);

        AssignReference(flow, "rewardIconImage", rewardIcon);
    }

    // 프로젝트의 적 편성 데이터를 지역 화면에 연결한다. 지역 ID와 단계로 찾아 쓴다.
    private static void ApplyFormationList(StageSelectFlow flow)
    {
        if (flow == null) { return; }

        List<EnemyFormationData> formations = LoadAssets<EnemyFormationData>();

        formations.Sort((left, right) =>
            string.CompareOrdinal(left.FormationId, right.FormationId));

        SerializedObject serializedFlow = new SerializedObject(flow);

        AssignList(serializedFlow, "formations", formations);

        serializedFlow.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"적 편성 {formations.Count}개를 연결했습니다.");
    }
}
