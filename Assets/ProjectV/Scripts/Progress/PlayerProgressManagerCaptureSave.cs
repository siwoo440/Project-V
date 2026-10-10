using System.Collections.Generic; // 리스트 기능

public partial class PlayerProgressManager // 포획 목록의 저장과 복원 (기획서 9.17)
{
    private void SaveCaptureBoards(SaveData data)
    {
        foreach (KeyValuePair<RegionData, CaptureBoard> pair in captureBoards)
        {
            if (pair.Key == null || pair.Value.Slots.Count != CaptureRules.SlotCount) { continue; }

            SaveCaptureEntry captureEntry = new SaveCaptureEntry
            {
                regionId = pair.Key.RegionId,
                rerollsWithoutLegend = pair.Value.RerollsWithoutLegend
            };

            WriteCaptureSlot(pair.Value.Slots[0], captureEntry.slot1);
            WriteCaptureSlot(pair.Value.Slots[1], captureEntry.slot2);
            WriteCaptureSlot(pair.Value.Slots[2], captureEntry.slot3);

            data.captures.Add(captureEntry);
        }
    }

    private static void WriteCaptureSlot(List<MonsterData> slot, List<string> monsterIds)
    {
        foreach (MonsterData monster in slot)
        {
            if (monster != null) { monsterIds.Add(monster.MonsterId); }
        }
    }

    private void RestoreCaptureBoards(SaveData data)
    {
        InitializeCaptureProgress();

        foreach (SaveCaptureEntry captureEntry in data.captures)
        {
            RegionData region = FindRegionById(captureEntry.regionId);

            if (region == null) { continue; }

            List<MonsterData> pool = GetCapturePool(region);

            CaptureBoard board = new CaptureBoard
            {
                RerollsWithoutLegend = captureEntry.rerollsWithoutLegend < 0 ? 0 : captureEntry.rerollsWithoutLegend
            };

            board.Slots.Add(ReadCaptureSlot(captureEntry.slot1, pool));
            board.Slots.Add(ReadCaptureSlot(captureEntry.slot2, pool));
            board.Slots.Add(ReadCaptureSlot(captureEntry.slot3, pool));

            foreach (List<MonsterData> slot in board.Slots)
            {
                if (slot.Count > 0) { continue; }

                board.Slots.Clear(); // 지금 게임에 없는 마물뿐인 목록이 있으면 다음에 볼 때 새로 뽑는다.
                break;
            }

            captureBoards[region] = board;
        }
    }

    private static List<MonsterData> ReadCaptureSlot(List<string> monsterIds, List<MonsterData> pool)
    {
        List<MonsterData> slot = new List<MonsterData>();

        foreach (string monsterId in monsterIds)
        {
            foreach (MonsterData monster in pool)
            {
                if (monster.MonsterId != monsterId) { continue; }

                slot.Add(monster);
                break;
            }
        }

        return slot;
    }
}
