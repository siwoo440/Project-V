using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
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

    [Header("Card UI")] // 카드 UI 구분
    [SerializeField] private Transform handPanel; // 손패 카드 배치 영역
    [SerializeField] private Button cardButtonPrefab; // 카드 버튼 프리팹

    [Header("Deck Settings")] // 덱 설정 구분
    [SerializeField] private List<CardData> deckCards = new List<CardData>(); // 전투 시작 덱 목록
    [SerializeField] private int startingHandCount = 3; // 시작 손패 수
    [SerializeField] private int turnDrawCount = 1; // 턴 시작 드로우 수

    [Header("Battle Settings")] // 전투 설정 구분
    [SerializeField] private int playerMaxHp = 30; // 플레이어 최대 체력
    [SerializeField] private int heroineMaxHp = 30; // 히로인 최대 체력
    [SerializeField] private int heroineAttackDamage = 3; // 히로인 임시 공격력
    [SerializeField] private float heroineActionDelay = 0.8f; // 히로인 행동 대기 시간

    private readonly List<CardData> drawPile = new List<CardData>(); // 드로우 더미
    private readonly List<CardData> discardPile = new List<CardData>(); // 버린 카드 더미
    private readonly List<Button> handButtons = new List<Button>(); // 현재 손패 버튼 목록
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
        drawPile.Clear(); // 드로우 더미 초기화
        discardPile.Clear(); // 버린 카드 더미 초기화
        ClearHand(); // 기존 손패 초기화
        drawPile.AddRange(deckCards); // 덱 카드 드로우 더미 등록
        DrawCards(startingHandCount); // 시작 손패 드로우
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
        SetHandInteractable(false); // 손패 버튼 비활성화
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
        resultText.text = string.Empty; // 안내 텍스트 초기화
        DrawCards(turnDrawCount); // 턴 시작 카드 드로우
        isPlayerTurn = true; // 플레이어 턴 설정
        ShowPlayerTurn(); // 플레이어 턴 표시
        UpdateBattleUI(); // 전체 UI 갱신
    } // 메서드 끝

    private void ShowPlayerTurn() // 플레이어 턴 UI 표시
    { // 메서드 시작
        turnText.text = "Player Turn"; // 플레이어 턴 문구 설정
        endTurnButton.interactable = true; // 턴 종료 버튼 활성화
        SetHandInteractable(true); // 손패 버튼 활성화
    } // 메서드 끝

    private void DrawCards(int drawCount) // 카드 드로우 처리
    { // 메서드 시작
        for (int i = 0; i < drawCount; i++) // 드로우 횟수 반복
        { // 반복문 시작
            RefillDrawPileIfNeeded(); // 드로우 더미 보충 확인

            if (drawPile.Count == 0) // 드로우 가능 카드 확인
            { // 조건문 시작
                return; // 카드 부족 상태 종료
            } // 조건문 끝

            CardData drawnCard = drawPile[0]; // 첫 번째 카드 선택
            drawPile.RemoveAt(0); // 드로우 더미 카드 제거
            CreateCardButton(drawnCard); // 손패 카드 버튼 생성
        } // 반복문 끝
    } // 메서드 끝

    private void RefillDrawPileIfNeeded() // 드로우 더미 재보충
    { // 메서드 시작
        if (drawPile.Count > 0 || discardPile.Count == 0) // 보충 불필요 조건
        { // 조건문 시작
            return; // 메서드 종료
        } // 조건문 끝

        drawPile.AddRange(discardPile); // 버린 카드 드로우 더미 이동
        discardPile.Clear(); // 버린 카드 더미 초기화
    } // 메서드 끝

    private void CreateCardButton(CardData cardData) // 손패 카드 버튼 생성
    { // 메서드 시작
        Button newCardButton = Instantiate(cardButtonPrefab, handPanel); // 카드 버튼 복제
        TMP_Text cardText = newCardButton.GetComponentInChildren<TMP_Text>(); // 카드 버튼 텍스트 검색

        cardText.text = $"{cardData.CardName}\nCost: {cardData.ManaCost}\nDamage: {cardData.Damage}"; // 카드 정보 표시
        newCardButton.onClick.RemoveAllListeners(); // 기존 버튼 연결 초기화
        newCardButton.onClick.AddListener(() => TryPlayCard(cardData, newCardButton)); // 카드 사용 함수 연결
        handButtons.Add(newCardButton); // 손패 버튼 목록 등록
    } // 메서드 끝

    private void TryPlayCard(CardData cardData, Button cardButton) // 카드 사용 처리
    { // 메서드 시작
        if (!isPlayerTurn || isBattleEnded) // 카드 사용 불가 상태
        { // 조건문 시작
            return; // 중복 사용 차단
        } // 조건문 끝

        if (currentMana < cardData.ManaCost) // 마나 부족 확인
        { // 조건문 시작
            resultText.text = "Not Enough Mana"; // 마나 부족 안내
            return; // 카드 사용 차단
        } // 조건문 끝

        currentMana -= cardData.ManaCost; // 카드 비용 차감
        heroineCurrentHp = Mathf.Max(0, heroineCurrentHp - cardData.Damage); // 히로인 피해 적용
        discardPile.Add(cardData); // 사용 카드 버린 더미 이동
        handButtons.Remove(cardButton); // 손패 버튼 목록 제거
        Destroy(cardButton.gameObject); // 카드 버튼 제거
        resultText.text = string.Empty; // 안내 텍스트 초기화
        UpdateBattleUI(); // 카드 사용 결과 표시

        if (heroineCurrentHp <= 0) // 히로인 사망 확인
        { // 조건문 시작
            EndBattle("Victory"); // 승리 처리
        } // 조건문 끝
    } // 메서드 끝

    private void SetHandInteractable(bool isInteractable) // 손패 버튼 활성 상태 변경
    { // 메서드 시작
        foreach (Button handButton in handButtons) // 모든 손패 버튼 반복
        { // 반복문 시작
            if (handButton != null) // 버튼 존재 확인
            { // 조건문 시작
                handButton.interactable = isInteractable; // 버튼 활성 상태 적용
            } // 조건문 끝
        } // 반복문 끝
    } // 메서드 끝

    private void ClearHand() // 손패 버튼 전체 제거
    { // 메서드 시작
        foreach (Button handButton in handButtons) // 모든 손패 버튼 반복
        { // 반복문 시작
            if (handButton != null) // 버튼 존재 확인
            { // 조건문 시작
                Destroy(handButton.gameObject); // 카드 버튼 제거
            } // 조건문 끝
        } // 반복문 끝

        handButtons.Clear(); // 손패 버튼 목록 초기화
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
        SetHandInteractable(false); // 손패 버튼 비활성화
    } // 메서드 끝
} // 클래스 끝