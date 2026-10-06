using System.Collections.Generic; // 리스트 기능
using System.Text; // 문자열 조립 기능
using UnityEngine; // Unity 기본 기능

public partial class BattleManager // 타입 시너지 처리
{
    private readonly Dictionary<MonsterType, int> synergyCounts =
        new Dictionary<MonsterType, int>(); // 계열별 필드 마물 수

    private readonly Dictionary<MonsterType, int> activeSynergyStages =
        new Dictionary<MonsterType, int>(); // 계열별 활성 단계 수

    private readonly HashSet<MonsterType> usedFirstSummonMana =
        new HashSet<MonsterType>(); // 턴당 1회 소환 마나 사용 기록

    private readonly HashSet<MonsterType> usedFirstDeathDraw =
        new HashSet<MonsterType>(); // 턴당 1회 사망 드로우 사용 기록

    // 필드 구성이 바뀔 때마다 시너지를 다시 계산한다. (기획서 7.7)
    private void RefreshSynergies()
    {
        CountFieldMonsterTypes();
        ApplyContinuousSynergyBonuses();
        LogSynergyStageChanges();
        UpdateSynergyText();
    }

    private void CountFieldMonsterTypes()
    {
        synergyCounts.Clear();

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }
            if (fieldMonster.Data == null) { continue; }

            AddSynergyCount(fieldMonster.Data.MainType);

            if (fieldMonster.Data.SubType != fieldMonster.Data.MainType)
            {
                AddSynergyCount(fieldMonster.Data.SubType); // 두 타입은 각각 1체로 계산
            }
        }

        // 계열 집중 패시브: 덱에 가장 많은 계열이 필드에 있으면 계산 수를 더한다.
        int focusBonus = GetPassiveAmount(SummonerPassiveType.TypeFocus);

        if (focusBonus > 0 && synergyCounts.ContainsKey(passiveFocusType))
        {
            synergyCounts[passiveFocusType] += focusBonus;
        }
    }

    private void AddSynergyCount(MonsterType monsterType)
    {
        if (monsterType == MonsterType.None) { return; }

        if (!synergyCounts.ContainsKey(monsterType))
        {
            synergyCounts.Add(monsterType, 0);
        }

        synergyCounts[monsterType] += 1;
    }

    public int GetSynergyCount(MonsterType monsterType)
    {
        if (monsterType == MonsterType.None) { return 0; }

        return synergyCounts.TryGetValue(monsterType, out int count)
            ? count
            : 0;
    }

    // 지속 보정은 누적하지 않고 매번 전체를 다시 계산해 적용한다.
    private void ApplyContinuousSynergyBonuses()
    {
        int summonerAttackBonus = GetSummonerAttackBonus(); // 군단 지휘
        int summonerLustBonus = GetSummonerLustBonus();     // 욕망 증폭, 욕망 공명

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }

            fieldMonster.ApplySummonerBonus(
                summonerAttackBonus,
                summonerLustBonus
            ); // 소환사 보정 적용

            if (fieldMonster.Data == null) { continue; }

            int maxHpBonus = 0;
            int attackBonus = 0;
            int defenseBonus = 0;
            int lustBonus = 0;

            foreach (SynergyData synergy in synergyDataList)
            {
                if (synergy == null) { continue; }

                int typeCount = GetSynergyCount(synergy.MonsterType);

                if (typeCount <= 0) { continue; }

                foreach (SynergyData.SynergyStage stage in synergy.Stages)
                {
                    if (stage == null) { continue; }
                    if (!stage.IsContinuous) { continue; }
                    if (typeCount < stage.RequiredCount) { continue; }

                    bool isTargetMonster =
                        stage.ApplyToAllAllies ||
                        fieldMonster.Data.HasType(synergy.MonsterType);

                    if (!isTargetMonster) { continue; }

                    switch (stage.EffectType)
                    {
                        case SynergyEffectType.ModifyMaxHp:
                            maxHpBonus += stage.Amount;
                            break;

                        case SynergyEffectType.ModifyAttack:
                            attackBonus += stage.Amount;
                            break;

                        case SynergyEffectType.ModifyDefense:
                            defenseBonus += stage.Amount;
                            break;

                        case SynergyEffectType.ModifyLustDamage:
                            lustBonus += stage.Amount;
                            break;
                    }
                }
            }

            fieldMonster.ApplySynergyBonus(
                maxHpBonus,
                attackBonus,
                defenseBonus,
                lustBonus
            );
        }
    }

    private void LogSynergyStageChanges()
    {
        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }

            int typeCount = GetSynergyCount(synergy.MonsterType);
            int activeStages = synergy.GetActiveStageCount(typeCount);

            activeSynergyStages.TryGetValue(
                synergy.MonsterType,
                out int previousStages
            );

            if (activeStages == previousStages) { continue; }

            activeSynergyStages[synergy.MonsterType] = activeStages;

            string changeText = activeStages > previousStages
                ? "활성화"
                : "해제";

            AddBattleLog(
                BattleLogCategory.System,
                $"{synergy.DisplayName} 시너지 {activeStages}단계 {changeText} ({typeCount}체)"
            ); // 시너지 단계 변경 기록
        }
    }

    private void UpdateSynergyText()
    {
        if (synergyText == null) { return; }

        StringBuilder builder = new StringBuilder();

        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }

            int typeCount = GetSynergyCount(synergy.MonsterType);

            if (typeCount <= 0) { continue; }

            int activeStages = synergy.GetActiveStageCount(typeCount);
            int nextRequired = synergy.GetNextRequiredCount(typeCount);

            string colorCode = ColorUtility.ToHtmlStringRGB(
                MonsterTypeRules.GetDisplayColor(synergy.MonsterType)
            );

            string stageText = activeStages > 0
                ? $"{activeStages}단계"
                : "대기";

            string nextText = nextRequired > 0
                ? $" (다음 {nextRequired}체)"
                : string.Empty;

            if (builder.Length > 0) { builder.Append('\n'); }

            builder.Append(
                activeStages > 0
                    ? $"<color=#{colorCode}>{synergy.DisplayName} {typeCount}체 {stageText}{nextText}</color>"
                    : $"<color=#7F7F87>{synergy.DisplayName} {typeCount}체 {stageText}{nextText}</color>"
            );
        }

        synergyText.text = builder.Length > 0
            ? builder.ToString()
            : "활성 시너지 없음";
    }

    // 지정한 계열에서 활성화된 발동형 시너지 수치를 합산한다.
    private int GetTriggeredSynergyAmount(
        MonsterType monsterType,
        SynergyEffectType effectType
    )
    {
        int totalAmount = 0;
        int typeCount = GetSynergyCount(monsterType);

        if (typeCount <= 0) { return 0; }

        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }
            if (synergy.MonsterType != monsterType) { continue; }

            foreach (SynergyData.SynergyStage stage in synergy.Stages)
            {
                if (stage == null) { continue; }
                if (stage.EffectType != effectType) { continue; }
                if (typeCount < stage.RequiredCount) { continue; }

                totalAmount += stage.Amount;
            }
        }

        return totalAmount;
    }

    // 마물이 소환된 직후 처리
    private void OnMonsterSummonedForSynergy(MonsterUnit summonedMonster)
    {
        if (summonedMonster == null || summonedMonster.Data == null) { return; }

        MonsterType summonedType = summonedMonster.Data.MainType;

        int shieldAmount = GetTriggeredSynergyAmount(
            summonedType,
            SynergyEffectType.SummonGainShield
        );

        if (shieldAmount > 0)
        {
            summonedMonster.AddShield(shieldAmount);

            AddBattleLog(
                BattleLogCategory.PlayerAction,
                $"{MonsterTypeRules.GetDisplayName(summonedType)} 시너지: " +
                $"{summonedMonster.MonsterName} 보호막 +{shieldAmount}"
            );
        }

        int manaAmount = GetTriggeredSynergyAmount(
            summonedType,
            SynergyEffectType.FirstSummonRestoreMana
        );

        if (manaAmount > 0 && !usedFirstSummonMana.Contains(summonedType))
        {
            usedFirstSummonMana.Add(summonedType);
            currentMana += manaAmount;

            AddBattleLog(
                BattleLogCategory.PlayerAction,
                $"{MonsterTypeRules.GetDisplayName(summonedType)} 시너지: 마나 +{manaAmount}"
            );
        }
    }

    // 마물이 사망한 직후 처리
    private void OnMonsterDefeatedForSynergy(MonsterType defeatedType)
    {
        if (defeatedType == MonsterType.None) { return; }

        int drawAmount = GetTriggeredSynergyAmount(
            defeatedType,
            SynergyEffectType.FirstDeathDrawCard
        );

        if (drawAmount <= 0) { return; }
        if (usedFirstDeathDraw.Contains(defeatedType)) { return; }

        usedFirstDeathDraw.Add(defeatedType);
        DrawCards(drawAmount);

        AddBattleLog(
            BattleLogCategory.PlayerAction,
            $"{MonsterTypeRules.GetDisplayName(defeatedType)} 시너지: 카드 {drawAmount}장 드로우"
        );
    }

    // 플레이어 턴 시작 시 처리
    private void ApplyTurnStartSynergies()
    {
        usedFirstSummonMana.Clear(); // 턴당 1회 기록 초기화
        usedFirstDeathDraw.Clear();

        RefreshSynergies();

        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }

            int healAmount = GetTriggeredSynergyAmount(
                synergy.MonsterType,
                SynergyEffectType.TurnStartHealPlayer
            );

            if (healAmount > 0)
            {
                int previousHp = playerCurrentHp;

                playerCurrentHp = Mathf.Min(
                    PlayerMaxHp,
                    playerCurrentHp + healAmount
                );

                AddBattleLog(
                    BattleLogCategory.System,
                    $"{synergy.DisplayName} 시너지: 플레이어 HP +{playerCurrentHp - previousHp}"
                );
            }

            int cooldownAmount = GetTriggeredSynergyAmount(
                synergy.MonsterType,
                SynergyEffectType.TurnStartReduceCooldown
            );

            if (cooldownAmount > 0)
            {
                ReduceCooldownForType(synergy.MonsterType, cooldownAmount);
            }
        }
    }

    // 플레이어 턴 종료 시 처리
    private void ApplyTurnEndSynergies()
    {
        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }

            int cooldownAmount = GetTriggeredSynergyAmount(
                synergy.MonsterType,
                SynergyEffectType.TurnEndReduceCooldown
            );

            if (cooldownAmount > 0)
            {
                ReduceCooldownForType(synergy.MonsterType, cooldownAmount);
            }
        }
    }

    private void ReduceCooldownForType(
        MonsterType monsterType,
        int reduceAmount
    )
    {
        int reducedCount = 0;

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }
            if (fieldMonster.Data == null) { continue; }
            if (!fieldMonster.Data.HasType(monsterType)) { continue; }
            if (fieldMonster.CurrentCooldown <= 0) { continue; }

            for (int i = 0; i < reduceAmount; i++)
            {
                fieldMonster.ReduceCooldown();
            }

            reducedCount += 1;
        }

        if (reducedCount <= 0) { return; }

        AddBattleLog(
            BattleLogCategory.System,
            $"{MonsterTypeRules.GetDisplayName(monsterType)} 시너지: " +
            $"재사용 대기시간 {reduceAmount} 감소 ({reducedCount}체)"
        );
    }
}
