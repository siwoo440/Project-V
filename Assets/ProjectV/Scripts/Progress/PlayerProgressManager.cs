using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressManager : MonoBehaviour, ICardOwnershipSource
{
    [Serializable]
    private class StartingCardEntry // 시작 보유 카드 항목
    {
        [SerializeField] private CardData cardData; // 시작 카드
        [SerializeField, Min(1)] private int count = 1; // 시작 보유 수량

        public CardData CardData => cardData; // 시작 카드 반환
        public int Count => Mathf.Max(1, count); // 시작 수량 반환
    }

    public static PlayerProgressManager Instance { get; private set; }

    [Header("Starting Progress")]
    [SerializeField, Min(0)] private int startingGold;

    [SerializeField]
    private List<StartingCardEntry> startingCards =
        new List<StartingCardEntry>(); // 시작 보유 카드 목록

    private readonly List<OwnedCardData> ownedCards =
        new List<OwnedCardData>(); // 보유 카드 목록

    private readonly List<OwnedMonsterData> ownedMonsters =
        new List<OwnedMonsterData>(); // 보유 마물 성장 목록

    private int gold; // 현재 골드
    private int totalExperience; // 전체 경험치
    private int monsterEssence; // 마물의 정수

    public event Action ProgressChanged;

    public int Gold => gold; // 현재 골드
    public int TotalExperience => totalExperience; // 전체 경험치
    public int MonsterEssence => monsterEssence; // 마물의 정수

    public IReadOnlyList<OwnedCardData> OwnedCards =>
        ownedCards; // 보유 카드 목록 반환

    public IReadOnlyList<OwnedMonsterData> OwnedMonsters =>
        ownedMonsters; // 보유 마물 목록 반환

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

        int essenceGained = 0; // 이번 전투 획득 정수

        if (resultData.IsVictory)
        {
            gold += Mathf.Max(0, resultData.GoldReward); // 골드 지급
            totalExperience += Mathf.Max(0, resultData.ExperienceReward); // 전체 경험치 지급

            if (resultData.CaptureSucceeded &&
                resultData.CapturedMonster != null)
            {
                essenceGained = AddCapturedMonster(
                    resultData.CapturedMonster
                ); // 포획 카드 지급 또는 초과 변환
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
            essenceGained
        ); // 보상 결과 저장

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림

        return true;
    }

    public int GetOwnedCardCount(CardData cardData)
    {
        OwnedCardData ownedCard = GetOwnedCard(cardData); // 보유 카드 검색

        return ownedCard == null ? 0 : ownedCard.OwnedCount; // 보유 수량 반환
    }

    public OwnedCardData GetOwnedCard(CardData cardData)
    {
        if (cardData == null) { return null; } // 빈 카드 차단

        foreach (OwnedCardData ownedCard in ownedCards)
        {
            if (ownedCard == null ||
                ownedCard.CardData == null)
            {
                continue;
            }

            if (ownedCard.CardData == cardData)
            {
                return ownedCard;
            }

            if (ownedCard.CardData.CardId ==
                cardData.CardId)
            {
                return ownedCard;
            }
        }

        return null;
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

    public int AddCard(
        CardData cardData,
        bool convertExcessToEssence
    )
    {
        if (cardData == null) { return 0; } // 빈 카드 차단

        RegisterMonsterGrowth(cardData.SummonMonster); // 성장 데이터 등록

        OwnedCardData ownedCard = GetOwnedCard(cardData); // 기존 보유 카드 검색

        if (ownedCard == null)
        {
            ownedCards.Add(new OwnedCardData(cardData)); // 신규 카드 등록

            Debug.Log(
                $"New card added: {cardData.CardName} " +
                $"({CardRarityRules.GetDisplayName(cardData.Rarity)}) " +
                $"1 / {cardData.MaxCopies}"
            ); // 신규 보유 확인

            return 0;
        }

        if (ownedCard.TryAddCopy())
        {
            Debug.Log(
                $"Card copy added: {cardData.CardName} " +
                $"{ownedCard.OwnedCount} / {ownedCard.MaxOwnedCount}"
            ); // 중복 보유 확인

            return 0;
        }

        if (!convertExcessToEssence)
        {
            Debug.LogWarning(
                $"Card ownership limit reached: {cardData.CardName} " +
                $"{ownedCard.OwnedCount} / {ownedCard.MaxOwnedCount}"
            ); // 시작 데이터 초과 경고

            return 0;
        }

        int essenceReward = cardData.EssenceReward; // 희귀도별 변환량

        monsterEssence += essenceReward; // 마물의 정수 지급

        Debug.Log(
            $"Card converted to essence: {cardData.CardName} " +
            $"({CardRarityRules.GetDisplayName(cardData.Rarity)}), " +
            $"Essence +{essenceReward}"
        ); // 초과 변환 확인

        return essenceReward;
    }

    private int AddCapturedMonster(MonsterData monsterData)
    {
        if (monsterData == null) { return 0; } // 빈 마물 차단

        RegisterMonsterGrowth(monsterData); // 성장 데이터 등록

        CardData rewardCard = monsterData.CaptureRewardCard; // 포획 보상 카드

        if (rewardCard == null)
        {
            Debug.LogWarning(
                $"Capture reward card is missing: " +
                $"{monsterData.MonsterName}"
            ); // 보상 카드 누락 경고

            return 0;
        }

        return AddCard(rewardCard, true); // 카드 지급 또는 초과 변환
    }

    private void RegisterMonsterGrowth(MonsterData monsterData)
    {
        if (monsterData == null) { return; } // 빈 마물 차단
        if (GetOwnedMonster(monsterData) != null) { return; } // 기존 데이터 유지

        ownedMonsters.Add(new OwnedMonsterData(monsterData)); // 성장 데이터 등록
    }

    private void InitializeStartingProgress()
    {
        gold = Mathf.Max(0, startingGold); // 시작 골드
        totalExperience = 0; // 전체 경험치 초기화
        monsterEssence = 0; // 마물의 정수 초기화
        ownedCards.Clear(); // 보유 카드 초기화
        ownedMonsters.Clear(); // 보유 마물 초기화

        foreach (StartingCardEntry startingCard in startingCards)
        {
            if (startingCard == null) { continue; } // 빈 항목 제외
            if (startingCard.CardData == null) { continue; } // 빈 카드 제외

            for (int i = 0; i < startingCard.Count; i++)
            {
                AddCard(startingCard.CardData, false); // 시작 카드 지급
            }
        }
    }
}
