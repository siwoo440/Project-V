using System.Collections; // 코루틴 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class BattleManager : MonoBehaviour // 기본 전투 흐름 관리
{ // 클래스 시작
    [Header("Battle UI")] // 전투 UI 구분
    [SerializeField] private TMP_Text turnText; // 턴 상태 텍스트
    [SerializeField] private TMP_Text turnNumberText; // 턴 번호 텍스트
    [SerializeField] private TMP_Text playerHpText; // 플레이어 체력 텍스트
    [SerializeField] private TMP_Text manaText; // 마나 텍스트
    [SerializeField] private TMP_Text heroineHpText; // 히로인 체력 텍스트
    [SerializeField] private TMP_Text lustText; // 성욕 게이지 텍스트
    [SerializeField] private TMP_Text resultText; // 전투 결과 텍스트
    [SerializeField] private Button endTurnButton; // 턴 종료 버튼

    [Header("Battle Settings")] // 전투 설정 구분
    [SerializeField] private int playerMaxHp = 30; // 플레이어 최대 체력
    [SerializeField] private int heroineMaxHp = 30; // 히로인 최대 체력
    [SerializeField] private int heroineAttackDamage = 3; // 히로인 임시 공격력
    [SerializeField] private float heroineActionDelay = 0.8f; // 히로인 행동 대기 시간

    private int playerCurrentHp; // 플레이어 현재 체력
    private int heroineCurrentHp; // 히로인 현재 체력
    private int heroineLust; // 히로인 현재 성욕
    private int turnNumber; // 현재 턴 번호
    private int maximumMana; // 현재 최대 마나
    private int currentMana; // 현재 사용 가능 마나
    private bool isPlayerTurn; // 플레이어 턴 여부
    private bool isBattleEnded; // 전투 종료 여부

    private void Start() // 전투 초기화 진입
    { // 메서드 시작
        InitializeBattle(); // 기본 전투 초기화
    } // 메서드 끝

    private void InitializeBattle() // 전투 기본값 설정
    { // 메서드 시작
        playerCurrentHp = playerMaxHp; // 플레이어 체력 초기화
        heroineCurrentHp = heroineMaxHp; // 히로인 체력 초기화
        heroineLust = 0; // 성욕 게이지 초기화
        turnNumber = 1; // 첫 번째 턴 설정
        maximumMana = 1; // 첫 턴 최대 마나 설정
        currentMana = maximumMana; // 현재 마나 충전
        isPlayerTurn = true; // 플레이어 턴 설정
        isBattleEnded = false; // 전투 진행 상태 설정
        resultText.text = string.Empty; // 결과 텍스트 초기화
        ShowPlayerTurn(); // 플레이어 턴 표시
        UpdateBattleUI(); // 전체 UI 갱신
    } // 메서드 끝

    public void EndPlayerTurn() // 플레이어 턴 종료
    { // 메서드 시작
        if (!isPlayerTurn || isBattleEnded) // 턴 종료 불가 조건
        { // 조건문 시작
            return; // 중복 실행 차단
        } // 조건문 끝

        isPlayerTurn = false; // 플레이어 턴 종료
        endTurnButton.interactable = false; // 턴 종료 버튼 비활성화
        turnText.text = "Heroine Turn"; // 히로인 턴 표시
        StartCoroutine(HeroineTurnRoutine()); // 히로인 행동 시작
    } // 메서드 끝

    private IEnumerator HeroineTurnRoutine() // 히로인 턴 순차 처리
    { // 코루틴 시작
        yield return new WaitForSeconds(heroineActionDelay); // 공격 전 대기

        playerCurrentHp = Mathf.Max(0, playerCurrentHp - heroineAttackDamage); // 플레이어 피해 적용
        UpdateBattleUI(); // 피해 결과 표시

        if (playerCurrentHp <= 0) // 플레이어 사망 확인
        { // 조건문 시작
            EndBattle("Defeat"); // 패배 처리
            yield break; // 코루틴 종료
        } // 조건문 끝

        yield return new WaitForSeconds(heroineActionDelay); // 다음 턴 전 대기
        BeginNextPlayerTurn(); // 다음 플레이어 턴 시작
    } // 코루틴 끝

    private void BeginNextPlayerTurn() // 다음 플레이어 턴 준비
    { // 메서드 시작
        turnNumber += 1; // 턴 번호 증가
        maximumMana = Mathf.Min(10, maximumMana + 1); // 최대 마나 증가
        currentMana = maximumMana; // 마나 전체 회복
        isPlayerTurn = true; // 플레이어 턴 설정
        ShowPlayerTurn(); // 플레이어 턴 표시
        UpdateBattleUI(); // 전체 UI 갱신
    } // 메서드 끝

    private void ShowPlayerTurn() // 플레이어 턴 UI 표시
    { // 메서드 시작
        turnText.text = "Player Turn"; // 플레이어 턴 문구 설정
        endTurnButton.interactable = true; // 턴 종료 버튼 활성화
    } // 메서드 끝

    private void UpdateBattleUI() // 전투 수치 UI 갱신
    { // 메서드 시작
        turnNumberText.text = $"Turn {turnNumber}"; // 턴 번호 표시
        playerHpText.text = $"Player HP: {playerCurrentHp} / {playerMaxHp}"; // 플레이어 체력 표시
        manaText.text = $"Mana: {currentMana} / {maximumMana}"; // 마나 표시
        heroineHpText.text = $"Heroine HP: {heroineCurrentHp} / {heroineMaxHp}"; // 히로인 체력 표시
        lustText.text = $"Lust: {heroineLust} / 100"; // 성욕 게이지 표시
    } // 메서드 끝

    private void EndBattle(string resultMessage) // 전투 종료 처리
    { // 메서드 시작
        isBattleEnded = true; // 전투 종료 상태 설정
        isPlayerTurn = false; // 플레이어 턴 해제
        turnText.text = "Battle End"; // 전투 종료 문구 설정
        resultText.text = resultMessage; // 전투 결과 표시
        endTurnButton.interactable = false; // 턴 종료 버튼 비활성화
    } // 메서드 끝
} // 클래스 끝
