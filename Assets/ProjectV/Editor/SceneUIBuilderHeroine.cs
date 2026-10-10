using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 히로인전 화면 부분 (기획서 F.3.1)
public static partial class SceneUIBuilder
{
    // 지역 화면의 목록 탭. 목록 판 위쪽에 버튼 두 개를 놓고 목록은 그 아래에서 시작한다.
    // 메인 진행: 주요 히로인전과 일반전. 서브 콘텐츠: 서브 히로인전과 포획전.
    private static void BuildStageTabs(GameObject listPanel, GameObject listContent, StageSelectFlow flow)
    {
        string[] objectNames = { "MainTabButton", "SubTabButton" };
        string[] labels = { "메인 진행", "서브 콘텐츠" };
        string[] fieldNames = { "mainTabButton", "subTabButton" };
        string[] iconKeys = { UIKeys.StageHeroine, UIKeys.CaptureOrb };

        for (int i = 0; i < objectNames.Length; i++)
        {
            Button tabButton = EnsureButton(objectNames[i], listPanel.transform, labels[i], ButtonColor);

            SetAnchored(tabButton.gameObject,
                new Vector2(0.5f, 1f), new Vector2((i * 2 - 1) * 176f, -58f), new Vector2(336f, 52f));

            StyleButtonByName(objectNames[i], ButtonColor, 22f);
            EnsureButtonIcon(tabButton, iconKeys[i]);

            AssignReference(flow, fieldNames[i], tabButton);
        }

        SetStretch(listContent, new Vector4(30f, 100f, 30f, 24f)); // 탭 줄 아래에 여섯 줄이 들어간다.
    }

    // 지역 화면 상세 칸의 히로인 얼굴 그림. 스테이지 이름 오른쪽, 보상 상자 그림의 왼쪽에 둔다.
    // 전투 그림을 크게 넣고 칸으로 가려 얼굴 부분만 보이게 한다. 히로인전을 골랐을 때만 보인다.
    private static void BuildHeroinePortrait(Transform detailPanel, StageSelectFlow flow)
    {
        GameObject frame = EnsurePanel("HeroinePortraitFrame", detailPanel, PanelColor);

        SetAnchored(frame,
            new Vector2(0.5f, 1f), new Vector2(236f, -68f), new Vector2(110f, 110f));

        frame.AddComponentIfMissing<RectMask2D>();

        GameObject art = EnsureObject("HeroinePortraitImage", frame.transform);
        Image artImage = art.AddComponentIfMissing<Image>();

        artImage.sprite = null; // 실행 중에 고른 히로인의 그림을 넣는다.
        artImage.color = Color.white;
        artImage.preserveAspect = true;
        artImage.raycastTarget = false;
        artImage.enabled = false;

        RectTransform artRect = art.GetComponent<RectTransform>();

        artRect.anchorMin = new Vector2(0.5f, 1f);
        artRect.anchorMax = new Vector2(0.5f, 1f);
        artRect.pivot = new Vector2(0.5f, 1f);
        artRect.anchoredPosition = new Vector2(-7f, 28f); // 얼굴이 칸 가운데에 오도록 위와 왼쪽으로 조금 올린다.
        artRect.sizeDelta = new Vector2(230f, 345f);

        AssignReference(flow, "heroinePortraitFrame", frame);
        AssignReference(flow, "heroinePortraitImage", artImage);

        frame.SetActive(false);
    }

    // 전투 화면의 히로인 그림 칸. 위쪽 가운데에 두고, 세로로 긴 그림의 윗부분(머리부터 허리)만 보이게 가린다.
    // 그림이 있는 히로인과 싸울 때만 보이고, 그때 턴 표시는 왼쪽으로 옮겨진다. (기획서 11.8.1)
    private static void BuildHeroineArt(Canvas canvas, BattleManager battleManager)
    {
        GameObject frame = EnsurePanel("HeroineArtFrame", canvas.transform, PanelDeepColor);

        SetAnchored(frame,
            new Vector2(0.5f, 1f), new Vector2(0f, -154f), new Vector2(320f, 284f)); // 아래쪽은 마물 필드의 빈 윗부분까지 쓴다.

        frame.AddComponentIfMissing<RectMask2D>(); // 칸 밖으로 나가는 그림의 아랫부분을 가린다.

        GameObject art = EnsureObject("HeroineArtImage", frame.transform);
        Image artImage = art.AddComponentIfMissing<Image>();

        artImage.sprite = null; // 전투가 시작될 때 히로인의 그림을 불러 넣는다.
        artImage.color = Color.white;
        artImage.preserveAspect = true;
        artImage.raycastTarget = false;
        artImage.enabled = false;

        RectTransform artRect = art.GetComponent<RectTransform>();

        artRect.anchorMin = new Vector2(0.5f, 1f);
        artRect.anchorMax = new Vector2(0.5f, 1f);
        artRect.pivot = new Vector2(0.5f, 1f);
        artRect.anchoredPosition = new Vector2(0f, -6f);
        artRect.sizeDelta = new Vector2(308f, 462f); // 2:3 그림. 칸보다 길어 허리 아래는 가려진다.

        HeroineArtUI artUI = frame.AddComponentIfMissing<HeroineArtUI>();

        AssignReference(artUI, "artImage", artImage);
        AssignReference(battleManager, "heroineArtUI", artUI);

        GameObject turnPanel = Locate("TurnPanel");

        if (turnPanel != null)
        {
            frame.transform.SetSiblingIndex(turnPanel.transform.GetSiblingIndex()); // 결과 창과 전투 로그보다 뒤에 그린다.
        }

        frame.SetActive(false); // 전투가 시작될 때 그림이 있으면 켠다.
    }

    // 전투 화면의 히로인 이름. 히로인전에서는 전투 데이터의 이름으로 바뀐다.
    private static void AssignHeroineName(BattleManager battleManager)
    {
        GameObject nameObject = Locate("HeroineNameText");

        if (nameObject == null) { return; }

        AssignReference(battleManager, "heroineNameText", nameObject.GetComponent<TMP_Text>());
    }
}
