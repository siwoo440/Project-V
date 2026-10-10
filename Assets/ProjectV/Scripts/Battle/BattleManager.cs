using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager : MonoBehaviour // 기본 전투 흐름 관리
{
    [Header("전투 UI")] // 전투 UI 구분
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
    [SerializeField] private Slider heroineLustSlider; // 성욕 게이지

    [SerializeField] private TMP_Text heroineIntentText;    // 히로인 행동 예고 텍스트
    [SerializeField] private Image heroineIntentIcon;       // 히로인 행동 예고 아이콘

    [Header("게이지")] // 수치 막대 구분
    [SerializeField] private UIGauge playerHpGauge;    // 플레이어 체력 게이지
    [SerializeField] private UIGauge manaGauge;        // 마나 게이지
    [SerializeField] private UIGauge heroineHpGauge;   // 히로인 체력 게이지
    [SerializeField] private UIGauge heroineLustGauge; // 히로인 성욕 게이지
    [SerializeField] private TMP_Text resultText;           // 전투 결과 텍스트
    [SerializeField] private BattleLogUI battleLogUI; // 전투 로그 UI
    [SerializeField] private Button endTurnButton;          // 턴 종료 버튼
    [SerializeField] private Button monsterAttackButton;    // 마물 공격 버튼
    [SerializeField] private Button lustAttackButton; // 성욕 공격 버튼
    [SerializeField] private Button skillButton; // 마물 스킬 버튼

    [Header("카드 UI")] // 카드 UI 구분
    [SerializeField] private Transform handPanel;       // 손패 카드 배치 영역
    [SerializeField] private Button cardButtonPrefab;   // 카드 버튼 프리팹
    [SerializeField] private TMP_Text deckStatusText; // 덱 수량 텍스트

    [Header("마물 필드")] // 마물 필드 구분
    [SerializeField] private Transform monsterFieldContainer;// 마물 배치 영역
    [SerializeField] private MonsterUnit monsterUnitPrefab;  // 마물 UI 프리팹
    [SerializeField] private int maxFieldMonsterCount = 8;   // 최대 필드 마물 수

    [Header("덱 설정")] // 덱 설정 구분
    [SerializeField] private List<CardData> deckCards = new List<CardData>(); // 전투 시작 덱 목록
    [SerializeField] private int startingHandCount = 3; // 시작 손패 수
    [SerializeField] private int turnDrawCount = 1;     // 턴 시작 드로우 수
    [SerializeField, Min(1)] private int requiredDeckSize = 30; // 필요 덱 장수
    [SerializeField, Min(1)] private int maxHandSize = 10; // 최대 손패
    [SerializeField] private bool useProgressDeck = true; // 진행 데이터 덱 사용
    [SerializeField] private bool validateDeckOnStart = true; // 전투 시작 검증
    [SerializeField] private bool shuffleDeckAtBattleStart = true; // 시작 셔플

    [Header("전투 결과")]
    [SerializeField] private BattleRewardData battleRewardData;
    [SerializeField] private BattleResultUI battleResultUI;

    [Header("히로인 AI")] // 히로인 AI 구분
    [SerializeField] private List<HeroineActionData> heroineActions = new List<HeroineActionData>(); // 히로인 행동 데이터 목록

    [Header("타입 시너지")] // 타입 시너지 구분
    [SerializeField]
    private List<SynergyData> synergyDataList = new List<SynergyData>(); // 시너지 데이터 목록

    [SerializeField] private TMP_Text synergyText; // 시너지 표시 텍스트

    [Header("소환사 UI")] // 소환사 스킬 UI 구분
    [SerializeField] private Button summonerSkillButton;   // 소환사 액티브 스킬 버튼
    [SerializeField] private TMP_Text summonerSkillText;   // 액티브 스킬 효과 설명
    [SerializeField] private TMP_Text summonerPassiveText; // 장착 패시브 표시
    [SerializeField] private Button battleItemButton;      // 소모성 아이템 사용 버튼

    [Header("전투 설정")] // 전투 설정 구분
    [SerializeField] private int playerMaxHp = 30;              // 플레이어 최대 체력
    [SerializeField] private int playerDefense = 0;             // 플레이어 방어력
    [SerializeField] private int playerStartingShield = 2;      // 플레이어 시작 보호막
    [SerializeField] private int heroineMaxHp = 30;             // 히로인 최대 체력
    [SerializeField, Min(1)] private int heroineMaxLust = 100; // 최대 성욕
    [SerializeField] private int heroineDefense = 1;            // 히로인 방어력
    [SerializeField] private int heroineStartingShield = 3;     // 히로인 시작 보호막
    [SerializeField] private int heroineMaxShield = 10;         // 히로인 최대 보호막
    [SerializeField] private float heroineActionDelay = 0.8f;   // 히로인 행동 대기 시간


    private readonly List<CardCopy> battleDeck = new List<CardCopy>();          // 이번 전투에 사용할 사본 목록
    private readonly List<CardCopy> drawPile = new List<CardCopy>();            // 드로우 더미
    private readonly List<CardCopy> discardPile = new List<CardCopy>();         // 버린 카드 더미
    private readonly List<Button> handButtons = new List<Button>();             // 현재 손패 버튼 목록
    private readonly List<MonsterUnit> fieldMonsters = new List<MonsterUnit>(); // 현재 필드 마물 목록
    private readonly List<ActiveStatusEffect> activeHeroineStatusEffects = new List<ActiveStatusEffect>(); // 히로인 활성 상태 효과 목록
    private readonly List<StatusEffectIconUI> heroineStatusIconUIs = new List<StatusEffectIconUI>(); // 생성된 상태 효과 아이콘 목록
    private readonly Dictionary<HeroineActionData, int> heroineActionCooldowns = new Dictionary<HeroineActionData, int>(); // 행동별 남은 쿨타임

    private MonsterUnit selectedMonster; // 현재 선택 마물
    private MonsterUnit previewedHeroineTarget; // 히로인 공격 예정 대상

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
    private BattleResultData lastBattleResult;
    public BattleResultData LastBattleResult => lastBattleResult;
    private void Start() // 전투 초기화 진입
    {
        InitializeBattle(); // 기본 전투 초기화
    }
    private void InitializeBattle()
    {
        lastBattleResult = null;

        if (battleResultUI != null)
        {
            battleResultUI.Hide();
        }

        ApplyProgressDeck(); // 진행 데이터 덱 적용
        PrepareGrimoireForBattle(); // 그리모어 강화 수치 확인
        PrepareSummonerForBattle(); // 장착한 소환사 스킬과 패시브 확인
        PrepareBattleItemForBattle(); // 장착한 소모성 아이템 확인

        if (!ValidateBattleDeckBeforeStart())
        {
            UpdateSummonerUI(); // 덱 오류 시 스킬 버튼 비활성화
            UpdateBattleItemUI();
            return;
        }

        playerCurrentHp = PlayerMaxHp; // 플레이어 체력 초기화 (패시브 보정 포함)
        playerCurrentShield =
            Mathf.Max(0, playerStartingShield) +
            Grimoire(GrimoireEffectType.StartingShield); // 플레이어 보호막 초기화 (보호의 문장 포함)
        PrepareHeroineBattle(); // 히로인전 데이터가 있으면 능력치와 행동을 그 전투의 값으로 바꾼다.
        heroineCurrentHp = heroineMaxHp; // 히로인 체력 초기화
        heroineCurrentShield = Mathf.Clamp(heroineStartingShield, 0, heroineMaxShield); // 최대치 범위 내 보호막 초기화

        heroineLust = 0; // 성욕 게이지 초기화
        turnNumber = 1; // 첫 번째 턴 설정
        maximumMana = StartingMaximumMana; // 첫 턴 최대 마나 설정 (기획서 A.18)
        currentMana =
            maximumMana + Grimoire(GrimoireEffectType.ManaVessel); // 현재 마나 충전 (마나 그릇의 임시 마나 포함)

        isPlayerTurn = true; // 플레이어 턴 설정
        isBattleEnded = false; // 전투 진행 상태 설정
        activeHeroineStatusEffects.Clear(); // 히로인 상태 효과 초기화
        heroineActionCooldowns.Clear(); // 행동 쿨타임 초기화

        lastHeroineAction = null; // 마지막 행동 초기화
        consecutiveHeroineActionUses = 0; // 연속 사용 횟수 초기화
        ResetHeroineRules(); // 행동 계획, 반격, 페이즈 전환, 절정 상태 초기화
        PlanHeroineActions(); // 첫 히로인 턴의 행동 예고
        selectedMonster = null; // 선택 마물 초기화
        resultText.text = string.Empty; // 결과 텍스트 초기화
        if (battleLogUI != null) { battleLogUI.Clear(); } // 이전 전투 로그 초기화
        AddBattleLog(BattleLogCategory.System, "전투를 시작했습니다."); // 전투 시작 기록
        LogHeroineBattle(); // 히로인전이면 상대의 능력치와 난이도 기록
        LogSummonerLoadout(); // 장착한 소환사 스킬과 패시브 기록
        LogGrimoireLoadout(); // 적용된 그리모어 강화 기록
        LogBattleItemLoadout(); // 가져온 소모성 아이템 기록
        AddBattleLog(BattleLogCategory.System, "플레이어 턴을 시작했습니다.");
        SetAttackButtonsInteractable(false); // 공격 버튼 비활성화
        drawPile.Clear(); // 드로우 더미 초기화
        discardPile.Clear(); // 버린 카드 더미 초기화
        ClearHand(); // 기존 손패 초기화
        ClearMonsterField(); // 기존 마물 필드 초기화
        PrepareEnemyBattle(); // 전투 종류에 맞게 화면을 바꾸고 적 마물을 놓는다
        PrepareHeroineArt(); // 히로인 그림이 있으면 보여 준다


        drawPile.AddRange(battleDeck);

        if (shuffleDeckAtBattleStart) {ShuffleCards(drawPile); }
        AddBattleLog( BattleLogCategory.System, $"덱 준비 완료: {drawPile.Count}장"  );
        RefreshSynergies(); // 시너지 초기화
        DrawCards(
            startingHandCount +
            Grimoire(GrimoireEffectType.StartingHand) +
            GetPassiveAmount(SummonerPassiveType.QuickStudy)
        ); // 시작 손패 (기억 확장과 빠른 이해 패시브의 추가 드로우 포함)
        RefreshHeroineTargetPreview();
        ShowPlayerTurn();
        UpdateBattleUI();
    }
    public void EndPlayerTurn() // 플레이어 턴 종료
    {
        if (!isPlayerTurn || isBattleEnded) { return; } // 중복 실행 차단

        CancelSummonerSkillTargeting(string.Empty); // 스킬 대상 선택 중이면 취소
        CancelBattleItemTargeting(string.Empty); // 아이템 대상 선택 중이면 취소
        AddBattleLog(BattleLogCategory.System, "플레이어 턴을 종료했습니다."); // 플레이어 턴 종료 기록

        if (ApplyGrimoireTurnEnd()) { return; } // 그리모어 강화의 턴 종료 효과 (여운, 마나 비축)

        ApplyTurnEndSynergies(); // 턴 종료 시너지 처리
        ClearSummonerTurnEffects(); // 이번 턴 한정 스킬 효과 제거
        isPlayerTurn = false; // 플레이어 턴 종료
        ClearMonsterSelection(); // 마물 선택 상태 해제
        endTurnButton.interactable = false; // 턴 종료 버튼 비활성화
        SetAttackButtonsInteractable(false); // 공격 버튼 비활성화
        SetHandInteractable(false); // 손패 버튼 비활성화
        SetMonsterInteractable(false); // 마물 선택 비활성화
        SetEnemyInteractable(false); // 적 마물 선택 비활성화
        turnText.text = isEnemyBattle ? "적 턴" : "히로인 턴"; // 상대 턴 표시
        StartCoroutine(isEnemyBattle ? EnemyTurnRoutine() : HeroineTurnRoutine()); // 상대 행동 시작
    }
    private IEnumerator HeroineTurnRoutine() // 히로인 턴 순차 처리
    {
        AddBattleLog(BattleLogCategory.System, "히로인 턴을 시작했습니다."); // 히로인 턴 시작 기록
        ReduceHeroineStatusDurations( StatusDurationTiming.AfterPlayerTurn );
        ReduceMonsterStatusDurations( StatusDurationTiming.AfterPlayerTurn );
        ReduceMonsterCooldowns(); // 마물 재사용 대기시간 감소
        ApplyHeroineStartTurnStatusEffects();
        CheckHeroinePhase(); // 독 피해로 HP가 기준 이하가 됐으면 페이즈 전환

        UpdateBattleUI(); // 독 피해 결과 UI 갱신

        if (TryEndBattleByHeroineHp()) { yield break; } // 독 피해로 쓰러졌으면 승리

        heroineTurnsTaken += 1;

        List<HeroineActionData> turnActions =
            new List<HeroineActionData>(plannedHeroineActions); // 예고한 행동을 앞에서부터 실행한다. (기획서 D.2.3)

        if (turnActions.Count == 0) { turnActions.Add(null); } // 쓸 수 있는 행동이 없어도 턴은 넘긴다.

        for (int i = 0; i < turnActions.Count; i++)
        {
            nextHeroineAction = turnActions[i];

            if (i > 0)
            {
                RefreshHeroineTargetPreview(); // 앞선 행동으로 바뀐 상황에서 대상을 다시 정한다.
                UpdateBattleUI();
            }

            yield return new WaitForSeconds(heroineActionDelay); // 행동 전 대기

            HeroineActionData executedAction = nextHeroineAction; // 이번 실행 행동 저장
            ShowHeroineAttackArt(executedAction); // 공격 행동이면 공격 그림
            ResolveHeroineAttack(); // 히로인 행동 실행
            RegisterHeroineActionUse(executedAction); // 실행 행동과 쿨타임 기록

            UpdateBattleUI(); // 행동 결과 갱신

            if (playerCurrentHp <= 0) // 플레이어 사망 확인
            {
                EndBattle(BattleOutcome.Defeat); // 패배 처리
                yield break; // 코루틴 종료
            }
        }

        ReduceHeroineStatusDurations(StatusDurationTiming.AfterHeroineTurn); // 히로인 행동 기준 상태 지속시간 감소
        ReduceMonsterStatusDurations(StatusDurationTiming.AfterHeroineTurn);

        UpdateBattleUI();

        yield return new WaitForSeconds(heroineActionDelay); // 다음 턴 전 대기
        BeginNextPlayerTurn(); // 다음 플레이어 턴 시작
    }
    private void BeginNextPlayerTurn() // 다음 플레이어 턴 준비
    {
        if (TryEndBattleByClimax()) { return; } // 히로인의 턴이 지난 뒤에도 성욕이 최대이면 절정 승리 (기획서 5.14.4)

        turnNumber += 1;
        AddBattleLog(BattleLogCategory.System, "플레이어 턴을 시작했습니다.");

        ApplyMonsterStartTurnStatusEffects();
        ApplyTurnStartSynergies(); // 턴 시작 시너지 처리
        PlanHeroineActions(); // 현재 쿨타임 기준으로 다음 히로인 턴의 행동 예고
        ReduceHeroineActionCooldowns(); // 행동 선택 후 쿨타임 감소

        maximumMana = Mathf.Min(MaximumManaLimit, maximumMana + 1); // 최대 마나 증가
        summonerSkillUsedThisTurn = false; // 소환사 스킬 사용 횟수 초기화
        currentMana = maximumMana; // 마나 전체 회복
        resultText.text = string.Empty; // 안내 텍스트 초기화

        DrawCards(turnDrawCount); // 턴 시작 카드 드로우
        ApplyGrimoireTurnStart(); // 그리모어 강화의 턴 시작 효과
        isPlayerTurn = true; // 플레이어 턴 설정

        PrepareMonstersForNewTurn();
        RefreshHeroineTargetPreview();
        ShowPlayerTurn();
        UpdateBattleUI();
    }



}