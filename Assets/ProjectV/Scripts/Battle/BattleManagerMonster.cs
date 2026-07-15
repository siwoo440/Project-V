using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 분리된 전투 기능
{
    public void AttackWithSelectedMonster()
    {
        ExecuteMonsterAttack(MonsterAttackMode.Hp);
    }

    public void LustAttackWithSelectedMonster()
    {
        ExecuteMonsterAttack(MonsterAttackMode.Lust);
    }

    private void ExecuteMonsterAttack(MonsterAttackMode attackMode)
    {
        if (!isPlayerTurn || isBattleEnded) { return; }

        if (selectedMonster == null || !selectedMonster.CanAttack)
        {
            resultText.text = "Select Ready Monster";
            return;
        }

        if (attackMode == MonsterAttackMode.Lust && selectedMonster.LustDamage <= 0)
        {
            resultText.text = "Selected Monster Has No Lust Damage";
            return;
        }

        MonsterUnit attackingMonster = selectedMonster;

        string attackResultText = attackMode == MonsterAttackMode.Hp
            ? ExecuteMonsterHpAttack(attackingMonster)
            : ExecuteMonsterLustAttack(attackingMonster);

        string statusText = TryApplyMonsterAttackStatus(attackingMonster);

        attackingMonster.MarkActed();
        ClearMonsterSelection();

        resultText.text = string.IsNullOrEmpty(statusText)
            ? attackResultText
            : $"{attackResultText}\n{statusText}";

        AddBattleLog(BattleLogCategory.PlayerAction, attackResultText);

        if (!string.IsNullOrEmpty(statusText))
        {
            AddBattleLog(BattleLogCategory.StatusEffect, statusText);
        }

        UpdateBattleUI();

        if (heroineCurrentHp <= 0)
        {
            EndBattle(BattleOutcome.VictoryHp);
            return;
        }

        if (heroineLust >= heroineMaxLust)
        {
            EndBattle(BattleOutcome.VictoryLust);
        }
    }

    private string ExecuteMonsterHpAttack(MonsterUnit attackingMonster)
    {
        int currentHeroineDefense = GetHeroineCurrentDefense();

        DamageResult damageResult = DamageCalculator.CalculateDamageWithShield(
            attackingMonster.Attack,
            currentHeroineDefense,
            heroineCurrentShield
        );

        heroineCurrentShield = damageResult.RemainingShield;
        heroineCurrentHp = Mathf.Max(0, heroineCurrentHp - damageResult.HpDamage);

        string damageText = CreateDamageResultText(
            attackingMonster.MonsterName,
            damageResult
        );

        return $"[HP Attack] {damageText}";
    }

    private string ExecuteMonsterLustAttack(MonsterUnit attackingMonster)
    {
        int requestedLustDamage = attackingMonster.LustDamage;
        int appliedLustDamage = AddHeroineLust(requestedLustDamage);
        string lustGainText = GetLustGainText(requestedLustDamage, appliedLustDamage);

        return $"[Lust Attack] {attackingMonster.MonsterName}: {lustGainText}";
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

    private List<MonsterUnit> GetLivingMonsterCandidates()
    {
        fieldMonsters.RemoveAll(
            monsterUnit => monsterUnit == null || monsterUnit.IsDead
        );

        return new List<MonsterUnit>(fieldMonsters);
    }
    private List<MonsterUnit> GetTauntingMonsterCandidates(
    List<MonsterUnit> livingMonsters
)
    {
        List<MonsterUnit> tauntingMonsters = new List<MonsterUnit>();

        foreach (MonsterUnit monsterUnit in livingMonsters)
        {
            if (monsterUnit == null || monsterUnit.IsDead) { continue; }
            if (!monsterUnit.IsTaunting) { continue; }

            tauntingMonsters.Add(monsterUnit);
        }

        return tauntingMonsters;
    }
    private MonsterUnit GetLowestHpMonsterFromCandidates(
    List<MonsterUnit> candidates
)
    {
        if (candidates == null || candidates.Count == 0) { return null; }

        MonsterUnit lowestHpMonster = candidates[0];

        for (int i = 1; i < candidates.Count; i++)
        {
            MonsterUnit currentMonster = candidates[i];

            if (currentMonster.CurrentHp < lowestHpMonster.CurrentHp)
            {
                lowestHpMonster = currentMonster;
            }
        }

        return lowestHpMonster;
    }
    private MonsterUnit ResolveHeroineSingleTarget(
    HeroineActionData actionData
)
    {
        if (actionData == null) { return null; }

        List<MonsterUnit> livingMonsters = GetLivingMonsterCandidates();
        List<MonsterUnit> tauntingMonsters =
            GetTauntingMonsterCandidates(livingMonsters);

        bool mustTargetTaunt =
            !actionData.IgnoreTaunt &&
            tauntingMonsters.Count > 0;

        if (actionData.TargetType == HeroineTargetType.Player)
        {
            return mustTargetTaunt
                ? tauntingMonsters[0]
                : null;
        }

        List<MonsterUnit> targetCandidates = mustTargetTaunt
            ? tauntingMonsters
            : livingMonsters;

        if (targetCandidates.Count == 0) { return null; }

        switch (actionData.TargetType)
        {
            case HeroineTargetType.FirstMonster:
                return targetCandidates[0];

            case HeroineTargetType.RandomMonster:
                return targetCandidates[
                    Random.Range(0, targetCandidates.Count)
                ];

            case HeroineTargetType.LowestHpMonster:
                return GetLowestHpMonsterFromCandidates(targetCandidates);

            default:
                return null;
        }
    }
    private void ClearHeroineTargetPreview()
    {
        foreach (MonsterUnit monsterUnit in fieldMonsters)
        {
            if (monsterUnit == null) { continue; }

            monsterUnit.SetHeroineTargeted(false);
        }

        previewedHeroineTarget = null;
    }

    private void RefreshHeroineTargetPreview()
    {
        ClearHeroineTargetPreview();

        if (nextHeroineAction == null) { return; }

        List<MonsterUnit> livingMonsters = GetLivingMonsterCandidates();

        if (nextHeroineAction.TargetType == HeroineTargetType.AllMonsters)
        {
            foreach (MonsterUnit monsterUnit in livingMonsters)
            {
                monsterUnit.SetHeroineTargeted(true);
            }

            return;
        }

        bool canPreviewMonsterTarget =
            nextHeroineAction.ActionType == HeroineActionType.SingleAttack ||
            nextHeroineAction.ActionType == HeroineActionType.ApplyStatus;

        if (!canPreviewMonsterTarget) { return; }

        previewedHeroineTarget =
            ResolveHeroineSingleTarget(nextHeroineAction);

        if (previewedHeroineTarget != null)
        {
            previewedHeroineTarget.SetHeroineTargeted(true);
        }
    }

    private MonsterUnit ConsumePreviewedHeroineTarget()
    {
        MonsterUnit targetMonster = previewedHeroineTarget;

        if (targetMonster == null || targetMonster.IsDead)
        {
            targetMonster =
                ResolveHeroineSingleTarget(nextHeroineAction);
        }

        ClearHeroineTargetPreview();

        return targetMonster;
    }

    private void ExecutePreviewedSingleTargetAttack()
    {
        MonsterUnit targetMonster =
            ConsumePreviewedHeroineTarget();

        AttackTargetMonster(targetMonster);
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

    private void SelectMonster(MonsterUnit monsterUnit)
    {
        if (!isPlayerTurn || isBattleEnded || monsterUnit == null) { return; }
        if (!monsterUnit.CanAttack) { resultText.text = "Monster Cannot Attack"; return; }
        if (selectedMonster != null) { selectedMonster.SetSelected(false); }

        selectedMonster = monsterUnit;
        selectedMonster.SetSelected(true);
        SetAttackButtonsInteractable(true);
        resultText.text = $"{selectedMonster.MonsterName} Selected";
    }

    private void ClearMonsterSelection()
    {
        if (selectedMonster != null) { selectedMonster.SetSelected(false); }

        selectedMonster = null;
        SetAttackButtonsInteractable(false);
    }








    private void SummonMonster(MonsterData monsterData)
    {
        MonsterUnit newMonsterUnit = Instantiate(
            monsterUnitPrefab,
            monsterFieldContainer
        );

        newMonsterUnit.Initialize(
            monsterData,
            SelectMonster,
            statusEffectIconPrefab,
            statusEffectTooltipUI
        );

        fieldMonsters.Add(newMonsterUnit);
        newMonsterUnit.SetPlayerTurnInteraction(isPlayerTurn);

        RefreshHeroineTargetPreview();
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

    private void ClearMonsterField()
    {
        ClearHeroineTargetPreview();

        foreach (MonsterUnit monsterUnit in fieldMonsters)
        {
            if (monsterUnit != null) { Destroy(monsterUnit.gameObject); }
        }

        fieldMonsters.Clear();
        ClearMonsterSelection();
    }


    private void ReduceMonsterStatusDurations(StatusDurationTiming durationTiming)
    {
        foreach (MonsterUnit monsterUnit in fieldMonsters)
        {
            if (monsterUnit == null || monsterUnit.IsDead) { continue; }

            monsterUnit.ReduceStatusDurations(durationTiming);
        }
    }
    private void ApplyMonsterStartTurnStatusEffects()
    {
        for (int i = fieldMonsters.Count - 1; i >= 0; i--)
        {
            MonsterUnit monsterUnit = fieldMonsters[i];

            if (monsterUnit == null)
            {
                fieldMonsters.RemoveAt(i);
                continue;
            }

            int poisonDamage =
                monsterUnit.ApplyStartTurnStatusEffects();

            if (poisonDamage > 0)
            {
                AddBattleLog(
                    BattleLogCategory.StatusEffect,
                    $"Poison: {monsterUnit.MonsterName} HP -{poisonDamage}"
                );
            }

            if (!monsterUnit.IsDead) { continue; }

            string defeatedMonsterName = monsterUnit.MonsterName;

            if (selectedMonster == monsterUnit)
            {
                ClearMonsterSelection();
            }

            if (previewedHeroineTarget == monsterUnit)
            {
                previewedHeroineTarget = null;
            }

            fieldMonsters.RemoveAt(i);
            Destroy(monsterUnit.gameObject);

            AddBattleLog(
                BattleLogCategory.StatusEffect,
                $"{defeatedMonsterName} defeated by Poison"
            );
        }
    }



}