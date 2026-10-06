using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 테마 적용 부분.
// 테마 이미지가 있으면 이미지를 입히고, 없으면 기존 단색 표시를 그대로 둔다.
public static partial class SceneUIBuilder
{
    private static readonly Color BackdropColor = new Color(0.05f, 0.09f, 0.20f, 0.80f);   // 배경 그림 위 글자 받침
    private static readonly Color LegacyInsetColor = new Color(0.06f, 0.05f, 0.09f, 0.6f); // 단색 패널 안쪽 목록 영역
    private static readonly Color LightInsetColor = new Color(0.36f, 0.26f, 0.14f, 0.12f); // 양피지 패널 안쪽 목록 영역
    private static readonly Color DarkInsetColor = new Color(0f, 0f, 0f, 0.22f);           // 남색 패널 안쪽 목록 영역
    private static readonly Color ButtonLabelDark = new Color(0.10f, 0.08f, 0.05f, 1f);    // 금색 버튼 위 글자

    private static bool sceneHasArt; // 현재 씬에 배경 그림이 적용되었는지 여부

    private static bool SameColor(Color left, Color right)
    {
        return Mathf.Abs(left.r - right.r) < 0.01f &&
               Mathf.Abs(left.g - right.g) < 0.01f &&
               Mathf.Abs(left.b - right.b) < 0.01f;
    }

    // 패널에 이미지를 입힌다. 기본 패널은 양피지, 짙은 패널은 남색.
    private static void SkinPanel(Image panelImage, Color legacyColor)
    {
        string key = string.Empty;

        if (SameColor(legacyColor, PanelColor)) { key = UIKeys.PanelParchment; }
        if (SameColor(legacyColor, PanelDeepColor)) { key = UIKeys.PanelNavy; }

        if (UISkin.ApplySliced(panelImage, key, 36f)) { return; }

        panelImage.color = legacyColor;
    }

    // 버튼에 이미지를 입히고 글자색을 맞춘다. 강조 버튼은 금색, 경고 버튼은 붉은색, 나머지는 파란색.
    private static void SkinButton(Image buttonImage, Color legacyColor, TextMeshProUGUI label)
    {
        bool isAccent = SameColor(legacyColor, AccentColor);
        bool isWarning = SameColor(legacyColor, WarningColor);

        string key = isAccent
            ? UIKeys.ButtonGold
            : isWarning ? UIKeys.ButtonRed : UIKeys.ButtonBlue;

        if (!UISkin.ApplyBar(buttonImage, key))
        {
            buttonImage.color = legacyColor;
        }

        if (label != null)
        {
            label.color = isAccent ? ButtonLabelDark : TextColor; // 금색 위에는 어두운 글자
        }
    }

    // 글자가 놓인 바탕에 맞춰 글자색을 정한다. 양피지 위에서는 어두운 색으로 바꾼다.
    private static Color ResolveTextColor(Color legacyColor, Transform target)
    {
        if (!UISkin.IsOnLightSurface(target)) { return legacyColor; }

        if (SameColor(legacyColor, TextColor)) { return UISkin.Ink; }
        if (SameColor(legacyColor, SubTextColor)) { return UISkin.InkSub; }
        if (SameColor(legacyColor, AccentColor)) { return UISkin.InkAccent; }

        return legacyColor;
    }

    // 패널 안쪽 목록 영역의 바탕색
    private static Color InsetColor(Transform parent)
    {
        if (UISkin.IsOnLightSurface(parent)) { return LightInsetColor; }

        Image parentImage = parent == null ? null : parent.GetComponent<Image>();

        bool isThemed =
            parentImage != null &&
            parentImage.sprite != null &&
            UISkin.Get(parentImage.sprite.name) == parentImage.sprite;

        return isThemed ? DarkInsetColor : LegacyInsetColor;
    }

    // 배경 오브젝트에 씬 배경 그림을 입힌다. 화면이 16:9보다 넓어도 비율을 지키며 채운다.
    private static void ApplyBackgroundArt(GameObject background, string backgroundKey)
    {
        Image backgroundImage = background.AddComponentIfMissing<Image>();
        backgroundImage.raycastTarget = false;

        sceneHasArt = UISkin.ApplySimple(backgroundImage, backgroundKey, false);

        if (!sceneHasArt)
        {
            backgroundImage.color = BackgroundColor;
            return;
        }

        AspectRatioFitter fitter = background.AddComponentIfMissing<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = ReferenceWidth / ReferenceHeight;
    }

    // 배경 그림을 가리던 반투명 덮개를 숨긴다. 배경 그림이 없으면 그대로 둔다.
    private static void HideOverlayByName(string objectName)
    {
        GameObject target = FindInScene(objectName);

        if (target == null) { return; }

        Image overlay = target.GetComponent<Image>();

        if (overlay != null)
        {
            overlay.enabled = !sceneHasArt;
        }
    }
}
