using System.Collections.Generic; // 리스트 기능
using System.Text; // 문자열 조립 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 전투 준비 화면의 내 준비 상태 (기획서 6.14의 플레이어 정보, 덱 정보, 준비 기능)
// 덱 프리셋, 액티브 스킬, 패시브, 소모품을 이 화면에서 바꾼다. 스킬과 패시브는 누를 때마다 다음 것으로 바뀐다.
public partial class BattlePrepareFlow
{
    [Header("내 준비")]
    [SerializeField] private TMP_Text playerTitleText;  // 플레이어 레벨
    [SerializeField] private TMP_Text presetText;       // 고른 프리셋 이름
    [SerializeField] private TMP_Text deckText;         // 장수, 평균 마나, 계열 구성
    [SerializeField] private Button presetPrevButton;   // 앞 프리셋
    [SerializeField] private Button presetNextButton;   // 다음 프리셋
    [SerializeField] private Button deckBuilderButton;  // 덱 편성 화면
    [SerializeField] private Button skillButton;        // 액티브 스킬 변경
    [SerializeField] private Button passiveButton;      // 패시브 변경
    [SerializeField] private Button itemSlotButton;     // 소모품 변경
    [SerializeField] private Button summonerButton;     // 소환사 화면

    private void StartLoadoutButtons()
    {
        AddListener(presetPrevButton, () => ChangePreset(-1));
        AddListener(presetNextButton, () => ChangePreset(1));
        AddListener(deckBuilderButton, SceneFlow.LoadDeckBuilder);
        AddListener(skillButton, CycleSkill);
        AddListener(passiveButton, CyclePassive);
        AddListener(itemSlotButton, CycleBattleItem);
        AddListener(summonerButton, SceneFlow.LoadSummoner);
    }

    private void ChangePreset(int step) // 프리셋을 앞뒤로 넘긴다.
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        int count = PlayerProgressManager.PresetCount;
        int nextIndex = (progress.SelectedPresetIndex + step + count) % count;

        lastMessage = $"덱 프리셋을 바꿨습니다: {progress.GetPresetName(nextIndex)}";

        progress.SelectPreset(nextIndex); // 바뀌면 화면을 다시 그린다.
    }

    private void CycleSkill() // 해금한 액티브 스킬 가운데 다음 스킬을 장착한다.
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        List<SummonerSkillData> unlockedSkills = new List<SummonerSkillData>();

        foreach (SummonerSkillData skill in progress.SummonerSkills)
        {
            if (skill != null && progress.IsSkillUnlocked(skill)) { unlockedSkills.Add(skill); }
        }

        if (unlockedSkills.Count <= 1)
        {
            ShowMessage("바꿀 수 있는 다른 액티브 스킬이 없습니다. 플레이어 레벨이 오르면 열립니다.");
            return;
        }

        int nextIndex = (unlockedSkills.IndexOf(progress.EquippedSkill) + 1) % unlockedSkills.Count;

        progress.TryEquipSkill(unlockedSkills[nextIndex], out lastMessage);
        RefreshStartButton();
    }

    private void CyclePassive() // 해금한 패시브 가운데 다음 패시브를 장착한다.
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        if (!progress.IsPassiveSystemUnlocked)
        {
            ShowMessage("패시브는 아직 열리지 않았습니다.");
            return;
        }

        List<SummonerPassiveData> ownedPassives = new List<SummonerPassiveData>();

        foreach (SummonerPassiveData passive in progress.SummonerPassives)
        {
            if (passive != null && progress.GetPassiveRank(passive) > 0) { ownedPassives.Add(passive); }
        }

        if (ownedPassives.Count == 0)
        {
            ShowMessage("해금한 패시브가 없습니다. 소환사 화면에서 패시브 포인트로 해금하세요.");
            return;
        }

        int nextIndex = (ownedPassives.IndexOf(progress.EquippedPassive) + 1) % ownedPassives.Count;

        if (!progress.TryEquipPassive(ownedPassives[nextIndex], out lastMessage))
        {
            ShowMessage("바꿀 수 있는 다른 패시브가 없습니다.");
            return;
        }

        RefreshStartButton();
    }

    private void CycleBattleItem() // 누를 때마다 없음 → 가진 아이템 순으로 바꾼다. (기획서 9.12.3)
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        if (progress.GetOwnedBattleItems().Count == 0)
        {
            ShowMessage("가진 소모품이 없습니다. 상점에서 살 수 있습니다.");
            return;
        }

        BattleItemData equippedItem = progress.CycleEquippedBattleItem();

        ShowMessage(
            equippedItem == null
                ? "소모품을 가져가지 않습니다."
                : $"소모품을 골랐습니다: {equippedItem.DisplayName}"
        );
    }

    private void RefreshLoadout()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null)
        {
            SetText(playerTitleText, "내 준비");
            SetText(presetText, "진행 데이터 없음");
            SetText(deckText, string.Empty);
            return;
        }

        SetText(playerTitleText, $"내 준비    플레이어 {progress.PlayerLevelText}");

        SetText(
            presetText,
            $"프리셋 {progress.SelectedPresetIndex + 1}: {progress.GetPresetName(progress.SelectedPresetIndex)}"
        );

        SetText(deckText, GetDeckText(progress));

        SummonerSkillData skill = progress.EquippedSkill;

        SetButtonLabel(
            skillButton,
            skill == null
                ? "액티브 스킬: 없음"
                : $"액티브 스킬: {skill.DisplayName} ({UISkin.IconOr(UIIcons.Mana, "마나")} {skill.ManaCost})  {skill.EffectText}"
        );

        SummonerPassiveData passive = progress.EquippedPassive;

        SetButtonLabel(
            passiveButton,
            !progress.IsPassiveSystemUnlocked
                ? "패시브: 아직 열리지 않음"
                : passive == null
                    ? "패시브: 없음 (눌러서 선택)"
                    : $"패시브: {passive.DisplayName} {progress.EquippedPassiveRank}단계  {passive.GetEffectText(progress.EquippedPassiveRank)}"
        );

        RefreshItemSlot(progress);
    }

    // 덱 요약: 장수, 평균 마나, 계열별 장수 (많은 순)
    private string GetDeckText(PlayerProgressManager progress)
    {
        IReadOnlyList<CardCopy> deck = progress.CurrentDeck;
        Dictionary<MonsterType, int> typeCounts = new Dictionary<MonsterType, int>();

        foreach (CardCopy copy in deck)
        {
            if (copy == null || copy.CardData == null) { continue; }

            MonsterType type = copy.CardData.MainType;

            typeCounts.TryGetValue(type, out int count);
            typeCounts[type] = count + 1;
        }

        List<KeyValuePair<MonsterType, int>> sortedTypes =
            new List<KeyValuePair<MonsterType, int>>(typeCounts);

        sortedTypes.Sort((left, right) => right.Value.CompareTo(left.Value));

        List<string> typeTexts = new List<string>();

        foreach (KeyValuePair<MonsterType, int> pair in sortedTypes)
        {
            typeTexts.Add($"{MonsterTypeRules.GetDisplayName(pair.Key)} {pair.Value}");
        }

        StringBuilder builder = new StringBuilder();

        builder.Append(
            $"덱 {deck.Count} / {progress.RequiredDeckSize}장    " +
            $"평균 {UISkin.IconOr(UIIcons.Mana, "마나")} {progress.CurrentDeckAverageMana:0.0}\n"
        );

        builder.Append(typeTexts.Count == 0 ? "계열: 없음" : "계열: " + string.Join(", ", typeTexts));

        return builder.ToString();
    }

    private void RefreshItemSlot(PlayerProgressManager progress) // 소모품 칸의 글자와 아이콘
    {
        if (itemSlotButton == null) { return; }

        BattleItemData equippedItem = progress.EquippedBattleItem;
        bool hasAnyItem = progress.GetOwnedBattleItems().Count > 0;

        SetButtonLabel(
            itemSlotButton,
            equippedItem != null
                ? $"소모품: {equippedItem.DisplayName} ({progress.GetBattleItemCount(equippedItem)}개)"
                : hasAnyItem
                    ? "소모품: 없음 (눌러서 선택)"
                    : "소모품: 없음 (상점에서 구매)"
        );

        Transform iconTransform = itemSlotButton.transform.Find("Icon");
        Image slotIcon = iconTransform == null ? null : iconTransform.GetComponent<Image>();

        if (slotIcon != null)
        {
            slotIcon.enabled = UISkin.ApplySimple(
                slotIcon,
                equippedItem != null ? equippedItem.IconKey : UIKeys.ItemEmpty,
                true
            ); // 아이콘 이미지가 없으면 숨긴다.
        }
    }

    private static void SetButtonLabel(Button button, string label)
    {
        if (button == null) { return; }

        TMP_Text buttonLabel = button.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel != null) { buttonLabel.text = label; }
    }
}
