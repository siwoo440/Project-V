using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class SummonerFlow // 소환사 화면의 목록과 패시브 설명
{
    private static readonly Color RowColor = new Color(0.17f, 0.14f, 0.26f, 1f);         // 줄 이미지가 없을 때 기본 색
    private static readonly Color RowSelectedColor = new Color(0.42f, 0.34f, 0.16f, 1f); // 줄 이미지가 없을 때 강조 색
    private static readonly Color LockedTextColor = new Color(0.56f, 0.57f, 0.64f, 1f);  // 잠긴 항목 글자색

    private void BuildSkillList(PlayerProgressManager progress)
    {
        SummonerSkillData equippedSkill = progress.EquippedSkill;

        foreach (SummonerSkillData skill in progress.SummonerSkills)
        {
            if (skill == null) { continue; }

            bool isUnlocked = progress.IsSkillUnlocked(skill);
            bool isEquipped = skill == equippedSkill;

            string stateText = isEquipped
                ? "장착 중"
                : isUnlocked ? "눌러서 장착" : $"Lv.{skill.UnlockLevel} 해금";

            SummonerSkillData targetSkill = skill;

            CreateRow(
                skillListContent,
                isEquipped,
                isUnlocked,
                $"{skill.DisplayName}   {UISkin.IconOr(UIIcons.Mana, "마나")} {skill.ManaCost}",
                skill.EffectText,
                stateText,
                () => EquipSkill(targetSkill)
            );
        }
    }

    private void BuildPassiveList(PlayerProgressManager progress)
    {
        if (!progress.IsPassiveSystemUnlocked)
        {
            selectedPassive = null;

            CreateRow(
                passiveListContent, false, false, "패시브",
                $"플레이어 Lv.{SummonerRules.PassiveSystemUnlockLevel}에 해금됩니다.",
                "잠김", null
            );

            return;
        }

        if (selectedPassive == null)
        {
            selectedPassive = progress.EquippedPassive; // 장착한 패시브를 기본 선택
        }

        foreach (SummonerPassiveData passive in progress.SummonerPassives)
        {
            if (passive == null) { continue; }

            bool isVisible = progress.IsPassiveVisible(passive);
            bool isEquipped = passive == progress.EquippedPassive;
            int rank = progress.GetPassiveRank(passive);

            if (selectedPassive == null && isVisible)
            {
                selectedPassive = passive; // 처음 열린 패시브를 기본 선택
            }

            string titleText = rank > 0
                ? $"{passive.DisplayName}   {rank} / {passive.MaxRank}단계"
                : passive.DisplayName;

            string bodyText = isVisible
                ? passive.GetEffectText(Mathf.Max(1, rank))
                : "레벨이 오르면 내용이 공개됩니다.";

            string stateText = !isVisible
                ? $"Lv.{passive.UnlockLevel} 공개"
                : isEquipped ? "장착 중" : rank > 0 ? "해금됨" : "미해금";

            SummonerPassiveData targetPassive = passive;

            CreateRow(
                passiveListContent,
                passive == selectedPassive,
                isVisible,
                titleText,
                bodyText,
                stateText,
                () => SelectPassive(targetPassive)
            );
        }
    }

    private void UpdatePassiveDetail(PlayerProgressManager progress)
    {
        bool hasSelection =
            progress.IsPassiveSystemUnlocked && selectedPassive != null;

        if (!hasSelection)
        {
            if (passiveDetailText != null)
            {
                passiveDetailText.text = progress.IsPassiveSystemUnlocked
                    ? "패시브를 선택하세요."
                    : $"패시브는 플레이어 Lv.{SummonerRules.PassiveSystemUnlockLevel}에 해금됩니다.";
            }

            SetButton(equipPassiveButton, "장착", false);
            SetButton(upgradePassiveButton, "해금", false);

            return;
        }

        int rank = progress.GetPassiveRank(selectedPassive);
        bool isMaxRank = rank >= selectedPassive.MaxRank;
        bool isEquipped = progress.EquippedPassive == selectedPassive;
        bool canUpgrade = progress.CanUpgradePassive(selectedPassive, out string reason);

        if (passiveDetailText != null)
        {
            string currentText = rank > 0
                ? selectedPassive.GetEffectText(rank)
                : "미해금";

            string nextText = isMaxRank
                ? "최대 단계입니다."
                : selectedPassive.GetEffectText(rank + 1);

            string costText = isMaxRank
                ? string.Empty
                : $"\n필요 포인트 {selectedPassive.PointCostPerRank}   " +
                  (canUpgrade ? "해금하면 되돌릴 수 없습니다." : reason);

            passiveDetailText.text =
                $"{selectedPassive.DisplayName}   {rank} / {selectedPassive.MaxRank}단계\n" +
                $"현재: {currentText}\n" +
                $"다음: {nextText}" +
                costText;
        }

        SetButton(equipPassiveButton, isEquipped ? "장착 중" : "장착", rank > 0 && !isEquipped);

        SetButton(
            upgradePassiveButton,
            isMaxRank ? "최대 단계" : rank > 0 ? "강화" : "해금",
            canUpgrade
        );
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
