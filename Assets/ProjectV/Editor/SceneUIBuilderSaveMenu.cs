using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 메인 메뉴 저장 메뉴와 저장용 카드 목록 연결 (기획서 10.5)
public static partial class SceneUIBuilder
{
    // 메인 메뉴 왼쪽의 저장 메뉴: 이어하기, 새 게임, 불러오기와 최근 저장 정보.
    private static void BuildMainMenuSavePanel(Canvas canvas, MainMenuFlow flow, bool hasCrest)
    {
        GameObject savePanel = EnsurePanel("SaveMenuPanel", canvas.transform, PanelDeepColor);

        SetAnchored(savePanel, new Vector2(0.5f, 0.5f),
            new Vector2(-540f, hasCrest ? -160f : -40f), new Vector2(430f, 530f));

        ApplyVerticalLayout(savePanel, 10f, 34, TextAnchor.UpperCenter);

        Button continueButton = EnsureButton("ContinueButton", savePanel.transform, "이어하기", AccentColor);
        Button newGameButton = EnsureButton("NewGameButton", savePanel.transform, "새 게임", ButtonColor);
        Button loadButton = EnsureButton("LoadButton", savePanel.transform, "불러오기", ButtonColor);

        TextMeshProUGUI saveInfoText = EnsureText(
            "SaveInfoText", savePanel.transform, "",
            18f, TextColor, TextAlignmentOptions.TopLeft);

        continueButton.transform.SetSiblingIndex(0);
        newGameButton.transform.SetSiblingIndex(1);
        loadButton.transform.SetSiblingIndex(2);
        saveInfoText.transform.SetSiblingIndex(3);

        StyleButtonByName("ContinueButton", AccentColor, 25f);
        StyleButtonByName("NewGameButton", ButtonColor, 25f);
        StyleButtonByName("LoadButton", ButtonColor, 25f);

        foreach (Button saveMenuButton in new[] { continueButton, newGameButton, loadButton })
        {
            LayoutElement buttonLayout = saveMenuButton.GetComponent<LayoutElement>();

            buttonLayout.minHeight = 58f;
            buttonLayout.preferredHeight = 58f;
        }

        LayoutElement infoLayout = saveInfoText.gameObject.AddComponentIfMissing<LayoutElement>();

        infoLayout.minHeight = 240f; // 최근 저장 4줄과 안내 문구가 들어가는 높이
        infoLayout.preferredHeight = 240f;

        EnsureButtonIcon(continueButton, UIKeys.SaveContinue);
        EnsureButtonIcon(newGameButton, UIKeys.SaveNewGame);
        EnsureButtonIcon(loadButton, UIKeys.SaveLoad);

        AssignReference(flow, "continueButton", continueButton);
        AssignReference(flow, "newGameButton", newGameButton);
        AssignReference(flow, "loadButton", loadButton);
        AssignReference(flow, "saveInfoText", saveInfoText);
    }

    // 프로젝트의 모든 카드를 진행 데이터에 연결한다. 저장 데이터의 카드 ID를 카드로 되돌릴 때 쓴다.
    private static void ApplyCardCatalog(PlayerProgressManager progress)
    {
        if (progress == null) { return; }

        List<CardData> cards = LoadAssets<CardData>();

        cards.Sort((left, right) => string.CompareOrdinal(left.CardId, right.CardId));

        SerializedObject serializedProgress = new SerializedObject(progress);

        AssignList(serializedProgress, "cardCatalog", cards);

        serializedProgress.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"저장용 카드 목록 {cards.Count}종을 연결했습니다.");
    }
}
