using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 전투에 가져갈 소모성 아이템 선택 (기획서 9.12.3: 전투 전에 1개 장착)
// 기획서는 전투 준비 화면에서 장착하지만, 그 화면이 생기기 전까지 지역 화면에 둔다.
public partial class StageSelectFlow
{
    private void CycleBattleItem() // 누를 때마다 없음 → 가진 아이템 순으로 바꾼다.
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        if (progress.GetOwnedBattleItems().Count > 0)
        {
            progress.CycleEquippedBattleItem();
        }

        RefreshItemSlot();
    }

    private void RefreshItemSlot() // 장착 칸의 글자와 아이콘 갱신
    {
        if (itemSlotButton == null) { return; }

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        BattleItemData equippedItem =
            progress == null ? null : progress.EquippedBattleItem;

        bool hasAnyItem =
            progress != null && progress.GetOwnedBattleItems().Count > 0;

        TMP_Text slotLabel =
            itemSlotButton.GetComponentInChildren<TMP_Text>(true);

        if (slotLabel != null)
        {
            slotLabel.text = equippedItem != null
                ? $"소모품: {equippedItem.DisplayName} ({progress.GetBattleItemCount(equippedItem)}개)"
                : hasAnyItem
                    ? "소모품: 없음 (눌러서 선택)"
                    : "소모품: 없음 (상점에서 구매)";
        }

        Transform iconTransform = itemSlotButton.transform.Find("Icon");

        Image slotIcon =
            iconTransform == null ? null : iconTransform.GetComponent<Image>();

        if (slotIcon != null)
        {
            slotIcon.enabled = UISkin.ApplySimple(
                slotIcon,
                equippedItem != null ? equippedItem.IconKey : UIKeys.ItemEmpty,
                true
            ); // 아이콘 이미지가 없으면 숨긴다.
        }
    }
}
