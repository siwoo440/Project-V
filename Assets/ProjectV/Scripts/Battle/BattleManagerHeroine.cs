using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 분리된 전투 기능
{
    private int AddHeroineLust(int amount)
    {
        if (amount <= 0) { return 0; }

        int previousLust = heroineLust;
        heroineLust = Mathf.Clamp(heroineLust + amount, 0, heroineMaxLust);

        return heroineLust - previousLust;
    }

    private string GetLustGainText(int requestedAmount, int appliedAmount)
    {
        if (requestedAmount <= 0) { return "Lust +0"; }
        if (appliedAmount <= 0 && heroineLust >= heroineMaxLust) { return "Lust MAX"; }

        return $"Lust +{appliedAmount}";
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

        switch (nextHeroineAction.TargetType)
        {
            case HeroineTargetType.FirstMonster:
            case HeroineTargetType.RandomMonster:
            case HeroineTargetType.LowestHpMonster:
            case HeroineTargetType.Player:
                ExecutePreviewedSingleTargetAttack();
                break;

            case HeroineTargetType.AllMonsters:
                ClearHeroineTargetPreview();
                ExecuteAreaAttack();
                break;

            case HeroineTargetType.Self:
                ClearHeroineTargetPreview();
                resultText.text = "Invalid Self Target Action";
                break;

            default:
                ClearHeroineTargetPreview();
                resultText.text = "Unknown Target Type";
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
    private void ExecuteApplyStatusAction()
    {
        StatusEffectData statusData =
            nextHeroineAction.AppliedStatusEffect;

        if (statusData == null)
        {
            resultText.text = "Missing Status Effect Data";
            return;
        }

        string amountText = GetStatusAmountDisplay(statusData);

        if (nextHeroineAction.TargetType == HeroineTargetType.Self)
        {
            ClearHeroineTargetPreview();
            ApplyOrRefreshHeroineStatus(statusData);

            resultText.text =
                $"{nextHeroineAction.DisplayName}: " +
                $"{statusData.DisplayName} {amountText} " +
                $"({statusData.DurationTurns} Turns)";

            AddBattleLog(
                BattleLogCategory.StatusEffect,
                resultText.text
            );

            return;
        }

        if (nextHeroineAction.TargetType == HeroineTargetType.AllMonsters)
        {
            ClearHeroineTargetPreview();

            List<MonsterUnit> targetMonsters =
                GetLivingMonsterCandidates();

            if (targetMonsters.Count == 0)
            {
                resultText.text =
                    $"{nextHeroineAction.DisplayName}: No Monster Target";

                AddBattleLog(
                    BattleLogCategory.StatusEffect,
                    resultText.text
                );

                return;
            }

            foreach (MonsterUnit monsterUnit in targetMonsters)
            {
                monsterUnit.ApplyOrRefreshStatus(statusData);
            }

            resultText.text =
                $"{nextHeroineAction.DisplayName}: " +
                $"{statusData.DisplayName} applied to " +
                $"{targetMonsters.Count} monsters";

            AddBattleLog(
                BattleLogCategory.StatusEffect,
                resultText.text
            );

            return;
        }

        MonsterUnit targetMonster =
            ConsumePreviewedHeroineTarget();

        if (targetMonster == null)
        {
            resultText.text =
                $"{nextHeroineAction.DisplayName}: No Monster Target";

            AddBattleLog(
                BattleLogCategory.StatusEffect,
                resultText.text
            );

            return;
        }

        targetMonster.ApplyOrRefreshStatus(statusData);

        resultText.text =
            $"{nextHeroineAction.DisplayName}: " +
            $"{targetMonster.MonsterName} received " +
            $"{statusData.DisplayName} {amountText} " +
            $"({statusData.DurationTurns} Turns)";

        AddBattleLog(
            BattleLogCategory.StatusEffect,
            resultText.text
        );
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


    private int GetHeroineCurrentDefense()
    {
        int currentDefense = heroineDefense;

        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects)
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
    private int GetHeroineCurrentAttack(int baseAttack)
    {
        int currentAttack = Mathf.Max(0, baseAttack);

        foreach (ActiveStatusEffect activeStatus in activeHeroineStatusEffects)
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
        if (actionData.ActionType == HeroineActionType.GainShield && heroineCurrentShield >= heroineMaxShield) { return false; } // 최대 보호막 행동 차단
        if (actionData.ActionType == HeroineActionType.Heal && heroineCurrentHp >= heroineMaxHp) { return false; } // 최대 체력 회복 행동 차단
        if (actionData.ActionType == HeroineActionType.ApplyStatus && actionData.AppliedStatusEffect == null) { return false; } // 상태 데이터 누락 행동 차단
        if (actionData.ActionType == HeroineActionType.ApplyStatus && actionData.TargetType
            == HeroineTargetType.Self && HasHeroineStatusEffect(actionData.AppliedStatusEffect)) { return false; }
        if (actionData.ActionType == HeroineActionType.Cleanse && !HasHeroineNegativeStatus()) { return false; } // 해로운 상태 없는 정화 행동 제외

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

    private string GetHeroineTargetPreviewText()
    {
        if (nextHeroineAction == null) { return "None"; }

        if (nextHeroineAction.TargetType == HeroineTargetType.AllMonsters)
        {
            return fieldMonsters.Count > 0
                ? "All Monsters"
                : "Player (No Monsters)";
        }

        if (previewedHeroineTarget != null &&
            !previewedHeroineTarget.IsDead)
        {
            return previewedHeroineTarget.IsTaunting
                ? $"{previewedHeroineTarget.MonsterName} (Taunt)"
                : previewedHeroineTarget.MonsterName;
        }

        if (nextHeroineAction.TargetType == HeroineTargetType.Player) { return "Player"; }
        if (nextHeroineAction.TargetType == HeroineTargetType.Self) { return "Self"; }

        return "Player (No Monsters)";
    }


    private void UpdateHeroineIntentUI()
    {
        if (heroineIntentText == null) { return; }
        if (nextHeroineAction == null) { heroineIntentText.text = "Next Action: None"; return; }

        string targetName = GetHeroineTargetPreviewText();
        string effectName =
            GetHeroineActionEffectDisplay(nextHeroineAction);

        heroineIntentText.text =
            $"Next: {nextHeroineAction.DisplayName}\n" +
            $"{effectName} / Target: {targetName}";
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

            case StatusEffectType.AttackUp:
                return $"+{statusData.Amount}";

            case StatusEffectType.DefenseDown:
                return $"-{statusData.Amount}";

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

}