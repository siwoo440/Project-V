using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class StatusEffectIconUI : MonoBehaviour // 상태 효과 아이콘 UI 관리
{
    [SerializeField] private Image backgroundImage; // 아이콘 배경 이미지
    [SerializeField] private Image iconImage; // 상태 효과 아이콘 이미지
    [SerializeField] private TMP_Text symbolText; // 아이콘 대체 기호 텍스트
    [SerializeField] private TMP_Text durationText; // 남은 지속시간 텍스트

    public void Setup(StatusEffectData statusData, int remainingTurns) // 상태 효과 아이콘 설정
    {
        if (statusData == null) { gameObject.SetActive(false); return; } // 상태 데이터 누락 차단

        gameObject.SetActive(true); // 상태 효과 아이콘 활성화

        if (backgroundImage != null) { backgroundImage.color = statusData.DisplayColor; } // 상태 색상 적용
        if (durationText != null) { durationText.text = Mathf.Max(0, remainingTurns).ToString(); } // 남은 지속시간 표시

        bool hasIcon = statusData.Icon != null; // 실제 아이콘 존재 여부 확인

        if (iconImage != null) // 아이콘 이미지 확인
        {
            iconImage.enabled = hasIcon; // 실제 아이콘 존재 시 이미지 활성화
            iconImage.sprite = statusData.Icon; // 상태 효과 아이콘 적용
            iconImage.color = Color.white; // 아이콘 기본 색상 적용
        }

        if (symbolText != null) // 대체 기호 텍스트 확인
        {
            symbolText.gameObject.SetActive(!hasIcon); // 아이콘이 없을 때만 기호 표시
            symbolText.text = GetFallbackSymbol(statusData.StatusType); // 상태 종류별 기호 설정
        }
    }

    private string GetFallbackSymbol(StatusEffectType statusType) // 상태 효과 대체 기호 반환
    {
        switch (statusType) // 상태 효과 종류 확인
        {
            case StatusEffectType.DefenseUp: return "D+"; // 방어력 증가 기호
            case StatusEffectType.AttackDown: return "A-"; // 공격력 감소 기호
            case StatusEffectType.Poison: return "P"; // 독 기호
            default: return "?"; // 미지원 상태 효과 기호
        }
    }
}