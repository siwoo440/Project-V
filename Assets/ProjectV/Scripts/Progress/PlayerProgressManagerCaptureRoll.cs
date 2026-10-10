using System.Collections.Generic; // 리스트 기능
using System.Text; // 문자열 조립 기능
using UnityEngine; // Unity 기본 기능

public partial class PlayerProgressManager // 포획 목록 뽑기 (기획서 8.13 / 9.13.3 / F.18)
{
    // 목록 3개를 새로 뽑는다. 방금 전과 똑같은 목록이 연속으로 나오지 않게 다시 뽑는다. (기획서 F.18.3)
    private void GenerateCaptureSlots(RegionData region, CaptureBoard board, bool guaranteeLegend)
    {
        List<MonsterData> pool = GetCapturePool(region);
        string previousKey = GetBoardKey(board);

        for (int attempt = 0; attempt < 5; attempt++)
        {
            board.Slots.Clear();

            for (int slot = 0; slot < CaptureRules.SlotCount; slot++)
            {
                board.Slots.Add(RollCaptureSlot(region, pool));
            }

            if (guaranteeLegend && !BoardHasLegend(board))
            {
                ForceLegend(board, pool); // 전설 확정 보정 (기획서 9.13.4)
            }

            if (GetBoardKey(board) != previousKey) { break; }
        }

        if (BoardHasLegend(board))
        {
            board.RerollsWithoutLegend = 0; // 전설이 나오면 그 지역의 횟수를 되돌린다.
        }
    }

    private List<MonsterData> RollCaptureSlot(RegionData region, List<MonsterData> pool) // 목록 하나: 마물 1 ~ 5체
    {
        CaptureRules.GetCountRange(region.Order, out int minimum, out int maximum);

        int count = Random.Range(minimum, maximum + 1);
        bool hasBoss = false;
        List<MonsterData> slot = new List<MonsterData>();

        for (int i = 0; i < count; i++)
        {
            MonsterData monster = RollCaptureMonster(pool, hasBoss);

            if (monster == null) { break; }
            if (monster.IsBossOrigin) { hasBoss = true; } // 한 포획전에 보스 마물은 최대 1체 (기획서 F.18.5)

            slot.Add(monster); // 같은 마물이 여러 체 나올 수 있다.
        }

        return slot;
    }

    // 희귀도를 먼저 정하고 그 희귀도 안에서 마물을 고른다.
    // 지역에 없는 희귀도는 빼고 남은 비율로 뽑는다. 미포획 마물은 같은 희귀도 안에서 1.5배 더 잘 나온다.
    private MonsterData RollCaptureMonster(List<MonsterData> pool, bool excludeBoss)
    {
        CardRarity[] rarities =
        {
            CardRarity.Common, CardRarity.Rare, CardRarity.Special, CardRarity.Legendary
        };

        int totalRarityWeight = 0;

        foreach (CardRarity rarity in rarities)
        {
            if (HasCaptureCandidate(pool, rarity, excludeBoss))
            {
                totalRarityWeight += CaptureRules.GetRarityWeight(rarity);
            }
        }

        if (totalRarityWeight <= 0) { return null; }

        int rarityRoll = Random.Range(0, totalRarityWeight);
        CardRarity pickedRarity = CardRarity.Common;

        foreach (CardRarity rarity in rarities)
        {
            if (!HasCaptureCandidate(pool, rarity, excludeBoss)) { continue; }

            pickedRarity = rarity;
            rarityRoll -= CaptureRules.GetRarityWeight(rarity);

            if (rarityRoll < 0) { break; }
        }

        int totalWeight = 0;

        foreach (MonsterData monster in pool)
        {
            totalWeight += GetCaptureWeight(monster, pickedRarity, excludeBoss);
        }

        if (totalWeight <= 0) { return null; }

        int roll = Random.Range(0, totalWeight);

        foreach (MonsterData monster in pool)
        {
            roll -= GetCaptureWeight(monster, pickedRarity, excludeBoss);

            if (roll < 0) { return monster; }
        }

        return null;
    }

    private int GetCaptureWeight(MonsterData monster, CardRarity rarity, bool excludeBoss) // 마물 하나의 출현 가중치
    {
        if (monster == null || monster.Rarity != rarity) { return 0; }
        if (excludeBoss && monster.IsBossOrigin) { return 0; }

        return IsMonsterCaptured(monster) ? 100 : CaptureRules.UncapturedWeightPercent;
    }

    private static bool HasCaptureCandidate(List<MonsterData> pool, CardRarity rarity, bool excludeBoss)
    {
        foreach (MonsterData monster in pool)
        {
            if (monster == null || monster.Rarity != rarity) { continue; }
            if (excludeBoss && monster.IsBossOrigin) { continue; }

            return true;
        }

        return false;
    }

    private static bool PoolHasLegend(List<MonsterData> pool)
    {
        return HasCaptureCandidate(pool, CardRarity.Legendary, false);
    }

    private static bool BoardHasLegend(CaptureBoard board)
    {
        foreach (List<MonsterData> slot in board.Slots)
        {
            foreach (MonsterData monster in slot)
            {
                if (monster != null && monster.Rarity == CardRarity.Legendary) { return true; }
            }
        }

        return false;
    }

    private static void ForceLegend(CaptureBoard board, List<MonsterData> pool) // 첫 목록의 첫 마물을 전설 마물로 바꾼다.
    {
        List<MonsterData> legends = new List<MonsterData>();

        foreach (MonsterData monster in pool)
        {
            if (monster != null && monster.Rarity == CardRarity.Legendary) { legends.Add(monster); }
        }

        if (legends.Count == 0 || board.Slots.Count == 0) { return; }

        MonsterData legend = legends[Random.Range(0, legends.Count)];

        if (board.Slots[0].Count == 0)
        {
            board.Slots[0].Add(legend);
        }
        else
        {
            board.Slots[0][0] = legend;
        }
    }

    private static string GetBoardKey(CaptureBoard board) // 목록 전체를 한 줄로 적은 값 (같은 목록인지 비교용)
    {
        StringBuilder builder = new StringBuilder();

        foreach (List<MonsterData> slot in board.Slots)
        {
            foreach (MonsterData monster in slot)
            {
                builder.Append(monster == null ? "?" : monster.MonsterId).Append(',');
            }

            builder.Append('|');
        }

        return builder.ToString();
    }
}
