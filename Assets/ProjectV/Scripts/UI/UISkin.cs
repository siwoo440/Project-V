using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 테마 이미지를 UI에 입히는 도우미.
// 이미지가 없으면 false를 돌려주고 아무것도 바꾸지 않으므로, 호출하는 쪽이 기존 단색 표시를 이어 쓴다.
public static partial class UISkin
{
    // 밝은 바탕(양피지) 위 글자색
    public static readonly Color Ink = new Color(0.23f, 0.16f, 0.11f, 1f);       // 본문
    public static readonly Color InkSub = new Color(0.47f, 0.39f, 0.30f, 1f);    // 보조 설명
    public static readonly Color InkAccent = new Color(0.54f, 0.22f, 0.07f, 1f); // 제목과 강조

    // 어두운 바탕(남색) 위 글자색
    public static readonly Color Cream = new Color(0.98f, 0.95f, 0.85f, 1f);    // 본문
    public static readonly Color CreamSub = new Color(0.72f, 0.77f, 0.87f, 1f); // 보조 설명
    public static readonly Color Gold = new Color(1f, 0.82f, 0.36f, 1f);        // 제목과 강조

    public static readonly Color ButtonInk = new Color(0.22f, 0.14f, 0.04f, 1f); // 금색 버튼 위 글자

    public static Sprite Get(string key) // 테마 이미지 찾기
    {
        UITheme theme = UITheme.Current;

        return theme == null ? null : theme.GetSprite(key);
    }

    public static bool Has(string key) // 테마 이미지 보유 여부
    {
        return Get(key) != null;
    }

    // 네 변을 유지하고 가운데만 늘리는 패널 이미지를 입힌다.
    // corner는 화면에 보일 모서리 장식의 크기(캔버스 단위)이다.
    public static bool ApplySliced(Image image, string key, float corner)
    {
        Sprite sprite = Get(key);

        if (image == null || sprite == null) { return false; }

        RemoveFitter(image);

        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.fillCenter = true;
        image.color = Color.white;

        float border = Mathf.Max(sprite.border.x, sprite.border.y);
        float pixelsPerUnit = image.pixelsPerUnit;

        if (border > 0f && corner > 0f && pixelsPerUnit > 0f)
        {
            image.pixelsPerUnitMultiplier =
                Mathf.Max(0.01f, border / (corner * pixelsPerUnit));
        }

        return true;
    }

    // 좌우 끝 장식을 유지하고 가운데만 늘리는 막대 이미지를 입힌다. (버튼, 목록 줄, 게이지)
    public static bool ApplyBar(Image image, string key)
    {
        Sprite sprite = Get(key);

        if (image == null || sprite == null) { return false; }

        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.fillCenter = true;
        image.color = Color.white;

        UISliceFitter fitter = image.GetComponent<UISliceFitter>();

        if (fitter == null)
        {
            fitter = image.gameObject.AddComponent<UISliceFitter>();
        }

        fitter.Apply();

        return true;
    }

    // 늘리지 않고 그대로 보여주는 이미지를 입힌다. (카드 틀, 아이콘, 문장)
    public static bool ApplySimple(Image image, string key, bool preserveAspect)
    {
        Sprite sprite = Get(key);

        if (image == null || sprite == null) { return false; }

        RemoveFitter(image);

        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = preserveAspect;
        image.color = Color.white;

        return true;
    }

    // 선택 상태를 이미지 교체로 표시한다. 테마 이미지가 없으면 지정한 단색을 쓴다.
    public static bool ApplySelectable(
        Image image,
        bool isSelected,
        string normalKey,
        string selectedKey,
        Color normalColor,
        Color selectedColor
    )
    {
        if (image == null) { return false; }

        if (ApplyBar(image, isSelected ? selectedKey : normalKey))
        {
            return true;
        }

        image.color = isSelected ? selectedColor : normalColor;

        return false;
    }

    private static void RemoveFitter(Image image)
    {
        UISliceFitter fitter = image.GetComponent<UISliceFitter>();

        if (fitter == null) { return; }

        if (Application.isPlaying)
        {
            Object.Destroy(fitter);
        }
        else
        {
            Object.DestroyImmediate(fitter);
        }
    }

    // 글자 사이에 넣는 아이콘 표기. 아이콘이 없으면 빈 문자열을 돌려준다.
    public static string Icon(string iconName)
    {
        UITheme theme = UITheme.Current;

        if (theme == null || !theme.HasIcon(iconName)) { return string.Empty; }

        return $"<sprite=\"{theme.IconAssetName}\" name=\"{iconName}\">";
    }

    // 아이콘이 있으면 아이콘을, 없으면 글자 이름표를 돌려준다.
    public static string IconOr(string iconName, string fallbackLabel)
    {
        string iconTag = Icon(iconName);

        return string.IsNullOrEmpty(iconTag) ? fallbackLabel : iconTag;
    }

    // 가장 가까운 테마 바탕이 밝은 양피지 계열인지 확인한다. (글자색 결정용)
    public static bool IsOnLightSurface(Transform target)
    {
        if (UITheme.Current == null) { return false; }

        for (Transform node = target; node != null; node = node.parent)
        {
            Image image = node.GetComponent<Image>();

            if (image == null || !image.enabled || image.sprite == null) { continue; }

            string spriteName = image.sprite.name;

            if (spriteName == UIKeys.PanelParchment ||
                spriteName == UIKeys.InputField ||
                spriteName == UIKeys.PlateName ||
                spriteName == UIKeys.SlotUnit)
            {
                return true;
            }

            if (Get(spriteName) == image.sprite)
            {
                return false; // 그 밖의 테마 이미지는 어두운 바탕
            }
        }

        return false;
    }
}
