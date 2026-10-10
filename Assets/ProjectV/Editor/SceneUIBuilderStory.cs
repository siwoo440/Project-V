using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 스토리 화면 부분 (기획서 10.21 / 12.12)
public static partial class SceneUIBuilder
{
    // 인물 그림 세 자리(왼쪽, 가운데, 오른쪽), 자동 진행 버튼, 건너뛰기 확인 창을 만든다.
    // 인물 그림은 대화창 뒤에 그려 아랫부분이 대화창에 가려지게 한다.
    private static void BuildStoryStage(Canvas canvas, StoryFlow flow)
    {
        GameObject background = Locate("BackgroundImage");

        if (background != null)
        {
            AssignReference(flow, "backgroundImage", background.GetComponent<Image>()); // 장면이 배경을 바꿀 수 있다.
        }

        DestroyByName("CharacterImage"); // 예전에 가운데에 두었던 빈 인물 자리

        GameObject dialoguePanel = Locate("DialoguePanel");

        string[] objectNames = { "StoryPortraitLeft", "StoryPortraitCenter", "StoryPortraitRight" };
        string[] fieldNames = { "leftPortrait", "centerPortrait", "rightPortrait" };
        float[] positions = { -540f, 0f, 540f };

        for (int i = 0; i < objectNames.Length; i++)
        {
            GameObject portrait = EnsureObject(objectNames[i], canvas.transform);
            Image portraitImage = portrait.AddComponentIfMissing<Image>();

            portraitImage.sprite = null; // 실행 중에 대사의 지시에 따라 그림을 넣는다.
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
            portraitImage.enabled = false;

            RectTransform portraitRect = portrait.GetComponent<RectTransform>();

            portraitRect.anchorMin = new Vector2(0.5f, 0f);
            portraitRect.anchorMax = new Vector2(0.5f, 0f);
            portraitRect.pivot = new Vector2(0.5f, 0f);
            portraitRect.anchoredPosition = new Vector2(positions[i], 250f);
            portraitRect.sizeDelta = new Vector2(520f, 780f); // 2:3 그림

            if (dialoguePanel != null)
            {
                portrait.transform.SetSiblingIndex(dialoguePanel.transform.GetSiblingIndex()); // 대화창 바로 뒤
            }

            AssignReference(flow, fieldNames[i], portraitImage);
        }

        Button autoButton = EnsureButton("AutoButton", canvas.transform, "자동", ButtonColor);

        SetAnchored(autoButton.gameObject,
            new Vector2(1f, 1f), new Vector2(-400f, -70f), new Vector2(220f, 60f));

        StyleButtonByName("AutoButton", ButtonColor, 24f);
        AssignReference(flow, "autoButton", autoButton);

        // 건너뛰기 확인 창. 처음 보는 장면도 건너뛸 수 있지만 확인을 먼저 받는다. (기획서 10.21.1)
        GameObject confirmPanel = EnsurePanel("SkipConfirmPanel", canvas.transform, PanelDeepColor);
        SetAnchored(confirmPanel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 250f));

        TextMeshProUGUI confirmText = EnsureText(
            "SkipConfirmText", confirmPanel.transform,
            "이 장면을 건너뛸까요?",
            26f, TextColor, TextAlignmentOptions.Center);

        confirmText.text = "이 장면을 건너뛸까요?" + System.Environment.NewLine + "건너뛴 장면도 본 장면으로 기록됩니다.";

        SetAnchored(confirmText.gameObject,
            new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(600f, 100f));

        Button yesButton = EnsureButton("SkipConfirmYesButton", confirmPanel.transform, "건너뛰기", AccentColor);
        SetAnchored(yesButton.gameObject, new Vector2(0.5f, 0f), new Vector2(-160f, 58f), new Vector2(260f, 60f));
        StyleButtonByName("SkipConfirmYesButton", AccentColor, 24f);
        SetButtonLabelColor(yesButton, new Color(0.10f, 0.08f, 0.05f, 1f));

        Button noButton = EnsureButton("SkipConfirmNoButton", confirmPanel.transform, "계속 보기", ButtonColor);
        SetAnchored(noButton.gameObject, new Vector2(0.5f, 0f), new Vector2(160f, 58f), new Vector2(260f, 60f));
        StyleButtonByName("SkipConfirmNoButton", ButtonColor, 24f);

        confirmPanel.transform.SetAsLastSibling(); // 다른 버튼 위에 그린다.

        AssignReference(flow, "skipConfirmPanel", confirmPanel);
        AssignReference(flow, "skipConfirmYesButton", yesButton);
        AssignReference(flow, "skipConfirmNoButton", noButton);

        confirmPanel.SetActive(false);
    }
}
