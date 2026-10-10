using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

public partial class PlayerProgressManager // 진행 데이터를 저장 데이터로 옮기기 (기획서 15.2)
{
    private SaveData CreateSaveData()
    {
        SaveData data = new SaveData
        {
            version = SaveRules.CurrentVersion,
            gameVersion = Application.version,
            savedAt = SaveRules.CreateSavedAt(),
            playTimeSeconds = playTimeSeconds,
            gold = gold,
            monsterEssence = monsterEssence,
            desireShards = desireShards,
            totalExperience = totalExperience,
            selectedDeckIndex = selectedPresetIndex
        };

        foreach (OwnedCardData ownedCard in ownedCards) // 보유 카드와 사본별 강화 단계
        {
            if (ownedCard == null || ownedCard.CardData == null) { continue; }

            SaveCardEntry cardEntry = new SaveCardEntry
            {
                cardId = ownedCard.CardData.CardId,
                nextCopyNumber = ownedCard.NextCopyNumber
            };

            foreach (CardCopy copy in ownedCard.Copies)
            {
                if (copy == null) { continue; }

                cardEntry.copies.Add(
                    new SaveCopyEntry { number = copy.CopyNumber, level = copy.EnhanceLevel }
                );
            }

            data.cards.Add(cardEntry);
        }

        EnsureDeckPresets();

        foreach (DeckPreset preset in deckPresets) // 덱 프리셋 5개
        {
            SaveDeckEntry deckEntry = new SaveDeckEntry { name = preset.PresetName };

            foreach (CardCopy copy in preset.Cards)
            {
                if (copy == null || copy.CardData == null) { continue; }

                deckEntry.cards.Add(
                    new SaveDeckCardEntry { cardId = copy.CardId, copyNumber = copy.CopyNumber }
                );
            }

            data.decks.Add(deckEntry);
        }

        SummonerSkillData skill = EquippedSkill; // 장착하지 않았으면 기본 스킬

        data.equippedSkillId = skill == null ? string.Empty : skill.SkillId;
        data.equippedPassiveId = equippedPassive == null ? string.Empty : equippedPassive.PassiveId;

        foreach (KeyValuePair<SummonerPassiveData, int> pair in passiveRanks) // 패시브 단계
        {
            if (pair.Key == null || pair.Value <= 0) { continue; }

            data.passiveRanks.Add(new SaveCountEntry { id = pair.Key.PassiveId, value = pair.Value });
        }

        foreach (KeyValuePair<GrimoireNodeData, int> pair in grimoireLevels) // 그리모어 노드 단계
        {
            if (pair.Key == null || pair.Value <= 0) { continue; }

            data.grimoireLevels.Add(new SaveCountEntry { id = pair.Key.NodeId, value = pair.Value });
        }

        foreach (KeyValuePair<BattleItemData, int> pair in battleItemCounts) // 소모성 아이템 수량
        {
            if (pair.Key == null || pair.Value <= 0) { continue; }

            data.battleItems.Add(new SaveCountEntry { id = pair.Key.ItemId, value = pair.Value });
        }

        foreach (RegionData region in regions) // 지역별 진행
        {
            if (region == null) { continue; }

            bool isVisited = visitedRegions.Contains(region);
            bool isCleared = clearedRegions.Contains(region);

            if (!isVisited && !isCleared) { continue; } // 손대지 않은 지역은 적지 않는다.

            data.regions.Add(
                new SaveRegionEntry { id = region.RegionId, visited = isVisited, cleared = isCleared }
            );
        }

        data.currentRegionId = currentRegion == null ? string.Empty : currentRegion.RegionId;

        SaveCaptureBoards(data); // 지역별 포획 목록

        foreach (KeyValuePair<string, int> pair in stageClearMasks) // 스테이지 승리 기록
        {
            if (string.IsNullOrEmpty(pair.Key) || pair.Value == 0) { continue; }

            data.stageClears.Add(new SaveCountEntry { id = pair.Key, value = pair.Value });
        }

        BattleItemData equippedItem = EquippedBattleItem; // 다 쓴 아이템은 장착하지 않은 것으로 적는다.

        data.equippedBattleItemId = equippedItem == null ? string.Empty : equippedItem.ItemId;

        return data;
    }

    // ---------- ID로 데이터 찾기 ----------

    private CardData FindCardById(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) { return null; }

        foreach (CardData card in cardCatalog)
        {
            if (card != null && card.CardId == cardId) { return card; }
        }

        foreach (StartingCardEntry startingCard in startingCards) // 전체 목록이 비어 있을 때의 대비
        {
            if (startingCard == null || startingCard.CardData == null) { continue; }
            if (startingCard.CardData.CardId == cardId) { return startingCard.CardData; }
        }

        return null;
    }

    private OwnedCardData FindOwnedCardById(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) { return null; }

        foreach (OwnedCardData ownedCard in ownedCards)
        {
            if (ownedCard == null || ownedCard.CardData == null) { continue; }
            if (ownedCard.CardData.CardId == cardId) { return ownedCard; }
        }

        return null;
    }

    private SummonerSkillData FindSkillById(string skillId)
    {
        if (string.IsNullOrEmpty(skillId)) { return null; }

        foreach (SummonerSkillData skill in summonerSkills)
        {
            if (skill != null && skill.SkillId == skillId) { return skill; }
        }

        return null;
    }

    private SummonerPassiveData FindPassiveById(string passiveId)
    {
        if (string.IsNullOrEmpty(passiveId)) { return null; }

        foreach (SummonerPassiveData passive in summonerPassives)
        {
            if (passive != null && passive.PassiveId == passiveId) { return passive; }
        }

        return null;
    }

    private GrimoireNodeData FindGrimoireNodeById(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) { return null; }

        foreach (GrimoireNodeData node in grimoireNodes)
        {
            if (node != null && node.NodeId == nodeId) { return node; }
        }

        return null;
    }

    private BattleItemData FindBattleItemById(string itemId) // 소모성 아이템은 상점 상품을 거쳐 찾는다.
    {
        if (string.IsNullOrEmpty(itemId)) { return null; }

        foreach (ShopItemData shopItem in shopItems)
        {
            if (shopItem == null || shopItem.BattleItem == null) { continue; }
            if (shopItem.BattleItem.ItemId == itemId) { return shopItem.BattleItem; }
        }

        return null;
    }
}
