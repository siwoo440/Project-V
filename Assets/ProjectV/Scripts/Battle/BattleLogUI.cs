using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class BattleLogUI : MonoBehaviour // 전투 로그 UI 관리
{
    [Header("Panel")] // 전투 로그 패널 설정
    [SerializeField] private GameObject logPanel; // 전투 로그 패널 오브젝트
    [SerializeField] private GameObject openButtonObject; // 전투 로그 열기 버튼 오브젝트
    [SerializeField] private bool startOpened = true; // 게임 시작 시 패널 열림 여부

    [Header("Log UI")] // 전투 로그 표시 설정
    [SerializeField] private TMP_Text logText; // 전체 전투 로그 텍스트
    [SerializeField] private ScrollRect scrollRect; // 전투 로그 스크롤 영역
    [SerializeField] private TMP_Text filterNameText; // 현재 필터 이름 텍스트
    [SerializeField] private int maximumLogCount = 50; // 최대 로그 보관 개수

    private readonly List<BattleLogEntry> logEntries = new List<BattleLogEntry>(); // 전체 전투 로그 목록
    private BattleLogFilter currentFilter = BattleLogFilter.All; // 현재 로그 표시 필터

    private void Awake() // 전투 로그 초기화
    {
        Clear(); // 기존 로그 제거
    }

    private void Start() // 전투 로그 패널 초기 상태 적용
    {
        if (startOpened) { OpenPanel(); } // 시작 시 패널 열기
        else { ClosePanel(); } // 시작 시 패널 닫기
    }

    public void Clear() // 전체 전투 로그 제거
    {
        StopAllCoroutines(); // 자동 스크롤 처리 중단
        logEntries.Clear(); // 로그 목록 초기화
        currentFilter = BattleLogFilter.All; // 전체 필터 초기화
        RebuildLogText(); // 로그 화면 갱신
        UpdateFilterNameText(); // 필터 이름 갱신

        if (scrollRect != null) { scrollRect.verticalNormalizedPosition = 1f; } // 스크롤 상단 이동
    }

    public void AddEntry(int turnNumber, BattleLogCategory category, string message) // 새로운 전투 로그 추가
    {
        if (string.IsNullOrWhiteSpace(message)) { return; } // 비어 있는 로그 차단

        int safeTurnNumber = Mathf.Max(0, turnNumber); // 안전한 턴 번호 계산
        string safeMessage = message.Replace("\n", " | "); // 여러 줄 메시지 한 줄 변환
        BattleLogEntry newEntry = new BattleLogEntry(safeTurnNumber, category, safeMessage); // 신규 로그 항목 생성

        logEntries.Add(newEntry); // 전체 로그 목록 등록

        while (logEntries.Count > Mathf.Max(1, maximumLogCount)) // 최대 로그 개수 초과 확인
        {
            logEntries.RemoveAt(0); // 가장 오래된 로그 제거
        }

        RebuildLogText(); // 현재 필터 기준 로그 갱신
        RequestScrollToBottom(); // 최신 로그 위치 이동
    }

    public void OpenPanel() // 전투 로그 패널 열기
    {
        if (logPanel != null) { logPanel.SetActive(true); } // 전투 로그 패널 활성화
        if (openButtonObject != null) { openButtonObject.SetActive(false); } // 열기 버튼 숨김

        RebuildLogText(); // 현재 필터 기준 화면 갱신
        UpdateFilterNameText(); // 현재 필터 이름 갱신
        RequestScrollToBottom(); // 최신 로그 위치 이동
    }

    public void ClosePanel() // 전투 로그 패널 닫기
    {
        if (logPanel != null) { logPanel.SetActive(false); } // 전투 로그 패널 비활성화
        if (openButtonObject != null) { openButtonObject.SetActive(true); } // 열기 버튼 표시
    }

    public void TogglePanel() // 전투 로그 패널 열림 상태 전환
    {
        if (logPanel == null) { return; } // 전투 로그 패널 누락 차단

        if (logPanel.activeSelf) { ClosePanel(); } // 열린 패널 닫기
        else { OpenPanel(); } // 닫힌 패널 열기
    }

    public void ShowAllLogs() // 전체 로그 필터 적용
    {
        SetFilter(BattleLogFilter.All); // 전체 필터 설정
    }

    public void ShowSystemLogs() // 시스템 로그 필터 적용
    {
        SetFilter(BattleLogFilter.System); // 시스템 필터 설정
    }

    public void ShowPlayerLogs() // 플레이어 로그 필터 적용
    {
        SetFilter(BattleLogFilter.PlayerAction); // 플레이어 필터 설정
    }

    public void ShowHeroineLogs() // 히로인 로그 필터 적용
    {
        SetFilter(BattleLogFilter.HeroineAction); // 히로인 필터 설정
    }

    public void ShowStatusLogs() // 상태 효과 로그 필터 적용
    {
        SetFilter(BattleLogFilter.StatusEffect); // 상태 효과 필터 설정
    }

    private void SetFilter(BattleLogFilter filter) // 전투 로그 필터 변경
    {
        currentFilter = filter; // 현재 필터 저장
        RebuildLogText(); // 필터 기준 로그 갱신
        UpdateFilterNameText(); // 필터 이름 갱신
        RequestScrollToTop(); ; // 필터 결과 최상단 이동
    }
    private void RequestScrollToTop()
    {
        if (!gameObject.activeInHierarchy) { return; }
        if (logPanel != null && !logPanel.activeInHierarchy) { return; }

        StopAllCoroutines();
        StartCoroutine(ScrollToTop());
    }
    private void RebuildLogText() // 필터 기준 로그 텍스트 재생성
    {
        if (logText == null) { return; } // 로그 텍스트 누락 차단

        List<string> visibleLogEntries = new List<string>(); // 표시 가능한 로그 문구 목록

        foreach (BattleLogEntry logEntry in logEntries) // 전체 로그 반복
        {
            if (logEntry == null) { continue; } // 누락 로그 제외
            if (!ShouldDisplayEntry(logEntry)) { continue; } // 현재 필터와 다른 로그 제외

            visibleLogEntries.Add(FormatLogEntry(logEntry)); // 표시 로그 문구 등록
        }

        logText.text = visibleLogEntries.Count > 0
            ? string.Join("\n", visibleLogEntries) // 필터 결과 로그 표시
            : "<color=#AAAAAA>No matching logs.</color>"; // 필터 결과 없음 표시
    }

    private bool ShouldDisplayEntry(BattleLogEntry logEntry) // 현재 필터의 로그 표시 여부 확인
    {
        if (currentFilter == BattleLogFilter.All) { return true; } // 전체 필터 모든 로그 허용
        if (currentFilter == BattleLogFilter.System) { return logEntry.Category == BattleLogCategory.System; } // 시스템 로그 확인
        if (currentFilter == BattleLogFilter.PlayerAction) { return logEntry.Category == BattleLogCategory.PlayerAction; } // 플레이어 로그 확인
        if (currentFilter == BattleLogFilter.HeroineAction) { return logEntry.Category == BattleLogCategory.HeroineAction; } // 히로인 로그 확인
        if (currentFilter == BattleLogFilter.StatusEffect) { return logEntry.Category == BattleLogCategory.StatusEffect; } // 상태 효과 로그 확인

        return false; // 미지원 필터 로그 제외
    }

    private string FormatLogEntry(BattleLogEntry logEntry) // 전투 로그 화면 문구 생성
    {
        string categoryName = GetCategoryName(logEntry.Category); // 로그 분류 이름 확인
        string categoryColor = GetCategoryColor(logEntry.Category); // 로그 분류 색상 확인

        return $"<color=#{categoryColor}>[T{logEntry.TurnNumber:00}][{categoryName}]</color> {logEntry.Message}"; // 최종 로그 문구 반환
    }

    private void UpdateFilterNameText() // 현재 필터 이름 표시
    {
        if (filterNameText == null) { return; } // 필터 이름 텍스트 누락 차단

        filterNameText.text = $"Filter: {GetFilterName(currentFilter)}"; // 현재 필터 이름 적용
    }

    private void RequestScrollToBottom() // 최신 로그 자동 스크롤 요청
    {
        if (!gameObject.activeInHierarchy) { return; } // 로그 관리 오브젝트 비활성 상태 차단
        if (logPanel != null && !logPanel.activeInHierarchy) { return; } // 닫힌 로그 패널 스크롤 차단

        StopAllCoroutines(); // 기존 자동 스크롤 중단
        StartCoroutine(ScrollToBottom()); // 최신 로그 위치 이동
    }
    private IEnumerator ScrollToTop()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
    private IEnumerator ScrollToBottom() // 최신 로그 위치 자동 이동
    {
        yield return null; // UI 레이아웃 갱신 대기

        Canvas.ForceUpdateCanvases(); // Canvas 크기 즉시 갱신

        if (scrollRect != null) { scrollRect.verticalNormalizedPosition = 0f; } // 스크롤 최하단 이동
    }

    private string GetFilterName(BattleLogFilter filter) // 로그 필터 표시 이름 반환
    {
        switch (filter) // 로그 필터 확인
        {
            case BattleLogFilter.All: return "All"; // 전체 필터 이름
            case BattleLogFilter.System: return "System"; // 시스템 필터 이름
            case BattleLogFilter.PlayerAction: return "Player"; // 플레이어 필터 이름
            case BattleLogFilter.HeroineAction: return "Heroine"; // 히로인 필터 이름
            case BattleLogFilter.StatusEffect: return "Status"; // 상태 효과 필터 이름
            default: return "Unknown"; // 미지원 필터 이름
        }
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