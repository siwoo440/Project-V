using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUnit : MonoBehaviour
{
    [Header("마물 UI")]
    [SerializeField] private TMP_Text monsterNameText;
    [SerializeField] private TMP_Text monsterHpText;
    [SerializeField] private TMP_Text monsterAttackText;
    [SerializeField] private TMP_Text monsterLustDamageText;
    [SerializeField] private TMP_Text monsterDefenseText;
    [SerializeField] private TMP_Text monsterShieldText;
    [SerializeField] private TMP_Text monsterStateText;
    [SerializeField] private TMP_Text monsterSkillText;
    [SerializeField] private Transform statusIconContainer;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private UIGauge hpGauge;       // 체력 게이지
    [SerializeField] private Image typeIconImage;   // 계열 문양 (일러스트가 생기기 전까지의 자리 표시)

    [Header("선택 색상")]
    [SerializeField]
    private Color normalColor =
        new Color(0.34f, 0.24f, 0.45f, 1f);

    [SerializeField]
    private Color selectedColor =
        new Color(0.25f, 0.65f, 0.35f, 1f);

    [SerializeField]
    private Color heroineTargetColor =
        new Color(0.9f, 0.35f, 0.2f, 1f);

    [SerializeField]
    private Color skillTargetColor =
        new Color(0.25f, 0.5f, 0.85f, 1f); // 소환사 스킬 대상 후보 표시

    private readonly List<ActiveStatusEffect> activeStatusEffects =
        new List<ActiveStatusEffect>();

    private readonly List<StatusEffectIconUI> statusIconUIs =
        new List<StatusEffectIconUI>();

    private MonsterData monsterData;
    private MonsterActionState actionState;
    private Action<MonsterUnit> onSelected;
    private StatusEffectIconUI statusEffectIconPrefab;
    private StatusEffectTooltipUI statusEffectTooltipUI;

    private int currentHp;
    private int currentShield;
    private bool isSelected;
    private bool isHeroineTargeted;

    private int currentCooldown;

    private int enhanceLevel = CardEnhanceRules.MinLevel; // 소환에 사용한 사본의 강화 단계

    private int synergyMaxHpBonus;
    private int synergyAttackBonus;
    private int synergyDefenseBonus;
    private int synergyLustBonus;

    private int summonerAttackBonus; // 소환사 패시브로 받는 공격 보정
    private int summonerLustBonus;   // 소환사 패시브와 스킬로 받는 성욕 보정
    private int turnAttackBonus;     // 이번 턴에만 유지되는 공격 보정 (집중 명령)

    private CardCopy sourceCard;      // 소환에 사용한 카드 사본 (토큰은 없음)
    private bool isSkillTargetable;   // 소환사 스킬 대상 후보 여부
    private bool playerTurnInteraction; // 플레이어 턴 조작 허용 여부

    private int runtimeMaxHp;
    private int runtimeAttack;
    private int runtimeLustDamage;
    private int runtimeDefense;

    public int Attack => GetCurrentAttack();
    public int LustDamage =>
        Mathf.Max(0, runtimeLustDamage + synergyLustBonus + summonerLustBonus);
    public int Defense => GetCurrentDefense();
    public int CurrentShield => currentShield;
    public int CurrentHp => currentHp;
    public int MaxHp =>
        Mathf.Max(1, runtimeMaxHp + synergyMaxHpBonus);

    public StatusEffectData AttackStatusEffect =>
        monsterData != null ? monsterData.AttackStatusEffect : null;

    public string MonsterName =>
        monsterData != null ? monsterData.MonsterName : "알 수 없음";

    public bool IsTaunting =>
        monsterData != null && monsterData.IsTaunting;

    public bool IsDead => currentHp <= 0;

    public bool CanAttack =>
        actionState == MonsterActionState.Ready && !IsDead;

    public bool HasActed =>
        actionState == MonsterActionState.Acted && !IsDead; // 이번 턴 행동을 마쳤는지 여부

    public CardCopy SourceCard => sourceCard; // 소환에 사용한 카드 사본 반환

    public MonsterData Data => monsterData; // 마물 데이터 반환

    public MonsterEffectData ActiveSkill =>
        monsterData == null
            ? null
            : monsterData.GetEffect(MonsterEffectTrigger.ActiveSkill); // 능동 스킬 반환

    public int CurrentCooldown => currentCooldown; // 남은 재사용 대기시간 반환

    public int EnhanceLevel => enhanceLevel; // 소환 사본의 강화 단계 반환

    public bool HasActiveSkill => ActiveSkill != null; // 능동 스킬 보유 여부

    public bool CanUseSkill =>
        CanAttack && HasActiveSkill && currentCooldown <= 0; // 스킬 사용 가능 여부

    public void Initialize(

        MonsterData data,
        int summonEnhanceLevel,
        Action<MonsterUnit> selectedCallback,
        StatusEffectIconUI iconPrefab,
        StatusEffectTooltipUI tooltipUI
    )


    {
        if (data == null) { return; }

        monsterData = data;
        enhanceLevel = CardEnhanceRules.ClampLevel(summonEnhanceLevel);
        onSelected = selectedCallback;
        statusEffectIconPrefab = iconPrefab;
        statusEffectTooltipUI = tooltipUI;
        InitializeRuntimeStats();

        currentHp = MaxHp;
        currentShield = Mathf.Max(0, monsterData.StartingShield);
        currentCooldown = 0; // 재사용 대기시간 초기화
        synergyMaxHpBonus = 0; // 시너지 보정 초기화
        synergyAttackBonus = 0;
        synergyDefenseBonus = 0;
        synergyLustBonus = 0;
        summonerAttackBonus = 0; // 소환사 보정 초기화
        summonerLustBonus = 0;
        turnAttackBonus = 0;
        sourceCard = null;
        isSkillTargetable = false;
        actionState = MonsterActionState.Summoning;

        activeStatusEffects.Clear();
        ClearStatusIcons();

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(HandleSelectButton);
        }

        if (typeIconImage != null)
        {
            Sprite typeSprite = UISkin.Get(UISkin.TypeIconKey(monsterData.MainType));

            typeIconImage.sprite = typeSprite;
            typeIconImage.preserveAspect = true;
            typeIconImage.gameObject.SetActive(typeSprite != null); // 문양이 없으면 줄을 접는다.
        }

        SetSelected(false);
        SetHeroineTargeted(false);
        SetPlayerTurnInteraction(true);
        UpdateMonsterUI();
        RefreshStatusIcons();
    }

    public void PrepareForNewTurn()
    {
        if (IsDead) { return; }

        actionState = MonsterActionState.Ready;
        SetSelected(false);
        UpdateMonsterUI();
    }

    public void MarkActed()
    {
        actionState = MonsterActionState.Acted;

        SetSelected(false);
        SetPlayerTurnInteraction(true);
        UpdateMonsterUI();
    }

    public DamageResult TakeDamage(int incomingAttackPower)
    {
        DamageResult damageResult =
            DamageCalculator.CalculateDamageWithShield(
                incomingAttackPower,
                Defense,
                currentShield
            );

        currentShield = damageResult.RemainingShield;
        currentHp = Mathf.Max(0, currentHp - damageResult.HpDamage);

        UpdateMonsterUI();

        return damageResult;
    }

    public void ApplyOrRefreshStatus(StatusEffectData statusData)
    {
        if (statusData == null) { return; }

        foreach (ActiveStatusEffect activeStatus in activeStatusEffects)
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; }
            if (activeStatus.Data != statusData) { continue; }

            activeStatus.RefreshDuration();
            UpdateMonsterUI();
            RefreshStatusIcons();
            return;
        }

        activeStatusEffects.Add(new ActiveStatusEffect(statusData));

        UpdateMonsterUI();
        RefreshStatusIcons();
    }

    public bool HasStatus(StatusEffectData statusData)
    {
        if (statusData == null) { return false; }

        foreach (ActiveStatusEffect activeStatus in activeStatusEffects)
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; }
            if (activeStatus.Data == statusData) { return true; }
        }

        return false;
    }

    public void ReduceStatusDurations(StatusDurationTiming durationTiming)
    {
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            ActiveStatusEffect activeStatus = activeStatusEffects[i];

            if (activeStatus == null || activeStatus.IsExpired)
            {
                activeStatusEffects.RemoveAt(i);
                continue;
            }

            if (activeStatus.Data.DurationTiming != durationTiming) { continue; }

            activeStatus.ReduceDuration();

            if (activeStatus.IsExpired)
            {
                activeStatusEffects.RemoveAt(i);
            }
        }

        UpdateMonsterUI();
        RefreshStatusIcons();
    }

    public int ApplyStartTurnStatusEffects()
    {
        int totalPoisonDamage = 0;

        foreach (ActiveStatusEffect activeStatus in activeStatusEffects)
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; }
            if (activeStatus.Data.StatusType != StatusEffectType.Poison) { continue; }

            int poisonDamage = Mathf.Max(0, activeStatus.Data.Amount);
            int previousHp = currentHp;

            currentHp = Mathf.Max(0, currentHp - poisonDamage);
            totalPoisonDamage += previousHp - currentHp;

            if (IsDead) { break; }
        }

        UpdateMonsterUI();

        return totalPoisonDamage;
    }

    // 시너지 보정을 통째로 다시 설정한다. 해제 시 원복을 보장하기 위해 누적하지 않는다.
    public void ApplySynergyBonus(
        int maxHpBonus,
        int attackBonus,
        int defenseBonus,
        int lustBonus
    )
    {
        bool changed =
            synergyMaxHpBonus != maxHpBonus ||
            synergyAttackBonus != attackBonus ||
            synergyDefenseBonus != defenseBonus ||
            synergyLustBonus != lustBonus;

        if (!changed) { return; }

        synergyMaxHpBonus = maxHpBonus;
        synergyAttackBonus = attackBonus;
        synergyDefenseBonus = defenseBonus;
        synergyLustBonus = lustBonus;

        currentHp = Mathf.Min(currentHp, MaxHp); // 최대 체력 축소 시 보정

        UpdateMonsterUI();
    }

    public void SetSourceCard(CardCopy cardCopy) // 소환에 사용한 카드 사본 기록
    {
        sourceCard = cardCopy;
    }

    // 소환사 패시브와 스킬의 보정을 통째로 다시 설정한다. (누적하지 않는다)
    public void ApplySummonerBonus(int attackBonus, int lustBonus)
    {
        if (summonerAttackBonus == attackBonus &&
            summonerLustBonus == lustBonus)
        {
            return;
        }

        summonerAttackBonus = attackBonus;
        summonerLustBonus = lustBonus;

        UpdateMonsterUI();
    }

    public void AddTurnAttackBonus(int amount) // 이번 턴 공격 보정 추가 (집중 명령)
    {
        if (amount <= 0) { return; }

        turnAttackBonus += amount;
        UpdateMonsterUI();
    }

    public void ClearTurnBonus() // 턴 종료 시 이번 턴 보정 제거
    {
        if (turnAttackBonus == 0) { return; }

        turnAttackBonus = 0;
        UpdateMonsterUI();
    }

    public void RestoreAction() // 행동을 마친 마물이 다시 행동할 수 있게 한다. (절대 명령)
    {
        if (IsDead) { return; }

        actionState = MonsterActionState.Ready;
        SetPlayerTurnInteraction(true);
        UpdateMonsterUI();
    }

    public void SetSkillTargetable(bool targetable) // 소환사 스킬 대상 후보 표시
    {
        isSkillTargetable = targetable;
        RefreshSelectButton();
        UpdateBackgroundColor();
    }

    public void StartCooldown() // 스킬 사용 후 대기시간 적용
    {
        MonsterEffectData skill = ActiveSkill;

        if (skill == null) { return; }

        currentCooldown = skill.CooldownTurns;
        UpdateMonsterUI();
    }

    public void ReduceCooldown() // 턴 종료 시 대기시간 감소
    {
        if (currentCooldown <= 0) { return; }

        currentCooldown -= 1;
        UpdateMonsterUI();
    }

    public int AddShield(int amount) // 보호막 획득
    {
        if (amount <= 0) { return 0; }

        currentShield += amount;
        UpdateMonsterUI();

        return amount;
    }

    public int Heal(int amount) // 체력 회복
    {
        if (amount <= 0 || IsDead) { return 0; }

        int previousHp = currentHp;
        currentHp = Mathf.Min(MaxHp, currentHp + amount);
        UpdateMonsterUI();

        return currentHp - previousHp;
    }

    public int IncreaseMaxHp(int amount) // 최대 체력 증가
    {
        if (amount <= 0) { return 0; }

        runtimeMaxHp += amount;
        currentHp += amount;
        UpdateMonsterUI();

        return amount;
    }

    public int RemoveNegativeStatus(int removeCount) // 해로운 상태 효과 제거
    {
        int removedCount = 0;

        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            if (removedCount >= removeCount) { break; }

            ActiveStatusEffect activeStatus = activeStatusEffects[i];

            if (activeStatus == null || activeStatus.Data == null)
            {
                activeStatusEffects.RemoveAt(i);
                continue;
            }

            if (!activeStatus.Data.IsNegative) { continue; }

            activeStatusEffects.RemoveAt(i);
            removedCount += 1;
        }

        if (removedCount > 0)
        {
            UpdateMonsterUI();
            RefreshStatusIcons();
        }

        return removedCount;
    }

    public bool HasNegativeStatus() // 해로운 상태 효과 보유 여부
    {
        foreach (ActiveStatusEffect activeStatus in activeStatusEffects)
        {
            if (activeStatus == null || activeStatus.Data == null) { continue; }
            if (activeStatus.IsExpired) { continue; }
            if (activeStatus.Data.IsNegative) { return true; }
        }

        return false;
    }

    public void SetPlayerTurnInteraction(bool isPlayerTurn)
    {
        playerTurnInteraction = isPlayerTurn;
        RefreshSelectButton();
    }

    private void RefreshSelectButton() // 선택 버튼 활성 상태 갱신
    {
        if (selectButton == null) { return; }

        selectButton.interactable =
            playerTurnInteraction &&
            (CanAttack || isSkillTargetable); // 스킬 대상 후보는 행동을 마쳤어도 누를 수 있다.
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

    private int GetCurrentAttack()
    {
        int currentAttack = runtimeAttack;

        foreach (ActiveStatusEffect activeStatus in activeStatusEffects)
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; }

            if (activeStatus.Data.StatusType == StatusEffectType.AttackUp)
            {
                currentAttack += activeStatus.Data.Amount;
            }

            if (activeStatus.Data.StatusType == StatusEffectType.AttackDown)
            {
                currentAttack -= activeStatus.Data.Amount;
            }
        }

        return Mathf.Max(
            0,
            currentAttack + synergyAttackBonus + summonerAttackBonus + turnAttackBonus
        );
    }

    private int GetCurrentDefense()
    {
        int currentDefense = runtimeDefense;

        foreach (ActiveStatusEffect activeStatus in activeStatusEffects)
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; }

            if (activeStatus.Data.StatusType == StatusEffectType.DefenseUp)
            {
                currentDefense += activeStatus.Data.Amount;
            }

            if (activeStatus.Data.StatusType == StatusEffectType.DefenseDown)
            {
                currentDefense -= activeStatus.Data.Amount;
            }
        }

        return Mathf.Max(0, currentDefense + synergyDefenseBonus);
    }

    private void UpdateBackgroundColor()
    {
        if (backgroundImage == null) { return; }

        if (isSelected)
        {
            backgroundImage.color = selectedColor;
            return;
        }

        if (isSkillTargetable)
        {
            backgroundImage.color = skillTargetColor;
            return;
        }

        if (isHeroineTargeted)
        {
            backgroundImage.color = heroineTargetColor;
            return;
        }

        backgroundImage.color = normalColor;
    }

    private void HandleSelectButton()
    {
        if (!CanAttack && !isSkillTargetable) { return; }

        onSelected?.Invoke(this);
    }

    private void UpdateMonsterUI()
    {
        if (monsterData == null) { return; }

        if (monsterNameText != null)
        {
            monsterNameText.text =
                $"{monsterData.MonsterName} Lv.{enhanceLevel}"; // 강화 단계 표시
        }

        if (hpGauge != null)
        {
            hpGauge.SetValue(currentHp, MaxHp); // 체력 게이지
        }

        if (monsterHpText != null)
        {
            monsterHpText.text = hpGauge != null
                ? $"{currentHp} / {MaxHp}" // 게이지 위에는 숫자만 쓴다.
                : $"HP {currentHp} / {MaxHp}";
        }

        // 공격과 성욕, 방어와 보호막을 한 줄씩 묶어 표시한다. (아이콘이 있으면 이름 대신 아이콘)
        if (monsterAttackText != null)
        {
            monsterAttackText.text =
                $"{UISkin.IconOr(UIIcons.Attack, "공격")} {Attack}  " +
                $"{UISkin.IconOr(UIIcons.Lust, "성욕")} {LustDamage}";
        }

        if (monsterDefenseText != null)
        {
            monsterDefenseText.text =
                $"{UISkin.IconOr(UIIcons.Defense, "방어")} {Defense}  " +
                $"{UISkin.IconOr(UIIcons.Shield, "막")} {currentShield}";
        }

        if (monsterLustDamageText != null)
        {
            monsterLustDamageText.text = $"성욕 {LustDamage}";
        }

        if (monsterShieldText != null)
        {
            monsterShieldText.text = $"보호막 {currentShield}";
        }

        if (monsterStateText != null)
        {
            string tauntText = IsTaunting
                ? $"  {UISkin.IconOr(UIIcons.Taunt, "도발")}"
                : string.Empty;

            monsterStateText.text =
                $"{UISkin.Icon(UISkin.StateIconName(actionState))} {GetStateLabel()}{tauntText}".Trim();
        }

        if (monsterSkillText != null)
        {
            monsterSkillText.text = CreateSkillLabel(); // 스킬과 대기시간 표시
        }
    }

    private void ClearStatusIcons()
    {
        foreach (StatusEffectIconUI statusIconUI in statusIconUIs)
        {
            if (statusIconUI == null) { continue; }

            statusIconUI.gameObject.SetActive(false);
            Destroy(statusIconUI.gameObject);
        }

        statusIconUIs.Clear();
    }

    private void RefreshStatusIcons()
    {
        ClearStatusIcons();

        if (statusIconContainer == null) { return; }
        if (statusEffectIconPrefab == null) { return; }

        foreach (ActiveStatusEffect activeStatus in activeStatusEffects)
        {
            if (activeStatus == null || activeStatus.IsExpired) { continue; }
            if (activeStatus.Data == null) { continue; }

            StatusEffectIconUI newStatusIcon = Instantiate(
                statusEffectIconPrefab,
                statusIconContainer
            );

            newStatusIcon.Setup(
                activeStatus.Data,
                activeStatus.RemainingTurns,
                statusEffectTooltipUI
            );

            statusIconUIs.Add(newStatusIcon);
        }
    }

    private string GetStateLabel()
    {
        switch (actionState)
        {
            case MonsterActionState.Summoning: return "대기";
            case MonsterActionState.Ready: return "행동 가능";
            case MonsterActionState.Acted: return "행동 완료";
            default: return "알 수 없음";
        }
    }
    private string CreateSkillLabel() // 스킬 표시 문구 생성
    {
        MonsterEffectData skill = ActiveSkill;

        if (skill == null) { return string.Empty; }

        if (currentCooldown > 0)
        {
            return $"{UISkin.Icon(UIIcons.Cooldown)} {skill.DisplayName} ({currentCooldown})".Trim();
        }

        return $"{UISkin.Icon(UIIcons.Skill)} {skill.DisplayName}".Trim();
    }

    // 소환에 사용한 사본의 강화 단계로 능력치를 계산한다. (기획서 6.9.4)
    private void InitializeRuntimeStats()
    {
        if (monsterData == null)
        {
            runtimeMaxHp = 0;
            runtimeAttack = 0;
            runtimeLustDamage = 0;
            runtimeDefense = 0;
            return;
        }

        runtimeMaxHp = CardEnhanceRules.GetStatValue(
            monsterData.MaxHp,
            monsterData.HpGrowthPerLevel,
            enhanceLevel
        );

        runtimeAttack = CardEnhanceRules.GetStatValue(
            monsterData.Attack,
            monsterData.AttackGrowthPerLevel,
            enhanceLevel
        );

        runtimeLustDamage = CardEnhanceRules.GetStatValue(
            monsterData.LustDamage,
            monsterData.LustGrowthPerLevel,
            enhanceLevel
        );

        runtimeDefense = CardEnhanceRules.GetStatValue(
            monsterData.Defense,
            monsterData.DefenseGrowthPerLevel,
            enhanceLevel
        );
    }
}