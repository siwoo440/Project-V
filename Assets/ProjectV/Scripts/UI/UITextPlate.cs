using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 글자 뒤에 받침 이미지를 깔고, 글자가 비어 있으면 받침도 숨긴다.
// 배경 그림 위에 바로 놓인 안내 문구가 읽히도록 하는 데 쓴다.
public class UITextPlate : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText; // 살펴볼 글자
    [SerializeField] private Image plateImage;    // 받침 이미지

    private void LateUpdate()
    {
        if (targetText == null || plateImage == null) { return; }

        bool hasText =
            targetText.gameObject.activeInHierarchy &&
            !string.IsNullOrEmpty(targetText.text);

        if (plateImage.enabled != hasText)
        {
            plateImage.enabled = hasText;
        }
    }
}
