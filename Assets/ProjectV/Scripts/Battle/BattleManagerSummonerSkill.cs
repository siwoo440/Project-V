using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능

public partial class BattleManager // 소환사 액티브 스킬 사용 (기획서 6.4.3)
{
    // 스킬 버튼을 눌렀을 때 호출된다. 대상이 필요한 스킬은 대상 선택 상태로 들어간다.
    public void OnSummonerSkillButton()
    {
        if (!isPlayerTurn || isBattleEnded) { return; }

        if (isSelectingSkillTarget)
        {
            CancelSummonerSkillTargeting("스킬 사용을 취소했습니다");
            return;
        }

        if (!CanUseSummonerSkill(out string reason))
        {
            resultText.text = reason;
            return;
        }

        if (!battleSkill.NeedsTarget)
        {
            ExecuteSummonerSkill(null);
            return;
        }

        if (MarkSkillTargets(true) <= 0)
        {
            MarkSkillTargets(false);
            resultText.text = GetNoSkillTargetMessage();
            return;
        }

        isSelectingSkillTarget = true;
        ClearMonsterSelection();

        resultText.text =
            $"{battleSkill.DisplayName}: 대상 마물을 선택하세요 (버튼을 다시 누르면 취소)";

        UpdateSummonerUI();
    }

    private bool CanUseSummonerSkill(out string reason) // 사용 조건 확인
    {
        if (battleSkill == null)
        {
            reason = "장착한 소환사 스킬이 없습니다";
            return false;
        }

        if (summonerSkillUsedThisTurn)
        {
            reason = "소환사 스킬은 턴마다 한 번만 사용할 수 있습니다";
            return false;
        }

        if (currentMana < battleSkill.ManaCost)
        {
            reason = $"마나가 부족합니다 ({currentMana} / {battleSkill.ManaCost})";
            return false;
        }

        if (battleSkill.SkillType == SummonerSkillType.EmergencyDraw)
        {
            handButtons.RemoveAll(handButton => handButton == null);

            if (handButtons.Count >= maxHandSize)
            {
                reason = "손패가 가득 차서 카드를 뽑을 수 없습니다";
                return false;
            }
        }

        reason = string.Empty;
        return true;
    }

    private bool IsValidSkillTarget(MonsterUnit monsterUnit) // 스킬별 대상 조건
    {
        if (battleSkill == null) { return false; }
        if (monsterUnit == null || monsterUnit.IsDead) { return false; }

        switch (battleSkill.SkillType)
        {
            case SummonerSkillType.FocusCommand:
                return monsterUnit.CanAttack; // 이번 턴에 공격할 수 있는 마물

            case SummonerSkillType.EmergencyReturn:
                return monsterUnit.SourceCard != null; // 카드로 소환한 마물

            case SummonerSkillType.AbsoluteCommand:
                return monsterUnit.HasActed; // 행동을 마친 마물

            default:
                return false;
        }
    }

    private string GetNoSkillTargetMessage()
    {
        switch (battleSkill.SkillType)
        {
            case SummonerSkillType.FocusCommand:
                return "행동 가능한 마물이 없습니다";

            case SummonerSkillType.EmergencyReturn:
                return "손패로 되돌릴 수 있는 마물이 없습니다";

            case SummonerSkillType.AbsoluteCommand:
                return "행동을 마친 마물이 없습니다";

            default:
                return "대상이 없습니다";
        }
    }

    // 대상 후보 표시를 켜거나 끄고, 후보 수를 돌려준다.
    private int MarkSkillTargets(bool isMarked)
    {
        int targetCount = 0;

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null) { continue; }

            bool isTarget = isMarked && IsValidSkillTarget(fieldMonster);

            fieldMonster.SetSkillTargetable(isTarget);

            if (isTarget) { targetCount += 1; }
        }

        return targetCount;
    }

    private void CancelSummonerSkillTargeting(string message) // 대상 선택 취소
    {
        if (!isSelectingSkillTarget) { return; }

        isSelectingSkillTarget = false;
        MarkSkillTargets(false);

        if (!string.IsNullOrEmpty(message) && resultText != null)
        {
            resultText.text = message;
        }

        UpdateSummonerUI();
    }

    private void ResolveSummonerSkillTarget(MonsterUnit targetMonster) // 대상 선택 완료
    {
        if (!IsValidSkillTarget(targetMonster))
        {
            resultText.text = "이 마물은 대상으로 선택할 수 없습니다";
            return;
        }

        isSelectingSkillTarget = false;
        MarkSkillTargets(false);

        ExecuteSummonerSkill(targetMonster);
    }
}
