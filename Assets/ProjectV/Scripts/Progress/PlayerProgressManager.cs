using System;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerProgressManager : MonoBehaviour, ICardOwnershipSource
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

    [Header("시작 진행 데이터")]
    [SerializeField]
    private bool useTestStartingResources = true; // 켜면 아래 시험값으로, 끄면 기획서 9.2의 시작 재화로 시작한다.

    [SerializeField, Min(0)] private int startingGold;
    [SerializeField, Min(0)] private int startingMonsterEssence; // 시작 마물의 정수
    [SerializeField, Min(0)] private int startingExperience;     // 시작 누적 경험치

    [SerializeField]
    private List<StartingCardEntry> startingCards =
        new List<StartingCardEntry>(); // 시작 보유 카드 목록

    [Header("덱")]
    [SerializeField, Min(1)] private int requiredDeckSize = 30; // 필요 덱 장수

    [Header("소환사 스킬")]
    [SerializeField]
    private List<SummonerSkillData> summonerSkills =
        new List<SummonerSkillData>(); // 액티브 스킬 목록 (해금 레벨 순)

    [SerializeField]
    private List<SummonerPassiveData> summonerPassives =
        new List<SummonerPassiveData>(); // 패시브 목록 (해금 레벨 순)

    private readonly List<OwnedCardData> ownedCards =
        new List<OwnedCardData>(); // 보유 카드 목록

    private readonly List<OwnedMonsterData> ownedMonsters =
        new List<OwnedMonsterData>(); // 보유 마물 성장 목록

    private readonly List<DeckPreset> deckPresets =
        new List<DeckPreset>(); // 덱 프리셋 목록

    private int selectedPresetIndex; // 선택한 프리셋 번호

    private readonly Dictionary<SummonerPassiveData, int> passiveRanks =
        new Dictionary<SummonerPassiveData, int>(); // 패시브별 현재 단계 (없으면 미해금)

    private SummonerSkillData equippedSkill;     // 장착한 액티브 스킬
    private SummonerPassiveData equippedPassive; // 장착한 패시브
    private int spentPassivePoints;              // 사용한 패시브 포인트

    private int gold; // 현재 골드
    private int totalExperience; // 전체 경험치
    private int monsterEssence; // 마물의 정수

    public event Action ProgressChanged;

    public int Gold => gold; // 현재 골드
    public int TotalExperience => totalExperience; // 전체 경험치
    public int MonsterEssence => monsterEssence; // 마물의 정수

    public int PlayerLevel =>
        PlayerLevelRules.GetLevel(totalExperience); // 플레이어 레벨 (누적 경험치에서 계산)

    public bool IsMaxPlayerLevel =>
        PlayerLevelRules.IsMaxLevel(PlayerLevel); // 최대 레벨 도달 여부

    public int ExperienceIntoLevel =>
        PlayerLevelRules.GetExperienceIntoLevel(totalExperience); // 현재 레벨에서 쌓은 경험치

    public int ExperienceToNextLevel =>
        PlayerLevelRules.GetRequiredExperience(PlayerLevel); // 다음 레벨 필요 경험치

    public int EnhanceLevelCap =>
        PlayerLevelRules.GetEnhanceCap(PlayerLevel); // 플레이어 레벨에 따른 강화 상한 (기획서 6.3.6)

    public string PlayerLevelText =>
        PlayerLevelRules.GetLevelText(totalExperience); // 레벨 표시 문구

    // 경험치를 받았을 때 실제로 반영될 양을 미리 계산한다. (최대 레벨에서는 0)
    public int PreviewExperienceGain(int amount)
    {
        if (amount <= 0) { return 0; }

        return PlayerLevelRules.ClampTotalExperience(
            totalExperience + amount
        ) - totalExperience;
    }

    public IReadOnlyList<OwnedCardData> OwnedCards =>
        ownedCards; // 보유 카드 목록 반환

    public IReadOnlyList<OwnedMonsterData> OwnedMonsters =>
        ownedMonsters; // 보유 마물 목록 반환

    public const int PresetCount = 5; // 덱 프리셋 수 (기획서 6.12.3)

    public IReadOnlyList<DeckPreset> DeckPresets =>
        deckPresets; // 프리셋 목록 반환

    public int SelectedPresetIndex => selectedPresetIndex; // 선택 프리셋 번호 반환

    public DeckPreset SelectedPreset // 선택한 프리셋 반환
    {
        get
        {
            EnsureDeckPresets();

            return deckPresets[
                Mathf.Clamp(selectedPresetIndex, 0, deckPresets.Count - 1)
            ];
        }
    }

    public IReadOnlyList<CardCopy> CurrentDeck =>
        SelectedPreset.Cards; // 선택 프리셋의 덱 반환

    public string GetPresetName(int presetIndex) // 프리셋 이름 반환
    {
        EnsureDeckPresets();

        if (presetIndex < 0 || presetIndex >= deckPresets.Count)
        {
            return string.Empty;
        }

        return deckPresets[presetIndex].PresetName;
    }

    public int RequiredDeckSize =>
        Mathf.Max(1, requiredDeckSize); // 필요 덱 장수 반환

    public int TotalOwnedCardCount // 보유 카드 총 수량
    {
        get
        {
            int totalCount = 0;

            foreach (OwnedCardData ownedCard in ownedCards)
            {
                if (ownedCard == null) { continue; }

                totalCount += ownedCard.OwnedCount;
            }

            return totalCount;
        }
    }

    public float CurrentDeckAverageMana // 덱 평균 마나 비용
    {
        get
        {
            IReadOnlyList<CardCopy> deckCards = CurrentDeck;

            if (deckCards.Count == 0) { return 0f; }

            int totalMana = 0;

            foreach (CardCopy deckCopy in deckCards)
            {
                if (deckCopy == null) { continue; }

                totalMana += deckCopy.ManaCost;
            }

            return (float)totalMana / deckCards.Count;
        }
    }

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

        ProgressChanged += MarkUnsavedChanges; // 바뀐 것이 있을 때만 화면 이동 시 자동 저장한다.

        if (GetComponent<SaveIndicator>() == null)
        {
            gameObject.AddComponent<SaveIndicator>(); // 화면 오른쪽 위의 저장 표시
        }
    }

    public bool ApplyBattleResult(BattleResultData resultData)
    {
        if (resultData == null) { return false; } // 결과 누락 차단
        if (resultData.RewardsApplied) { return false; } // 중복 수령 차단

        int essenceGained = 0; // 이번 전투 획득 정수
        int shardsGained = 0; // 실제로 반영된 욕망의 파편
        int experienceGained = 0; // 실제로 반영된 경험치
        int goldGained = 0; // 실제로 반영된 골드
        int essenceBefore = monsterEssence; // 보유 한도로 받지 못한 정수 계산용
        int levelBefore = PlayerLevel; // 보상 반영 전 레벨

        if (resultData.IsVictory)
        {
            goldGained = AddGold(resultData.GoldReward); // 골드 지급 (보유 한도 적용)
            experienceGained = AddExperience(resultData.ExperienceReward); // 경험치 지급
            shardsGained = AddDesireShards(resultData.DesireShardReward); // 욕망의 파편 지급
            AddMonsterEssence(resultData.BonusEssenceReward); // 그리모어 강화로 얻는 정수

            if (resultData.CaptureSucceeded &&
                resultData.CapturedMonster != null)
            {
                essenceGained = AddCapturedMonster(
                    resultData.CapturedMonster
                ); // 포획 카드 지급 또는 초과 변환
            }

            // 기획서 6.9의 성장 수단은 강화뿐이므로 전투 경험치로는 마물이 성장하지 않는다.

            foreach (MonsterData capturedMonster in resultData.CapturedMonsters)
            {
                essenceGained += AddCapturedMonster(capturedMonster); // 포획전: 카드 지급 또는 초과 변환 (기획서 9.14)
            }

            RecordStageClear(resultData.StageId, resultData.Difficulty); // 스테이지 승리 기록 (일반전과 포획전)

            if (resultData.ClearsRegion)
            {
                MarkRegionCleared(currentRegion); // 임시 규칙 (RegionRules 참고)
            }
        }
        else
        {
            experienceGained = AddExperience(resultData.ExperienceReward); // 패배 경험치 (기획서 9.8.3)
        }

        if (resultData.IsCaptureBattle)
        {
            RefreshCaptureBoard(currentRegion); // 포획전이 끝나면 이기든 지든 목록을 새로 뽑는다. (기획서 9.14)
        }

        resultData.MarkRewardsApplied(
            essenceGained,
            shardsGained
        ); // 보상 결과 저장

        if (resultData.IsVictory)
        {
            resultData.SetLimitLoss(
                resultData.GoldReward - goldGained,
                essenceGained + resultData.BonusEssenceReward - (monsterEssence - essenceBefore),
                resultData.DesireShardReward - shardsGained
            ); // 보유 한도 때문에 받지 못한 수량 (기획서 A.47)
        }

        resultData.SetLevelResult(
            experienceGained,
            levelBefore,
            PlayerLevel
        ); // 레벨 변화 저장

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림
        AutoSave("전투 결과"); // 기획서 15.6

        return true;
    }

    // 경험치를 더하고 실제로 반영된 양을 반환한다.
    // 최대 레벨 이후에는 경험치를 더 받지 않는다. (기획서 6.17.1)
    private int AddExperience(int amount)
    {
        if (amount <= 0) { return 0; } // 잘못된 지급량 차단

        int experienceBefore = totalExperience;
        int levelBefore = PlayerLevel;

        totalExperience = PlayerLevelRules.ClampTotalExperience(
            totalExperience + amount
        ); // 최대 누적 경험치에서 멈춤

        int levelAfter = PlayerLevel;

        if (levelAfter > levelBefore)
        {
            Debug.Log(
                $"플레이어 레벨 상승: Lv.{levelBefore} → Lv.{levelAfter} " +
                $"(마물 강화 상한 Lv.{EnhanceLevelCap}, " +
                $"패시브 포인트 {PassivePoints})"
            ); // 레벨 상승 기록
        }

        return totalExperience - experienceBefore;
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

    public IReadOnlyList<CardCopy> GetCardCopies(CardData cardData) // 카드 사본 목록 반환
    {
        OwnedCardData ownedCard = GetOwnedCard(cardData);

        return ownedCard == null
            ? Array.Empty<CardCopy>()
            : ownedCard.Copies;
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
                $"신규 카드 등록: {cardData.CardName} " +
                $"({CardRarityRules.GetDisplayName(cardData.Rarity)}) " +
                $"1 / {cardData.MaxCopies}"
            ); // 신규 보유 확인

            return 0;
        }

        CardCopy addedCopy = ownedCard.AddCopy(); // 새 사본 추가

        if (addedCopy != null)
        {
            Debug.Log(
                $"카드 추가 보유: {cardData.CardName} " +
                $"{addedCopy.DisplayName} " +
                $"{ownedCard.OwnedCount} / {ownedCard.MaxOwnedCount}"
            ); // 중복 보유 확인

            return 0;
        }

        if (!convertExcessToEssence)
        {
            Debug.LogWarning(
                $"카드 보유 한도 도달: {cardData.CardName} " +
                $"{ownedCard.OwnedCount} / {ownedCard.MaxOwnedCount}"
            ); // 시작 데이터 초과 경고

            return 0;
        }

        int essenceReward =
            cardData.EssenceReward +
            GetGrimoireAmount(GrimoireEffectType.EssenceExtract) +
            GetEquippedPassiveAmount(SummonerPassiveType.CaptureRecord); // 희귀도별 변환량 + 그리모어 + 포획 기록 패시브

        AddMonsterEssence(essenceReward); // 마물의 정수 지급 (보유 한도 적용)

        Debug.Log(
            $"카드를 정수로 변환: {cardData.CardName} " +
            $"({CardRarityRules.GetDisplayName(cardData.Rarity)}), " +
            $"정수 +{essenceReward}"
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
                $"포획 보상 카드가 없습니다: " +
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

    // ===== 마물 카드 강화 (기획서 6.9) =====

    public bool CanEnhanceCopy( // 강화 가능 여부와 사유 확인
        CardCopy targetCopy,
        out string errorMessage
    )
    {
        if (targetCopy == null || targetCopy.CardData == null)
        {
            errorMessage = "강화할 카드를 선택하세요.";
            return false;
        }

        OwnedCardData ownedCard = GetOwnedCard(targetCopy.CardData);

        if (ownedCard == null || !ownedCard.HasCopy(targetCopy))
        {
            errorMessage = "보유하지 않은 사본입니다."; // 기획서 6.9.6 임시 카드 차단
            return false;
        }

        if (targetCopy.IsMaxLevel)
        {
            errorMessage =
                $"{targetCopy.CardName}은 이미 최대 단계입니다. " +
                $"(Lv.{CardEnhanceRules.MaxLevel})";
            return false;
        }

        if (targetCopy.EnhanceLevel >= EnhanceLevelCap)
        {
            int nextLevel = targetCopy.EnhanceLevel + 1;

            errorMessage =
                $"플레이어 Lv.{PlayerLevelRules.GetEnhanceUnlockLevel(nextLevel)}에 " +
                $"마물 Lv.{nextLevel} 강화가 해금됩니다. " +
                $"(현재 Lv.{PlayerLevel})"; // 기획서 6.9.6
            return false;
        }

        int essenceCost = targetCopy.NextEssenceCost;
        int goldCost = targetCopy.NextGoldCost;

        if (monsterEssence < essenceCost)
        {
            errorMessage =
                $"마물의 정수가 부족합니다. " +
                $"({monsterEssence} / {essenceCost})";
            return false;
        }

        if (gold < goldCost)
        {
            errorMessage =
                $"골드가 부족합니다. ({gold} / {goldCost})";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public bool TryEnhanceCopy( // 사본 1장을 한 단계 강화 (기획서 6.9.7: 되돌리기 없음)
        CardCopy targetCopy,
        out string resultMessage
    )
    {
        if (!CanEnhanceCopy(targetCopy, out resultMessage))
        {
            return false;
        }

        int essenceCost = targetCopy.NextEssenceCost;
        int goldCost = targetCopy.NextGoldCost;
        int beforeLevel = targetCopy.EnhanceLevel;

        if (!targetCopy.RaiseEnhanceLevel())
        {
            resultMessage = "강화에 실패했습니다.";
            return false;
        }

        monsterEssence -= essenceCost; // 정수 차감
        gold -= goldCost; // 골드 차감

        SyncOwnedMonsterLevel(targetCopy); // 도감 표시 단계 갱신

        resultMessage =
            $"{targetCopy.CardName} " +
            $"Lv.{beforeLevel} → Lv.{targetCopy.EnhanceLevel} 강화 완료 " +
            $"(정수 -{essenceCost}, 골드 -{goldCost})";

        Debug.Log(resultMessage); // 강화 결과 기록

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림
        AutoSave("마물 강화"); // 기획서 9.17

        return true;
    }

    private void SyncOwnedMonsterLevel(CardCopy targetCopy) // 도감 표시 단계 동기화
    {
        if (targetCopy == null) { return; }

        OwnedMonsterData ownedMonster =
            GetOwnedMonster(targetCopy.SummonMonster);

        if (ownedMonster == null) { return; }

        ownedMonster.RaiseToLevel(targetCopy.EnhanceLevel);
    }

    public int GetDeckCardCount(CardData cardData) // 덱 편성 수량 반환
    {
        return SelectedPreset.CountCard(cardData);
    }

    public bool IsCopyInDeck(CardCopy targetCopy) // 사본 편성 여부 반환
    {
        return SelectedPreset.Contains(targetCopy);
    }

    public bool TryAddCardToDeck( // 덱에 카드 1장 추가 (편성 안 된 사본 중 최고 단계)
        CardData cardData,
        out string errorMessage
    )
    {
        CardCopy availableCopy = FindAvailableCopy(cardData);

        if (availableCopy == null)
        {
            errorMessage =
                cardData == null
                    ? "추가할 카드가 없습니다."
                    : $"{cardData.CardName}의 편성 가능한 사본이 없습니다.";

            return false;
        }

        return TryAddCopyToDeck(availableCopy, out errorMessage);
    }

    public bool TryAddCopyToDeck( // 덱에 사본 1장 추가
        CardCopy targetCopy,
        out string errorMessage
    )
    {
        bool canAdd = DeckValidator.TryAddCard(
            CurrentDeck,
            targetCopy,
            RequiredDeckSize,
            this,
            out errorMessage
        ); // 편성 규칙 검사

        if (!canAdd) { return false; }

        SelectedPreset.Cards.Add(targetCopy);
        ProgressChanged?.Invoke();

        return true;
    }

    private CardCopy FindAvailableCopy(CardData cardData) // 편성 안 된 최고 단계 사본 검색
    {
        OwnedCardData ownedCard = GetOwnedCard(cardData);

        if (ownedCard == null) { return null; }

        CardCopy bestCopy = null;

        foreach (CardCopy copy in ownedCard.Copies)
        {
            if (copy == null) { continue; }
            if (SelectedPreset.Contains(copy)) { continue; }

            if (bestCopy == null ||
                copy.EnhanceLevel > bestCopy.EnhanceLevel)
            {
                bestCopy = copy;
            }
        }

        return bestCopy;
    }

    public bool RemoveCardFromDeck(CardData cardData) // 덱에서 카드 1장 제거
    {
        if (cardData == null) { return false; }

        List<CardCopy> deckCards = SelectedPreset.Cards;

        for (int i = deckCards.Count - 1; i >= 0; i--)
        {
            if (deckCards[i] == null) { continue; }
            if (!deckCards[i].IsSameCard(cardData)) { continue; }

            deckCards.RemoveAt(i);
            ProgressChanged?.Invoke();

            return true;
        }

        return false;
    }

    public bool RemoveCopyFromDeck(CardCopy targetCopy) // 덱에서 사본 1장 제거
    {
        if (targetCopy == null) { return false; }

        if (!SelectedPreset.Cards.Remove(targetCopy)) { return false; }

        ProgressChanged?.Invoke();

        return true;
    }

    public void ClearDeck() // 덱 비우기
    {
        if (CurrentDeck.Count == 0) { return; }

        SelectedPreset.Cards.Clear();
        ProgressChanged?.Invoke();
    }

    public void FillDeckFromOwnedCards() // 보유 카드로 덱 채우기
    {
        RebuildDeckFromOwnedCards();
        ProgressChanged?.Invoke();
    }

    private void EnsureDeckPresets() // 프리셋 5개 확보
    {
        while (deckPresets.Count < PresetCount)
        {
            deckPresets.Add(new DeckPreset($"덱 {deckPresets.Count + 1}"));
        }

        selectedPresetIndex =
            Mathf.Clamp(selectedPresetIndex, 0, deckPresets.Count - 1);
    }

    public bool SelectPreset(int presetIndex) // 사용할 프리셋 선택
    {
        EnsureDeckPresets();

        if (presetIndex < 0 || presetIndex >= deckPresets.Count)
        {
            return false;
        }

        if (selectedPresetIndex == presetIndex) { return false; }

        selectedPresetIndex = presetIndex;
        ProgressChanged?.Invoke();

        return true;
    }

    public void RenameSelectedPreset(string presetName) // 프리셋 이름 변경
    {
        if (string.IsNullOrWhiteSpace(presetName)) { return; }

        SelectedPreset.SetName(presetName);
        ProgressChanged?.Invoke();
    }

    public bool CopyPresetToSelected(int sourceIndex) // 다른 프리셋 구성 복사
    {
        EnsureDeckPresets();

        if (sourceIndex < 0 || sourceIndex >= deckPresets.Count)
        {
            return false;
        }

        if (sourceIndex == selectedPresetIndex) { return false; }

        SelectedPreset.CopyFrom(deckPresets[sourceIndex]);
        ProgressChanged?.Invoke();

        return true;
    }

    public bool SetCurrentDeck(IReadOnlyList<CardCopy> deckCards) // 덱 교체
    {
        if (deckCards == null) { return false; } // 빈 목록 차단

        bool isValid = DeckValidator.TryValidate(
            deckCards,
            RequiredDeckSize,
            this,
            out string errorMessage
        ); // 보유 및 편성 검증

        if (!isValid)
        {
            Debug.LogWarning($"덱 변경 실패: {errorMessage}");
            return false;
        }

        SelectedPreset.Cards.Clear();
        SelectedPreset.Cards.AddRange(deckCards); // 덱 저장

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림

        return true;
    }

    private void RebuildDeckFromOwnedCards() // 보유 카드로 덱 구성
    {
        List<CardCopy> deckCards = SelectedPreset.Cards;

        deckCards.Clear();

        foreach (OwnedCardData ownedCard in ownedCards)
        {
            if (ownedCard == null) { continue; }
            if (ownedCard.CardData == null) { continue; }

            foreach (CardCopy copy in ownedCard.Copies)
            {
                if (copy == null) { continue; }

                deckCards.Add(copy); // 보유 사본을 모두 편성
            }
        }

        if (deckCards.Count != RequiredDeckSize)
        {
            Debug.LogWarning(
                $"시작 덱이 {RequiredDeckSize}장이 아닙니다. " +
                $"현재 {deckCards.Count}장."
            ); // 시작 덱 장수 경고
        }
    }

    private void InitializeStartingProgress()
    {
        // 시험값을 끄면 기획서 9.2의 새 게임 값으로 시작한다. (골드 500, 정수 5, 경험치 0)
        gold = Mathf.Clamp(
            useTestStartingResources ? startingGold : CurrencyRules.StartingGold,
            0, CurrencyRules.GoldLimit
        ); // 시작 골드

        totalExperience = PlayerLevelRules.ClampTotalExperience(
            useTestStartingResources ? startingExperience : 0
        ); // 시작 누적 경험치

        monsterEssence = Mathf.Clamp(
            useTestStartingResources ? startingMonsterEssence : CurrencyRules.StartingEssence,
            0, CurrencyRules.EssenceLimit
        ); // 마물의 정수 초기화
        deckPresets.Clear(); // 덱 프리셋 초기화
        selectedPresetIndex = 0;
        passiveRanks.Clear(); // 패시브 성장 초기화
        spentPassivePoints = 0;
        equippedPassive = null;
        equippedSkill = null; // 처음 조회할 때 기본 스킬을 장착한다.
        InitializeGrimoireProgress(); // 욕망의 파편과 그리모어 강화 초기화
        InitializeShopProgress(); // 소모성 아이템 초기화
        InitializeRegionProgress(); // 지역 진행 초기화
        InitializeStageProgress(); // 스테이지 승리 기록 초기화
        InitializeCaptureProgress(); // 포획 목록 초기화
        EnsureDeckPresets();
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

        RebuildDeckFromOwnedCards(); // 시작 덱 구성
    }
}
