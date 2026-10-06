using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 전투 화면 부품
public static partial class SceneUIBuilder
{
    // 바로 아래 자식을 이름으로 찾고, 없으면 만든다. (프리팹 안에서도 쓸 수 있다)
    private static GameObject EnsureChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);

        if (existing != null) { return existing.gameObject; }

        GameObject child = new GameObject(childName, typeof(RectTransform));
        child.transform.SetParent(parent, false);

        return child;
    }

    private static void EnsureTextPlateByName(string textName, Vector2 padding)
    {
        GameObject target = Locate(textName);

        if (target == null) { return; }

        EnsureTextPlate(target.GetComponent<TextMeshProUGUI>(), padding);
    }

    // 자식들이 같은 너비로 한 줄을 나눠 쓰는 가로 배치 (필터 버튼 줄)
    private static void EvenHorizontalLayoutByName(string objectName, float spacing)
    {
        GameObject target = Locate(objectName);

        if (target == null) { return; }

        HorizontalLayoutGroup layout =
            target.AddComponentIfMissing<HorizontalLayoutGroup>();

        layout.spacing = spacing;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    // 스크롤 영역의 바탕과 스크롤 막대 색을 패널에 맞춘다.
    private static void StyleScrollViewByName(string viewName)
    {
        GameObject view = FindInScene(viewName);

        if (view == null) { return; }

        Image viewImage = view.GetComponent<Image>();

        if (viewImage != null)
        {
            viewImage.color = InsetColor(view.transform.parent);
        }

        if (UITheme.Current == null) { return; }

        foreach (Scrollbar scrollbar in view.GetComponentsInChildren<Scrollbar>(true))
        {
            Image track = scrollbar.GetComponent<Image>();

            if (track != null)
            {
                track.color = new Color(0f, 0f, 0f, 0.25f);
            }

            Image handle = scrollbar.handleRect == null
                ? null
                : scrollbar.handleRect.GetComponent<Image>();

            if (handle != null)
            {
                handle.color = new Color(0.83f, 0.69f, 0.33f, 1f);
            }
        }
    }

    // 전투 결과 패널 위쪽에 승패 문장을 걸친다. 그림은 결과가 나올 때 정해진다.
    private static void EnsureResultEmblem()
    {
        GameObject resultPanel = FindInScene("BattleResultPanel");

        if (resultPanel == null) { return; }

        GameObject emblem = EnsureObject("ResultEmblem", resultPanel.transform);

        Image emblemImage = emblem.AddComponentIfMissing<Image>();
        emblemImage.raycastTarget = false;
        emblemImage.preserveAspect = true;
        emblemImage.enabled = false;

        LayoutElement emblemLayout = emblem.AddComponentIfMissing<LayoutElement>();
        emblemLayout.ignoreLayout = true; // 세로 배치에 끼지 않고 패널 위에 걸친다.

        SetAnchored(emblem, new Vector2(0.5f, 1f), new Vector2(0f, 52f), new Vector2(170f, 170f));

        BattleResultUI resultUI = Object.FindFirstObjectByType<BattleResultUI>(
            FindObjectsInactive.Include
        );

        if (resultUI != null)
        {
            AssignReference(resultUI, "emblemImage", emblemImage);
        }
    }
}
