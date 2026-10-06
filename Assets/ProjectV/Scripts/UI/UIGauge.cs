using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 체력, 성욕, 마나 등을 막대로 표시한다. (기획서 11.8.1)
// 채움 영역의 오른쪽 끝을 비율만큼 옮겨, 둥근 끝 모양을 유지한 채 길이만 바꾼다.
public class UIGauge : MonoBehaviour
{
    [SerializeField] private RectTransform fillRect; // 채움 막대 영역
    [SerializeField] private Image fillImage;        // 채움 막대 이미지

    [SerializeField, Range(0f, 1f)]
    private float ratio = 1f; // 현재 채움 비율

    public float Ratio => ratio; // 현재 채움 비율 반환

    private void OnEnable()
    {
        Apply();
    }

    public void SetValue(int currentValue, int maximumValue) // 현재 값과 최대 값으로 설정
    {
        SetRatio(
            maximumValue <= 0
                ? 0f
                : (float)currentValue / maximumValue
        );
    }

    public void SetRatio(float newRatio) // 비율로 설정
    {
        ratio = Mathf.Clamp01(newRatio);
        Apply();
    }

    public void SetFillColor(Color fillColor) // 채움 색 변경
    {
        if (fillImage == null) { return; }

        fillImage.color = fillColor;
    }

    private void Apply()
    {
        if (fillRect == null) { return; }

        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(ratio, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        if (fillImage != null)
        {
            fillImage.enabled = ratio > 0.001f; // 비었을 때는 끝 모양도 숨긴다.
        }
    }
}
