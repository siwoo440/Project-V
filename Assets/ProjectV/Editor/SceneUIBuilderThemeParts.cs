using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 씬 UI 구성 도구의 테마 부품: 상단 띠, 받침, 제목 리본, 아이콘
public static partial class SceneUIBuilder
{
    // 뒤에 깔릴 오브젝트가 앞 오브젝트보다 먼저 그려지도록 순서를 맞춘다.
    private static void PlaceBehind(GameObject back, GameObject front)
    {
        if (back.transform.parent != front.transform.parent) { return; }

        int frontIndex = front.transform.GetSiblingIndex();

        if (back.transform.GetSiblingIndex() > frontIndex)
        {
            back.transform.SetSiblingIndex(frontIndex);
        }
    }

    // 화면 위쪽 띠. 제목과 상단 버튼의 받침이 된다. 배경 그림도 띠 이미지도 없으면 보이지 않는다.
    private static void EnsureTopBar(Transform canvasTransform, float centerY, float height)
    {
        GameObject bar = EnsureObject("TopBar", canvasTransform);

        Image barImage = bar.AddComponentIfMissing<Image>();
        barImage.raycastTarget = false;

        RectTransform rect = bar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(24f, -centerY - height * 0.5f);
        rect.offsetMax = new Vector2(-24f, -centerY + height * 0.5f);

        bool hasSprite = UISkin.ApplyBar(barImage, UIKeys.BarHeader);

        if (!hasSprite) { barImage.color = BackdropColor; }

        barImage.enabled = hasSprite || sceneHasArt;
        bar.transform.SetSiblingIndex(1); // 배경 바로 위
    }

    // 화면 아래쪽 받침. 안내 문구와 하단 버튼이 올라간다.
    private static void EnsureBottomBar(Transform canvasTransform, float height)
    {
        GameObject bar = EnsureObject("BottomBar", canvasTransform);

        Image barImage = bar.AddComponentIfMissing<Image>();
        barImage.raycastTarget = false;

        RectTransform rect = bar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(24f, 10f);
        rect.offsetMax = new Vector2(-24f, 10f + height);

        bool hasSprite = UISkin.ApplySliced(barImage, UIKeys.PanelNavy, 30f);

        if (!hasSprite) { barImage.color = BackdropColor; }

        barImage.enabled = hasSprite || sceneHasArt;
        bar.transform.SetSiblingIndex(1); // 배경 바로 위
    }

    // 화면 제목을 리본 위에 올린다. 리본 이미지가 없으면 글자만 기존 자리에 둔다.
    private static void PlaceSceneTitle(
        TextMeshProUGUI title,
        Vector2 legacyPosition,
        Vector2 legacySize,
        float legacyFontSize)
    {
        GameObject ribbon = EnsureObject("TitleRibbon", title.transform.parent);

        Image ribbonImage = ribbon.AddComponentIfMissing<Image>();
        ribbonImage.raycastTarget = false;

        SetAnchored(ribbon, new Vector2(0f, 1f), new Vector2(300f, -72f), new Vector2(540f, 104f));

        bool hasRibbon = UISkin.ApplyBar(ribbonImage, UIKeys.RibbonTitle);

        ribbonImage.enabled = hasRibbon;

        if (hasRibbon)
        {
            SetAnchored(title.gameObject, new Vector2(0f, 1f), new Vector2(300f, -76f), new Vector2(360f, 60f));
            title.fontSize = 40f;
            title.alignment = TextAlignmentOptions.Center;
            title.color = UISkin.Cream;
        }
        else
        {
            SetAnchored(title.gameObject, new Vector2(0f, 1f), legacyPosition, legacySize);
            title.fontSize = legacyFontSize;
            title.alignment = TextAlignmentOptions.Left;
            title.color = AccentColor;
        }

        PlaceBehind(ribbon, title.gameObject);
    }

    // 배경 그림 위에 바로 놓인 글자 뒤에 받침을 깐다. 글자가 비어 있으면 받침도 숨는다.
    private static void EnsureTextPlate(TextMeshProUGUI text, Vector2 padding)
    {
        if (text == null) { return; }

        GameObject plate = EnsureObject(text.gameObject.name + "Plate", text.transform.parent);

        Image plateImage = plate.AddComponentIfMissing<Image>();
        plateImage.raycastTarget = false;

        RectTransform textRect = text.rectTransform;
        RectTransform plateRect = plate.GetComponent<RectTransform>();

        plateRect.anchorMin = textRect.anchorMin;
        plateRect.anchorMax = textRect.anchorMax;
        plateRect.pivot = textRect.pivot;
        plateRect.anchoredPosition = textRect.anchoredPosition;
        plateRect.sizeDelta = textRect.sizeDelta + padding * 2f;

        bool hasSprite = UISkin.ApplyBar(plateImage, UIKeys.PlateLabel);

        if (!hasSprite) { plateImage.color = BackdropColor; }

        bool isShown = hasSprite || sceneHasArt;

        UITextPlate watcher = plate.AddComponentIfMissing<UITextPlate>();

        AssignReference(watcher, "targetText", text);
        AssignReference(watcher, "plateImage", plateImage);

        watcher.enabled = isShown;
        plateImage.enabled = isShown && !string.IsNullOrEmpty(text.text);

        PlaceBehind(plate, text.gameObject);
    }

    // 배경 그림 위에 놓이는 글자 묶음 뒤에 남색 받침 패널을 깐다.
    private static void EnsureBackdropPanel(
        string objectName,
        Transform parent,
        Vector2 anchor,
        Vector2 position,
        Vector2 size)
    {
        GameObject backdrop = EnsureObject(objectName, parent);

        Image backdropImage = backdrop.AddComponentIfMissing<Image>();
        backdropImage.raycastTarget = false;

        SetAnchored(backdrop, anchor, position, size);

        bool hasSprite = UISkin.ApplySliced(backdropImage, UIKeys.PanelNavy, 36f);

        if (!hasSprite) { backdropImage.color = BackdropColor; }

        backdropImage.enabled = hasSprite || sceneHasArt;
        backdrop.transform.SetSiblingIndex(1); // 배경 바로 위
    }

    // 버튼 왼쪽에 아이콘을 넣는다. 이미지가 없으면 숨긴다.
    private static void EnsureButtonIcon(Button button, string spriteKey)
    {
        if (button == null) { return; }

        Transform existing = button.transform.Find("Icon");

        GameObject icon = existing != null
            ? existing.gameObject
            : new GameObject("Icon", typeof(RectTransform));

        icon.transform.SetParent(button.transform, false);

        Image iconImage = icon.AddComponentIfMissing<Image>();
        iconImage.raycastTarget = false;

        SetAnchored(icon, new Vector2(0f, 0.5f), new Vector2(60f, 0f), new Vector2(46f, 46f));

        iconImage.enabled = UISkin.ApplySimple(iconImage, spriteKey, true);
    }

    // 아이콘 이미지를 만든다. 이미지가 없으면 숨긴다.
    private static Image EnsureIcon(
        string objectName,
        Transform parent,
        string spriteKey,
        Vector2 anchor,
        Vector2 position,
        float size)
    {
        GameObject icon = EnsureObject(objectName, parent);

        Image iconImage = icon.AddComponentIfMissing<Image>();
        iconImage.raycastTarget = false;

        SetAnchored(icon, anchor, position, new Vector2(size, size));

        iconImage.enabled = UISkin.ApplySimple(iconImage, spriteKey, true);

        return iconImage;
    }

    // 패널 이미지의 모서리 장식 크기를 패널 크기에 맞추고, 막대 이미지의 끝 장식을 높이에 맞춘다.
    private static void FinalizeTheme(Transform root)
    {
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.sprite == null) { continue; }

            string spriteName = image.sprite.name;

            bool isPanel =
                spriteName == UIKeys.PanelParchment ||
                spriteName == UIKeys.PanelNavy ||
                spriteName == UIKeys.SlotUnit ||
                spriteName == UIKeys.SlotEmpty;

            if (isPanel && image.type == Image.Type.Sliced)
            {
                Rect rect = image.rectTransform.rect;
                float shortSide = Mathf.Min(rect.width, rect.height);
                float corner = Mathf.Clamp(shortSide * 0.16f, 18f, 40f);
                float border = Mathf.Max(image.sprite.border.x, image.sprite.border.y);
                float pixelsPerUnit = image.pixelsPerUnit;

                if (shortSide > 1f && border > 0f && pixelsPerUnit > 0f)
                {
                    image.pixelsPerUnitMultiplier = border / (corner * pixelsPerUnit);
                }
            }

            UISliceFitter fitter = image.GetComponent<UISliceFitter>();

            if (fitter != null) { fitter.Apply(); }
        }
    }

    private static void FinalizeSceneTheme() // 현재 씬 전체에 마무리 적용
    {
        Canvas.ForceUpdateCanvases();

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            FinalizeTheme(root.transform);
        }
    }
}
