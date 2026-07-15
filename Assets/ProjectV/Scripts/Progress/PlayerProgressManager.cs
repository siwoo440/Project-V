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


    [Header("Duplicate Capture")] // 중복 포획 설정
    [SerializeField, Min(0)]
    private int duplicateMaterialReward = 5; // 중복 포획 강화 재료

    private readonly List<OwnedMonsterData> ownedMonsters = new List<OwnedMonsterData>();

    private int gold; // 현재 골드
    private int totalExperience; // 전체 경험치
    private int monsterEnhancementMaterial; // 마물 강화 재료

    public event Action ProgressChanged;

    public int Gold => gold; // 현재 골드
    public int TotalExperience => totalExperience; // 전체 경험치
    public int MonsterEnhancementMaterial => monsterEnhancementMaterial; // 강화 재료



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
        if (resultData == null) { return false; } // 결과 누락 차단
        if (resultData.RewardsApplied) { return false; } // 중복 수령 차단

        int duplicateMaterialGained = 0; // 이번 전투 획득 재료

        if (resultData.IsVictory)
        {
            gold += Mathf.Max(0, resultData.GoldReward); // 골드 지급
            totalExperience += Mathf.Max(0, resultData.ExperienceReward); // 전체 경험치 지급

            if (resultData.CaptureSucceeded && resultData.CapturedMonster != null)
            {
                duplicateMaterialGained = AddMonsterInternal(resultData.CapturedMonster, true ); // 포획 또는 중복 변환
            }

            foreach (OwnedMonsterData ownedMonster in ownedMonsters)
            {
                if (ownedMonster == null) { continue; } // 빈 데이터 제외

                ownedMonster.AddExperience(
                    resultData.ExperienceReward
                ); // 보유 마물 경험치 지급
            }
        }

        resultData.MarkRewardsApplied(
            duplicateMaterialGained
        ); // 보상 결과 저장

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림

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
        gold = Mathf.Max(0, startingGold); // 시작 골드
        totalExperience = 0; // 전체 경험치 초기화
        monsterEnhancementMaterial = 0; // 강화 재료 초기화
        ownedMonsters.Clear(); // 보유 마물 초기화

        foreach (MonsterData monsterData in startingMonsters)
        {
            if (monsterData == null) { continue; }

            AddMonsterInternal(monsterData, false);
        }
    }

    private int AddMonsterInternal( MonsterData monsterData,bool convertDuplicateToMaterial)
    {
        if (monsterData == null) { return 0; } // 빈 마물 차단

        OwnedMonsterData existingMonster = GetOwnedMonster(monsterData); // 기존 보유 마물 검색

        if (existingMonster == null)
        {
            ownedMonsters.Add( new OwnedMonsterData(monsterData) ); // 새로운 마물 등록
            Debug.Log($"New monster added: {monsterData.MonsterName}" ); // 신규 포획 확인
            return 0;
        }

        if (!convertDuplicateToMaterial) { return 0; } // 시작 데이터 중복 무시

        int materialReward = Mathf.Max(0, duplicateMaterialReward); // 안전한 재료 수량

        monsterEnhancementMaterial += materialReward; // 강화 재료 지급

        Debug.Log(
            $"Duplicate monster converted: " +
            $"{monsterData.MonsterName}, " +
            $"Material +{materialReward}"
        ); // 중복 변환 확인

        return materialReward;
    }
}