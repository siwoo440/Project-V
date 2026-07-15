using System;
using UnityEngine;

[Serializable]
public class OwnedMonsterData
{
    [SerializeField] private MonsterData monsterData;
    [SerializeField] private int copyCount = 1;
    [SerializeField] private int level = 1;
    [SerializeField] private int currentExperience;

    public MonsterData MonsterData => monsterData;
    public int CopyCount => copyCount;
    public int Level => level;
    public int CurrentExperience => currentExperience;

    public int RequiredExperience =>
        Mathf.Max(10, level * 10);

    public int MaxHp =>
        monsterData == null
            ? 0
            : monsterData.MaxHp +
              (level - 1) * monsterData.HpGrowthPerLevel;

    public int Attack =>
        monsterData == null
            ? 0
            : monsterData.Attack +
              (level - 1) * monsterData.AttackGrowthPerLevel;

    public int LustDamage =>
        monsterData == null
            ? 0
            : monsterData.LustDamage +
              (level - 1) * monsterData.LustGrowthPerLevel;

    public int Defense =>
        monsterData == null
            ? 0
            : monsterData.Defense +
              (level - 1) * monsterData.DefenseGrowthPerLevel;

    public OwnedMonsterData(MonsterData data)
    {
        monsterData = data;
        copyCount = 1;
        level = 1;
        currentExperience = 0;
    }

    public void IncreaseCopyCount()
    {
        copyCount += 1;
    }

    public int AddExperience(int amount)
    {
        if (amount <= 0) { return 0; }

        currentExperience += amount;

        int gainedLevels = 0;

        while (currentExperience >= RequiredExperience)
        {
            currentExperience -= RequiredExperience;
            level += 1;
            gainedLevels += 1;
        }

        return gainedLevels;
    }
}