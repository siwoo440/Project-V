using UnityEngine; // Unity 기본 기능
using UnityEngine.EventSystems; // UI 기본 동작
using UnityEngine.UI; // Unity UI 기능

// 좌우 끝 장식이 있는 막대 이미지를 높이에 맞춰 그리도록 조정한다.
// 분할 경계는 이미지 픽셀 기준이라, 높이가 다른 버튼에 그대로 쓰면 끝 장식이 찌그러진다.
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UISliceFitter : UIBehaviour
{
    private Image targetImage; // 조정할 이미지

    protected override void OnEnable()
    {
        base.OnEnable();
        Apply();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        Apply();
    }

    public void Apply() // 이미지 높이가 영역 높이와 같아지도록 배율을 맞춘다.
    {
        if (targetImage == null) { targetImage = GetComponent<Image>(); }
        if (targetImage == null || targetImage.sprite == null) { return; }

        float rectHeight = ((RectTransform)transform).rect.height;
        float pixelsPerUnit = targetImage.pixelsPerUnit;

        if (rectHeight <= 0.01f || pixelsPerUnit <= 0f) { return; }

        float multiplier = Mathf.Max(
            0.01f,
            targetImage.sprite.rect.height / (rectHeight * pixelsPerUnit)
        );

        if (Mathf.Approximately(targetImage.pixelsPerUnitMultiplier, multiplier))
        {
            return; // 이미 맞는 배율
        }

        targetImage.pixelsPerUnitMultiplier = multiplier;
    }
}
