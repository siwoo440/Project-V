using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능

public class StatusEffectTooltipUI : MonoBehaviour // 상태 효과 툴팁 UI 관리
{
    [SerializeField] private Canvas rootCanvas; // 최상위 UI Canvas
    [SerializeField] private RectTransform tooltipPanel; // 툴팁 패널 RectTransform
    [SerializeField] private TMP_Text statusNameText; // 상태 효과 이름 텍스트
    [SerializeField] private TMP_Text categoryText; // 버프 및 디버프 분류 텍스트
    [SerializeField] private TMP_Text effectText; // 상태 효과 설명 텍스트
    [SerializeField] private TMP_Text remainingTurnsText; // 남은 지속시간 텍스트
    [SerializeField] private Vector2 cursorOffset = new Vector2(18f, -18f); // 마우스 기준 툴팁 위치 보정

    private void Awake() // 툴팁 초기화
    {
        Hide(); // 게임 시작 시 툴팁 숨김
    }

    public void Show(StatusEffectData statusData, int remainingTurns, Vector2 screenPosition) // 상태 효과 툴팁 표시
    {
        if (statusData == null) { Hide(); return; } // 상태 데이터 누락 차단
        if (tooltipPanel == null) { return; } // 툴팁 패널 누락 차단

        tooltipPanel.gameObject.SetActive(true); // 툴팁 패널 활성화

        if (statusNameText != null) // 상태 이름 텍스트 확인
        {
            statusNameText.text = statusData.DisplayName; // 상태 효과 이름 표시
            statusNameText.color = statusData.DisplayColor; // 상태 효과 색상 적용
        }

        if (categoryText != null) { categoryText.text = statusData.IsNegative ? "Debuff" : "Buff"; } // 상태 분류 표시
        if (effectText != null) { effectText.text = CreateEffectDescription(statusData); } // 상태 효과 설명 표시
        if (remainingTurnsText != null) { remainingTurnsText.text = CreateDurationDescription(statusData, remainingTurns); } // 지속시간 표시

        Move(screenPosition); // 마우스 위치로 툴팁 이동
    }

    public void Move(Vector2 screenPosition) // 툴팁 마우스 위치 이동
    {
        if (rootCanvas == null || tooltipPanel == null) { return; } // 필수 UI 참조 누락 차단

        RectTransform canvasRect = rootCanvas.transform as RectTransform; // Canvas RectTransform 확인
        if (canvasRect == null) { return; } // Canvas RectTransform 누락 차단

        Camera uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera; // Canvas UI 카메라 확인

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, uiCamera, out Vector2 localPoint)) { return; } // 화면 좌표 변환 실패 차단

        Vector2 desiredPosition = localPoint + cursorOffset; // 마우스 위치와 간격 적용
        Vector2 panelSize = tooltipPanel.rect.size; // 툴팁 패널 크기 확인
        Rect canvasBounds = canvasRect.rect; // Canvas 표시 범위 확인
        Vector2 panelPivot = tooltipPanel.pivot; // 툴팁 패널 피벗 확인

        float minimumX = canvasBounds.xMin + panelSize.x * panelPivot.x; // 왼쪽 최소 위치 계산
        float maximumX = canvasBounds.xMax - panelSize.x * (1f - panelPivot.x); // 오른쪽 최대 위치 계산
        float minimumY = canvasBounds.yMin + panelSize.y * panelPivot.y; // 아래쪽 최소 위치 계산
        float maximumY = canvasBounds.yMax - panelSize.y * (1f - panelPivot.y); // 위쪽 최대 위치 계산

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minimumX, maximumX); // 좌우 화면 범위 제한
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minimumY, maximumY); // 상하 화면 범위 제한

        tooltipPanel.anchoredPosition = desiredPosition; // 보정된 툴팁 위치 적용
    }

    public void Hide() // 상태 효과 툴팁 숨김
    {
        if (tooltipPanel != null) { tooltipPanel.gameObject.SetActive(false); } // 툴팁 패널 비활성화
    }

    private string CreateEffectDescription(StatusEffectData statusData) // 상태 효과 설명 문구 생성
    {
        string effectValue = GetEffectValueText(statusData); // 상태 효과 수치 문구 생성

        if (!string.IsNullOrWhiteSpace(statusData.Description)) // 직접 작성한 설명 확인
        {
            return $"{statusData.Description}\nEffect: {effectValue}"; // 설명과 수치 표시
        }

        return $"Effect: {effectValue}"; // 기본 수치 설명 표시
    }

    private string GetEffectValueText(StatusEffectData statusData) // 상태 효과 수치 문구 생성
    {
        if (statusData == null) { return "None"; } // 상태 데이터 누락 처리

        switch (statusData.StatusType) // 상태 효과 종류 확인
        {
            case StatusEffectType.DefenseUp: return $"Defense +{statusData.Amount}"; // 방어력 증가 수치
            case StatusEffectType.AttackDown: return $"Attack -{statusData.Amount}"; // 공격력 감소 수치
            case StatusEffectType.Poison: return $"HP -{statusData.Amount} per action"; // 독 피해 수치
            default: return statusData.Amount.ToString(); // 기본 상태 효과 수치
        }
    }

    private string CreateDurationDescription(StatusEffectData statusData, int remainingTurns) // 상태 효과 지속시간 문구 생성
    {
        int safeRemainingTurns = Mathf.Max(0, remainingTurns); // 안전한 남은 지속시간 계산

        switch (statusData.DurationTiming) // 지속시간 감소 시점 확인
        {
            case StatusDurationTiming.AfterPlayerTurn: return $"Remaining: {safeRemainingTurns} Player Turns"; // 플레이어 턴 기준 표시
            case StatusDurationTiming.AfterHeroineTurn: return $"Remaining: {safeRemainingTurns} Heroine Actions"; // 히로인 행동 기준 표시
            default: return $"Remaining: {safeRemainingTurns}"; // 기본 지속시간 표시
        }
    }
}