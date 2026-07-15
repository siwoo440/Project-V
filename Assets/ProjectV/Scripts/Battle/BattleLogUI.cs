using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class BattleLogUI : MonoBehaviour // 전투 로그 UI 관리
{
    [SerializeField] private TMP_Text logText; // 전체 전투 로그 텍스트
    [SerializeField] private ScrollRect scrollRect; // 전투 로그 스크롤 영역
    [SerializeField] private int maximumLogCount = 50; // 최대 로그 보관 개수

    private readonly List<string> logEntries = new List<string>(); // 현재 전투 로그 목록

    private void Awake() // 전투 로그 초기화
    {
        Clear(); // 기존 로그 제거
    }

    public void Clear() // 전체 전투 로그 제거
    {
        StopAllCoroutines(); // 자동 스크롤 처리 중단
        logEntries.Clear(); // 로그 목록 초기화

        if (logText != null) { logText.text = string.Empty; } // 화면 로그 초기화
        if (scrollRect != null) { scrollRect.verticalNormalizedPosition = 1f; } // 스크롤 상단 이동
    }

    public void AddEntry(int turnNumber, BattleLogCategory category, string message) // 새로운 전투 로그 추가
    {
        if (string.IsNullOrWhiteSpace(message)) { return; } // 비어 있는 로그 차단

        int safeTurnNumber = Mathf.Max(0, turnNumber); // 안전한 턴 번호 계산
        string safeMessage = message.Replace("\n", " | "); // 여러 줄 로그 한 줄 변환
        string categoryName = GetCategoryName(category); // 로그 분류 이름 확인
        string categoryColor = GetCategoryColor(category); // 로그 분류 색상 확인
        string formattedEntry = $"<color=#{categoryColor}>[T{safeTurnNumber:00}][{categoryName}]</color> {safeMessage}"; // 최종 로그 문구 생성

        logEntries.Add(formattedEntry); // 로그 목록 등록

        while (logEntries.Count > Mathf.Max(1, maximumLogCount)) // 최대 로그 개수 초과 확인
        {
            logEntries.RemoveAt(0); // 가장 오래된 로그 제거
        }

        RebuildLogText(); // 전체 로그 텍스트 갱신
        StopAllCoroutines(); // 기존 자동 스크롤 중단
        StartCoroutine(ScrollToBottom()); // 최신 로그 위치 이동
    }

    private void RebuildLogText() // 전체 로그 텍스트 재생성
    {
        if (logText == null) { return; } // 로그 텍스트 누락 차단

        logText.text = string.Join("\n", logEntries); // 로그 항목 줄바꿈 결합
    }

    private IEnumerator ScrollToBottom() // 최신 로그 위치 자동 이동
    {
        yield return null; // UI 레이아웃 갱신 대기

        Canvas.ForceUpdateCanvases(); // Canvas 크기 즉시 갱신

        if (scrollRect != null) { scrollRect.verticalNormalizedPosition = 0f; } // 스크롤 최하단 이동
    }

    private string GetCategoryName(BattleLogCategory category) // 로그 분류 이름 반환
    {
        switch (category) // 로그 분류 확인
        {
            case BattleLogCategory.System: return "System"; // 시스템 분류 이름
            case BattleLogCategory.PlayerAction: return "Player"; // 플레이어 분류 이름
            case BattleLogCategory.HeroineAction: return "Heroine"; // 히로인 분류 이름
            case BattleLogCategory.StatusEffect: return "Status"; // 상태 효과 분류 이름
            default: return "Unknown"; // 미지원 분류 이름
        }
    }

    private string GetCategoryColor(BattleLogCategory category) // 로그 분류 색상 반환
    {
        switch (category) // 로그 분류 확인
        {
            case BattleLogCategory.System: return "FFD166"; // 시스템 노란색
            case BattleLogCategory.PlayerAction: return "59C3FF"; // 플레이어 파란색
            case BattleLogCategory.HeroineAction: return "FF7AA2"; // 히로인 분홍색
            case BattleLogCategory.StatusEffect: return "C77DFF"; // 상태 효과 보라색
            default: return "FFFFFF"; // 기본 흰색
        }
    }
}