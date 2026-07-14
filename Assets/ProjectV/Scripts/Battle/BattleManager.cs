using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class BattleManager : MonoBehaviour // 기본 전투 흐름 관리
{ // 클래스 시작
    [Header("Battle UI")] // 전투 UI 구분
    [SerializeField] private TMP_Text turnText;             // 턴 상태 텍스트
    [SerializeField] private TMP_Text turnNumberText;       // 턴 번호 텍스트
    [SerializeField] private TMP_Text playerHpText;         // 플레이어 체력 텍스트
    [SerializeField] private TMP_Text playerShieldText;     // 플레이어 보호막 텍스트
    [SerializeField] private TMP_Text manaText;             // 마나 텍스트

    [SerializeField] private TMP_Text heroineHpText;        // 히로인 체력 텍스트
    [SerializeField] private TMP_Text heroineDefenseText;   // 히로인 방어력 텍스트
    [SerializeField] private TMP_Text heroineShieldText;    // 히로인 보호막 텍스트
    [SerializeField] private TMP_Text lustText;             // 성욕 게이지 텍스트

    [SerializeField] private TMP_Text heroineIntentText;    // 히로인 행동 예고 텍스트
    [SerializeField] private TMP_Text resultText;           // 전투 결과 텍스트
    [SerializeField] private Button endTurnButton;          // 턴 종료 버튼
    [SerializeField] private Button monsterAttackButton;    // 마물 공격 버튼

    [Header("Card UI")] // 카드 UI 구분
    [SerializeField] private Transform handPanel;       // 손패 카드 배치 영역
    [SerializeField] private Button cardButtonPrefab;   // 카드 버튼 프리팹

    [Header("Monster Field")] // 마물 필드 구분
    [SerializeField] private Transform monsterFieldContainer;// 마물 배치 영역
    [SerializeField] private MonsterUnit monsterUnitPrefab;  // 마물 UI 프리팹
    [SerializeField] private int maxFieldMonsterCount = 8;   // 최대 필드 마물 수

    [Header("Deck Settings")] // 덱 설정 구분
    [SerializeField] private List<CardData> deckCards = new List<CardData>(); // 전투 시작 덱 목록
    [SerializeField] private int startingHandCount = 3; // 시작 손패 수
    [SerializeField] private int turnDrawCount = 1;     // 턴 시작 드로우 수
   
    [Header("Heroine AI")] // 히로인 AI 구분
    [SerializeField] private List<HeroineActionData> heroineActions = new List<HeroineActionData>(); // 히로인 행동 데이터 목록

    [Header("Battle Settings")] // 전투 설정 구분
    [SerializeField] private int playerMaxHp = 30;              // 플레이어 최대 체력
    [SerializeField] private int playerDefense = 0;             // 플레이어 방어력
    [SerializeField] private int playerStartingShield = 2;      // 플레이어 시작 보호막
    [SerializeField] private int heroineMaxHp = 30;             // 히로인 최대 체력
    [SerializeField] private int heroineDefense = 1;            // 히로인 방어력
    [SerializeField] private int heroineStartingShield = 3;     // 히로인 시작 보호막
    [SerializeField] private int heroineMaxShield = 10;         // 히로인 최대 보호막
    [SerializeField] private float heroineActionDelay = 0.8f;   // 히로인 행동 대기 시간


    private readonly List<CardData> drawPile = new List<CardData>();            // 드로우 더미
    private readonly List<CardData> discardPile = new List<CardData>();         // 버린 카드 더미
    private readonly List<Button> handButtons = new List<Button>();             // 현재 손패 버튼 목록
    private readonly List<MonsterUnit> fieldMonsters = new List<MonsterUnit>(); // 현재 필드 마물 목록
    private readonly Dictionary<HeroineActionData, int> heroineActionCooldowns  = new Dictionary<HeroineActionData, int>(); // 행동별 남은 쿨타임

    private MonsterUnit selectedMonster; // 현재 선택 마물

    private HeroineActionData nextHeroineAction; // 다음 히로인 행동 데이터
    private HeroineActionData lastHeroineAction; // 마지막으로 실행한 히로인 행동

    private int consecutiveHeroineActionUses;   // 같은 행동 연속 사용 횟수
    private int playerCurrentHp;                // 플레이어 현재 체력
    private int playerCurrentShield;            // 플레이어 현재 보호막
    private int heroineCurrentShield;           // 히로인 현재 보호막
    private int heroineCurrentHp;               // 히로인 현재 체력
    private int heroineLust;                    // 히로인 현재 성욕
    private int turnNumber;                     // 현재 턴 번호
    private int maximumMana;                    // 현재 최대 마나
    private int currentMana;                    // 현재 사용 가능 마나
    private bool isPlayerTurn;                  // 플레이어 턴 여부
    private bool isBattleEnded;                 // 전투 종료 여부

    private void Start() // 전투 초기화 진입
    { // 메서드 시작
        InitializeBattle(); // 기본 전투 초기화
    } // 메서드 끝

    private void InitializeBattle() // 전투 기본값 설정
    { // 메서드 시작
        playerCurrentHp = playerMaxHp; // 플레이어 체력 초기화
        playerCurrentShield = Mathf.Max(0, playerStartingShield); // 플레이어 보호막 초기화
        heroineCurrentHp = heroineMaxHp; // 히로인 체력 초기화
        heroineCurrentShield = Mathf.Clamp(heroineStartingShield, 0, heroineMaxShield); // 최대치 범위 내 보호막 초기화

        heroineLust = 0; // 성욕 게이지 초기화
        turnNumber = 1; // 첫 번째 턴 설정
        maximumMana = 1; // 첫 턴 최대 마나 설정
        currentMana = maximumMana; // 현재 마나 충전
        isPlayerTurn = true; // 플레이어 턴 설정
        isBattleEnded = false; // 전투 진행 상태 설정
        heroineActionCooldowns.Clear(); // 행동 쿨타임 초기화
        lastHeroineAction = null; // 마지막 행동 초기화
        consecutiveHeroineActionUses = 0; // 연속 사용 횟수 초기화
        SelectNextHeroineAction(); // 첫 번째 히로인 행동 선택
        selectedMonster = null; // 선택 마물 초기화
        resultText.text = string.Empty; // 결과 텍스트 초기화
        monsterAttackButton.interactable = false; // 공격 버튼 비활성화
        drawPile.Clear(); // 드로우 더미 초기화
        discardPile.Clear(); // 버린 카드 더미 초기화
        ClearHand(); // 기존 손패 초기화
        ClearMonsterField(); // 기존 마물 필드 초기화
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
        ClearMonsterSelection(); // 마물 선택 상태 해제
        endTurnButton.interactable = false; // 턴 종료 버튼 비활성화
        monsterAttackButton.interactable = false; // 공격 버튼 비활성화
        SetHandInteractable(false); // 손패 버튼 비활성화
        SetMonsterInteractable(false); // 마물 선택 비활성화
        turnText.text = "Heroine Turn"; // 히로인 턴 표시
        StartCoroutine(HeroineTurnRoutine()); // 히로인 행동 시작
    } // 메서드 끝

    public void AttackWithSelectedMonster() // 선택 마물 공격 처리
    { // 메서드 시작
        if (!isPlayerTurn || isBattleEnded) // 공격 불가 전투 상태
        { // 조건문 시작
            return; // 공격 차단
        } // 조건문 끝

        if (selectedMonster == null || !selectedMonster.CanAttack) // 선택 마물 상태 확인
        { // 조건문 시작
            resultText.text = "Select Ready Monster"; // 마물 선택 안내
            return; // 공격 차단
        } // 조건문 끝

        MonsterUnit attackingMonster = selectedMonster; // 공격 마물 저장
        string attackerName = attackingMonster.MonsterName; // 공격 마물 이름 저장
        int attackPower = attackingMonster.Attack; // 마물 공격력 저장
        DamageResult damageResult = DamageCalculator.CalculateDamageWithShield(attackPower, heroineDefense, heroineCurrentShield); // 히로인 보호막 포함 피해 계산

        heroineCurrentShield = damageResult.RemainingShield; // 히로인 남은 보호막 적용
        heroineCurrentHp = Mathf.Max(0, heroineCurrentHp - damageResult.HpDamage); // 히로인 실제 HP 피해 적용
        attackingMonster.MarkActed(); // 마물 행동 완료 처리
        ClearMonsterSelection(); // 마물 선택 해제
        resultText.text = CreateDamageResultText(attackerName, damageResult); // 마물 공격 결과 표시
        UpdateBattleUI(); // 전투 UI 갱신

        if (heroineCurrentHp <= 0) // 히로인 사망 확인
        { // 조건문 시작
            EndBattle("Victory"); // 승리 처리
        } // 조건문 끝
    } // 메서드 끝

    private IEnumerator HeroineTurnRoutine() // 히로인 턴 순차 처리
    { // 코루틴 시작
        yield return new WaitForSeconds(heroineActionDelay); // 공격 전 대기

        HeroineActionData executedAction = nextHeroineAction; // 이번 실행 행동 저장
        ResolveHeroineAttack(); // 히로인 공격 실행
        RegisterHeroineActionUse(executedAction); // 실행 행동과 쿨타임 기록
        UpdateBattleUI(); // 히로인 공격 결과 갱신

        if (playerCurrentHp <= 0) // 플레이어 사망 확인
        { // 조건문 시작
            EndBattle("Defeat"); // 패배 처리
            yield break; // 코루틴 종료
        } // 조건문 끝

        yield return new WaitForSeconds(heroineActionDelay); // 다음 턴 전 대기
        BeginNextPlayerTurn(); // 다음 플레이어 턴 시작
    } // 코루틴 끝

    private void ResolveHeroineAttack() // 선택된 히로인 행동 실행
    {
        if (nextHeroineAction == null)
        {
            resultText.text = "No Available Heroine Action"; // 행동 누락 안내
            return;
        }

        if (nextHeroineAction.ActionType == HeroineActionType.GainShield)
        {
            ExecuteGainShieldAction(); // 보호막 획득 행동 실행
            return;
        }

        fieldMonsters.RemoveAll(monsterUnit => monsterUnit == null); // 삭제된 마물 참조 정리

        switch (nextHeroineAction.TargetType) // 행동 대상 규칙 확인
        {
            case HeroineTargetType.FirstMonster: // 첫 번째 마물 대상
                AttackTargetMonster(GetFirstMonster()); // 첫 번째 마물 공격
                break;

            case HeroineTargetType.RandomMonster: // 무작위 마물 대상
                AttackTargetMonster(GetRandomMonster()); // 무작위 마물 공격
                break;

            case HeroineTargetType.LowestHpMonster: // 최저 HP 마물 대상
                AttackTargetMonster(GetLowestHpMonster()); // 최저 HP 마물 공격
                break;

            case HeroineTargetType.AllMonsters: // 모든 마물 대상
                ExecuteAreaAttack(); // 전체 마물 공격
                break;

            case HeroineTargetType.Player: // 플레이어 직접 대상
                AttackPlayer(); // 플레이어 직접 공격
                break;

            case HeroineTargetType.Self: // 히로인 자신 대상
                resultText.text = "Invalid Self Target Action"; // 잘못된 자기 대상 안내
                break;

            default: // 정의되지 않은 대상
                resultText.text = "Unknown Target Type"; // 대상 규칙 오류 안내
                break;
        }
    }
    private void ExecuteGainShieldAction() // 히로인 보호막 획득 행동 실행
    {
        int safeShieldAmount = Mathf.Max(0, nextHeroineAction.ShieldAmount); // 음수 보호막 획득 차단
        int previousShield = heroineCurrentShield; // 행동 전 보호막 저장
        heroineCurrentShield = Mathf.Min(heroineMaxShield, heroineCurrentShield + safeShieldAmount); // 최대치 범위 내 보호막 증가
        int gainedShield = heroineCurrentShield - previousShield; // 실제 보호막 획득량 계산

        resultText.text = $"{nextHeroineAction.DisplayName}: Shield +{gainedShield}"; // 보호막 행동 결과 표시
    }
    private MonsterUnit GetFirstMonster() // 첫 번째 마물 반환
    { // 메서드 시작
        if (fieldMonsters.Count == 0) // 필드 마물 부재 확인
        { // 조건문 시작
            return null; // 대상 없음 반환
        } // 조건문 끝

        return fieldMonsters[0]; // 첫 번째 마물 반환
    } // 메서드 끝

    private MonsterUnit GetRandomMonster() // 무작위 마물 반환
    { // 메서드 시작
        if (fieldMonsters.Count == 0) // 필드 마물 부재 확인
        { // 조건문 시작
            return null; // 대상 없음 반환
        } // 조건문 끝

        int randomIndex = Random.Range(0, fieldMonsters.Count); // 무작위 필드 인덱스 생성
        return fieldMonsters[randomIndex]; // 무작위 마물 반환
    } // 메서드 끝

    private MonsterUnit GetLowestHpMonster() // 현재 HP가 가장 낮은 마물 반환
    { // 메서드 시작
        if (fieldMonsters.Count == 0) // 필드 마물 부재 확인
        { // 조건문 시작
            return null; // 대상 없음 반환
        } // 조건문 끝

        MonsterUnit lowestHpMonster = fieldMonsters[0]; // 첫 번째 마물을 초기 대상으로 설정

        for (int i = 1; i < fieldMonsters.Count; i++) // 두 번째 마물부터 반복
        { // 반복문 시작
            MonsterUnit currentMonster = fieldMonsters[i]; // 현재 비교 마물 저장

            if (currentMonster.CurrentHp < lowestHpMonster.CurrentHp) // 더 낮은 HP 확인
            { // 조건문 시작
                lowestHpMonster = currentMonster; // 최저 HP 마물 교체
            } // 조건문 끝
        } // 반복문 끝

        return lowestHpMonster; // 최저 HP 마물 반환
    } // 메서드 끝



    private void ExecuteAreaAttack() // 히로인 광역 공격 실행
    { // 메서드 시작
        fieldMonsters.RemoveAll(monsterUnit => monsterUnit == null); // 삭제된 마물 참조 정리
        int attackPower = nextHeroineAction.Damage; // 행동 공격력 저장

        if (fieldMonsters.Count == 0) // 필드 마물 부재 확인
        { // 조건문 시작
            ApplyDamageToPlayer(attackPower, nextHeroineAction.DisplayName); // 플레이어 보호막 포함 피해 적용
            return; // 광역 공격 종료
        } // 조건문 끝

        int defeatedMonsterCount = 0; // 사망 마물 수 초기화
        int totalShieldAbsorbed = 0; // 전체 보호막 흡수량 초기화
        int totalHpDamage = 0; // 전체 HP 피해량 초기화

        for (int i = fieldMonsters.Count - 1; i >= 0; i--) // 필드 마물 역순 반복
        { // 반복문 시작
            MonsterUnit targetMonster = fieldMonsters[i]; // 현재 공격 대상 저장
            DamageResult damageResult = targetMonster.TakeDamage(attackPower); // 마물별 보호막 포함 피해 계산
            totalShieldAbsorbed += damageResult.ShieldAbsorbed; // 보호막 흡수량 합산
            totalHpDamage += damageResult.HpDamage; // HP 피해량 합산

            if (targetMonster.IsDead) // 마물 사망 확인
            { // 조건문 시작
                if (selectedMonster == targetMonster) // 선택 마물 사망 확인
                { // 조건문 시작
                    ClearMonsterSelection(); // 선택 상태 해제
                } // 조건문 끝

                fieldMonsters.RemoveAt(i); // 필드 목록에서 마물 제거
                Destroy(targetMonster.gameObject); // 마물 오브젝트 제거
                defeatedMonsterCount += 1; // 사망 마물 수 증가
            } // 조건문 끝
        } // 반복문 끝

        resultText.text = $"{nextHeroineAction.DisplayName}: Shield -{totalShieldAbsorbed}, HP -{totalHpDamage}, Defeated {defeatedMonsterCount}"; // 광역 공격 결과 표시
    } // 메서드 끝




    private void AttackTargetMonster(MonsterUnit targetMonster) // 선택된 마물 공격
    { // 메서드 시작
        if (targetMonster == null) // 공격 대상 마물 확인
        { // 조건문 시작
            AttackPlayer(); // 마물 부재 시 플레이어 공격
            return; // 마물 공격 종료
        } // 조건문 끝

        string targetName = targetMonster.MonsterName; // 공격 대상 이름 저장
        int attackPower = nextHeroineAction.Damage; // 행동 공격력 저장
        DamageResult damageResult = targetMonster.TakeDamage(attackPower); // 마물 보호막 포함 피해 처리

        resultText.text = CreateDamageResultText(targetName, damageResult); // 마물 피해 결과 표시

        if (targetMonster.IsDead) // 대상 마물 사망 확인
        { // 조건문 시작
            if (selectedMonster == targetMonster) // 선택 마물 사망 확인
            { // 조건문 시작
                ClearMonsterSelection(); // 선택 상태 해제
            } // 조건문 끝

            fieldMonsters.Remove(targetMonster); // 필드 목록에서 대상 마물 제거
            Destroy(targetMonster.gameObject); // 대상 마물 오브젝트 제거
            resultText.text = $"{nextHeroineAction.DisplayName}: {targetName} Defeated"; // 마물 사망 결과 표시
        } // 조건문 끝
    } // 메서드 끝






    private void AttackPlayer() // 플레이어 직접 공격
    { // 메서드 시작
        ApplyDamageToPlayer(nextHeroineAction.Damage, nextHeroineAction.DisplayName); // 플레이어 보호막 포함 피해 적용
    } // 메서드 끝



    private void ApplyDamageToPlayer(int attackPower, string actionName) // 플레이어 보호막 포함 피해 처리
    { // 메서드 시작
        DamageResult damageResult = DamageCalculator.CalculateDamageWithShield(attackPower, playerDefense, playerCurrentShield); // 플레이어 피해 계산
        playerCurrentShield = damageResult.RemainingShield; // 플레이어 남은 보호막 적용
        playerCurrentHp = Mathf.Max(0, playerCurrentHp - damageResult.HpDamage); // 플레이어 실제 HP 피해 적용
        resultText.text = $"{actionName}: {CreateDamageResultText("Player", damageResult)}"; // 플레이어 피해 결과 표시
    } // 메서드 끝


    private string CreateDamageResultText(string targetName, DamageResult damageResult) // 피해 결과 문구 생성
    { // 메서드 시작
        return $"{targetName}: Shield -{damageResult.ShieldAbsorbed}, HP -{damageResult.HpDamage}"; // 보호막과 HP 피해 문구 반환
    } // 메서드 끝



    private void SelectNextHeroineAction() // AI 제약 조건 기반 행동 선택
    { // 메서드 시작
        float currentHpRatio = heroineMaxHp > 0 ? (float)heroineCurrentHp / heroineMaxHp : 0f; // 현재 히로인 HP 비율 계산
        List<HeroineActionData> availableActions = new List<HeroineActionData>(); // 사용 가능한 행동 목록 생성
        int totalWeight = 0; // 전체 가중치 초기화

        foreach (HeroineActionData actionData in heroineActions) // 모든 행동 데이터 반복
        { // 반복문 시작
            if (actionData == null) // 행동 데이터 누락 확인
            { // 조건문 시작
                continue; // 누락 데이터 건너뛰기
            } // 조건문 끝

            if (!actionData.IsAvailable(currentHpRatio)) // HP 조건 확인
            { // 조건문 시작
                continue; // HP 조건 불충족 행동 제외
            } // 조건문 끝
            
            if (!CanUseHeroineAction(actionData)) { continue; } // 현재 전투 상태상 사용할 수 없는 행동 제외

            if (actionData.Weight <= 0) // 가중치 유효성 확인
            { // 조건문 시작
                continue; // 잘못된 가중치 행동 제외
            } // 조건문 끝

            if (IsHeroineActionOnCooldown(actionData)) // 행동 쿨타임 확인
            { // 조건문 시작
                continue; // 쿨타임 행동 제외
            } // 조건문 끝

            if (IsConsecutiveUseLimitReached(actionData)) // 연속 사용 제한 확인
            { // 조건문 시작
                continue; // 연속 사용 제한 행동 제외
            } // 조건문 끝

            availableActions.Add(actionData); // 사용 가능 행동 등록
            totalWeight += actionData.Weight; // 전체 가중치 합산
        } // 반복문 끝

        if (availableActions.Count == 0 || totalWeight <= 0) // 선택 가능 행동 확인
        { // 조건문 시작
            nextHeroineAction = null; // 다음 행동 초기화
            return; // 행동 선택 종료
        } // 조건문 끝

        int randomWeight = Random.Range(0, totalWeight); // 가중치 범위 난수 생성

        foreach (HeroineActionData actionData in availableActions) // 사용 가능 행동 반복
        { // 반복문 시작
            if (randomWeight < actionData.Weight) // 현재 행동 선택 범위 확인
            { // 조건문 시작
                nextHeroineAction = actionData; // 다음 행동 설정
                return; // 행동 선택 종료
            } // 조건문 끝

            randomWeight -= actionData.Weight; // 다음 행동 범위 이동
        } // 반복문 끝

        nextHeroineAction = availableActions[availableActions.Count - 1]; // 마지막 행동 안전 설정
    } // 메서드 끝

    private bool CanUseHeroineAction(HeroineActionData actionData) // 현재 전투 상태의 행동 사용 가능 여부 확인
    {
        if (actionData == null) { return false; } // 행동 데이터 누락 차단

        if (actionData.ActionType == HeroineActionType.GainShield && heroineCurrentShield >= heroineMaxShield)
        {
            return false; // 최대 보호막 상태의 보호막 행동 차단
        }

        return true; // 행동 사용 허용
    }
    private bool IsHeroineActionOnCooldown(HeroineActionData actionData) // 행동 쿨타임 확인
    { // 메서드 시작
        if (!heroineActionCooldowns.TryGetValue(actionData, out int remainingCooldown)) // 쿨타임 데이터 확인
        { // 조건문 시작
            return false; // 쿨타임 없음 반환
        } // 조건문 끝

        return remainingCooldown > 0; // 남은 쿨타임 여부 반환
    } // 메서드 끝
    private bool IsConsecutiveUseLimitReached(HeroineActionData actionData) // 연속 사용 제한 확인
    { // 메서드 시작
        if (lastHeroineAction != actionData) // 마지막 행동과 다른지 확인
        { // 조건문 시작
            return false; // 연속 사용 아님 반환
        } // 조건문 끝

        return consecutiveHeroineActionUses >= actionData.MaxConsecutiveUses; // 최대 연속 사용 도달 여부 반환
    } // 메서드 끝

    private void RegisterHeroineActionUse(HeroineActionData actionData) // 실행 행동 기록
    { // 메서드 시작
        if (actionData == null) // 행동 데이터 누락 확인
        { // 조건문 시작
            return; // 기록 차단
        } // 조건문 끝

        if (lastHeroineAction == actionData) // 이전 행동과 동일한지 확인
        { // 조건문 시작
            consecutiveHeroineActionUses += 1; // 연속 사용 횟수 증가
        } // 조건문 끝
        else // 다른 행동 실행 상태
        { // 조건문 시작
            lastHeroineAction = actionData; // 마지막 행동 변경
            consecutiveHeroineActionUses = 1; // 연속 사용 횟수 초기화
        } // 조건문 끝

        heroineActionCooldowns[actionData] = actionData.CooldownTurns; // 행동 쿨타임 적용
    } // 메서드 끝

    private void ReduceHeroineActionCooldowns() // 모든 행동 쿨타임 감소
    { // 메서드 시작
        List<HeroineActionData> cooldownActions = new List<HeroineActionData>(heroineActionCooldowns.Keys); // 쿨타임 행동 목록 복사

        foreach (HeroineActionData actionData in cooldownActions) // 모든 쿨타임 행동 반복
        { // 반복문 시작
            int remainingCooldown = heroineActionCooldowns[actionData]; // 현재 남은 쿨타임 확인
            heroineActionCooldowns[actionData] = Mathf.Max(0, remainingCooldown - 1); // 쿨타임 1 감소
        } // 반복문 끝
    } // 메서드 끝




    private void UpdateHeroineIntentUI() // 히로인 행동 예고 UI 갱신
    {
        if (heroineIntentText == null) { return; } // 행동 예고 텍스트 누락 차단

        if (nextHeroineAction == null)
        {
            heroineIntentText.text = "Next Action: None"; // 다음 행동 없음 표시
            return;
        }

        string targetName = GetHeroineTargetDisplayName(nextHeroineAction.TargetType); // 대상 표시 이름 확인
        string effectName = GetHeroineActionEffectDisplay(nextHeroineAction); // 행동 효과 문구 확인
        heroineIntentText.text = $"Next: {nextHeroineAction.DisplayName}\n{effectName} / Target: {targetName}"; // 행동 효과와 대상 표시
    }
    private string GetHeroineActionEffectDisplay(HeroineActionData actionData) // 행동 효과 표시 문구 반환
    {
        if (actionData == null) { return "Effect: None"; } // 행동 데이터 누락 표시

        if (actionData.ActionType == HeroineActionType.GainShield)
        {
            return $"Shield +{actionData.ShieldAmount}"; // 보호막 효과 표시
        }

        return $"Damage {actionData.Damage}"; // 공격 피해 효과 표시
    }

    private string GetHeroineTargetDisplayName(HeroineTargetType targetType) // 대상 규칙 표시 이름 반환
    {
        switch (targetType) // 대상 규칙 확인
        { 
            case HeroineTargetType.FirstMonster    : return "First Monster";    // 첫 번째 마물 대상 -> 첫 번째 마물 문구 반환
            case HeroineTargetType.RandomMonster   : return "Random Monster";   // 무작위 마물 대상 -> 무작위 마물 문구 반환
            case HeroineTargetType.LowestHpMonster : return "Lowest HP Monster";// 최저 HP 마물 대상 -> 최저 HP 마물 문구 반환
            case HeroineTargetType.AllMonsters     : return "All Monsters";     // 전체 마물 대상 p -> 전체 마물 문구 반환
            case HeroineTargetType.Player          : return "Player";           // 플레이어 직접 대상 -> 플레이어 문구 반환
            case HeroineTargetType.Self            : return "Self";             // 히로인 자신 대상 -> 자기 자신 문구 반환
            default                                : return "Unknown";          // 정의되지 않은 대상 -> 알 수 없는 대상 반환
        }
    }


    private void BeginNextPlayerTurn() // 다음 플레이어 턴 준비
    { // 메서드 시작
        turnNumber += 1; // 턴 번호 증가
        SelectNextHeroineAction(); // 현재 쿨타임 기준 다음 행동 선택
        ReduceHeroineActionCooldowns(); // 행동 선택 후 쿨타임 감소
        
        maximumMana = Mathf.Min(10, maximumMana + 1); // 최대 마나 증가
        currentMana = maximumMana; // 마나 전체 회복
        resultText.text = string.Empty; // 안내 텍스트 초기화
       
        DrawCards(turnDrawCount); // 턴 시작 카드 드로우
        isPlayerTurn = true; // 플레이어 턴 설정
        
        PrepareMonstersForNewTurn(); // 마물 공격 상태 준비
        ShowPlayerTurn(); // 플레이어 턴 표시
        UpdateBattleUI(); // 전체 UI 갱신
    } // 메서드 끝

    private void PrepareMonstersForNewTurn() // 필드 마물 새 턴 준비
    { // 메서드 시작
        foreach (MonsterUnit monsterUnit in fieldMonsters) // 모든 필드 마물 반복
        { // 반복문 시작
            if (monsterUnit != null) // 마물 존재 확인
            { // 조건문 시작
                monsterUnit.PrepareForNewTurn(); // 공격 가능 상태 적용
            } // 조건문 끝
        } // 반복문 끝
    } // 메서드 끝

    private void ShowPlayerTurn() // 플레이어 턴 UI 표시
    { // 메서드 시작
        turnText.text = "Player Turn"; // 플레이어 턴 문구 설정
        endTurnButton.interactable = true; // 턴 종료 버튼 활성화
        monsterAttackButton.interactable = false; // 공격 버튼 초기 비활성화
        SetHandInteractable(true); // 손패 버튼 활성화
        SetMonsterInteractable(true); // 공격 가능 마물 선택 활성화
    } // 메서드 끝

    private void SelectMonster(MonsterUnit monsterUnit) // 공격 마물 선택
    { // 메서드 시작
        if (!isPlayerTurn || isBattleEnded || monsterUnit == null) // 선택 불가 상태
        { // 조건문 시작
            return; // 선택 차단
        } // 조건문 끝

        if (!monsterUnit.CanAttack) // 마물 공격 가능 여부 확인
        { // 조건문 시작
            resultText.text = "Monster Cannot Attack"; // 공격 불가 안내
            return; // 선택 차단
        } // 조건문 끝

        if (selectedMonster != null) // 기존 선택 마물 확인
        { // 조건문 시작
            selectedMonster.SetSelected(false); // 기존 선택 표시 해제
        } // 조건문 끝

        selectedMonster = monsterUnit; // 새로운 마물 선택
        selectedMonster.SetSelected(true); // 선택 색상 표시
        monsterAttackButton.interactable = true; // 공격 버튼 활성화
        resultText.text = $"{selectedMonster.MonsterName} Selected"; // 선택 결과 표시
    } // 메서드 끝

    private void ClearMonsterSelection() // 선택 마물 초기화
    { // 메서드 시작
        if (selectedMonster != null) // 선택 마물 존재 확인
        { // 조건문 시작
            selectedMonster.SetSelected(false); // 선택 색상 해제
        } // 조건문 끝

        selectedMonster = null; // 선택 참조 초기화
        monsterAttackButton.interactable = false; // 공격 버튼 비활성화
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
        string monsterName = cardData.SummonMonster != null ? cardData.SummonMonster.MonsterName : "None"; // 소환 마물 이름 확인

        cardText.text = $"{cardData.CardName}\nCost: {cardData.ManaCost}\nSummon: {monsterName}"; // 카드 정보 표시
        newCardButton.onClick.RemoveAllListeners(); // 기존 버튼 연결 초기화
        newCardButton.onClick.AddListener(() => TryPlayCard(cardData, newCardButton)); // 카드 사용 함수 연결
        handButtons.Add(newCardButton); // 손패 버튼 목록 등록
    } // 메서드 끝

    private void TryPlayCard(CardData cardData, Button cardButton) // 카드 사용 처리
    { // 메서드 시작
        if (!isPlayerTurn || isBattleEnded) // 카드 사용 불가 상태
        { // 조건문 시작
            return; // 카드 사용 차단
        } // 조건문 끝

        if (cardData.SummonMonster == null) // 마물 데이터 누락 확인
        { // 조건문 시작
            resultText.text = "Missing Monster Data"; // 데이터 누락 안내
            return; // 카드 사용 차단
        } // 조건문 끝

        if (fieldMonsters.Count >= maxFieldMonsterCount) // 필드 최대 수 확인
        { // 조건문 시작
            resultText.text = "Field Full"; // 필드 초과 안내
            return; // 카드 사용 차단
        } // 조건문 끝

        if (currentMana < cardData.ManaCost) // 마나 부족 확인
        { // 조건문 시작
            resultText.text = "Not Enough Mana"; // 마나 부족 안내
            return; // 카드 사용 차단
        } // 조건문 끝

        currentMana -= cardData.ManaCost; // 카드 비용 차감
        SummonMonster(cardData.SummonMonster); // 마물 필드 소환
        discardPile.Add(cardData); // 사용 카드 버린 더미 이동
        handButtons.Remove(cardButton); // 손패 버튼 목록 제거
        Destroy(cardButton.gameObject); // 카드 버튼 제거
        resultText.text = string.Empty; // 안내 텍스트 초기화
        UpdateBattleUI(); // 카드 사용 결과 표시
    } // 메서드 끝

    private void SummonMonster(MonsterData monsterData) // 마물 필드 소환
    { // 메서드 시작
        MonsterUnit newMonsterUnit = Instantiate(monsterUnitPrefab, monsterFieldContainer); // 마물 UI 복제
        newMonsterUnit.Initialize(monsterData, SelectMonster); // 마물 데이터와 선택 콜백 초기화
        fieldMonsters.Add(newMonsterUnit); // 필드 마물 목록 등록
        newMonsterUnit.SetPlayerTurnInteraction(isPlayerTurn); // 소환 대기 선택 상태 적용
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

    private void SetMonsterInteractable(bool isInteractable) // 마물 선택 활성 상태 변경
    { // 메서드 시작
        foreach (MonsterUnit monsterUnit in fieldMonsters) // 모든 필드 마물 반복
        { // 반복문 시작
            if (monsterUnit != null) // 마물 존재 확인
            { // 조건문 시작
                monsterUnit.SetPlayerTurnInteraction(isInteractable); // 마물 선택 상태 적용
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

    private void ClearMonsterField() // 마물 필드 전체 제거
    { // 메서드 시작
        foreach (MonsterUnit monsterUnit in fieldMonsters) // 모든 필드 마물 반복
        { // 반복문 시작
            if (monsterUnit != null) // 마물 존재 확인
            { // 조건문 시작
                Destroy(monsterUnit.gameObject); // 마물 UI 제거
            } // 조건문 끝
        } // 반복문 끝

        fieldMonsters.Clear(); // 필드 마물 목록 초기화
        ClearMonsterSelection(); // 마물 선택 상태 초기화
    } // 메서드 끝

    private void UpdateBattleUI() // 전투 수치 UI 갱신
    { // 메서드 시작
        turnNumberText.text     = $"Turn {turnNumber}"; // 턴 번호 표시
        playerHpText.text       = $"Player HP: {playerCurrentHp} / {playerMaxHp}"; // 플레이어 체력 표시
        playerShieldText.text   = $"Shield: {playerCurrentShield}"; // 플레이어 보호막 표시
        manaText.text           = $"Mana: {currentMana} / {maximumMana}"; // 마나 표시
        heroineHpText.text      = $"Heroine HP: {heroineCurrentHp} / {heroineMaxHp}"; // 히로인 체력 표시
        heroineDefenseText.text = $"DEF: {heroineDefense}"; // 히로인 방어력 표시
        heroineShieldText.text = $"Shield: {heroineCurrentShield} / {heroineMaxShield}"; // 히로인 현재 및 최대 보호막 표시
        lustText.text           = $"Lust: {heroineLust} / 100"; // 성욕 게이지 표시
        UpdateHeroineIntentUI(); // 히로인 행동 예고 표시
    } // 메서드 끝

    private void EndBattle(string resultMessage) // 전투 종료 처리
    { // 메서드 시작
        isBattleEnded = true; // 전투 종료 상태 설정
        isPlayerTurn = false; // 플레이어 턴 해제
        ClearMonsterSelection(); // 마물 선택 상태 해제
        turnText.text = "Battle End"; // 전투 종료 문구 설정
        resultText.text = resultMessage; // 전투 결과 표시
        heroineIntentText.text = "Next Action: None"; // 행동 예고 종료 표시
        endTurnButton.interactable = false; // 턴 종료 버튼 비활성화
        monsterAttackButton.interactable = false; // 공격 버튼 비활성화
        SetHandInteractable(false); // 손패 버튼 비활성화
        SetMonsterInteractable(false); // 마물 선택 비활성화
    } // 메서드 끝
} // 클래스 끝