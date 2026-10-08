using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능

// 소모성 전투 아이템 사용 (기획서 9.12.3)
// 전투마다 한 번, 플레이어 턴에 쓴다. 마나와 마물의 행동은 쓰지 않는다.
public partial class BattleManager
{
    private BattleItemData battleItem;    // 이번 전투에 가져온 아이템
    private bool battleItemUsed;          // 이번 전투에서 사용했는지 여부
    private bool isSelectingItemTarget;   // 아이템 대상 선택 중 여부

    // 전투 시작 시 장착한 아이템을 읽는다.
    private void PrepareBattleItemForBattle()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        battleItem = progress == null ? null : progress.EquippedBattleItem;
        battleItemUsed = false;
        isSelectingItemTarget = false;

        if (battleItemButton != null)
        {
            battleItemButton.onClick.RemoveAllListeners();
            battleItemButton.onClick.AddListener(OnBattleItemButton);
        }
    }

    // 아이템 버튼을 눌렀을 때 호출된다. 대상이 필요한 아이템은 대상 선택 상태로 들어간다.
    public void OnBattleItemButton()
    {
        if (!isPlayerTurn || isBattleEnded) { return; }

        if (isSelectingItemTarget)
        {
            CancelBattleItemTargeting("아이템 사용을 취소했습니다");
            return;
        }

        if (!CanUseBattleItem(out string reason))
        {
            resultText.text = reason;
            return;
        }

        if (!battleItem.NeedsTarget)
        {
            ExecuteBattleItem(null);
            return;
        }

        CancelSummonerSkillTargeting(string.Empty); // 스킬 대상 선택과 겹치지 않게 한다.

        if (MarkItemTargets(true) <= 0)
        {
            MarkItemTargets(false);
            resultText.text = "대상으로 삼을 아군 마물이 없습니다";
            return;
        }

        isSelectingItemTarget = true;
        ClearMonsterSelection();

        resultText.text =
            $"{battleItem.DisplayName}: 대상 마물을 선택하세요 (버튼을 다시 누르면 취소)";

        UpdateBattleItemUI();
    }

    private bool CanUseBattleItem(out string reason) // 사용 조건 확인
    {
        if (battleItem == null)
        {
            reason = "장착한 소모성 아이템이 없습니다";
            return false;
        }

        if (battleItemUsed)
        {
            reason = "소모성 아이템은 전투마다 한 번만 사용할 수 있습니다";
            return false;
        }

        switch (battleItem.ItemType)
        {
            case BattleItemType.HealPlayer:
                if (playerCurrentHp >= PlayerMaxHp)
                {
                    reason = "플레이어 HP가 가득 차 있습니다";
                    return false;
                }

                break;

            case BattleItemType.RestoreMana:
                if (currentMana >= maximumMana)
                {
                    reason = "마나가 가득 차 있습니다";
                    return false;
                }

                break;

            case BattleItemType.DrawCards:
                handButtons.RemoveAll(handButton => handButton == null);

                if (handButtons.Count >= maxHandSize)
                {
                    reason = "손패가 가득 차서 카드를 뽑을 수 없습니다";
                    return false;
                }

                break;

            case BattleItemType.CleanseAllies:
                if (!HasAllyNegativeStatus())
                {
                    reason = "해제할 해로운 상태 효과가 없습니다";
                    return false;
                }

                break;
        }

        reason = string.Empty;
        return true;
    }

    private bool HasAllyNegativeStatus() // 해로운 상태 효과에 걸린 아군 마물이 있는지 여부
    {
        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }
            if (fieldMonster.HasNegativeStatus()) { return true; }
        }

        return false;
    }

    // 대상 후보 표시를 켜거나 끄고, 후보 수를 돌려준다. 살아 있는 아군 마물이 모두 후보다.
    private int MarkItemTargets(bool isMarked)
    {
        int targetCount = 0;

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null) { continue; }

            bool isTarget = isMarked && !fieldMonster.IsDead;

            fieldMonster.SetSkillTargetable(isTarget);

            if (isTarget) { targetCount += 1; }
        }

        return targetCount;
    }

    private void CancelBattleItemTargeting(string message) // 대상 선택 취소
    {
        if (!isSelectingItemTarget) { return; }

        isSelectingItemTarget = false;
        MarkItemTargets(false);

        if (!string.IsNullOrEmpty(message) && resultText != null)
        {
            resultText.text = message;
        }

        UpdateBattleItemUI();
    }

    private void ResolveBattleItemTarget(MonsterUnit targetMonster) // 대상 선택 완료
    {
        if (targetMonster == null || targetMonster.IsDead)
        {
            resultText.text = "이 마물은 대상으로 선택할 수 없습니다";
            return;
        }

        isSelectingItemTarget = false;
        MarkItemTargets(false);

        ExecuteBattleItem(targetMonster);
    }
}
