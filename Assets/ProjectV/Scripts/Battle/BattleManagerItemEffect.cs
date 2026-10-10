using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 소모성 전투 아이템의 효과와 표시
{
    private void LogBattleItemLoadout() // 전투 시작 시 가져온 아이템 기록
    {
        if (battleItem == null) { return; }

        AddBattleLog(
            BattleLogCategory.System,
            $"소모품: {battleItem.DisplayName}"
        );
    }

    private void ExecuteBattleItem(MonsterUnit targetMonster) // 효과 발동 후 아이템 소모
    {
        if (!CanUseBattleItem(out string reason))
        {
            resultText.text = reason;
            UpdateBattleItemUI();
            return;
        }

        if (battleItem.NeedsTarget && (targetMonster == null || targetMonster.IsDead))
        {
            resultText.text = "대상 마물이 없습니다";
            UpdateBattleItemUI();
            return;
        }

        int amount = battleItem.Amount;
        string effectMessage;

        switch (battleItem.ItemType)
        {
            case BattleItemType.HealPlayer:
                int previousHp = playerCurrentHp;
                playerCurrentHp = Mathf.Min(PlayerMaxHp, playerCurrentHp + amount);
                effectMessage = $"플레이어 HP +{playerCurrentHp - previousHp}";
                break;

            case BattleItemType.RestoreMana:
                int previousMana = currentMana;
                currentMana = Mathf.Min(maximumMana, currentMana + amount); // 최대 마나를 넘지 않는다.
                effectMessage = $"마나 +{currentMana - previousMana}";
                break;

            case BattleItemType.ShieldMonster:
                targetMonster.AddShield(amount);
                effectMessage = $"{targetMonster.MonsterName} 보호막 +{amount}";
                break;

            case BattleItemType.DrawCards:
                int handCountBefore = handButtons.Count;
                DrawCards(amount);
                effectMessage = $"카드 {handButtons.Count - handCountBefore}장 드로우";
                break;

            case BattleItemType.CleanseAllies:
                int removedCount = 0;

                foreach (MonsterUnit fieldMonster in fieldMonsters)
                {
                    if (fieldMonster == null || fieldMonster.IsDead) { continue; }

                    removedCount += fieldMonster.RemoveNegativeStatus(amount);
                }

                effectMessage = $"해로운 상태 효과 {removedCount}개 제거";
                break;

            default:
                effectMessage = "효과 없음";
                break;
        }

        battleItemUsed = true; // 전투마다 한 번

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress != null)
        {
            progress.ConsumeBattleItem(battleItem); // 사용한 아이템은 전투 결과와 관계없이 소모된다.
        }

        string itemLog = $"[소모품] {battleItem.DisplayName}: {effectMessage}";

        resultText.text = itemLog;
        AddBattleLog(BattleLogCategory.PlayerAction, itemLog); // 아이템 사용 기록

        UpdateBattleUI();
    }

    private void UpdateBattleItemUI() // 아이템 버튼의 글자와 사용 가능 여부
    {
        if (battleItemButton == null) { return; }

        bool canUse = battleItem != null && !battleItemUsed;

        battleItemButton.interactable =
            isPlayerTurn &&
            !isBattleEnded &&
            (isSelectingItemTarget || canUse);

        TMP_Text buttonLabel =
            battleItemButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel != null)
        {
            buttonLabel.text = GetBattleItemButtonLabel();
        }

        // 그림 버튼: 장착한 아이템의 그림을 보여 주고, 쓸 수 없으면 흐리게 한다.
        Transform iconTransform = battleItemButton.transform.Find("Icon");
        Image iconImage = iconTransform == null ? null : iconTransform.GetComponent<Image>();

        if (iconImage != null)
        {
            iconImage.enabled = UISkin.ApplySimple(
                iconImage,
                battleItem == null ? UIKeys.ItemEmpty : battleItem.IconKey,
                true
            );

            iconImage.color = canUse || isSelectingItemTarget
                ? Color.white
                : new Color(1f, 1f, 1f, 0.4f);
        }
    }

    private string GetBattleItemButtonLabel()
    {
        if (battleItem == null) { return "소모품 없음"; }
        if (isSelectingItemTarget) { return "대상 선택 취소"; }
        if (battleItemUsed) { return "소모품 사용 완료"; }

        return battleItem.DisplayName;
    }
}
