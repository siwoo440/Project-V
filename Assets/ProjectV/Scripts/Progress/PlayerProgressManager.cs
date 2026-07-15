using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance { get; private set; }

    [Header("Starting Progress")]
    [SerializeField, Min(0)] private int startingGold;
    [SerializeField]
    private List<MonsterData> startingMonsters =
        new List<MonsterData>();

    [Header("Duplicate Capture")]
    [SerializeField, Min(0)]
    private int duplicateExperienceBonus = 5;

    private readonly List<OwnedMonsterData> ownedMonsters =
        new List<OwnedMonsterData>();

    private int gold;
    private int totalExperience;

    public event Action ProgressChanged;

    public int Gold => gold;
    public int TotalExperience => totalExperience;

    public IReadOnlyList<OwnedMonsterData> OwnedMonsters =>
        ownedMonsters;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeStartingProgress();
    }

    public bool ApplyBattleResult(BattleResultData resultData)
    {
        if (resultData == null) { return false; }
        if (resultData.RewardsApplied) { return false; }

        if (resultData.IsVictory)
        {
            gold += Mathf.Max(0, resultData.GoldReward);
            totalExperience +=
                Mathf.Max(0, resultData.ExperienceReward);

            if (resultData.CaptureSucceeded &&
                resultData.CapturedMonster != null)
            {
                AddMonsterInternal(
                    resultData.CapturedMonster,
                    true
                );
            }

            foreach (OwnedMonsterData ownedMonster in ownedMonsters)
            {
                if (ownedMonster == null) { continue; }

                ownedMonster.AddExperience(
                    resultData.ExperienceReward
                );
            }
        }

        resultData.MarkRewardsApplied();
        ProgressChanged?.Invoke();

        return true;
    }

    public OwnedMonsterData GetOwnedMonster(
        MonsterData monsterData
    )
    {
        if (monsterData == null) { return null; }

        foreach (OwnedMonsterData ownedMonster in ownedMonsters)
        {
            if (ownedMonster == null ||
                ownedMonster.MonsterData == null)
            {
                continue;
            }

            if (ownedMonster.MonsterData == monsterData)
            {
                return ownedMonster;
            }

            if (ownedMonster.MonsterData.MonsterId ==
                monsterData.MonsterId)
            {
                return ownedMonster;
            }
        }

        return null;
    }

    private void InitializeStartingProgress()
    {
        gold = Mathf.Max(0, startingGold);
        totalExperience = 0;
        ownedMonsters.Clear();

        foreach (MonsterData monsterData in startingMonsters)
        {
            if (monsterData == null) { continue; }

            AddMonsterInternal(monsterData, false);
        }
    }

    private void AddMonsterInternal(
        MonsterData monsterData,
        bool grantDuplicateExperience
    )
    {
        if (monsterData == null) { return; }

        OwnedMonsterData existingMonster =
            GetOwnedMonster(monsterData);

        if (existingMonster == null)
        {
            ownedMonsters.Add(
                new OwnedMonsterData(monsterData)
            );

            return;
        }

        existingMonster.IncreaseCopyCount();

        if (grantDuplicateExperience)
        {
            existingMonster.AddExperience(
                duplicateExperienceBonus
            );
        }
    }
}