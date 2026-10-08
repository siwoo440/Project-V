using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class SaveLoadFlow // 저장 화면의 칸 목록 (기획서 15.3)
{
    private const float SlotRowHeight = 150f; // 칸 한 줄의 높이
    private const float RowInset = 30f;       // 줄 양 끝 장식을 피하는 여백
    private const float SlotIconSize = 88f;   // 칸 아이콘 크기

    private static readonly Color WarningTextColor = new Color(1f, 0.58f, 0.52f, 1f); // 불러올 수 없는 사유

    private void BuildSlotList()
    {
        foreach (SaveSlot slot in SaveRules.Slots)
        {
            SaveSlot targetSlot = slot;

            CreateSlotRow(
                slot, GetInfo(slot), slot == selectedSlot,
                () => SelectSlot(targetSlot)
            );
        }
    }

    private static string GetSlotIconKey(SaveSlot slot, SaveSlotInfo info) // 칸 상태에 맞는 아이콘
    {
        if (info == null || !info.Exists) { return UIKeys.SaveEmpty; }
        if (!info.IsValid) { return UIKeys.SaveWarning; }

        return slot == SaveSlot.Auto ? UIKeys.SaveAuto : UIKeys.SaveWrite;
    }

    // 칸 한 줄: 아이콘, 칸 이름, 저장 일시, 레벨과 골드, 플레이 시간
    private void CreateSlotRow(
        SaveSlot slot,
        SaveSlotInfo info,
        bool isSelected,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        GameObject rowObject = UIListRow.Create(
            slotListContent, "SlotRow", SlotRowHeight, isSelected, clickAction
        );

        if (rowObject == null) { return; }

        generatedRows.Add(rowObject);

        bool hasData = info != null && info.IsValid;
        bool isBroken = info != null && info.Exists && !info.IsValid;
        Vector2 leftCenter = new Vector2(0f, 0.5f);

        Image icon = CardEntryFactory.CreateImage(
            rowObject.transform, "SlotIcon", Color.white,
            leftCenter, leftCenter,
            new Vector2(RowInset - 4f, -SlotIconSize * 0.5f),
            new Vector2(RowInset - 4f + SlotIconSize, SlotIconSize * 0.5f)
        );

        bool hasIcon = UISkin.ApplySimple(icon, GetSlotIconKey(slot, info), true);

        icon.enabled = hasIcon; // 아이콘 이미지가 없으면 글자만 보여준다.

        float textLeft = hasIcon ? RowInset + SlotIconSize + 12f : RowInset;
        Color titleColor = hasData ? UISkin.Cream : UIListRow.LockedTextColor;
        Color bodyColor = isBroken
            ? WarningTextColor
            : hasData ? UISkin.CreamSub : UIListRow.LockedTextColor;

        CardEntryFactory.CreateLabel(
            rowObject.transform, "TitleText", SaveRules.GetSlotName(slot),
            26f, titleColor, TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0.55f), new Vector2(0.55f, 1f),
            new Vector2(textLeft, 0f), new Vector2(0f, -16f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "SavedAtText",
            hasData ? SaveRules.FormatSavedAt(info.Data.savedAt) : string.Empty,
            20f, UISkin.Gold, TextAlignmentOptions.BottomRight,
            new Vector2(0.55f, 0.55f), new Vector2(1f, 1f),
            Vector2.zero, new Vector2(-RowInset, -16f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "BodyText", GetSlotSummary(info),
            19f, bodyColor, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0f), new Vector2(0.68f, 0.55f),
            new Vector2(textLeft, 16f), new Vector2(0f, -6f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "PlayTimeText",
            hasData ? $"플레이 {SaveRules.FormatPlayTime(info.Data.playTimeSeconds)}" : string.Empty,
            18f, bodyColor, TextAlignmentOptions.TopRight,
            new Vector2(0.68f, 0f), new Vector2(1f, 0.55f),
            new Vector2(0f, 16f), new Vector2(-RowInset, -6f)
        );
    }

    private static string GetSlotSummary(SaveSlotInfo info) // 칸 줄에 적는 한 줄 요약
    {
        if (info == null || !info.Exists) { return "저장 데이터 없음"; }
        if (!info.IsValid) { return info.Error; }

        return
            $"{PlayerLevelRules.GetLevelText(info.Data.totalExperience)}    " +
            $"{UISkin.IconOr(UIIcons.Gold, "골드")} {info.Data.gold}" +
            (info.UsedBackup ? "    (백업에서 복구)" : string.Empty);
    }
}
