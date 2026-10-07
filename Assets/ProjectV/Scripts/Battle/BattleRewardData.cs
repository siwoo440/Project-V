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

    [Header("그리모어 재화")]
    [SerializeField, Min(0)]
    private int desireShards; // 욕망의 파편. 히로인전과 지역 클리어에서만 준다. (기획서 9.9.1)

    [Header("포획 보상")]
    [SerializeField, Range(0f, 1f)]
    private float captureChance = 0.3f;

    [SerializeField]
    private List<MonsterData> captureCandidates =
        new List<MonsterData>();

    public float CaptureChance => captureChance;

    public int DesireShards => Mathf.Max(0, desireShards); // 욕망의 파편 보상 반환

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

    public bool RollCapture(float bonusChance = 0f) // bonusChance: 그리모어 강화로 더해지는 확률
    {
        if (captureChance <= 0f) { return false; }

        return Random.value <= captureChance + Mathf.Max(0f, bonusChance);
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