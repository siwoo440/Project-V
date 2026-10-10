using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능

public partial class StoryRecallFlow // 스토리 회상 화면의 목록과 상세
{
    private const float RowHeight = 84f; // 장면 한 줄의 높이
    private const float RowInset = 30f;  // 줄 양 끝 장식을 피하는 여백

    private void Refresh()
    {
        ClearRows();

        if (PlayerProgressManager.Instance == null)
        {
            SetText(countText, string.Empty);
            SetText(detailTitleText, "진행 데이터가 없습니다");
            SetText(detailText, string.Empty);
            SetText(messageText, "메인 메뉴에서 이어하기나 새 게임을 먼저 선택하세요.");

            if (playButton != null) { playButton.interactable = false; }

            return;
        }

        StoryCatalogEntry selectedEntry = GetSelectedEntry();
        int seenCount = 0;

        foreach (StoryCatalogEntry entry in entries)
        {
            StoryCatalogEntry targetEntry = entry;
            bool isSeen = IsSeen(entry);

            if (isSeen) { seenCount += 1; }

            CreateRow(entry, isSeen, entry == selectedEntry, () => SelectEntry(targetEntry));
        }

        SetText(countText, $"본 장면 {seenCount} / {entries.Count}");
        UpdateDetail(selectedEntry);
    }

    // 장면 한 줄: 제목, 지역과 나오는 곳, 상태. 보지 않은 장면은 제목을 감춘다.
    private void CreateRow(
        StoryCatalogEntry entry,
        bool isSeen,
        bool isSelected,
        UnityEngine.Events.UnityAction clickAction
    )
    {
        GameObject rowObject = UIListRow.Create(
            listContent, "RecallRow", RowHeight, isSelected, clickAction
        );

        if (rowObject == null) { return; }

        generatedRows.Add(rowObject);

        Color titleColor = isSeen ? UISkin.Cream : UIListRow.LockedTextColor;
        Color bodyColor = isSeen ? UISkin.CreamSub : UIListRow.LockedTextColor;

        string stateText = !isSeen
            ? $"{UISkin.IconOr(UIIcons.Lock, "잠김")}"
            : entry.Scene.ChoiceCount > 0 ? "선택지 있음" : string.Empty;

        CardEntryFactory.CreateLabel(
            rowObject.transform, "TitleText", isSeen ? entry.Scene.Title : "아직 보지 않은 장면",
            23f, titleColor, TextAlignmentOptions.BottomLeft,
            new Vector2(0f, 0.5f), new Vector2(0.76f, 1f),
            new Vector2(RowInset, 0f), new Vector2(0f, -10f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "PlaceText", $"{entry.RegionLabel}  ·  {entry.Place}",
            17f, bodyColor, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 0f), new Vector2(0.76f, 0.5f),
            new Vector2(RowInset, 10f), new Vector2(0f, -4f)
        );

        CardEntryFactory.CreateLabel(
            rowObject.transform, "StateText", stateText,
            18f, bodyColor, TextAlignmentOptions.Right,
            new Vector2(0.76f, 0f), new Vector2(1f, 1f),
            Vector2.zero, new Vector2(-RowInset, 0f)
        );
    }

    private void UpdateDetail(StoryCatalogEntry entry)
    {
        bool isSeen = IsSeen(entry);

        if (playButton != null) { playButton.interactable = isSeen; }

        if (entry == null)
        {
            SetText(detailTitleText, "장면이 없습니다");
            SetText(detailText, string.Empty);
            SetText(messageText, "스토리를 진행하면 본 장면이 이곳에 모입니다.");
            return;
        }

        if (!isSeen)
        {
            SetText(detailTitleText, "아직 보지 않은 장면");

            SetText(detailText,
                $"{entry.RegionLabel}\n" +
                $"볼 수 있는 곳: {entry.Place}\n\n" +
                "스토리를 진행해 이 장면을 보면 여기서 다시 볼 수 있습니다.");

            SetText(messageText, "보지 않은 장면은 다시 볼 수 없습니다.");
            return;
        }

        int choiceCount = entry.Scene.ChoiceCount;

        string choiceText = choiceCount > 0
            ? $"선택지 {choiceCount}곳 (다시 볼 때 다른 답을 골라 볼 수 있습니다)"
            : "선택지 없음";

        SetText(detailTitleText, entry.Scene.Title);

        SetText(detailText,
            $"{entry.RegionLabel}\n" +
            $"나오는 곳: {entry.Place}\n" +
            $"대사 {entry.Scene.Lines.Count}줄\n" +
            $"{choiceText}\n\n" +
            "다시 봐도 진행과 보상은 바뀌지 않습니다.");

        SetText(messageText, "다시 보기를 누르면 장면을 처음부터 봅니다.");
    }
}
