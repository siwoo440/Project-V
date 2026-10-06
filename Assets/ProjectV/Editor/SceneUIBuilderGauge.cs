using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 게이지 구성 (기획서 11.8.1)
public static partial class SceneUIBuilder
{
    private static readonly Color GaugeBackColor = new Color(0.07f, 0.07f, 0.12f, 1f);  // 게이지 틀 이미지가 없을 때 바탕
    private static readonly Color HpGaugeColor = new Color(0.78f, 0.18f, 0.20f, 1f);    // 체력
    private static readonly Color LustGaugeColor = new Color(0.93f, 0.36f, 0.62f, 1f);  // 성욕
    private static readonly Color ManaGaugeColor = new Color(0.20f, 0.46f, 0.90f, 1f);  // 마나

    // 글자 줄 뒤에 게이지를 깔아 한 줄로 만든다. 글자는 게이지 위에 그대로 남는다.
    private static UIGauge EnsureGaugeRow(
        string textName,
        string rowName,
        string fillKey,
        Color fillColor,
        float height,
        float fontSize)
    {
        GameObject textObject = Locate(textName);

        if (textObject == null) { return null; }

        GameObject existingRow = FindInScene(rowName);

        Transform rowParent = existingRow != null
            ? existingRow.transform.parent
            : textObject.transform.parent;

        int rowIndex = existingRow != null
            ? existingRow.transform.GetSiblingIndex()
            : textObject.transform.GetSiblingIndex();

        return BuildGaugeRow(
            textObject, EnsureObject(rowName, rowParent), rowIndex,
            fillKey, fillColor, height, fontSize
        );
    }

    private static UIGauge BuildGaugeRow(
        GameObject textObject,
        GameObject row,
        int rowIndex,
        string fillKey,
        Color fillColor,
        float height,
        float fontSize)
    {
        row.transform.SetSiblingIndex(rowIndex); // 글자가 있던 자리에 줄을 놓는다.

        Image frame = row.AddComponentIfMissing<Image>();
        frame.raycastTarget = false;

        bool hasFrame = UISkin.ApplyBar(frame, UIKeys.GaugeFrame);

        if (!hasFrame) { frame.color = GaugeBackColor; }

        LayoutElement rowLayout = row.AddComponentIfMissing<LayoutElement>();
        rowLayout.minHeight = height;
        rowLayout.preferredHeight = height;

        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(rowRect.sizeDelta.x, height);

        // 틀 이미지의 금테 두께만큼 안쪽으로 채움 영역을 둔다.
        float sideInset = hasFrame ? height * 0.24f : 2f;
        float edgeInset = hasFrame ? height * 0.2f : 2f;

        GameObject area = EnsureChild(row.transform, "FillArea");
        SetStretch(area, new Vector4(sideInset, edgeInset, sideInset, edgeInset));

        GameObject fill = EnsureChild(area.transform, "Fill");

        Image fillImage = fill.AddComponentIfMissing<Image>();
        fillImage.raycastTarget = false;

        if (!UISkin.ApplyBar(fillImage, fillKey)) { fillImage.color = fillColor; }

        SetStretch(fill, Vector4.zero);

        UIGauge gauge = row.AddComponentIfMissing<UIGauge>();

        AssignReference(gauge, "fillRect", fill.GetComponent<RectTransform>());
        AssignReference(gauge, "fillImage", fillImage);

        textObject.transform.SetParent(row.transform, false);
        SetStretch(textObject, Vector4.zero);
        textObject.transform.SetAsLastSibling(); // 글자가 게이지 위에 그려지도록

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();

        if (text != null)
        {
            text.color = TextColor; // 게이지 위 글자는 바탕과 관계없이 밝은 색
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }

        return gauge;
    }
}
