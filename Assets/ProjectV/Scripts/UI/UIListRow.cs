using UnityEngine; // Unity 기본 기능
using UnityEngine.Events; // 버튼 이벤트 기능
using UnityEngine.UI; // Unity UI 기능

// 세로 목록에 넣는 누를 수 있는 줄의 바탕을 만든다. (그리모어 강화, 상점 목록에서 함께 쓴다)
public static class UIListRow
{
    public static readonly Color LockedTextColor = new Color(0.56f, 0.57f, 0.64f, 1f); // 잠긴 항목 글자색

    private static readonly Color RowColor = new Color(0.17f, 0.14f, 0.26f, 1f);         // 줄 이미지가 없을 때 기본 색
    private static readonly Color RowSelectedColor = new Color(0.42f, 0.34f, 0.16f, 1f); // 줄 이미지가 없을 때 강조 색

    public static GameObject Create(
        Transform parent,
        string objectName,
        float height,
        bool isHighlighted,
        UnityAction clickAction
    )
    {
        if (parent == null) { return null; }

        GameObject rowObject = new GameObject(objectName, typeof(RectTransform));
        rowObject.transform.SetParent(parent, false);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = height;
        layoutElement.preferredHeight = height;

        Image background = rowObject.AddComponent<Image>();

        UISkin.ApplySelectable(
            background, isHighlighted,
            UIKeys.RowNormal, UIKeys.RowSelected,
            RowColor, RowSelectedColor
        ); // 선택한 줄은 밝은 줄 이미지로 표시

        Button rowButton = rowObject.AddComponent<Button>();
        rowButton.targetGraphic = background;

        ColorBlock colors = rowButton.colors;
        colors.highlightedColor = new Color(1f, 0.95f, 0.82f, 1f);
        colors.pressedColor = new Color(0.8f, 0.76f, 0.66f, 1f);
        rowButton.colors = colors;

        if (clickAction != null)
        {
            rowButton.onClick.AddListener(clickAction);
        }

        return rowObject;
    }
}
