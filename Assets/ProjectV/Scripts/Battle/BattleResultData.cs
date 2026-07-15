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

    public bool IsVictory =>
    Outcome == BattleOutcome.VictoryHp ||
    Outcome == BattleOutcome.VictoryLust;

    public bool RewardsApplied { get; private set; }
    public void MarkRewardsApplied()
    {
        RewardsApplied = true;
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