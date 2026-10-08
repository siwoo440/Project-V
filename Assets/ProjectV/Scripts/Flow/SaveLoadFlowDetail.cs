using System.Collections.Generic; // 리스트 기능
using System.Text; // 문자열 조립 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class SaveLoadFlow // 저장 화면의 선택한 칸 상세와 버튼 상태
{
    private void UpdateDetail()
    {
        SaveSlotInfo info = GetInfo(selectedSlot);

        bool exists = info != null && info.Exists;
        bool isValid = info != null && info.IsValid;

        if (detailIconImage != null)
        {
            detailIconImage.enabled =
                UISkin.ApplySimple(detailIconImage, GetSlotIconKey(selectedSlot, info), true);
        }

        SetText(detailTitleText, SaveRules.GetSlotName(selectedSlot));
        SetText(detailText, GetDetailText(info));

        if (saveButton != null)
        {
            saveButton.gameObject.SetActive(allowSave); // 불러오기만 하는 화면에서는 저장 버튼을 숨긴다.
        }

        SetButton(
            saveButton,
            pendingAction == PendingAction.Save ? "한 번 더 눌러 덮어쓰기" : "저장",
            allowSave && SaveRules.IsManual(selectedSlot)
        );

        SetButton(
            loadButton,
            pendingAction == PendingAction.Load ? "한 번 더 눌러 불러오기" : "불러오기",
            isValid
        );

        SetButton(
            deleteButton,
            pendingAction == PendingAction.Delete
                ? "삭제 확인"
                : pendingAction == PendingAction.DeleteFinal ? "한 번 더 눌러 삭제" : "삭제",
            exists
        );
    }

    private string GetDetailText(SaveSlotInfo info) // 선택한 칸의 진행 정보
    {
        if (info == null || !info.Exists)
        {
            return allowSave && SaveRules.IsManual(selectedSlot)
                ? "저장 데이터 없음\n\n이 칸에 지금 진행을 저장할 수 있습니다."
                : "저장 데이터 없음";
        }

        string warningHex = ColorUtility.ToHtmlStringRGB(WarningTextColor);

        if (!info.IsValid)
        {
            return
                $"<color=#{warningHex}>{info.Error}</color>\n\n" +
                "이 칸은 불러올 수 없습니다. 삭제한 뒤 다시 저장할 수 있습니다.";
        }

        SaveData data = info.Data;
        StringBuilder builder = new StringBuilder();

        builder.Append($"{PlayerLevelRules.GetLevelText(data.totalExperience)}\n");

        builder.Append(
            $"{UISkin.IconOr(UIIcons.Gold, "골드")} {data.gold}    " +
            $"{UISkin.IconOr(UIIcons.Essence, "정수")} {data.monsterEssence}    " +
            $"{UISkin.IconOr(UIIcons.Shard, "파편")} {data.desireShards}\n\n"
        );

        builder.Append($"보유 카드  {CountCopies(data)}장\n");
        builder.Append($"사용 중인 덱  {GetDeckText(data)}\n");
        builder.Append($"그리모어 강화  {SumValues(data.grimoireLevels)}단계\n");
        builder.Append($"소모성 아이템  {SumValues(data.battleItems)}개\n\n");
        builder.Append($"플레이 시간  {SaveRules.FormatPlayTime(data.playTimeSeconds)}\n");
        builder.Append($"저장 일시  {SaveRules.FormatSavedAt(data.savedAt)}\n");
        builder.Append($"게임 버전  {data.gameVersion}");

        if (info.UsedBackup)
        {
            builder.Append(
                $"\n\n<color=#{warningHex}>본 파일에 문제가 있어 백업 데이터를 읽었습니다.</color>"
            );
        }

        return builder.ToString();
    }

    private static int CountCopies(SaveData data) // 보유 카드 사본 수
    {
        int count = 0;

        foreach (SaveCardEntry cardEntry in data.cards)
        {
            if (cardEntry == null) { continue; }

            count += cardEntry.copies.Count;
        }

        return count;
    }

    private static string GetDeckText(SaveData data) // 사용 중인 덱의 이름과 장수
    {
        if (data.selectedDeckIndex < 0 || data.selectedDeckIndex >= data.decks.Count)
        {
            return "없음";
        }

        SaveDeckEntry deck = data.decks[data.selectedDeckIndex];

        return $"{deck.name} ({deck.cards.Count}장)";
    }

    private static int SumValues(List<SaveCountEntry> entries) // 단계나 수량의 합
    {
        int total = 0;

        foreach (SaveCountEntry entry in entries)
        {
            if (entry == null) { continue; }

            total += entry.value;
        }

        return total;
    }

    private void SetText(TMP_Text targetText, string content)
    {
        if (targetText == null) { return; }

        targetText.text = content;
    }

    private void SetButton(Button targetButton, string label, bool isInteractable)
    {
        if (targetButton == null) { return; }

        targetButton.interactable = isInteractable;

        TMP_Text buttonLabel = targetButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel != null)
        {
            buttonLabel.text = label;
        }
    }
}
