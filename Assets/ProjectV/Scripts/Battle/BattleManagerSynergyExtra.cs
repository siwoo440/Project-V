using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 높은 단계의 계열 시너지 (기획서 7.8)
// 촉수 6체, 슬라임 8체, 고블린 2·6·8체, 정령 6체, 천사 4체의 효과를 다룬다.
public partial class BattleManager
{
    private const int SynergyActedRequirement = 3; // 행동을 마쳐야 하는 마물 수 (고블린 6체 효과)

    private readonly HashSet<MonsterType> summonedTypesThisTurn =
        new HashSet<MonsterType>(); // 이번 턴에 소환한 계열 (매 턴 처음 소환 판정)

    private readonly Dictionary<MonsterType, int> actedCountsThisTurn =
        new Dictionary<MonsterType, int>(); // 이번 턴에 행동을 마친 계열별 횟수

    private readonly HashSet<MonsterType> usedActedDraw =
        new HashSet<MonsterType>(); // 행동 완료 드로우를 이번 턴에 쓴 계열

    private bool synergyLethalGuardUsed; // 쓰러질 피해를 버티는 효과를 이번 전투에 썼는지 여부

    private void ResetSynergyBattleRecords() // 전투를 시작할 때 기록을 비운다.
    {
        synergyLethalGuardUsed = false;
        lastCostDiscountState = -1;
        ResetSynergyTurnRecords();
    }

    private void ResetSynergyTurnRecords() // 내 턴이 시작될 때 턴당 기록을 비운다.
    {
        summonedTypesThisTurn.Clear();
        actedCountsThisTurn.Clear();
        usedActedDraw.Clear();
    }

    // 내 턴이 시작될 때의 높은 단계 효과: 촉수 6체의 성욕 증가, 정령 6체의 아군 강화
    private void ApplyTurnStartExtraSynergies(SynergyData synergy)
    {
        MonsterType type = synergy.MonsterType;

        int shakenBonus = GetTriggeredSynergyAmount(type, SynergyEffectType.TurnStartLustByCount);

        if (shakenBonus > 0)
        {
            int requestedLust = GetSynergyCount(type) + (IsHeroineShaken ? shakenBonus : 0);
            int appliedLust = AddHeroineLust(requestedLust); // 적 마물 전투에는 성욕이 없어 0이 된다.

            if (appliedLust > 0)
            {
                AddBattleLog(
                    BattleLogCategory.System,
                    $"{synergy.DisplayName} 시너지: 히로인 성욕 +{appliedLust}"
                );
            }
        }

        int buffAmount = GetTriggeredSynergyAmount(type, SynergyEffectType.TurnStartAllyBuff);

        if (buffAmount <= 0) { return; }

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }

            fieldMonster.AddTurnAttackBonus(buffAmount);
        }

        turnLustBonus += buffAmount; // 턴이 끝나면 공격 보정과 함께 사라진다.
        RefreshSynergies();

        AddBattleLog(
            BattleLogCategory.System,
            $"{synergy.DisplayName} 시너지: 이번 턴 모든 아군 공격력 +{buffAmount}, 성욕 부여량 +{buffAmount}"
        );
    }

    // 소환 직후의 높은 단계 효과: 슬라임 8체와 고블린 8체는 소환된 턴에도 행동할 수 있다.
    private void ApplySummonExtraSynergies(MonsterUnit summonedMonster)
    {
        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }
            if (!summonedMonster.Data.HasType(synergy.MonsterType)) { continue; }

            summonedTypesThisTurn.Add(synergy.MonsterType); // 이 계열은 이번 턴에 이미 소환했다.

            if (GetTriggeredSynergyAmount(synergy.MonsterType, SynergyEffectType.SummonReady) <= 0) { continue; }

            summonedMonster.RestoreAction();

            AddBattleLog(
                BattleLogCategory.PlayerAction,
                $"{synergy.DisplayName} 시너지: {summonedMonster.MonsterName}은 소환된 턴에도 행동할 수 있습니다"
            );
        }

        RefreshSynergyCostViews(); // 처음 소환 할인이 끝났으면 손패의 비용 표시를 되돌린다.
    }

    private int lastCostDiscountState = -1; // 손패 비용 표시를 마지막으로 맞춘 할인 상태

    // 처음 소환 할인을 받을 수 있는 계열이 바뀌었을 때만 손패의 비용 표시를 다시 그린다.
    private void RefreshSynergyCostViews()
    {
        int discountState = 0;

        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }
            if (summonedTypesThisTurn.Contains(synergy.MonsterType)) { continue; }
            if (GetTriggeredSynergyAmount(synergy.MonsterType, SynergyEffectType.FirstSummonCostDown) <= 0) { continue; }

            discountState |= 1 << (int)synergy.MonsterType;
        }

        if (discountState == lastCostDiscountState) { return; }

        lastCostDiscountState = discountState;
        RefreshHandCardViews();
    }

    // 매 턴 처음 소환하는 계열 마물의 비용 감소 (고블린 2체)
    private int GetSynergyCardDiscount(CardCopy cardCopy)
    {
        MonsterData monster = cardCopy == null ? null : cardCopy.SummonMonster;

        if (monster == null) { return 0; }

        int discount = 0;

        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }
            if (!monster.HasType(synergy.MonsterType)) { continue; }
            if (summonedTypesThisTurn.Contains(synergy.MonsterType)) { continue; }

            discount += GetTriggeredSynergyAmount(synergy.MonsterType, SynergyEffectType.FirstSummonCostDown);
        }

        return discount;
    }

    // 마물이 행동을 마친 직후: 한 턴에 그 계열 3체가 행동을 마치면 카드를 뽑는다. 턴당 1회 (고블린 6체)
    private void OnMonsterActedForSynergy(MonsterUnit actedMonster)
    {
        if (actedMonster == null || actedMonster.Data == null) { return; }

        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }

            MonsterType type = synergy.MonsterType;

            if (!actedMonster.Data.HasType(type)) { continue; }

            actedCountsThisTurn.TryGetValue(type, out int actedCount);
            actedCountsThisTurn[type] = actedCount + 1;

            int drawAmount = GetTriggeredSynergyAmount(type, SynergyEffectType.ActedDrawCard);

            if (drawAmount <= 0 || usedActedDraw.Contains(type)) { continue; }
            if (actedCountsThisTurn[type] < SynergyActedRequirement) { continue; }

            usedActedDraw.Add(type);
            DrawCards(drawAmount);

            AddBattleLog(
                BattleLogCategory.PlayerAction,
                $"{synergy.DisplayName} 시너지: 카드 {drawAmount}장 드로우"
            );
        }
    }

    // 플레이어가 쓰러질 피해를 받았을 때: 전투당 1회 HP 1로 버티고 그 계열 마물이 보호막을 얻는다. (천사 4체)
    private void ApplySynergyLethalGuard()
    {
        if (playerCurrentHp > 0 || synergyLethalGuardUsed) { return; }

        foreach (SynergyData synergy in synergyDataList)
        {
            if (synergy == null) { continue; }

            int shieldAmount = GetTriggeredSynergyAmount(synergy.MonsterType, SynergyEffectType.SurviveLethal);

            if (shieldAmount <= 0) { continue; }

            synergyLethalGuardUsed = true;
            playerCurrentHp = 1;

            foreach (MonsterUnit fieldMonster in fieldMonsters)
            {
                if (fieldMonster == null || fieldMonster.IsDead || fieldMonster.Data == null) { continue; }
                if (!fieldMonster.Data.HasType(synergy.MonsterType)) { continue; }

                fieldMonster.AddShield(shieldAmount);
            }

            AddBattleLog(
                BattleLogCategory.System,
                $"{synergy.DisplayName} 시너지: 쓰러지지 않고 HP 1로 버텼습니다. {synergy.DisplayName} 마물 보호막 +{shieldAmount}"
            );

            return;
        }
    }
}
