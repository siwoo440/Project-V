using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능

public partial class BattleManager // 소환사 액티브 스킬 효과와 표시
{
    private void ExecuteSummonerSkill(MonsterUnit targetMonster) // 마나 소비 후 효과 발동
    {
        if (!CanUseSummonerSkill(out string reason))
        {
            resultText.text = reason;
            UpdateSummonerUI();
            return;
        }

        if (battleSkill.SkillType == SummonerSkillType.EmergencyReturn &&
            !CanReturnMonsterToHand(targetMonster, out reason))
        {
            resultText.text = reason;
            UpdateSummonerUI();
            return;
        }

        currentMana -= battleSkill.ManaCost; // 마나 소비
        summonerSkillUsedThisTurn = true;    // 이번 턴 사용 완료

        int amount =
            battleSkill.Amount + GetGrimoireSkillBonus(battleSkill.SkillType); // 신속 명령 반영
        string effectMessage;

        switch (battleSkill.SkillType)
        {
            case SummonerSkillType.FocusCommand:
                targetMonster.AddTurnAttackBonus(amount);
                effectMessage = $"{targetMonster.MonsterName} 이번 턴 공격 +{amount}";
                break;

            case SummonerSkillType.EmergencyDraw:
                int handCountBefore = handButtons.Count;
                DrawCards(amount);
                effectMessage = $"카드 {handButtons.Count - handCountBefore}장 드로우";
                break;

            case SummonerSkillType.ManaCycle:
                currentMana += amount; // 임시 마나는 최대 마나를 넘을 수 있고 다음 턴에 사라진다.
                effectMessage = $"임시 마나 +{amount}";
                break;

            case SummonerSkillType.ContractShield:
                playerCurrentShield += amount;
                effectMessage = $"플레이어 보호막 +{amount}";
                break;

            case SummonerSkillType.EmergencyReturn:
                string returnedName = targetMonster.MonsterName;
                ReturnMonsterToHand(targetMonster);
                effectMessage = $"{returnedName}을 손패로 되돌렸습니다";
                break;

            case SummonerSkillType.LustResonance:
                turnLustBonus += amount;
                RefreshSynergies(); // 성욕 보정 다시 계산
                effectMessage = $"이번 턴 모든 아군의 성욕 부여량 +{amount}";
                break;

            case SummonerSkillType.AbsoluteCommand:
                targetMonster.RestoreAction();
                effectMessage = $"{targetMonster.MonsterName}이 다시 행동할 수 있습니다";
                break;

            default:
                effectMessage = "효과 없음";
                break;
        }

        string skillLog = $"[소환사 스킬] {battleSkill.DisplayName}: {effectMessage}";

        resultText.text = skillLog;
        AddBattleLog(BattleLogCategory.PlayerAction, skillLog); // 스킬 사용 기록

        ApplyGrimoireSkillDraw(); // 지휘의 기억
        UpdateBattleUI();
    }

    private bool CanReturnMonsterToHand(MonsterUnit targetMonster, out string reason)
    {
        if (targetMonster == null || targetMonster.IsDead)
        {
            reason = "대상 마물이 없습니다";
            return false;
        }

        if (targetMonster.SourceCard == null)
        {
            reason = "카드 없이 소환된 마물은 손패로 되돌릴 수 없습니다";
            return false;
        }

        handButtons.RemoveAll(handButton => handButton == null);

        if (handButtons.Count >= maxHandSize)
        {
            reason = "손패가 가득 차서 되돌릴 수 없습니다";
            return false;
        }

        if (handCardCopies.ContainsValue(targetMonster.SourceCard))
        {
            reason = "이 마물의 카드가 이미 손패에 있습니다";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    // 마물을 사망 처리 없이 필드에서 치우고 카드를 손패에 넣는다. (긴급 귀환)
    private void ReturnMonsterToHand(MonsterUnit targetMonster)
    {
        CardCopy sourceCard = targetMonster.SourceCard;

        discardPile.Remove(sourceCard); // 사용할 때 버린 더미로 간 사본 회수
        drawPile.Remove(sourceCard);    // 다시 섞여 덱에 들어갔다면 덱에서 꺼낸다.

        if (selectedMonster == targetMonster)
        {
            ClearMonsterSelection();
        }

        if (previewedHeroineTarget == targetMonster)
        {
            previewedHeroineTarget = null;
        }

        fieldMonsters.Remove(targetMonster);
        Destroy(targetMonster.gameObject);

        RefreshSynergies(); // 필드 구성 변경 반영
        RefreshHeroineTargetPreview();

        CreateCardButton(sourceCard); // 손패에 카드 추가
        UpdateDeckStatusUI();
    }

    // ---------- 소환사 UI ----------

    private void UpdateSummonerUI()
    {
        if (summonerSkillButton != null)
        {
            bool canUse =
                battleSkill != null &&
                !summonerSkillUsedThisTurn &&
                currentMana >= battleSkill.ManaCost;

            summonerSkillButton.interactable =
                isPlayerTurn &&
                !isBattleEnded &&
                (isSelectingSkillTarget || canUse);

            TMP_Text buttonLabel =
                summonerSkillButton.GetComponentInChildren<TMP_Text>(true);

            if (buttonLabel != null)
            {
                buttonLabel.text = GetSummonerSkillButtonLabel();
            }
        }

        if (summonerSkillText != null)
        {
            summonerSkillText.text = battleSkill == null
                ? "장착한 스킬이 없습니다"
                : battleSkill.GetEffectText(
                    GetGrimoireSkillBonus(battleSkill.SkillType)); // 신속 명령을 반영한 수치
        }

        if (summonerPassiveText != null)
        {
            summonerPassiveText.text = GetSummonerPassiveDisplay();
        }
    }

    private string GetSummonerSkillButtonLabel()
    {
        if (battleSkill == null) { return "스킬 없음"; }
        if (isSelectingSkillTarget) { return "대상 선택 취소"; }

        if (summonerSkillUsedThisTurn)
        {
            return $"{battleSkill.DisplayName} (사용 완료)";
        }

        return $"{battleSkill.DisplayName} (마나 {battleSkill.ManaCost})";
    }

    private string GetSummonerPassiveDisplay()
    {
        if (battlePassive == null || battlePassiveRank <= 0)
        {
            PlayerProgressManager progress = PlayerProgressManager.Instance;

            return progress != null && !progress.IsPassiveSystemUnlocked
                ? $"패시브: 플레이어 Lv.{SummonerRules.PassiveSystemUnlockLevel}에 해금"
                : "패시브: 없음";
        }

        return
            $"패시브: {battlePassive.DisplayName} {battlePassiveRank}단계\n" +
            battlePassive.GetEffectText(battlePassiveRank);
    }
}
