using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class BattleManager : MonoBehaviour // 기본 전투 흐름 관리
{
    [Header("Battle UI")] // 전투 UI 구분
    [SerializeField] private TMP_Text turnText;             // 턴 상태 텍스트
    [SerializeField] private TMP_Text turnNumberText;       // 턴 번호 텍스트
    [SerializeField] private TMP_Text playerHpText;         // 플레이어 체력 텍스트
    [SerializeField] private TMP_Text playerShieldText;     // 플레이어 보호막 텍스트
    [SerializeField] private TMP_Text manaText;             // 마나 텍스트

    [SerializeField] private TMP_Text heroineHpText;        // 히로인 체력 텍스트
    [SerializeField] private TMP_Text heroineDefenseText;   // 히로인 방어력 텍스트
    [SerializeField] private TMP_Text heroineShieldText;    // 히로인 보호막 텍스트
    [SerializeField] private TMP_Text heroineStatusText; // 히로인 상태 효과 텍스트
    [SerializeField] private Transform heroineStatusIconContainer; // 히로인 상태 아이콘 배치 영역
    [SerializeField] private StatusEffectIconUI statusEffectIconPrefab; // 상태 효과 아이콘 프리팹
    [SerializeField] private StatusEffectTooltipUI statusEffectTooltipUI; // 상태 효과 툴팁 UI
    [SerializeField] private TMP_Text lustText;             // 성욕 게이지 텍스트

    [SerializeField] private TMP_Text heroineIntentText;    // 히로인 행동 예고 텍스트
    [SerializeField] private TMP_Text resultText;           // 전투 결과 텍스트
    [SerializeField] private BattleLogUI battleLogUI; // 전투 로그 UI
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
    private readonly List<ActiveStatusEffect> activeHeroineStatusEffects = new List<ActiveStatusEffect>(); // 히로인 활성 상태 효과 목록
    private readonly List<StatusEffectIconUI> heroineStatusIconUIs = new List<StatusEffectIconUI>(); // 생성된 상태 효과 아이콘 목록
    private readonly Dictionary<HeroineActionData, int> heroineActionCooldowns = new Dictionary<HeroineActionData, int>(); // 행동별 남은 쿨타임

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
    {
        InitializeBattle(); // 기본 전투 초기화
    }

    private void InitializeBattle() // 전투 기본값 설정
    {
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
        activeHeroineStatusEffects.Clear(); // 히로인 상태 효과 초기화
        heroineActionCooldowns.Clear(); // 행동 쿨타임 초기화

        lastHeroineAction = null; // 마지막 행동 초기화
        consecutiveHeroineActionUses = 0; // 연속 사용 횟수 초기화
        SelectNextHeroineAction(); // 첫 번째 히로인 행동 선택
        selectedMonster = null; // 선택 마물 초기화
        resultText.text = string.Empty; // 결과 텍스트 초기화
        if (battleLogUI != null) { battleLogUI.Clear(); } // 이전 전투 로그 초기화
        AddBattleLog(BattleLogCategory.System, "Battle started."); // 전투 시작 기록
        monsterAttackButton.interactable = false; // 공격 버튼 비활성화
        drawPile.Clear(); // 드로우 더미 초기화
        discardPile.Clear(); // 버린 카드 더미 초기화
        ClearHand(); // 기존 손패 초기화
        ClearMonsterField(); // 기존 마물 필드 초기화
        drawPile.AddRange(deckCards); // 덱 카드 드로우 더미 등록
        DrawCards(startingHandCount); // 시작 손패 드로우
        ShowPlayerTurn(); // 플레이어 턴 표시
        UpdateBattleUI(); // 전체 UI 갱신
    }

    public void EndPlayerTurn() // 플레이어 턴 종료
    {
        if (!isPlayerTurn || isBattleEnded) { return; } // 중복 실행 차단

        AddBattleLog(BattleLogCategory.System, "Player turn ended."); // 플레이어 턴 종료 기록
        isPlayerTurn = false; // 플레이어 턴 종료
        ClearMonsterSelection(); // 마물 선택 상태 해제
        endTurnButton.interactable = false; // 턴 종료 버튼 비활성화
        monsterAttackButton.interactable = false; // 공격 버튼 비활성화
        SetHandInteractable(false); // 손패 버튼 비활성화
        SetMonsterInteractable(false); // 마물 선택 비활성화
        turnText.text = "Heroine Turn"; // 히로인 턴 표시
        StartCoroutine(HeroineTurnRoutine()); // 히로인 행동 시작
    }

    public void AttackWithSelectedMonster() // 선택 마물 공격 처리
    {
        if (!isPlayerTurn || isBattleEnded) { return; } // 공격 차단

        if (selectedMonster == null || !selectedMonster.CanAttack) // 선택 마물 상태 확인
        {
            resultText.text = "Select Ready Monster"; // 마물 선택 안내
            return; // 공격 차단
        }

        MonsterUnit attackingMonster = selectedMonster; // 공격 마물 저장
        string attackerName = attackingMonster.MonsterName; // 공격 마물 이름 저장
        int attackPower = attackingMonster.Attack; // 마물 공격력 저장
        int currentHeroineDefense = GetHeroineCurrentDefense(); // 상태 효과 포함 방어력 계산
        DamageResult damageResult = DamageCalculator.CalculateDamageWithShield(attackPower, currentHeroineDefense, heroineCurrentShield); // 현재 방어력 기반 피해 계산


        heroineCurrentShield = damageResult.RemainingShield; // 히로인 남은 보호막 적용
        heroineCurrentHp = Mathf.Max(0, heroineCurrentHp - damageResult.HpDamage); // 히로인 실제 HP 피해 적용

        string damageText = CreateDamageResultText(attackerName, damageResult); // 마물 공격 피해 문구 생성
        string statusText = TryApplyMonsterAttackStatus(attackingMonster); // 마물 공격 상태 효과 적용

        attackingMonster.MarkActed(); // 마물 행동 완료 처리
        ClearMonsterSelection(); // 마물 선택 해제

        resultText.text = string.IsNullOrEmpty(statusText)
            ? damageText // 피해 결과만 표시
            : $"{damageText}\n{statusText}"; // 피해와 상태 효과 표시

        AddBattleLog(BattleLogCategory.PlayerAction, damageText); // 마물 공격 피해 기록

        if (!string.IsNullOrEmpty(statusText)) // 상태 효과 적용 확인
        {
            AddBattleLog(BattleLogCategory.StatusEffect, statusText); // 마물 공격 상태 효과 기록
        }


        if (heroineCurrentHp <= 0) // 히로인 사망 확인
        {
            EndBattle("Victory"); // 승리 처리
        }

        UpdateBattleUI(); // 전투 UI 갱신
    }

    private IEnumerator HeroineTurnRoutine() // 히로인 턴 순차 처리
    {
        AddBattleLog(BattleLogCategory.System, "Heroine turn started."); // 히로인 턴 시작 기록
        ReduceHeroineStatusDurations(StatusDurationTiming.AfterPlayerTurn); // 플레이어 턴 기준 상태 지속시간 감소
        ApplyHeroineStartTurnStatusEffects(); // 히로인 행동 시작 상태 효과 처리
        UpdateBattleUI(); // 독 피해 결과 UI 갱신

        if (heroineCurrentHp <= 0) // 독 피해 히로인 사망 확인
        {
            EndBattle("Victory"); // 플레이어 승리 처리
            yield break; // 히로인 행동 중단
        }

        yield return new WaitForSeconds(heroineActionDelay); // 공격 전 대기

        HeroineActionData executedAction = nextHeroineAction; // 이번 실행 행동 저장
        ResolveHeroineAttack(); // 히로인 공격 실행
        RegisterHeroineActionUse(executedAction); // 실행 행동과 쿨타임 기록
        ReduceHeroineStatusDurations(StatusDurationTiming.AfterHeroineTurn); // 히로인 행동 기준 상태 지속시간 감소
        UpdateBattleUI(); // 히로인 공격 결과 갱신

        if (playerCurrentHp <= 0) // 플레이어 사망 확인
        {
            EndBattle("Defeat"); // 패배 처리
            yield break; // 코루틴 종료
        }

        yield return new WaitForSeconds(heroineActionDelay); // 다음 턴 전 대기
        BeginNextPlayerTurn(); // 다음 플레이어 턴 시작
    }

    private void ResolveHeroineAttack() // 선택된 히로인 행동 실행
    {
        if (nextHeroineAction == null) { resultText.text = "No Available Heroine Action"; return; } // 행동 누락 차단

        if (nextHeroineAction.ActionType == HeroineActionType.GainShield) // 보호막 행동 확인
        {
            ExecuteGainShieldAction(); // 보호막 행동 실행
            return; // 공격 처리 종료
        }

        if (nextHeroineAction.ActionType == HeroineActionType.Heal) // 회복 행동 확인
        {
            ExecuteHealAction(); // 회복 행동 실행
            return; // 공격 처리 종료
        }

        if (nextHeroineAction.ActionType == HeroineActionType.ApplyStatus) // 상태 효과 행동 확인
        {
            ExecuteApplyStatusAction(); // 상태 효과 행동 실행
            return; // 공격 처리 종료
        }

        if (nextHeroineAction.ActionType == HeroineActionType.Cleanse) // 정화 행동 확인
        {
            ExecuteCleanseAction(); // 해로운 상태 효과 제거
            return; // 공격 처리 종료
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
        AddBattleLog(BattleLogCategory.HeroineAction, resultText.text); // 히로인 보호막 행동 기록
    }

    private void ExecuteHealAction() // 히로인 체력 회복 행동 실행
    {
        int safeHealAmount = Mathf.Max(0, nextHeroineAction.HealAmount); // 음수 회복량 차단
        int previousHp = heroineCurrentHp; // 행동 전 체력 저장
        heroineCurrentHp = Mathf.Min(heroineMaxHp, heroineCurrentHp + safeHealAmount); // 최대치 범위 내 체력 회복
        int recoveredHp = heroineCurrentHp - previousHp; // 실제 체력 회복량 계산

        resultText.text = $"{nextHeroineAction.DisplayName}: HP +{recoveredHp}"; // 회복 행동 결과 표시
        AddBattleLog(BattleLogCategory.HeroineAction, resultText.text); // 히로인 회복 행동 기록
    }
    private void ExecuteApplyStatusAction() // 히로인 상태 효과 행동 실행
    {
        StatusEffectData statusData = nextHeroineAction.AppliedStatusEffect; // 적용 상태 효과 확인

        if (statusData == null) { resultText.text = "Missing Status Effect Data"; return; } // 상태 데이터 누락 차단

        ApplyOrRefreshHeroineStatus(statusData); // 상태 효과 적용 또는 지속시간 갱신

        string amountText = GetStatusAmountDisplay(statusData); // 상태 효과 수치 문구 생성
        resultText.text = $"{nextHeroineAction.DisplayName}: {statusData.DisplayName} {amountText} ({statusData.DurationTurns} Turns)"; // 상태 효과 결과 표시
        AddBattleLog(BattleLogCategory.StatusEffect, resultText.text); // 히로인 상태 효과 행동 기록
    }
    private void ExecuteCleanseAction() // 히로인 해로운 상태 효과 제거
    {
        int maximumCleanseCount = Mathf.Max(1, nextHeroineAction.CleanseCount); // 안전한 최대 정화 개수 계산
        List<string> removedStatusNames = new List<string>(); // 제거 상태 이름 목록 생성

        for (int i = 0; i < maximumCleanseCount; i++) // 정화 가능 개수 반복
        {
            int targetIndex = FindHighestPriorityNegativeStatusIndex(); // 최우선 정화 대상 검색

            if (targetIndex < 0) { break; } // 정화 대상 없음 처리

            ActiveStatusEffect targetStatus = activeHeroineStatusEffects[targetIndex]; // 정화 대상 상태 확인
            removedStatusNames.Add(targetStatus.Data.DisplayName); // 제거 상태 이름 저장
            activeHeroineStatusEffects.RemoveAt(targetIndex); // 해로운 상태 효과 제거
        }

        if (removedStatusNames.Count == 0) // 제거 상태 없음 확인
        {
            resultText.text = $"{nextHeroineAction.DisplayName}: No Negative Status"; // 정화 대상 없음 표시
            AddBattleLog(BattleLogCategory.StatusEffect, resultText.text); // 정화 실패 기록
            return; // 정화 처리 종료
        }

        string removedStatusText = string.Join(", ", removedStatusNames); // 제거 상태 이름 결합
        resultText.text = $"{nextHeroineAction.DisplayName}: Removed {removedStatusText}"; // 정화 결과 표시
        AddBattleLog(BattleLogCategory.StatusEffect, resultText.text); // 정화 결과 기록
    }

    private void ReduceHeroineStatusDurations(StatusDurationTiming durationTiming) // 지정 시점 상태 효과 지속시간 감소
    {
        for (int i = activeHeroineStatusEffects.Count - 1; i >= 0; i--) // 상태 효과 역순 반복
        {
            ActiveStatusEffect activeStatus = activeHeroineStatusEffects[i]; // 현재 상태 효과 확인

            if (activeStatus == null) // 비어 있는 상태 효과 확인
            {
                activeHeroineStatusEffects.RemoveAt(i); // 비어 있는 상태 제거
                continue; // 다음 상태 효과 처리
            }

            if (activeStatus.Data.DurationTiming != durationTiming) { continue; } // 다른 감소 시점 상태 제외

            activeStatus.ReduceDuration(); // 상태 효과 지속시간 감소

            if (activeStatus.IsExpired) { activeHeroineStatusEffects.RemoveAt(i); } // 만료 상태 효과 제거
        }
    }


    private int GetHeroineCurrentDefense() // 상태 효과 포함 히로인 방어력 계산
    {
        int currentDefense = heroineDefense; // 기본 방어력 저장

        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects) // 활성 상태 효과 반복
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; } // 만료 상태 효과 제외
            if (activeStatus.Data.StatusType != StatusEffectType.DefenseUp) { continue; } // 방어력 효과 외 제외

            currentDefense += activeStatus.Data.Amount; // 방어력 증가량 적용
        }

        return Mathf.Max(0, currentDefense); // 음수 방어력 차단
    }
    private int GetHeroineCurrentAttack(int baseAttack) // 상태 효과 포함 히로인 공격력 계산
    {
        int currentAttack = Mathf.Max(0, baseAttack); // 기본 공격력 저장

        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects) // 활성 상태 효과 반복
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; } // 만료 상태 효과 제외
            if (activeStatus.Data.StatusType != StatusEffectType.AttackDown) { continue; } // 공격력 감소 외 제외

            currentAttack -= activeStatus.Data.Amount; // 공격력 감소량 적용
        }

        return Mathf.Max(0, currentAttack); // 음수 공격력 차단
    }
    private int ApplyHeroineStartTurnStatusEffects() // 히로인 행동 시작 상태 효과 처리
    {
        int totalPoisonDamage = 0; // 전체 독 피해 초기화

        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects) // 활성 상태 효과 반복
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; } // 만료 상태 효과 제외
            if (activeStatus.Data.StatusType != StatusEffectType.Poison) { continue; } // 독 이외 효과 제외

            int poisonDamage = Mathf.Max(0, activeStatus.Data.Amount); // 안전한 독 피해 계산
            int previousHp = heroineCurrentHp; // 독 피해 전 체력 저장

            heroineCurrentHp = Mathf.Max(0, heroineCurrentHp - poisonDamage); // 방어 무시 직접 체력 피해
            totalPoisonDamage += previousHp - heroineCurrentHp; // 실제 독 피해 합산

            if (heroineCurrentHp <= 0) { break; } // 히로인 사망 시 추가 처리 중단
        }

        if (totalPoisonDamage > 0) // 독 피해 발생 확인
        {
            resultText.text = $"Poison: Heroine HP -{totalPoisonDamage}"; // 독 피해 결과 표시
            AddBattleLog(BattleLogCategory.StatusEffect, resultText.text); // 독 피해 전투 로그 기록
        }

        return totalPoisonDamage; // 실제 독 피해 반환
    }

    private bool HasHeroineStatusEffect(StatusEffectData statusData) // 히로인 상태 효과 보유 여부 확인
    {
        if (statusData == null) { return false; } // 비어 있는 상태 데이터 차단

        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects) // 활성 상태 효과 반복
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; } // 만료 상태 효과 제외
            if (activeStatus.Data == statusData) { return true; } // 동일 상태 효과 확인
        }

        return false; // 동일 상태 효과 없음
    }

    private bool HasHeroineNegativeStatus() // 히로인 해로운 상태 효과 보유 여부 확인
    {
        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects) // 활성 상태 효과 반복
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; } // 만료 상태 효과 제외
            if (activeStatus.Data == null) { continue; } // 상태 효과 데이터 누락 제외
            if (activeStatus.Data.IsNegative) { return true; } // 해로운 상태 효과 확인
        }

        return false; // 해로운 상태 효과 없음
    }
    private int GetCleansePriority(StatusEffectData statusData) // 상태 효과 정화 우선순위 반환
    {
        if (statusData == null) { return -1; } // 상태 효과 누락 처리

        switch (statusData.StatusType) // 상태 효과 종류 확인
        {
            case StatusEffectType.Poison: // 독 상태
                return 100; // 최우선 정화

            case StatusEffectType.AttackDown: // 공격력 감소 상태
                return 50; // 두 번째 정화

            default: // 기타 해로운 상태
                return 0; // 기본 정화 우선순위
        }
    }
    private int FindHighestPriorityNegativeStatusIndex() // 최우선 해로운 상태 효과 위치 검색
    {
        int selectedIndex = -1; // 선택 상태 효과 위치 초기화
        int highestPriority = int.MinValue; // 최고 우선순위 초기화

        for (int i = 0; i < activeHeroineStatusEffects.Count; i++) // 활성 상태 효과 반복
        {
            ActiveStatusEffect activeStatus = activeHeroineStatusEffects[i]; // 현재 상태 효과 확인

            if (activeStatus == null || activeStatus.IsExpired) { continue; } // 만료 상태 효과 제외
            if (activeStatus.Data == null || !activeStatus.Data.IsNegative) { continue; } // 이로운 상태 효과 제외

            int currentPriority = GetCleansePriority(activeStatus.Data); // 현재 정화 우선순위 확인

            if (currentPriority <= highestPriority) { continue; } // 더 낮은 우선순위 제외

            highestPriority = currentPriority; // 최고 우선순위 갱신
            selectedIndex = i; // 정화 대상 위치 저장
        }

        return selectedIndex; // 정화 대상 위치 반환
    }

    private void ApplyOrRefreshHeroineStatus(StatusEffectData statusData) // 히로인 상태 효과 적용 및 갱신
    {
        if (statusData == null) { return; } // 비어 있는 상태 효과 차단

        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects) // 활성 상태 효과 반복
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; } // 만료 상태 효과 제외
            if (activeStatus.Data != statusData) { continue; } // 다른 상태 효과 제외

            activeStatus.RefreshDuration(); // 동일 상태 지속시간 갱신
            return; // 신규 상태 등록 차단
        }

        ActiveStatusEffect newStatus = new ActiveStatusEffect(statusData); // 신규 활성 상태 생성
        activeHeroineStatusEffects.Add(newStatus); // 히로인 상태 효과 등록
    }
    private string TryApplyMonsterAttackStatus(MonsterUnit attackingMonster) // 마물 공격 상태 효과 적용
    {
        if (attackingMonster == null) { return string.Empty; } // 공격 마물 누락 차단

        StatusEffectData statusData = attackingMonster.AttackStatusEffect; // 마물 공격 상태 효과 확인

        if (statusData == null) { return string.Empty; } // 상태 효과 없는 공격 처리

        ApplyOrRefreshHeroineStatus(statusData); // 상태 효과 적용 또는 갱신

        string amountText = GetStatusAmountDisplay(statusData); // 상태 효과 수치 문구 생성
        return $"Heroine: {statusData.DisplayName} {amountText}"; // 상태 효과 적용 결과 반환
    }
    private MonsterUnit GetFirstMonster() // 첫 번째 마물 반환
    {
        if (fieldMonsters.Count == 0) { return null; } // 대상 없음 반환

        return fieldMonsters[0]; // 첫 번째 마물 반환
    }

    private MonsterUnit GetRandomMonster() // 무작위 마물 반환
    {
        if (fieldMonsters.Count == 0) { return null; } // 대상 없음 반환

        int randomIndex = Random.Range(0, fieldMonsters.Count); // 무작위 필드 인덱스 생성
        return fieldMonsters[randomIndex]; // 무작위 마물 반환
    }

    private MonsterUnit GetLowestHpMonster() // 현재 HP가 가장 낮은 마물 반환
    {
        if (fieldMonsters.Count == 0) { return null; } // 대상 없음 반환

        MonsterUnit lowestHpMonster = fieldMonsters[0]; // 첫 번째 마물을 초기 대상으로 설정

        for (int i = 1; i < fieldMonsters.Count; i++) // 두 번째 마물부터 반복
        {
            MonsterUnit currentMonster = fieldMonsters[i]; // 현재 비교 마물 저장

            if (currentMonster.CurrentHp < lowestHpMonster.CurrentHp) // 더 낮은 HP 확인
            {
                lowestHpMonster = currentMonster; // 최저 HP 마물 교체
            }
        }

        return lowestHpMonster; // 최저 HP 마물 반환
    }



    private void ExecuteAreaAttack() // 히로인 광역 공격 실행
    {
        fieldMonsters.RemoveAll(monsterUnit => monsterUnit == null); // 삭제된 마물 참조 정리
        int attackPower = GetHeroineCurrentAttack(nextHeroineAction.Damage); // 상태 효과 포함 광역 공격력 계산

        if (fieldMonsters.Count == 0) // 필드 마물 부재 확인
        {
            ApplyDamageToPlayer(attackPower, nextHeroineAction.DisplayName); // 플레이어 보호막 포함 피해 적용
            return; // 광역 공격 종료
        }

        int defeatedMonsterCount = 0; // 사망 마물 수 초기화
        int totalShieldAbsorbed = 0; // 전체 보호막 흡수량 초기화
        int totalHpDamage = 0; // 전체 HP 피해량 초기화

        for (int i = fieldMonsters.Count - 1; i >= 0; i--) // 필드 마물 역순 반복
        {
            MonsterUnit targetMonster = fieldMonsters[i]; // 현재 공격 대상 저장
            DamageResult damageResult = targetMonster.TakeDamage(attackPower); // 마물별 보호막 포함 피해 계산
            totalShieldAbsorbed += damageResult.ShieldAbsorbed; // 보호막 흡수량 합산
            totalHpDamage += damageResult.HpDamage; // HP 피해량 합산

            if (targetMonster.IsDead) // 마물 사망 확인
            {
                if (selectedMonster == targetMonster) // 선택 마물 사망 확인
                {
                    ClearMonsterSelection(); // 선택 상태 해제
                }

                fieldMonsters.RemoveAt(i); // 필드 목록에서 마물 제거
                Destroy(targetMonster.gameObject); // 마물 오브젝트 제거
                defeatedMonsterCount += 1; // 사망 마물 수 증가
            }
        }

        resultText.text = $"{nextHeroineAction.DisplayName}: Shield -{totalShieldAbsorbed}, HP -{totalHpDamage}, Defeated {defeatedMonsterCount}"; // 광역 공격 결과 표시
AddBattleLog(BattleLogCategory.HeroineAction, resultText.text); // 히로인 광역 공격 기록
    }




    private void AttackTargetMonster(MonsterUnit targetMonster) // 선택된 마물 공격
    {
        if (targetMonster == null) // 공격 대상 마물 확인
        {
            AttackPlayer(); // 마물 부재 시 플레이어 공격
            return; // 마물 공격 종료
        }

        string targetName = targetMonster.MonsterName; // 공격 대상 이름 저장
        int attackPower = GetHeroineCurrentAttack(nextHeroineAction.Damage); // 상태 효과 포함 행동 공격력 계산
        DamageResult damageResult = targetMonster.TakeDamage(attackPower); // 마물 보호막 포함 피해 처리

        resultText.text = CreateDamageResultText(targetName, damageResult); // 마물 피해 결과 표시

        if (targetMonster.IsDead) // 대상 마물 사망 확인
        {
            if (selectedMonster == targetMonster) // 선택 마물 사망 확인
            {
                ClearMonsterSelection(); // 선택 상태 해제
            }

            fieldMonsters.Remove(targetMonster); // 필드 목록에서 대상 마물 제거
            Destroy(targetMonster.gameObject); // 대상 마물 오브젝트 제거
            resultText.text = $"{nextHeroineAction.DisplayName}: {targetName} Defeated"; // 마물 사망 결과 표시
        }

        AddBattleLog(BattleLogCategory.HeroineAction, resultText.text); // 히로인 단일 공격 기록
    }



    private void AttackPlayer() // 플레이어 직접 공격
    {
        int attackPower = GetHeroineCurrentAttack(nextHeroineAction.Damage); // 상태 효과 포함 공격력 계산
        ApplyDamageToPlayer(attackPower, nextHeroineAction.DisplayName); // 플레이어 보호막 포함 피해 적용
    }



    private void ApplyDamageToPlayer(int attackPower, string actionName) // 플레이어 보호막 포함 피해 처리
    {
        DamageResult damageResult = DamageCalculator.CalculateDamageWithShield(attackPower, playerDefense, playerCurrentShield); // 플레이어 피해 계산
        playerCurrentShield = damageResult.RemainingShield; // 플레이어 남은 보호막 적용
        playerCurrentHp = Mathf.Max(0, playerCurrentHp - damageResult.HpDamage); // 플레이어 실제 HP 피해 적용
        resultText.text = $"{actionName}: {CreateDamageResultText("Player", damageResult)}"; // 플레이어 피해 결과 표시
        AddBattleLog(BattleLogCategory.HeroineAction, resultText.text); // 히로인 플레이어 공격 기록
    }


    private string CreateDamageResultText(string targetName, DamageResult damageResult) // 피해 결과 문구 생성
    {
        return $"{targetName}: Shield -{damageResult.ShieldAbsorbed}, HP -{damageResult.HpDamage}"; // 보호막과 HP 피해 문구 반환
    }

    private void AddBattleLog(BattleLogCategory category, string message) // 전투 로그 추가
    {
        if (battleLogUI == null) { return; } // 전투 로그 UI 누락 차단

        battleLogUI.AddEntry(turnNumber, category, message); // 현재 턴 전투 로그 등록
    }

    private void SelectNextHeroineAction() // AI 제약 조건 기반 행동 선택
    {
        float currentHpRatio = heroineMaxHp > 0 ? (float)heroineCurrentHp / heroineMaxHp : 0f; // 현재 히로인 HP 비율 계산
        List<HeroineActionData> availableActions = new List<HeroineActionData>(); // 사용 가능한 행동 목록 생성
        int totalWeight = 0; // 전체 가중치 초기화

        foreach (HeroineActionData actionData in heroineActions) // 모든 행동 데이터 반복
        {
            if (actionData == null) { continue; } // 누락 데이터 건너뛰기

            if (!actionData.IsAvailable(currentHpRatio)) { continue; } // HP 조건 불충족 행동 제외

            if (!CanUseHeroineAction(actionData)) { continue; } // 현재 전투 상태상 사용할 수 없는 행동 제외

            if (actionData.Weight <= 0) { continue; } // 잘못된 가중치 행동 제외

            if (IsHeroineActionOnCooldown(actionData)) { continue; } // 쿨타임 행동 제외

            if (IsConsecutiveUseLimitReached(actionData)) { continue; } // 연속 사용 제한 행동 제외

            availableActions.Add(actionData); // 사용 가능 행동 등록
            totalWeight += actionData.Weight; // 전체 가중치 합산
        }

        if (availableActions.Count == 0 || totalWeight <= 0) // 선택 가능 행동 확인
        {
            nextHeroineAction = null; // 다음 행동 초기화
            return; // 행동 선택 종료
        }

        int randomWeight = Random.Range(0, totalWeight); // 가중치 범위 난수 생성

        foreach (HeroineActionData actionData in availableActions) // 사용 가능 행동 반복
        {
            if (randomWeight < actionData.Weight) // 현재 행동 선택 범위 확인
            {
                nextHeroineAction = actionData; // 다음 행동 설정
                return; // 행동 선택 종료
            }

            randomWeight -= actionData.Weight; // 다음 행동 범위 이동
        }

        nextHeroineAction = availableActions[availableActions.Count - 1]; // 마지막 행동 안전 설정
    }

    private bool CanUseHeroineAction(HeroineActionData actionData) // 행동 사용 가능 여부 확인
    {
        if (actionData == null) { return false; } // 행동 데이터 누락 차단
        if (actionData.ActionType == HeroineActionType.GainShield && heroineCurrentShield >= heroineMaxShield)                  { return false; } // 최대 보호막 행동 차단
        if (actionData.ActionType == HeroineActionType.Heal && heroineCurrentHp >= heroineMaxHp)                                { return false; } // 최대 체력 회복 행동 차단
        if (actionData.ActionType == HeroineActionType.ApplyStatus && actionData.AppliedStatusEffect == null)                   { return false; } // 상태 데이터 누락 행동 차단
        if (actionData.ActionType == HeroineActionType.ApplyStatus && HasHeroineStatusEffect(actionData.AppliedStatusEffect))   { return false; } // 중복 상태 행동 차단
        if (actionData.ActionType == HeroineActionType.Cleanse && !HasHeroineNegativeStatus())                                  { return false; } // 해로운 상태 없는 정화 행동 제외

        return true; // 행동 사용 허용
    }
    private bool IsHeroineActionOnCooldown(HeroineActionData actionData) // 행동 쿨타임 확인
    {
        if (!heroineActionCooldowns.TryGetValue(actionData, out int remainingCooldown)) { return false; } // 쿨타임 없음 반환

        return remainingCooldown > 0; // 남은 쿨타임 여부 반환
    }
    private bool IsConsecutiveUseLimitReached(HeroineActionData actionData) // 연속 사용 제한 확인
    {
        if (lastHeroineAction != actionData) { return false; } // 연속 사용 아님 반환

        return consecutiveHeroineActionUses >= actionData.MaxConsecutiveUses; // 최대 연속 사용 도달 여부 반환
    }

    private void RegisterHeroineActionUse(HeroineActionData actionData) // 실행 행동 기록
    {
        if (actionData == null) { return; } // 기록 차단

        if (lastHeroineAction == actionData) // 이전 행동과 동일한지 확인
        {
            consecutiveHeroineActionUses += 1; // 연속 사용 횟수 증가
        }
        else // 다른 행동 실행 상태
        {
            lastHeroineAction = actionData; // 마지막 행동 변경
            consecutiveHeroineActionUses = 1; // 연속 사용 횟수 초기화
        }

        heroineActionCooldowns[actionData] = actionData.CooldownTurns; // 행동 쿨타임 적용
    }

    private void ReduceHeroineActionCooldowns() // 모든 행동 쿨타임 감소
    {
        List<HeroineActionData> cooldownActions = new List<HeroineActionData>(heroineActionCooldowns.Keys); // 쿨타임 행동 목록 복사

        foreach (HeroineActionData actionData in cooldownActions) // 모든 쿨타임 행동 반복
        {
            int remainingCooldown = heroineActionCooldowns[actionData]; // 현재 남은 쿨타임 확인
            heroineActionCooldowns[actionData] = Mathf.Max(0, remainingCooldown - 1); // 쿨타임 1 감소
        }
    }




    private void UpdateHeroineIntentUI() // 히로인 행동 예고 UI 갱신
    {
        if (heroineIntentText == null) { return; } // 행동 예고 텍스트 누락 차단

        if (nextHeroineAction == null) { heroineIntentText.text = "Next Action: None"; return; } // 다음 행동 없음 표시

        string targetName = GetHeroineTargetDisplayName(nextHeroineAction.TargetType); // 대상 표시 이름 확인
        string effectName = GetHeroineActionEffectDisplay(nextHeroineAction); // 행동 효과 문구 확인
        heroineIntentText.text = $"Next: {nextHeroineAction.DisplayName}\n{effectName} / Target: {targetName}"; // 행동 효과와 대상 표시
    }


    private string GetHeroineActionEffectDisplay(HeroineActionData actionData) // 행동 효과 표시 문구 반환
    {
        if (actionData == null) { return "Effect: None"; } // 행동 데이터 누락 표시
        if (actionData.ActionType == HeroineActionType.GainShield) { return $"Shield +{actionData.ShieldAmount}"; } // 보호막 효과 표시
        if (actionData.ActionType == HeroineActionType.Heal) { return $"HP +{actionData.HealAmount}"; } // 체력 회복 효과 표시
        if (actionData.ActionType == HeroineActionType.Cleanse) { return $"Cleanse {actionData.CleanseCount} Negative Status"; } // 정화 행동 효과 표시

        if (actionData.ActionType == HeroineActionType.ApplyStatus) // 상태 효과 행동 확인
        {
            StatusEffectData statusData = actionData.AppliedStatusEffect; // 적용 상태 효과 확인
            if (statusData == null) { return "Status: None"; } // 상태 효과 누락 표시

            string amountText = GetStatusAmountDisplay(statusData); // 상태 효과 수치 문구 생성
            return $"{statusData.DisplayName} {amountText} ({statusData.DurationTurns} Turns)"; // 상태 효과 문구 반환
        }

        int currentAttack = GetHeroineCurrentAttack(actionData.Damage); // 상태 효과 포함 예고 공격력 계산
        return $"Damage {currentAttack}"; // 현재 공격 피해 효과 표시
    }
    private string GetStatusAmountDisplay(StatusEffectData statusData) // 상태 효과 수치 문구 반환
    {
        if (statusData == null) { return "0"; } // 상태 효과 누락 처리

        switch (statusData.StatusType) // 상태 효과 종류 확인
        {
            case StatusEffectType.DefenseUp: // 방어력 증가
                return $"+{statusData.Amount}"; // 증가 수치 반환

            case StatusEffectType.AttackDown: // 공격력 감소
                return $"-{statusData.Amount}"; // 감소 수치 반환

            case StatusEffectType.Poison: // 지속 독 피해
                return $"HP -{statusData.Amount}"; // 독 피해 수치 반환

            default: // 정의되지 않은 상태 효과
                return statusData.Amount.ToString(); // 기본 수치 반환
        }
    }
    private string GetHeroineStatusDisplay() // 히로인 상태 효과 UI 문구 생성
    {
        if (activeHeroineStatusEffects.Count == 0) { return "Status: None"; } // 활성 상태 효과 없음 표시

        List<string> statusNames = new List<string>(); // 상태 효과 문구 목록 생성

        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects) // 활성 상태 효과 반복
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; } // 만료 상태 효과 제외

            string amountText = GetStatusAmountDisplay(activeStatus.Data); // 상태 효과 수치 문구 생성
            string statusName = $"{activeStatus.Data.DisplayName} {amountText} ({activeStatus.RemainingTurns})"; // 상태 효과 문구 생성
            statusNames.Add(statusName); // 상태 효과 문구 등록
        }

        if (statusNames.Count == 0) { return "Status: None"; } // 표시 가능한 상태 없음 처리

        return $"Status: {string.Join(", ", statusNames)}"; // 전체 상태 효과 문구 반환
    }
    private void ClearHeroineStatusIcons() // 생성된 히로인 상태 아이콘 제거
    {
        foreach (StatusEffectIconUI statusIconUI in heroineStatusIconUIs) // 생성된 아이콘 반복
        {
            if (statusIconUI == null) { continue; } // 삭제된 아이콘 제외

            statusIconUI.gameObject.SetActive(false); // 중복 표시 방지 비활성화
            Destroy(statusIconUI.gameObject); // 상태 아이콘 오브젝트 제거
        }

        heroineStatusIconUIs.Clear(); // 상태 아이콘 목록 초기화
    }
    private void RefreshHeroineStatusIcons() // 히로인 상태 효과 아이콘 갱신
    {
        ClearHeroineStatusIcons(); // 기존 상태 효과 아이콘 제거

        if (heroineStatusIconContainer == null) { return; } // 아이콘 배치 영역 누락 차단
        if (statusEffectIconPrefab == null) { return; } // 아이콘 프리팹 누락 차단

        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects) // 활성 상태 효과 반복
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; } // 만료 상태 효과 제외
            if (activeStatus.Data == null) { continue; } // 상태 데이터 누락 제외

            StatusEffectIconUI newStatusIcon = Instantiate(statusEffectIconPrefab, heroineStatusIconContainer); // 상태 아이콘 생성
            newStatusIcon.Setup(activeStatus.Data, activeStatus.RemainingTurns, statusEffectTooltipUI); // 상태 효과와 툴팁 정보 적용
            heroineStatusIconUIs.Add(newStatusIcon); // 생성 아이콘 목록 등록
        }
    }
    private string GetHeroineTargetDisplayName(HeroineTargetType targetType) // 대상 규칙 표시 이름 반환
    {
        switch (targetType) // 대상 규칙 확인
        {
            case HeroineTargetType.FirstMonster: return "First Monster";    // 첫 번째 마물 대상 -> 첫 번째 마물 문구 반환
            case HeroineTargetType.RandomMonster: return "Random Monster";   // 무작위 마물 대상 -> 무작위 마물 문구 반환
            case HeroineTargetType.LowestHpMonster: return "Lowest HP Monster";// 최저 HP 마물 대상 -> 최저 HP 마물 문구 반환
            case HeroineTargetType.AllMonsters: return "All Monsters";     // 전체 마물 대상 p -> 전체 마물 문구 반환
            case HeroineTargetType.Player: return "Player";           // 플레이어 직접 대상 -> 플레이어 문구 반환
            case HeroineTargetType.Self: return "Self";             // 히로인 자신 대상 -> 자기 자신 문구 반환
            default: return "Unknown";          // 정의되지 않은 대상 -> 알 수 없는 대상 반환
        }
    }


    private void BeginNextPlayerTurn() // 다음 플레이어 턴 준비
    {
        turnNumber += 1; // 턴 번호 증가
        AddBattleLog(BattleLogCategory.System, "Player turn started."); // 플레이어 턴 시작 기록
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
    }

    private void PrepareMonstersForNewTurn() // 필드 마물 새 턴 준비
    {
        foreach (MonsterUnit monsterUnit in fieldMonsters) // 모든 필드 마물 반복
        {
            if (monsterUnit != null) // 마물 존재 확인
            {
                monsterUnit.PrepareForNewTurn(); // 공격 가능 상태 적용
            }
        }
    }

    private void ShowPlayerTurn() // 플레이어 턴 UI 표시
    {
        turnText.text = "Player Turn"; // 플레이어 턴 문구 설정
        endTurnButton.interactable = true; // 턴 종료 버튼 활성화
        monsterAttackButton.interactable = false; // 공격 버튼 초기 비활성화
        SetHandInteractable(true); // 손패 버튼 활성화
        SetMonsterInteractable(true); // 공격 가능 마물 선택 활성화
    }

    private void SelectMonster(MonsterUnit monsterUnit) // 공격 마물 선택
    {
        if (!isPlayerTurn || isBattleEnded || monsterUnit == null) { return; } // 선택 차단

        if (!monsterUnit.CanAttack) // 마물 공격 가능 여부 확인
        {
            resultText.text = "Monster Cannot Attack"; // 공격 불가 안내
            return; // 선택 차단
        }

        if (selectedMonster != null) // 기존 선택 마물 확인
        {
            selectedMonster.SetSelected(false); // 기존 선택 표시 해제
        }

        selectedMonster = monsterUnit; // 새로운 마물 선택
        selectedMonster.SetSelected(true); // 선택 색상 표시
        monsterAttackButton.interactable = true; // 공격 버튼 활성화
        resultText.text = $"{selectedMonster.MonsterName} Selected"; // 선택 결과 표시
    }

    private void ClearMonsterSelection() // 선택 마물 초기화
    {
        if (selectedMonster != null) // 선택 마물 존재 확인
        {
            selectedMonster.SetSelected(false); // 선택 색상 해제
        }

        selectedMonster = null; // 선택 참조 초기화
        monsterAttackButton.interactable = false; // 공격 버튼 비활성화
    }

    private void DrawCards(int drawCount) // 카드 드로우 처리
    {
        for (int i = 0; i < drawCount; i++) // 드로우 횟수 반복
        {
            RefillDrawPileIfNeeded(); // 드로우 더미 보충 확인

            if (drawPile.Count == 0) { return; } // 카드 부족 상태 종료

            CardData drawnCard = drawPile[0]; // 첫 번째 카드 선택
            drawPile.RemoveAt(0); // 드로우 더미 카드 제거
            CreateCardButton(drawnCard); // 손패 카드 버튼 생성
        }
    }

    private void RefillDrawPileIfNeeded() // 드로우 더미 재보충
    {
        if (drawPile.Count > 0 || discardPile.Count == 0) { return; } // 보충 불필요 조건

        drawPile.AddRange(discardPile); // 버린 카드 드로우 더미 이동
        discardPile.Clear(); // 버린 카드 더미 초기화
    }

    private void CreateCardButton(CardData cardData) // 손패 카드 버튼 생성
    {
        Button newCardButton = Instantiate(cardButtonPrefab, handPanel); // 카드 버튼 복제
        TMP_Text cardText = newCardButton.GetComponentInChildren<TMP_Text>(); // 카드 버튼 텍스트 검색
        string monsterName = cardData.SummonMonster != null ? cardData.SummonMonster.MonsterName : "None"; // 소환 마물 이름 확인

        cardText.text = $"{cardData.CardName}\nCost: {cardData.ManaCost}\nSummon: {monsterName}"; // 카드 정보 표시
        newCardButton.onClick.RemoveAllListeners(); // 기존 버튼 연결 초기화
        newCardButton.onClick.AddListener(() => TryPlayCard(cardData, newCardButton)); // 카드 사용 함수 연결
        handButtons.Add(newCardButton); // 손패 버튼 목록 등록
    }

    private void TryPlayCard(CardData cardData, Button cardButton) // 카드 사용 처리
    {
        if (!isPlayerTurn || isBattleEnded) { return; } // 카드 사용 차단

        if (cardData.SummonMonster == null) // 마물 데이터 누락 확인
        {
            resultText.text = "Missing Monster Data"; // 데이터 누락 안내
            return; // 카드 사용 차단
        }

        if (fieldMonsters.Count >= maxFieldMonsterCount) // 필드 최대 수 확인
        {
            resultText.text = "Field Full"; // 필드 초과 안내
            return; // 카드 사용 차단
        }

        if (currentMana < cardData.ManaCost) // 마나 부족 확인
        {
            resultText.text = "Not Enough Mana"; // 마나 부족 안내
            return; // 카드 사용 차단
        }

        currentMana -= cardData.ManaCost; // 카드 비용 차감
        SummonMonster(cardData.SummonMonster); // 마물 필드 소환
        AddBattleLog(BattleLogCategory.PlayerAction, $"{cardData.CardName}: Summoned {cardData.SummonMonster.MonsterName}."); // 카드 소환 기록
        discardPile.Add(cardData); // 사용 카드 버린 더미 이동
        handButtons.Remove(cardButton); // 손패 버튼 목록 제거
        Destroy(cardButton.gameObject); // 카드 버튼 제거
        resultText.text = string.Empty; // 안내 텍스트 초기화
        UpdateBattleUI(); // 카드 사용 결과 표시
    }

    private void SummonMonster(MonsterData monsterData) // 마물 필드 소환
    {
        MonsterUnit newMonsterUnit = Instantiate(monsterUnitPrefab, monsterFieldContainer); // 마물 UI 복제
        newMonsterUnit.Initialize(monsterData, SelectMonster); // 마물 데이터와 선택 콜백 초기화
        fieldMonsters.Add(newMonsterUnit); // 필드 마물 목록 등록
        newMonsterUnit.SetPlayerTurnInteraction(isPlayerTurn); // 소환 대기 선택 상태 적용
    }

    private void SetHandInteractable(bool isInteractable) // 손패 버튼 활성 상태 변경
    {
        foreach (Button handButton in handButtons) // 모든 손패 버튼 반복
        {
            if (handButton != null) // 버튼 존재 확인
            {
                handButton.interactable = isInteractable; // 버튼 활성 상태 적용
            }
        }
    }

    private void SetMonsterInteractable(bool isInteractable) // 마물 선택 활성 상태 변경
    {
        foreach (MonsterUnit monsterUnit in fieldMonsters) // 모든 필드 마물 반복
        {
            if (monsterUnit != null) // 마물 존재 확인
            {
                monsterUnit.SetPlayerTurnInteraction(isInteractable); // 마물 선택 상태 적용
            }
        }
    }

    private void ClearHand() // 손패 버튼 전체 제거
    {
        foreach (Button handButton in handButtons) // 모든 손패 버튼 반복
        {
            if (handButton != null) // 버튼 존재 확인
            {
                Destroy(handButton.gameObject); // 카드 버튼 제거
            }
        }

        handButtons.Clear(); // 손패 버튼 목록 초기화
    }

    private void ClearMonsterField() // 마물 필드 전체 제거
    {
        foreach (MonsterUnit monsterUnit in fieldMonsters) // 모든 필드 마물 반복
        {
            if (monsterUnit != null) // 마물 존재 확인
            {
                Destroy(monsterUnit.gameObject); // 마물 UI 제거
            }
        }

        fieldMonsters.Clear(); // 필드 마물 목록 초기화
        ClearMonsterSelection(); // 마물 선택 상태 초기화
    }

    private void UpdateBattleUI() // 전투 수치 UI 갱신
    {
        int currentHeroineDefense = GetHeroineCurrentDefense(); // 상태 효과 포함 히로인 방어력 계산
        turnNumberText.text = $"Turn {turnNumber}"; // 턴 번호 표시
        playerHpText.text = $"Player HP: {playerCurrentHp} / {playerMaxHp}"; // 플레이어 체력 표시
        playerShieldText.text = $"Shield: {playerCurrentShield}"; // 플레이어 보호막 표시
        manaText.text = $"Mana: {currentMana} / {maximumMana}"; // 마나 표시
        heroineHpText.text = $"Heroine HP: {heroineCurrentHp} / {heroineMaxHp}"; // 히로인 체력 표시
        heroineDefenseText.text = $"DEF: {currentHeroineDefense}"; // 현재 히로인 방어력 표시
        heroineShieldText.text = $"Shield: {heroineCurrentShield} / {heroineMaxShield}"; // 히로인 현재 및 최대 보호막 표시
        if (heroineStatusText != null) { heroineStatusText.text = GetHeroineStatusDisplay(); } // 히로인 상태 효과 표시
        RefreshHeroineStatusIcons(); // 히로인 상태 효과 아이콘 갱신
        lustText.text = $"Lust: {heroineLust} / 100"; // 성욕 게이지 표시
        UpdateHeroineIntentUI(); // 히로인 행동 예고 표시
    }

    private void EndBattle(string resultMessage) // 전투 종료 처리
    {
        isBattleEnded = true; // 전투 종료 상태 설정
        isPlayerTurn = false; // 플레이어 턴 해제
        ClearMonsterSelection(); // 마물 선택 상태 해제
        turnText.text = "Battle End"; // 전투 종료 문구 설정
        resultText.text = resultMessage; // 전투 결과 표시
        AddBattleLog(BattleLogCategory.System, $"Battle ended: {resultMessage}."); // 전투 종료 기록
        heroineIntentText.text = "Next Action: None"; // 행동 예고 종료 표시
        endTurnButton.interactable = false; // 턴 종료 버튼 비활성화
        monsterAttackButton.interactable = false; // 공격 버튼 비활성화
        SetHandInteractable(false); // 손패 버튼 비활성화
        SetMonsterInteractable(false); // 마물 선택 비활성화
    }
}