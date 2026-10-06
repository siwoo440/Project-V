using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class SummonerFlow // 소환사 화면의 목록 줄 생성
{
    private const float RowHeight = 84f; // 목록 한 줄의 높이
    private const float RowInset = 36f;  // 줄 양 끝 장식을 피하는 여백

    // 제목, 상태, 설명으로 이루어진 목록 한 줄을 만든다.
    private void CreateRow(
        Transform parent,
        bool isHighlighted,
        bool isAvailable,
        string titleText,
        string bodyText,
        string stateText,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        if (parent == null) { return; }

        GameObject rowObject = new GameObject("SummonerRow", typeof(RectTransform));
        rowObject.transform.SetParent(parent, false);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = RowHeight;
        layoutElement.preferredHeight = RowHeight;

        Image background = rowObject.AddComponent<Image>();

        UISkin.ApplySelectable(
            background, isHighlighted,
            UIKeys.RowNormal, UIKeys.RowSelected,
            RowColor, RowSelectedColor
        ); // 강조 줄은 밝은 줄 이미지로 표시

        Button rowButton = rowObject.AddComponent<Button>();
        rowButton.targetGraphic = background;
        rowButton.interactable = isAvailable && clickAction != null;

        ColorBlock colors = rowButton.colors;
        colors.highlightedColor = new Color(1f, 0.95f, 0.82f, 1f);
        colors.pressedColor = new Color(0.8f, 0.76f, 0.66f, 1f);
        colors.disabledColor = new Color(0.62f, 0.62f, 0.68f, 1f);
        rowButton.colors = colors;

        if (clickAction != null)
        {
            rowButton.onClick.AddListener(clickAction);
        }

        Color titleColor = isAvailable ? UISkin.Cream : LockedTextColor;
        Color bodyColor = isAvailable ? UISkin.CreamSub : LockedTextColor;

        Color stateColor = !isAvailable
            ? LockedTextColor
            : isHighlighted ? UISkin.Gold : UISkin.CreamSub;

        CardEntryFactory.CreateLabel(
            rowObject.transform, "TitleText", titleText,
            21f, titleColor, TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0.5f), new Vector2(0.68f, 1f),
            new Vector2(RowInset, 0f), new Vector2(0f, -8f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "StateText", stateText,
            18f, stateColor, TextAlignmentOptions.BottomRight,
            new Vector2(0.68f, 0.5f), new Vector2(1f, 1f),
            Vector2.zero, new Vector2(-RowInset, -8f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "BodyText", bodyText,
            17f, bodyColor, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0f), new Vector2(1f, 0.5f),
            new Vector2(RowInset, 8f), new Vector2(-RowInset, -2f)
        );

        generatedRows.Add(rowObject);
    }
}
