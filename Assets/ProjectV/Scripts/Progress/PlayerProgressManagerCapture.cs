using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 포획 콘텐츠 (기획서 8.12 / 9.13 / 9.14)
// 지역마다 포획 목록 3개를 가진다. 목록은 리롤하거나 포획전이 끝나면 새로 뽑힌다.
public partial class PlayerProgressManager
{
    private class CaptureBoard // 한 지역의 포획 목록
    {
        public readonly List<List<MonsterData>> Slots =
            new List<List<MonsterData>>(); // 목록 3개, 목록마다 등장 마물

        public int RerollsWithoutLegend; // 전설 없이 유료 리롤을 한 횟수 (기획서 9.13.4)
    }

    private readonly Dictionary<RegionData, CaptureBoard> captureBoards =
        new Dictionary<RegionData, CaptureBoard>(); // 지역별 포획 목록

    private void InitializeCaptureProgress()
    {
        captureBoards.Clear();
    }

    private static List<MonsterData> GetCapturePool(RegionData region) // 이 지역에서 포획할 수 있는 마물
    {
        List<MonsterData> pool = new List<MonsterData>();

        if (region == null) { return pool; }

        foreach (MonsterData monster in region.CaptureMonsters)
        {
            if (monster == null || monster.IsToken) { continue; } // 토큰 마물은 포획 대상이 아니다.
            if (monster.CaptureRewardCard == null) { continue; }  // 얻을 카드가 없는 마물 제외
            if (pool.Contains(monster)) { continue; }

            pool.Add(monster);
        }

        return pool;
    }

    public bool HasCaptureContent(RegionData region) // 포획 목록을 만들 수 있는 지역인지 여부
    {
        return GetCapturePool(region).Count > 0;
    }

    // 임시 규칙: 일반전 2단계를 이기면 열린다. 정식 조건은 주요 히로인 2차전 승리다. (RegionRules 참고)
    public bool IsCaptureUnlocked(RegionData region)
    {
        return region != null &&
               IsStageCleared(
                   RegionRules.GetNormalStageId(region.RegionId, RegionRules.CaptureUnlockNormalStage)
               );
    }

    public bool IsMonsterCaptured(MonsterData monster) // 카드를 한 장이라도 가지고 있는지 여부
    {
        return monster != null && GetOwnedCardCount(monster.CaptureRewardCard) > 0;
    }

    public IReadOnlyList<MonsterData> GetCaptureSlot(RegionData region, int slotIndex) // 포획 목록 하나
    {
        CaptureBoard board = EnsureCaptureBoard(region);

        return board == null || slotIndex < 0 || slotIndex >= board.Slots.Count
            ? new List<MonsterData>()
            : board.Slots[slotIndex];
    }

    public int GetRerollsUntilLegend(RegionData region) // 전설 확정까지 남은 유료 리롤 수 (전설이 없는 지역은 -1)
    {
        if (!PoolHasLegend(GetCapturePool(region))) { return -1; }

        captureBoards.TryGetValue(region, out CaptureBoard board);

        int used = board == null ? 0 : board.RerollsWithoutLegend;

        return Mathf.Max(0, CaptureRules.LegendPityRerolls - used);
    }

    // 지역의 포획 목록을 돌려준다. 처음 볼 때 만들고 바로 저장한다. (기획서 9.17: 포획 목록 최초 생성)
    private CaptureBoard EnsureCaptureBoard(RegionData region)
    {
        if (region == null || !HasCaptureContent(region)) { return null; }

        if (!captureBoards.TryGetValue(region, out CaptureBoard board))
        {
            board = new CaptureBoard();
            captureBoards[region] = board;
        }

        if (board.Slots.Count != CaptureRules.SlotCount)
        {
            GenerateCaptureSlots(region, board, false);
            MarkUnsavedChanges();
            AutoSave("포획 목록 생성");
        }

        return board;
    }

    public bool CanRerollCapture(RegionData region, out string reason) // 리롤 가능 여부와 사유
    {
        if (!HasCaptureContent(region))
        {
            reason = "이 지역에는 포획할 수 있는 마물이 없습니다.";
            return false;
        }

        if (!IsCaptureUnlocked(region))
        {
            reason = "포획 콘텐츠가 아직 열리지 않았습니다.";
            return false;
        }

        if (gold < CaptureRules.RerollCost)
        {
            reason = $"골드가 부족합니다. ({gold} / {CaptureRules.RerollCost})";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    // 골드를 내고 목록 3개를 모두 다시 뽑는다. 되돌릴 수 없고 바로 저장한다. (기획서 9.14)
    public bool TryRerollCapture(RegionData region, out string message)
    {
        if (!CanRerollCapture(region, out message)) { return false; }

        CaptureBoard board = EnsureCaptureBoard(region);

        bool guaranteeLegend =
            PoolHasLegend(GetCapturePool(region)) &&
            board.RerollsWithoutLegend >= CaptureRules.LegendPityRerolls; // 20회 동안 없었으면 이번에 확정

        gold -= CaptureRules.RerollCost;

        GenerateCaptureSlots(region, board, guaranteeLegend);

        if (!BoardHasLegend(board))
        {
            board.RerollsWithoutLegend += 1; // 전설이 나오면 GenerateCaptureSlots가 0으로 되돌린다.
        }

        message = $"포획 목록을 다시 뽑았습니다. (골드 -{CaptureRules.RerollCost})";

        Debug.Log($"포획 목록 리롤: {region.DisplayName} (전설 없는 리롤 {board.RerollsWithoutLegend}회)"); // 리롤 기록

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림
        AutoSave("포획 목록 리롤"); // 기획서 9.17

        return true;
    }

    // 포획전이 끝나면 이기든 지든 목록 3개를 새로 뽑는다. 리롤 횟수에는 넣지 않는다. (기획서 9.14)
    private void RefreshCaptureBoard(RegionData region)
    {
        if (region == null || !captureBoards.TryGetValue(region, out CaptureBoard board)) { return; }

        GenerateCaptureSlots(region, board, false);
    }
}
