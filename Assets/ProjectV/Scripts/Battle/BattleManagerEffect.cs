using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

public partial class BattleManager // 마물 고유 효과 처리
{
    // 발동 시점에 해당하는 효과를 실행한다.
    private void ExecuteMonsterEffects(
        MonsterUnit sourceMonster,
        MonsterEffectTrigger trigger
    )
    {
        if (sourceMonster == null) { return; }
        if (sourceMonster.Data == null) { return; }

        foreach (MonsterEffectData effect in sourceMonster.Data.Effects)
        {
            if (effect == null) { continue; }
            if (effect.Trigger != trigger) { continue; }

            ExecuteMonsterEffect(sourceMonster, effect);
        }
    }

    private void ExecuteMonsterEffect(
        MonsterUnit sourceMonster,
        MonsterEffectData effect
    )
    {
        if (effect == null) { return; }

        int allyCount = CountAllyMonsters(sourceMonster, effect.RequiredAllyType);

        if (effect.RequireOtherAlly && allyCount <= 0)
        {
            AddBattleLog(
                BattleLogCategory.PlayerAction,
                $"{sourceMonster.MonsterName}: {effect.DisplayName} 조건을 만족하지 않습니다."
            ); // 조건 미충족 기록

            return;
        }

        if (effect.PlayerHpCost > 0)
        {
            playerCurrentHp = Mathf.Max(
                1,
                playerCurrentHp - effect.PlayerHpCost
            ); // 플레이어가 효과로 사망하지 않도록 최소 1 유지
        }

        int resolvedAmount = effect.ResolveAmount(allyCount);
        string resultMessage = string.Empty;

        switch (effect.EffectType)
        {
            case MonsterEffectType.DrawCard:
                DrawCards(resolvedAmount);
                resultMessage = $"카드 {resolvedAmount}장 드로우";
                break;

            case MonsterEffectType.RestoreMana:
                currentMana += resolvedAmount;
                resultMessage = $"마나 +{resolvedAmount}";
                break;

            case MonsterEffectType.GainShield:
                sourceMonster.AddShield(resolvedAmount);
                resultMessage = $"보호막 +{resolvedAmount}";
                break;

            case MonsterEffectType.CopyShieldFromLeft:
                resultMessage = CopyShieldFromLeftMonster(
                    sourceMonster,
                    effect.MaximumAmount
                );
                break;

            case MonsterEffectType.BuffAllyAttack:
                resultMessage = ApplyStatusToAlly(
                    sourceMonster,
                    effect
                );
                break;

            case MonsterEffectType.HealAlly:
                resultMessage = HealAllyMonster(
                    sourceMonster,
                    resolvedAmount,
                    effect.SecondaryAmount
                );
                break;

            case MonsterEffectType.CleanseAllyDebuff:
                resultMessage = CleanseAllyMonster(
                    sourceMonster,
                    resolvedAmount
                );
                break;

            case MonsterEffectType.AddHeroineLust:
                int appliedLust = AddHeroineLust(resolvedAmount);
                resultMessage = GetLustGainText(resolvedAmount, appliedLust);
                break;

            case MonsterEffectType.HealPlayer:
                int previousPlayerHp = playerCurrentHp;
                playerCurrentHp = Mathf.Min(
                    playerMaxHp,
                    playerCurrentHp + resolvedAmount
                );
                resultMessage = $"플레이어 HP +{playerCurrentHp - previousPlayerHp}";
                break;

            case MonsterEffectType.SummonToken:
                resultMessage = SummonTokenMonster(
                    effect.TokenMonster,
                    resolvedAmount
                );
                break;

            default:
                return; // 지속 효과 등 아직 처리하지 않는 종류
        }

        if (string.IsNullOrEmpty(resultMessage)) { return; }

        string effectLog =
            $"{sourceMonster.MonsterName} {effect.DisplayName}: {resultMessage}";

        AddBattleLog(BattleLogCategory.PlayerAction, effectLog); // 효과 발동 기록

        if (resultText != null)
        {
            resultText.text = effectLog; // 효과 결과 표시
        }
    }

    // 조건에 맞는 다른 아군 수를 센다.
    private int CountAllyMonsters(
        MonsterUnit sourceMonster,
        MonsterType requiredType
    )
    {
        int allyCount = 0;

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }
            if (fieldMonster == sourceMonster) { continue; }

            if (requiredType != MonsterType.None)
            {
                if (fieldMonster.Data == null) { continue; }
                if (!fieldMonster.Data.HasType(requiredType)) { continue; }
            }

            allyCount += 1;
        }

        return allyCount;
    }

    // 효과 대상이 될 아군 1체를 선택한다.
    private MonsterUnit SelectAllyTarget(
        MonsterUnit sourceMonster,
        MonsterType requiredType,
        bool preferDamaged,
        bool requireNegativeStatus
    )
    {
        MonsterUnit bestTarget = null;
        int bestScore = int.MaxValue;

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }

            if (requiredType != MonsterType.None)
            {
                if (fieldMonster.Data == null) { continue; }
                if (!fieldMonster.Data.HasType(requiredType)) { continue; }
            }

            if (requireNegativeStatus && !fieldMonster.HasNegativeStatus())
            {
                continue;
            }

            if (requiredType != MonsterType.None &&
                fieldMonster == sourceMonster)
            {
                continue; // 계열 지정 효과는 다른 아군을 우선한다
            }

            int score = preferDamaged
                ? fieldMonster.CurrentHp
                : 0;

            if (bestTarget == null || score < bestScore)
            {
                bestTarget = fieldMonster;
                bestScore = score;
            }
        }

        if (bestTarget == null && !requireNegativeStatus)
        {
            bestTarget = sourceMonster; // 대상이 없으면 자신에게 적용
        }

        return bestTarget;
    }

    private string CopyShieldFromLeftMonster(
        MonsterUnit sourceMonster,
        int maximumAmount
    )
    {
        int sourceIndex = fieldMonsters.IndexOf(sourceMonster);

        if (sourceIndex <= 0) { return "왼쪽 마물 없음"; }

        MonsterUnit leftMonster = fieldMonsters[sourceIndex - 1];

        if (leftMonster == null || leftMonster.IsDead)
        {
            return "왼쪽 마물 없음";
        }

        int copiedShield = leftMonster.CurrentShield;

        if (maximumAmount > 0)
        {
            copiedShield = Mathf.Min(copiedShield, maximumAmount);
        }

        if (copiedShield <= 0) { return "복사할 보호막 없음"; }

        sourceMonster.AddShield(copiedShield);

        return $"보호막 +{copiedShield}";
    }

    private string ApplyStatusToAlly(
        MonsterUnit sourceMonster,
        MonsterEffectData effect
    )
    {
        if (effect.StatusEffect == null) { return "상태 효과 데이터 없음"; }

        MonsterUnit targetMonster = SelectAllyTarget(
            sourceMonster,
            effect.RequiredAllyType,
            false,
            false
        );

        if (targetMonster == null) { return "대상 아군 없음"; }

        targetMonster.ApplyOrRefreshStatus(effect.StatusEffect);

        return
            $"{targetMonster.MonsterName} " +
            $"{effect.StatusEffect.DisplayName} " +
            $"({effect.StatusEffect.DurationTurns}턴)";
    }

    private string HealAllyMonster(
        MonsterUnit sourceMonster,
        int healAmount,
        int maxHpAmount
    )
    {
        MonsterUnit targetMonster = SelectAllyTarget(
            sourceMonster,
            MonsterType.None,
            true,
            false
        );

        if (targetMonster == null) { return "대상 아군 없음"; }

        int increasedMaxHp = targetMonster.IncreaseMaxHp(maxHpAmount);
        int healedAmount = targetMonster.Heal(healAmount);

        string maxHpText = increasedMaxHp > 0
            ? $", 최대 HP +{increasedMaxHp}"
            : string.Empty;

        return $"{targetMonster.MonsterName} HP +{healedAmount}{maxHpText}";
    }

    private string CleanseAllyMonster(
        MonsterUnit sourceMonster,
        int removeCount
    )
    {
        MonsterUnit targetMonster = SelectAllyTarget(
            sourceMonster,
            MonsterType.None,
            false,
            true
        );

        if (targetMonster == null) { return "제거할 디버프 없음"; }

        int removedCount = targetMonster.RemoveNegativeStatus(
            Mathf.Max(1, removeCount)
        );

        if (removedCount <= 0) { return "제거할 디버프 없음"; }

        return $"{targetMonster.MonsterName} 디버프 {removedCount}개 제거";
    }

    private string SummonTokenMonster(
        MonsterData tokenMonsterData,
        int summonCount
    )
    {
        if (tokenMonsterData == null) { return "토큰 마물 데이터 없음"; }

        int summonedCount = 0;

        for (int i = 0; i < Mathf.Max(1, summonCount); i++)
        {
            if (fieldMonsters.Count >= maxFieldMonsterCount) { break; }

            SummonMonster(tokenMonsterData);
            summonedCount += 1;
        }

        if (summonedCount <= 0) { return "필드가 가득 찼습니다"; }

        return $"{tokenMonsterData.MonsterName} {summonedCount}체 소환";
    }

    // 마물 사망 처리 (사망 효과 실행 후 필드에서 제거)
    private void HandleMonsterDefeated(MonsterUnit defeatedMonster)
    {
        if (defeatedMonster == null) { return; }

        if (selectedMonster == defeatedMonster)
        {
            ClearMonsterSelection(); // 선택 상태 해제
        }

        if (previewedHeroineTarget == defeatedMonster)
        {
            previewedHeroineTarget = null; // 예고 대상 해제
        }

        fieldMonsters.Remove(defeatedMonster); // 자리를 먼저 비운다

        ExecuteMonsterEffects(
            defeatedMonster,
            MonsterEffectTrigger.Death
        ); // 빈 자리를 사용할 수 있도록 제거 후 사망 효과 실행

        Destroy(defeatedMonster.gameObject); // 오브젝트 제거
    }

    // 선택한 마물의 능동 스킬 사용
    public void UseSelectedMonsterSkill()
    {
        if (!isPlayerTurn || isBattleEnded) { return; }

        if (selectedMonster == null || !selectedMonster.CanAttack)
        {
            resultText.text = "행동 가능한 마물을 선택하세요";
            return;
        }

        if (!selectedMonster.HasActiveSkill)
        {
            resultText.text = "선택한 마물은 스킬이 없습니다";
            return;
        }

        if (!selectedMonster.CanUseSkill)
        {
            resultText.text =
                $"재사용 대기 중입니다 ({selectedMonster.CurrentCooldown}턴)";
            return;
        }

        MonsterUnit skillMonster = selectedMonster;

        ExecuteMonsterEffects(skillMonster, MonsterEffectTrigger.ActiveSkill); // 스킬 효과 실행

        skillMonster.StartCooldown(); // 재사용 대기시간 적용
        skillMonster.MarkActed(); // 행동 완료 처리
        ClearMonsterSelection();

        UpdateBattleUI();

        if (heroineCurrentHp <= 0)
        {
            EndBattle(BattleOutcome.VictoryHp); // 스킬로 승리 확인
            return;
        }

        if (heroineLust >= heroineMaxLust)
        {
            EndBattle(BattleOutcome.VictoryLust); // 성욕 승리 확인
        }
    }

    // 턴 종료 시 모든 마물의 재사용 대기시간 감소
    private void ReduceMonsterCooldowns()
    {
        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }

            fieldMonster.ReduceCooldown();
        }
    }
}
