using System; // 콜백 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public class MonsterUnit : MonoBehaviour // 필드 마물 UI 관리
{ // 클래스 시작
    [Header("Monster UI")] // 마물 UI 구분
    [SerializeField] private TMP_Text monsterNameText;      // 마물 이름 텍스트
    [SerializeField] private TMP_Text monsterHpText;        // 마물 체력 텍스트
    [SerializeField] private TMP_Text monsterAttackText;    // 마물 공격력 텍스트
    [SerializeField] private TMP_Text monsterLustDamageText; // 성욕 피해 텍스트
    [SerializeField] private TMP_Text monsterDefenseText;   // 마물 방어력 텍스트
    [SerializeField] private TMP_Text monsterShieldText;    // 마물 보호막 텍스트
    [SerializeField] private TMP_Text monsterStateText;     // 마물 행동 상태 텍스트
    [SerializeField] private Button selectButton;           // 마물 선택 버튼
    [SerializeField] private Image backgroundImage;         // 마물 배경 이미지

    [Header("Selection Colors")] // 선택 색상 구분
    [SerializeField] private Color normalColor   = new Color(0.34f, 0.24f, 0.45f, 1f); // 기본 배경 색상
    [SerializeField] private Color selectedColor = new Color(0.25f, 0.65f, 0.35f, 1f); // 선택 배경 색상
    [SerializeField] private Color heroineTargetColor = new Color(0.9f, 0.35f, 0.2f, 1f); // 히로인 타겟 색상

    private MonsterData monsterData; // 연결 마물 데이터
    private MonsterActionState actionState; // 현재 행동 상태
    private Action<MonsterUnit> onSelected; // 마물 선택 콜백

    private int currentHp; // 현재 마물 체력
    private int currentShield; // 현재 마물 보호막

    private bool isSelected; // 플레이어 선택
    private bool isHeroineTargeted; // 히로인 타겟
    public int Attack => monsterData != null ? monsterData.Attack : 0; // 현재 마물 공격력 반환
    public int LustDamage => monsterData != null ? monsterData.LustDamage : 0; // 성욕 피해 반환
    public bool IsTaunting => monsterData != null && monsterData.IsTaunting;
    public StatusEffectData AttackStatusEffect => monsterData != null ? monsterData.AttackStatusEffect : null; // 공격 상태 효과 반환
    public int Defense => monsterData != null ? monsterData.Defense : 0; // 현재 마물 방어력 반환
    public int CurrentShield => currentShield; // 현재 마물 보호막 반환
    public int CurrentHp => currentHp; // 현재 마물 체력 반환
    public int MaxHp => monsterData != null ? monsterData.MaxHp : 0; // 마물 최대 체력 반환
    public string MonsterName => monsterData != null ? monsterData.MonsterName : "Unknown"; // 현재 마물 이름 반환
    public bool IsDead => currentHp <= 0; // 마물 사망 여부 반환
    public bool CanAttack => actionState == MonsterActionState.Ready && !IsDead; // 현재 공격 가능 여부 반환

    public void Initialize(MonsterData data, Action<MonsterUnit> selectedCallback) // 마물 정보 초기화
    { 
        monsterData = data; // 마물 데이터 저장
        onSelected = selectedCallback; // 선택 콜백 저장
        currentHp = monsterData.MaxHp; // 현재 체력 초기화
        currentShield = Mathf.Max(0, monsterData.StartingShield); // 현재 보호막 초기화
        actionState = MonsterActionState.Summoning; // 소환 대기 상태 설정
        selectButton.onClick.RemoveAllListeners(); // 기존 버튼 이벤트 제거
        selectButton.onClick.AddListener(HandleSelectButton); // 마물 선택 이벤트 연결
        SetSelected(false); // 선택 상태 초기화
        SetHeroineTargeted(false);
        SetPlayerTurnInteraction(true); // 초기 선택 가능 여부 갱신
        UpdateMonsterUI(); // 마물 UI 갱신
    } 

    public void PrepareForNewTurn() // 새 플레이어 턴 준비
    {
        if (IsDead) { return; }  // // 사망 상태 확인 -> 상태 변경 차단
       
        actionState = MonsterActionState.Ready; // 공격 가능 상태 설정
        SetSelected(false); // 선택 상태 초기화
        UpdateMonsterUI(); // 행동 상태 UI 갱신
    } 

    public void MarkActed() // 공격 완료 상태 설정
    { 
        actionState = MonsterActionState.Acted; // 행동 완료 상태 적용

        SetSelected(false); // 선택 상태 해제
        SetPlayerTurnInteraction(true); // 버튼 상태 갱신
        UpdateMonsterUI(); // 행동 상태 UI 갱신
    } 

    public DamageResult TakeDamage(int incomingAttackPower) // 마물 보호막 포함 피해 처리
    { 
        DamageResult damageResult = DamageCalculator.CalculateDamageWithShield(incomingAttackPower, Defense, currentShield); // 보호막과 방어력 적용 피해 계산
        currentShield = damageResult.RemainingShield; // 남은 보호막 적용
        currentHp = Mathf.Max(0, currentHp - damageResult.HpDamage); // 실제 HP 피해 적용
        UpdateMonsterUI(); // 변경 전투 수치 표시
        return damageResult; // 피해 결과 반환
    }

    public void SetPlayerTurnInteraction(bool isPlayerTurn) // 플레이어 턴 상호작용 설정
    { 
        selectButton.interactable = isPlayerTurn && CanAttack; // 공격 가능 마물만 선택 허용
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackgroundColor();
    }

    public void SetHeroineTargeted(bool targeted)
    {
        isHeroineTargeted = targeted;
        UpdateBackgroundColor();
    }

    private void UpdateBackgroundColor()
    {
        if (backgroundImage == null) { return; }

        if (isSelected)
        {
            backgroundImage.color = selectedColor;
            return;
        }

        if (isHeroineTargeted)
        {
            backgroundImage.color = heroineTargetColor;
            return;
        }

        backgroundImage.color = normalColor;
    }

    private void HandleSelectButton() // 마물 선택 버튼 처리
    { 
        if (!CanAttack) { return; }  //공격 불가 상태 확인 -> 선택 차단

        onSelected?.Invoke(this); // BattleManager 선택 전달
    } 

    private void UpdateMonsterUI() // 마물 UI 표시 갱신
    { 
        monsterNameText.text    = monsterData.MonsterName; // 마물 이름 표시
        monsterHpText.text      = $"HP: {currentHp} / {monsterData.MaxHp}"; // 마물 체력 표시
        monsterAttackText.text  = $"ATK: {monsterData.Attack}"; // 마물 공격력 표시
        if (monsterLustDamageText != null)  { monsterLustDamageText.text = $"LST: {monsterData.LustDamage}"; }
        monsterDefenseText.text = $"DEF: {monsterData.Defense}"; // 마물 방어력 표시
        monsterShieldText.text  = $"Shield: {currentShield}"; // 마물 보호막 표시
        string tauntText = IsTaunting ? " | Taunt" : string.Empty;
        monsterStateText.text   = $"State: {GetStateLabel()}"; // 마물 행동 상태 표시
    }

    private string GetStateLabel() // 행동 상태 표시 이름 반환
    { 
        switch (actionState) // 현재 행동 상태 확인
        { 
            case MonsterActionState.Summoning:  return "Waiting"; // // 소환 대기 상태 -> 대기 문구 반환
            case MonsterActionState.Ready:      return "Ready"; //  // 공격 가능 상태 -> 준비 문구 반환
            case MonsterActionState.Acted:      return "Acted";  // 공격 완료 상태 -> 완료 문구 반환
            default:                            return "Unknown";// 정의되지 않은 상태 -> 알 수 없음 문구 반환
        }
    } 
} 