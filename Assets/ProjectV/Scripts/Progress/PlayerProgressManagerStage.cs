using System.Collections.Generic; // 리스트 기능

// 스테이지 승리 기록 (기획서 5.21.3 / 8.10 / F.6)
// 승리 여부와 최초 승리 여부만 기록한다. 난이도별 첫 승리를 따로 남긴다.
public partial class PlayerProgressManager
{
    private readonly Dictionary<string, int> stageClearMasks =
        new Dictionary<string, int>(); // 스테이지 ID별로 승리한 난이도 (쉬움 1, 보통 2, 어려움 4를 더한 값)

    private void InitializeStageProgress()
    {
        stageClearMasks.Clear();
    }

    private static int GetDifficultyBit(BattleDifficulty difficulty)
    {
        return 1 << (int)difficulty;
    }

    public bool IsStageCleared(string stageId) // 어느 난이도로든 승리한 적이 있는지 여부
    {
        return !string.IsNullOrEmpty(stageId) &&
               stageClearMasks.TryGetValue(stageId, out int mask) &&
               mask != 0;
    }

    public bool IsStageCleared(string stageId, BattleDifficulty difficulty) // 그 난이도로 승리한 적이 있는지 여부
    {
        return !string.IsNullOrEmpty(stageId) &&
               stageClearMasks.TryGetValue(stageId, out int mask) &&
               (mask & GetDifficultyBit(difficulty)) != 0;
    }

    private void RecordStageClear(string stageId, BattleDifficulty difficulty) // 승리 기록
    {
        if (string.IsNullOrEmpty(stageId)) { return; }

        stageClearMasks.TryGetValue(stageId, out int mask);
        stageClearMasks[stageId] = mask | GetDifficultyBit(difficulty);
    }
}
