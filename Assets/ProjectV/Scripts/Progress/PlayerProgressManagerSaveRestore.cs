using UnityEngine; // Unity 기본 기능

public partial class PlayerProgressManager // 저장 데이터에서 소환사, 그리모어, 소모성 아이템 되돌리기
{
    private int RestoreSummoner(SaveData data) // 소환사 스킬과 패시브
    {
        int skippedCount = 0;

        passiveRanks.Clear();
        spentPassivePoints = 0;

        foreach (SaveCountEntry rankEntry in data.passiveRanks)
        {
            SummonerPassiveData passive = FindPassiveById(rankEntry.id);

            if (passive == null)
            {
                skippedCount += 1;
                continue;
            }

            int rank = Mathf.Clamp(rankEntry.value, 0, passive.MaxRank);

            if (rank <= 0) { continue; }

            passiveRanks[passive] = rank;
            spentPassivePoints += rank * passive.PointCostPerRank; // 사용한 포인트는 단계에서 다시 계산한다.
        }

        equippedPassive = FindPassiveById(data.equippedPassiveId);

        if (GetPassiveRank(equippedPassive) <= 0) { equippedPassive = null; }

        equippedSkill = FindSkillById(data.equippedSkillId); // 없거나 잠겨 있으면 처음 조회할 때 기본 스킬로 바뀐다.

        return skippedCount;
    }

    private int RestoreGrimoire(SaveData data) // 그리모어 노드 단계
    {
        int skippedCount = 0;

        grimoireLevels.Clear();

        foreach (SaveCountEntry levelEntry in data.grimoireLevels)
        {
            GrimoireNodeData node = FindGrimoireNodeById(levelEntry.id);

            if (node == null)
            {
                skippedCount += 1;
                continue;
            }

            int level = Mathf.Clamp(levelEntry.value, 0, GrimoireRules.MaxLevel);

            if (level > 0) { grimoireLevels[node] = level; }
        }

        return skippedCount;
    }

    private int RestoreBattleItems(SaveData data) // 소모성 아이템 수량과 장착
    {
        int skippedCount = 0;

        battleItemCounts.Clear();

        foreach (SaveCountEntry countEntry in data.battleItems)
        {
            BattleItemData item = FindBattleItemById(countEntry.id);

            if (item == null)
            {
                skippedCount += 1;
                continue;
            }

            int count = Mathf.Clamp(countEntry.value, 0, ShopRules.MaxBattleItemCount);

            if (count > 0) { battleItemCounts[item] = count; }
        }

        equippedBattleItem = FindBattleItemById(data.equippedBattleItemId);

        return skippedCount;
    }

    private int RestoreRegions(SaveData data) // 지역별 진행과 마지막으로 들어간 지역
    {
        int skippedCount = 0;

        InitializeRegionProgress();

        foreach (SaveRegionEntry regionEntry in data.regions)
        {
            RegionData region = FindRegionById(regionEntry.id);

            if (region == null)
            {
                skippedCount += 1;
                continue;
            }

            if (regionEntry.visited || regionEntry.cleared) { visitedRegions.Add(region); }
            if (regionEntry.cleared) { clearedRegions.Add(region); }
        }

        currentRegion = FindRegionById(data.currentRegionId);

        if (!IsRegionUnlocked(currentRegion)) { currentRegion = null; } // 지금 규칙으로 들어갈 수 없는 지역이면 비운다.

        return skippedCount;
    }
}
