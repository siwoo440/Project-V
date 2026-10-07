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
    public int EssenceReward { get; private set; } // 초과 포획 변환 정수
    public int DesireShardReward { get; }  // 욕망의 파편 보상 (기획서 9.9)
    public int BonusEssenceReward { get; } // 그리모어 강화로 얻는 정수
    public int DesireShardsGained { get; private set; } // 실제로 반영된 욕망의 파편

    public int ShownDesireShards =>
        RewardsApplied ? DesireShardsGained : DesireShardReward; // 화면에 보여줄 파편 수

    public int ExperienceGained { get; private set; }  // 실제로 반영된 경험치
    public int PlayerLevelBefore { get; private set; } // 보상 반영 전 플레이어 레벨
    public int PlayerLevelAfter { get; private set; }  // 보상 반영 후 플레이어 레벨

    public bool DuplicateConverted =>
        EssenceReward > 0; // 초과 변환 여부

    public bool LeveledUp =>
        RewardsApplied &&
        PlayerLevelAfter > PlayerLevelBefore; // 레벨 상승 여부

    public bool IsVictory =>
    Outcome == BattleOutcome.VictoryHp ||
    Outcome == BattleOutcome.VictoryLust;

    public bool RewardsApplied { get; private set; }
    public void MarkRewardsApplied(int essenceReward, int shardsGained = 0)
    {
        EssenceReward = Math.Max(0, essenceReward); // 획득 정수 저장
        DesireShardsGained = Math.Max(0, shardsGained); // 반영된 파편 저장
        RewardsApplied = true; // 보상 적용 완료
    }

    public void SetLevelResult( // 경험치 반영 결과 저장
        int experienceGained,
        int levelBefore,
        int levelAfter
    )
    {
        ExperienceGained = Math.Max(0, experienceGained);
        PlayerLevelBefore = levelBefore;
        PlayerLevelAfter = levelAfter;
    }

    public BattleResultData(
        BattleOutcome outcome,
        int goldReward,
        int experienceReward,
        bool captureAttempted,
        bool captureSucceeded,
        MonsterData capturedMonster,
        int desireShardReward = 0,
        int bonusEssenceReward = 0
    )
    {
        DesireShardReward = Math.Max(0, desireShardReward);
        BonusEssenceReward = Math.Max(0, bonusEssenceReward);
        Outcome = outcome;
        GoldReward = goldReward;
        ExperienceReward = experienceReward;
        CaptureAttempted = captureAttempted;
        CaptureSucceeded = captureSucceeded;
        CapturedMonster = capturedMonster;
    }
}
