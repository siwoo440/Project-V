using System;
using System.Collections.Generic;

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

    public int LostGold { get; private set; }    // 보유 한도 때문에 받지 못한 골드 (기획서 A.47)
    public int LostEssence { get; private set; } // 보유 한도 때문에 받지 못한 정수
    public int LostShards { get; private set; }  // 보유 한도 때문에 받지 못한 파편

    public string OutcomeLabel { get; private set; } = string.Empty; // 결과 화면에 적을 승패 문구 (비어 있으면 기본 문구)

    public void SetOutcomeLabel(string label) // 전투 종류에 맞는 승패 문구 지정 (일반전의 적 전멸 등)
    {
        OutcomeLabel = label ?? string.Empty;
    }

    private readonly List<MonsterData> capturedMonsters = new List<MonsterData>(); // 포획전에서 쓰러뜨린 마물

    public bool IsCaptureBattle { get; private set; } // 포획전인지 여부 (끝나면 포획 목록이 새로 뽑힌다)
    public IReadOnlyList<MonsterData> CapturedMonsters => capturedMonsters; // 포획한 마물 (승리했을 때만 채워진다)

    // 포획전 결과 기록. 승리하면 쓰러뜨린 마물을 모두 얻고 패배하면 얻지 못한다. (기획서 9.13.1)
    public void SetCaptureResult(IReadOnlyList<MonsterData> monsters)
    {
        IsCaptureBattle = true;
        capturedMonsters.Clear();

        if (monsters == null) { return; }

        foreach (MonsterData monster in monsters)
        {
            if (monster != null) { capturedMonsters.Add(monster); }
        }
    }

    public string ClearedRegionName { get; private set; } = string.Empty;  // 이 승리로 처음 클리어하는 지역 (없으면 빈 문자열)
    public string UnlockedRegionName { get; private set; } = string.Empty; // 그 클리어로 열리는 다음 지역
    public int RegionClearGold { get; private set; }   // 지역 클리어 보상 골드 (기획서 C.28)
    public int RegionClearShards { get; private set; } // 지역 클리어 보상 욕망의 파편

    public bool HasRegionClear => !string.IsNullOrEmpty(ClearedRegionName); // 지역을 처음 클리어하는 승리인지 여부

    // 지역 최초 클리어 기록. 보상은 난이도와 관계없이 한 번만 준다.
    public void SetRegionClear(string regionName, string nextRegionName, int gold, int shards)
    {
        ClearedRegionName = regionName ?? string.Empty;
        UnlockedRegionName = nextRegionName ?? string.Empty;
        RegionClearGold = Math.Max(0, gold);
        RegionClearShards = Math.Max(0, shards);
    }

    public string StageId { get; private set; } = string.Empty; // 승리 기록에 쓰는 스테이지 ID (일반전, 포획전, 히로인전)
    public BattleDifficulty Difficulty { get; private set; } = BattleDifficulty.Normal; // 전투 난이도
    public bool IsFirstClear { get; private set; } // 이 스테이지의 최초 승리인지 여부
    public bool ClearsRegion { get; private set; } // 이 승리로 지역을 클리어하는지 여부

    public bool HasStageInfo => !string.IsNullOrEmpty(StageId); // 스테이지 기록이 있는 전투인지 여부

    public void SetStageInfo( // 스테이지와 난이도, 최초 승리 여부 기록
        string stageId,
        BattleDifficulty difficulty,
        bool isFirstClear,
        bool clearsRegion
    )
    {
        StageId = stageId ?? string.Empty;
        Difficulty = difficulty;
        IsFirstClear = isFirstClear;
        ClearsRegion = clearsRegion;
    }

    public void SetLimitLoss(int lostGold, int lostEssence, int lostShards)
    {
        LostGold = Math.Max(0, lostGold);
        LostEssence = Math.Max(0, lostEssence);
        LostShards = Math.Max(0, lostShards);
    }

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
