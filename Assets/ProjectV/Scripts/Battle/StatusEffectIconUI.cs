using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.EventSystems; // 포인터 이벤트 기능
using UnityEngine.UI; // Unity UI 기능

public class StatusEffectIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler // 상태 효과 아이콘 UI 관리
{
    [SerializeField] private Image backgroundImage; // 아이콘 배경 이미지
    [SerializeField] private Image iconImage; // 상태 효과 아이콘 이미지
    [SerializeField] private TMP_Text symbolText; // 아이콘 대체 기호 텍스트
    [SerializeField] private TMP_Text durationText; // 남은 지속시간 텍스트

    private StatusEffectData currentStatusData; // 현재 표시 상태 효과 데이터
    private int currentRemainingTurns; // 현재 남은 지속시간
    private StatusEffectTooltipUI tooltipUI; // 상태 효과 툴팁 UI

    public void Setup(StatusEffectData statusData, int remainingTurns, StatusEffectTooltipUI targetTooltipUI) // 상태 효과 아이콘 설정
    {
        if (statusData == null) { gameObject.SetActive(false); return; } // 상태 데이터 누락 차단

        currentStatusData = statusData; // 현재 상태 효과 데이터 저장
        currentRemainingTurns = Mathf.Max(0, remainingTurns); // 현재 지속시간 저장
        tooltipUI = targetTooltipUI; // 툴팁 UI 참조 저장

        gameObject.SetActive(true); // 상태 효과 아이콘 활성화

        if (backgroundImage != null) { backgroundImage.color = statusData.DisplayColor; } // 상태 색상 적용
        if (durationText != null) { durationText.text = currentRemainingTurns.ToString(); } // 남은 지속시간 표시

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

    public void OnPointerEnter(PointerEventData eventData) // 마우스 진입 처리
    {
        if (tooltipUI == null || currentStatusData == null) { return; } // 툴팁 표시 불가 상태 차단

        tooltipUI.Show(currentStatusData, currentRemainingTurns, eventData.position); // 상태 효과 툴팁 표시
    }

    public void OnPointerMove(PointerEventData eventData) // 마우스 이동 처리
    {
        if (tooltipUI == null) { return; } // 툴팁 누락 차단

        tooltipUI.Move(eventData.position); // 마우스 위치로 툴팁 이동
    }

    public void OnPointerExit(PointerEventData eventData) // 마우스 이탈 처리
    {
        if (tooltipUI != null) { tooltipUI.Hide(); } // 상태 효과 툴팁 숨김
    }

    private void OnDisable() // 아이콘 비활성화 처리
    {
        if (tooltipUI != null) { tooltipUI.Hide(); } // 남아 있는 툴팁 숨김
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