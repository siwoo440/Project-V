using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewBattleRewardData",
    menuName = "Project V/전투 보상 데이터"
)]
public class BattleRewardData : ScriptableObject
{
    [Header("재화 보상")]
    [SerializeField, Min(0)] private int minimumGold = 10;
    [SerializeField, Min(0)] private int maximumGold = 20;

    [Header("경험치 보상")]
    [SerializeField, Min(0)] private int minimumExperience = 5;
    [SerializeField, Min(0)] private int maximumExperience = 10;

    [Header("포획 보상")]
    [SerializeField, Range(0f, 1f)]
    private float captureChance = 0.3f;

    [SerializeField]
    private List<MonsterData> captureCandidates =
        new List<MonsterData>();

    public float CaptureChance => captureChance;

    public bool HasCaptureCandidate
    {
        get
        {
            foreach (MonsterData monsterData in captureCandidates)
            {
                if (monsterData != null) { return true; }
            }

            return false;
        }
    }

    public int RollGold()
    {
        return Random.Range(
            minimumGold,
            maximumGold + 1
        );
    }

    public int RollExperience()
    {
        return Random.Range(
            minimumExperience,
            maximumExperience + 1
        );
    }

    public bool RollCapture()
    {
        if (captureChance <= 0f) { return false; }

        return Random.value <= captureChance;
    }

    public MonsterData GetRandomCaptureCandidate()
    {
        List<MonsterData> validCandidates =
            new List<MonsterData>();

        foreach (MonsterData monsterData in captureCandidates)
        {
            if (monsterData != null)
            {
                validCandidates.Add(monsterData);
            }
        }

        if (validCandidates.Count == 0) { return null; }

        int randomIndex = Random.Range(
            0,
            validCandidates.Count
        );

        return validCandidates[randomIndex];
    }

    private void OnValidate()
    {
        maximumGold = Mathf.Max(
            minimumGold,
            maximumGold
        );

        maximumExperience = Mathf.Max(
            minimumExperience,
            maximumExperience
        );
    }
}