using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

public partial class PlayerProgressManager // 소환사 액티브 스킬과 패시브 (기획서 6.4 / 6.5)
{
    public IReadOnlyList<SummonerSkillData> SummonerSkills =>
        summonerSkills; // 액티브 스킬 목록 반환

    public IReadOnlyList<SummonerPassiveData> SummonerPassives =>
        summonerPassives; // 패시브 목록 반환

    public bool IsPassiveSystemUnlocked =>
        SummonerRules.IsPassiveSystemUnlocked(PlayerLevel); // 패시브 사용 가능 여부

    public int TotalPassivePoints =>
        SummonerRules.GetTotalPassivePoints(PlayerLevel); // 지금까지 받은 패시브 포인트

    public int PassivePoints =>
        Mathf.Max(0, TotalPassivePoints - spentPassivePoints); // 남은 패시브 포인트

    public SummonerSkillData EquippedSkill // 장착한 액티브 스킬 (없으면 해금된 첫 스킬)
    {
        get
        {
            if (equippedSkill == null || !IsSkillUnlocked(equippedSkill))
            {
                equippedSkill = FindDefaultSkill();
            }

            return equippedSkill;
        }
    }

    public SummonerPassiveData EquippedPassive =>
        IsPassiveSystemUnlocked ? equippedPassive : null; // 장착한 패시브 반환

    public int EquippedPassiveRank =>
        GetPassiveRank(EquippedPassive); // 장착한 패시브의 단계 반환

    public bool IsSkillUnlocked(SummonerSkillData skill) // 액티브 스킬 해금 여부
    {
        return skill != null && PlayerLevel >= skill.UnlockLevel;
    }

    public bool IsPassiveVisible(SummonerPassiveData passive) // 패시브가 목록에 열렸는지 여부
    {
        return passive != null &&
               IsPassiveSystemUnlocked &&
               PlayerLevel >= passive.UnlockLevel;
    }

    public int GetPassiveRank(SummonerPassiveData passive) // 패시브 현재 단계 (0이면 미해금)
    {
        if (passive == null) { return 0; }

        return passiveRanks.TryGetValue(passive, out int rank) ? rank : 0;
    }

    private SummonerSkillData FindDefaultSkill() // 해금된 스킬 중 해금 레벨이 가장 낮은 스킬
    {
        SummonerSkillData defaultSkill = null;

        foreach (SummonerSkillData skill in summonerSkills)
        {
            if (!IsSkillUnlocked(skill)) { continue; }

            if (defaultSkill == null ||
                skill.UnlockLevel < defaultSkill.UnlockLevel)
            {
                defaultSkill = skill;
            }
        }

        return defaultSkill;
    }

    public bool TryEquipSkill( // 액티브 스킬 장착 (기획서 6.4.1: 1개)
        SummonerSkillData skill,
        out string message
    )
    {
        if (skill == null)
        {
            message = "장착할 스킬을 선택하세요.";
            return false;
        }

        if (!IsSkillUnlocked(skill))
        {
            message =
                $"{skill.DisplayName}은 플레이어 Lv.{skill.UnlockLevel}에 해금됩니다. " +
                $"(현재 Lv.{PlayerLevel})";
            return false;
        }

        if (EquippedSkill == skill)
        {
            message = $"{skill.DisplayName}은 이미 장착 중입니다.";
            return false;
        }

        equippedSkill = skill;
        message = $"{skill.DisplayName}을 장착했습니다.";

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림

        return true;
    }

    public bool CanUpgradePassive( // 패시브 해금 또는 강화 가능 여부와 사유
        SummonerPassiveData passive,
        out string reason
    )
    {
        if (passive == null)
        {
            reason = "패시브를 선택하세요.";
            return false;
        }

        if (!IsPassiveSystemUnlocked)
        {
            reason =
                $"패시브는 플레이어 Lv.{SummonerRules.PassiveSystemUnlockLevel}에 해금됩니다.";
            return false;
        }

        if (PlayerLevel < passive.UnlockLevel)
        {
            reason =
                $"{passive.DisplayName}은 플레이어 Lv.{passive.UnlockLevel}에 열립니다. " +
                $"(현재 Lv.{PlayerLevel})";
            return false;
        }

        if (GetPassiveRank(passive) >= passive.MaxRank)
        {
            reason = $"{passive.DisplayName}은 이미 최대 단계입니다.";
            return false;
        }

        if (PassivePoints < passive.PointCostPerRank)
        {
            reason =
                $"패시브 포인트가 부족합니다. " +
                $"({PassivePoints} / {passive.PointCostPerRank})";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public bool TryUpgradePassive( // 패시브 포인트로 해금 또는 한 단계 강화 (기획서 6.5.2: 되돌리기 없음)
        SummonerPassiveData passive,
        out string message
    )
    {
        if (!CanUpgradePassive(passive, out message)) { return false; }

        int rankBefore = GetPassiveRank(passive);

        passiveRanks[passive] = rankBefore + 1;
        spentPassivePoints += passive.PointCostPerRank;

        message = rankBefore <= 0
            ? $"{passive.DisplayName}을 해금했습니다. (포인트 -{passive.PointCostPerRank})"
            : $"{passive.DisplayName} {rankBefore}단계 → {rankBefore + 1}단계 " +
              $"(포인트 -{passive.PointCostPerRank})";

        if (equippedPassive == null)
        {
            equippedPassive = passive; // 처음 해금한 패시브는 바로 장착
            message += " 장착했습니다.";
        }

        Debug.Log(message); // 패시브 성장 기록

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림

        return true;
    }

    public bool TryEquipPassive( // 패시브 장착 (기획서 6.5.1: 1개)
        SummonerPassiveData passive,
        out string message
    )
    {
        if (passive == null)
        {
            message = "장착할 패시브를 선택하세요.";
            return false;
        }

        if (GetPassiveRank(passive) <= 0)
        {
            message = $"{passive.DisplayName}은 아직 해금하지 않았습니다.";
            return false;
        }

        if (equippedPassive == passive)
        {
            message = $"{passive.DisplayName}은 이미 장착 중입니다.";
            return false;
        }

        equippedPassive = passive;
        message = $"{passive.DisplayName}을 장착했습니다.";

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림

        return true;
    }

    // 장착한 패시브가 해당 종류일 때만 현재 단계의 수치를 돌려준다. (아니면 0)
    public int GetEquippedPassiveAmount(SummonerPassiveType passiveType)
    {
        SummonerPassiveData passive = EquippedPassive;

        if (passive == null || passive.PassiveType != passiveType) { return 0; }

        return passive.GetAmount(GetPassiveRank(passive));
    }

    // 레벨이 오른 구간에서 새로 열린 스킬과 패시브, 받은 패시브 포인트를 줄 단위로 모은다.
    public string GetSummonerUnlockSummary(int levelBefore, int levelAfter)
    {
        List<string> lines = new List<string>();

        foreach (SummonerSkillData skill in summonerSkills)
        {
            if (skill == null) { continue; }
            if (skill.UnlockLevel <= levelBefore || skill.UnlockLevel > levelAfter) { continue; }

            lines.Add($"새 액티브 스킬: {skill.DisplayName}");
        }

        foreach (SummonerPassiveData passive in summonerPassives)
        {
            if (passive == null) { continue; }

            int openLevel = Mathf.Max(
                passive.UnlockLevel,
                SummonerRules.PassiveSystemUnlockLevel
            );

            if (openLevel <= levelBefore || openLevel > levelAfter) { continue; }

            lines.Add($"새 패시브: {passive.DisplayName}");
        }

        int gainedPoints =
            SummonerRules.GetTotalPassivePoints(levelAfter) -
            SummonerRules.GetTotalPassivePoints(levelBefore);

        if (gainedPoints > 0)
        {
            lines.Add($"패시브 포인트 +{gainedPoints}");
        }

        return string.Join("\n", lines);
    }
}
