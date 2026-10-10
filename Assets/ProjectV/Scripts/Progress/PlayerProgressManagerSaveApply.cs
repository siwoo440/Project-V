using System; // 수학 기능
using UnityEngine; // Unity 기본 기능

// 저장 데이터를 진행 데이터로 되돌리기 (기획서 15.2)
// 지금 게임에 없는 ID는 건너뛰고, 수치는 지금 규칙의 범위 안으로 맞춘다.
public partial class PlayerProgressManager
{
    private void ApplySaveData(SaveData data)
    {
        totalExperience = PlayerLevelRules.ClampTotalExperience(data.totalExperience); // 레벨 조건 판정에 먼저 필요하다.
        gold = Mathf.Clamp(data.gold, 0, CurrencyRules.GoldLimit);
        monsterEssence = Mathf.Clamp(data.monsterEssence, 0, CurrencyRules.EssenceLimit);
        desireShards = Mathf.Clamp(data.desireShards, 0, GrimoireRules.ShardLimit);
        playTimeSeconds = Math.Max(0d, data.playTimeSeconds);

        int skippedCount =
            RestoreCards(data) +
            RestoreDecks(data) +
            RestoreSummoner(data) +
            RestoreGrimoire(data) +
            RestoreBattleItems(data) +
            RestoreRegions(data);

        RestoreStageClears(data); // 스테이지 ID는 에셋을 찾지 않고 그대로 되살린다.

        if (skippedCount > 0)
        {
            Debug.LogWarning(
                $"저장 데이터에서 지금 게임에 없는 항목 {skippedCount}개를 건너뛰었습니다."
            );
        }
    }

    private int RestoreCards(SaveData data) // 보유 카드, 사본별 강화 단계, 도감의 마물 기록
    {
        int skippedCount = 0;

        ownedCards.Clear();
        ownedMonsters.Clear();

        foreach (SaveCardEntry cardEntry in data.cards)
        {
            CardData card = FindCardById(cardEntry.cardId);

            if (card == null)
            {
                skippedCount += 1;
                continue;
            }

            if (GetOwnedCard(card) != null) { continue; } // 같은 카드가 두 번 적힌 경우

            OwnedCardData ownedCard = OwnedCardData.CreateForRestore(card);

            foreach (SaveCopyEntry copyEntry in cardEntry.copies)
            {
                ownedCard.RestoreCopy(copyEntry.number, copyEntry.level);
            }

            if (ownedCard.OwnedCount <= 0) { continue; }

            ownedCard.RestoreNextCopyNumber(cardEntry.nextCopyNumber);
            ownedCards.Add(ownedCard);

            RegisterMonsterGrowth(card.SummonMonster); // 도감 기록은 보유 카드에서 다시 만든다.

            OwnedMonsterData ownedMonster = GetOwnedMonster(card.SummonMonster);

            if (ownedMonster != null)
            {
                ownedMonster.RaiseToLevel(ownedCard.HighestEnhanceLevel);
            }
        }

        return skippedCount;
    }

    private int RestoreDecks(SaveData data) // 덱 프리셋. 사본은 카드 ID와 사본 번호로 다시 찾는다.
    {
        int skippedCount = 0;

        deckPresets.Clear();

        foreach (SaveDeckEntry deckEntry in data.decks)
        {
            if (deckPresets.Count >= PresetCount) { break; }

            DeckPreset preset = new DeckPreset(
                string.IsNullOrWhiteSpace(deckEntry.name)
                    ? $"덱 {deckPresets.Count + 1}"
                    : deckEntry.name
            );

            foreach (SaveDeckCardEntry deckCard in deckEntry.cards)
            {
                OwnedCardData ownedCard = FindOwnedCardById(deckCard.cardId);
                CardCopy copy = ownedCard == null ? null : ownedCard.GetCopy(deckCard.copyNumber);

                if (copy == null)
                {
                    skippedCount += 1;
                    continue;
                }

                if (preset.Contains(copy)) { continue; }

                preset.Cards.Add(copy);
            }

            deckPresets.Add(preset);
        }

        selectedPresetIndex = data.selectedDeckIndex;
        EnsureDeckPresets(); // 모자란 프리셋을 채우고 선택 번호를 범위 안으로 맞춘다.

        return skippedCount;
    }
}
