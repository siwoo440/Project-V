using System;

[Serializable]
public class BattleResultData
{
    public BattleOutcome Outcome { get; }
    public int GoldReward { get; }
    public int ExperienceReward { get; }
    public bool CaptureAttempted { get; }
    public bool CaptureSucceeded { get; }
    public MonsterData CapturedMonster { get; }
    public int DuplicateMaterialReward { get; private set; } // 중복 포획 강화 재료


    public bool IsVictory =>
    Outcome == BattleOutcome.VictoryHp ||
    Outcome == BattleOutcome.VictoryLust;

    public bool RewardsApplied { get; private set; }
    public void MarkRewardsApplied(int duplicateMaterialReward)
    {
        DuplicateMaterialReward = Math.Max(0, duplicateMaterialReward); // 획득 재료 저장
        RewardsApplied = true; // 보상 적용 완료
    }
    public BattleResultData(
        BattleOutcome outcome,
        int goldReward,
        int experienceReward,
        bool captureAttempted,
        bool captureSucceeded,
        MonsterData capturedMonster
    )
    {
        Outcome = outcome;
        GoldReward = goldReward;
        ExperienceReward = experienceReward;
        CaptureAttempted = captureAttempted;
        CaptureSucceeded = captureSucceeded;
        CapturedMonster = capturedMonster;
    }
}