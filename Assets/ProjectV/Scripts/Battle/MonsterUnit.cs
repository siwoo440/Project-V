using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUnit : MonoBehaviour
{
    [Header("Monster UI")]
    [SerializeField] private TMP_Text monsterNameText;
    [SerializeField] private TMP_Text monsterHpText;
    [SerializeField] private TMP_Text monsterAttackText;
    [SerializeField] private TMP_Text monsterLustDamageText;
    [SerializeField] private TMP_Text monsterDefenseText;
    [SerializeField] private TMP_Text monsterShieldText;
    [SerializeField] private TMP_Text monsterStateText;
    [SerializeField] private Transform statusIconContainer;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image backgroundImage;

    [Header("Selection Colors")]
    [SerializeField]
    private Color normalColor =
        new Color(0.34f, 0.24f, 0.45f, 1f);

    [SerializeField]
    private Color selectedColor =
        new Color(0.25f, 0.65f, 0.35f, 1f);

    [SerializeField]
    private Color heroineTargetColor =
        new Color(0.9f, 0.35f, 0.2f, 1f);

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

    public int Attack => GetCurrentAttack();
    public int LustDamage => monsterData != null ? monsterData.LustDamage : 0;
    public int Defense => GetCurrentDefense();
    public int CurrentShield => currentShield;
    public int CurrentHp => currentHp;
    public int MaxHp => monsterData != null ? monsterData.MaxHp : 0;

    public StatusEffectData AttackStatusEffect =>
        monsterData != null ? monsterData.AttackStatusEffect : null;

    public string MonsterName =>
        monsterData != null ? monsterData.MonsterName : "Unknown";

    public bool IsTaunting =>
        monsterData != null && monsterData.IsTaunting;

    public bool IsDead => currentHp <= 0;

    public bool CanAttack =>
        actionState == MonsterActionState.Ready && !IsDead;

    public void Initialize(
        MonsterData data,
        Action<MonsterUnit> selectedCallback,
        StatusEffectIconUI iconPrefab,
        StatusEffectTooltipUI tooltipUI
    )
    {
        if (data == null) { return; }

        monsterData = data;
        onSelected = selectedCallback;
        statusEffectIconPrefab = iconPrefab;
        statusEffectTooltipUI = tooltipUI;

        currentHp = monsterData.MaxHp;
        currentShield = Mathf.Max(0, monsterData.StartingShield);
        actionState = MonsterActionState.Summoning;

        activeStatusEffects.Clear();
        ClearStatusIcons();

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(HandleSelectButton);
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

    public void SetPlayerTurnInteraction(bool isPlayerTurn)
    {
        if (selectButton == null) { return; }

        selectButton.interactable = isPlayerTurn && CanAttack;
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
        int currentAttack = monsterData != null ? monsterData.Attack : 0;

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

        return Mathf.Max(0, currentAttack);
    }

    private int GetCurrentDefense()
    {
        int currentDefense = monsterData != null ? monsterData.Defense : 0;

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

        return Mathf.Max(0, currentDefense);
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

    private void HandleSelectButton()
    {
        if (!CanAttack) { return; }

        onSelected?.Invoke(this);
    }

    private void UpdateMonsterUI()
    {
        if (monsterData == null) { return; }

        if (monsterNameText != null)
        {
            monsterNameText.text = monsterData.MonsterName;
        }

        if (monsterHpText != null)
        {
            monsterHpText.text = $"HP: {currentHp} / {monsterData.MaxHp}";
        }

        if (monsterAttackText != null)
        {
            monsterAttackText.text = $"ATK: {Attack}";
        }

        if (monsterLustDamageText != null)
        {
            monsterLustDamageText.text = $"LST: {monsterData.LustDamage}";
        }

        if (monsterDefenseText != null)
        {
            monsterDefenseText.text = $"DEF: {Defense}";
        }

        if (monsterShieldText != null)
        {
            monsterShieldText.text = $"Shield: {currentShield}";
        }

        if (monsterStateText != null)
        {
            string tauntText = IsTaunting ? " | Taunt" : string.Empty;
            monsterStateText.text =
                $"State: {GetStateLabel()}{tauntText}";
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
            case MonsterActionState.Summoning: return "Waiting";
            case MonsterActionState.Ready: return "Ready";
            case MonsterActionState.Acted: return "Acted";
            default: return "Unknown";
        }
    }
}