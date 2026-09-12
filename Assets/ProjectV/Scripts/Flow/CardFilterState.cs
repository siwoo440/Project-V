using System.Collections.Generic; // 리스트 기능

// 보유 카드 목록의 필터와 정렬 상태를 보관한다. (기획서 6.12.4 / 6.12.5)
public class CardFilterState
{
    private static readonly MonsterType[] TypeFilters =
    {
        MonsterType.None, MonsterType.Tentacle, MonsterType.Slime,
        MonsterType.Goblin, MonsterType.Demon, MonsterType.Undead,
        MonsterType.Beast, MonsterType.Spirit, MonsterType.Machine,
        MonsterType.Angel,
    }; // 0번은 전체

    private static readonly CardRarity[] RarityFilters =
    {
        CardRarity.Common, CardRarity.Rare,
        CardRarity.Special, CardRarity.Legendary,
    };

    private static readonly string[] ManaFilterNames =
    {
        "전체", "0~2", "3~5", "6 이상",
    };

    private static readonly CardSortMode[] SortModes =
    {
        CardSortMode.Name, CardSortMode.ManaAsc, CardSortMode.ManaDesc,
        CardSortMode.Rarity, CardSortMode.MonsterType,
        CardSortMode.EnhanceLevel,
    };

    private int typeFilterIndex;   // 0이면 전체
    private int rarityFilterIndex; // 0이면 전체
    private int manaFilterIndex;   // 0이면 전체
    private int sortModeIndex;     // 정렬 방식 번호
    private string searchText = string.Empty; // 이름 검색어

    public void CycleType()
    {
        typeFilterIndex = (typeFilterIndex + 1) % TypeFilters.Length;
    }

    public void CycleRarity()
    {
        rarityFilterIndex =
            (rarityFilterIndex + 1) % (RarityFilters.Length + 1);
    }

    public void CycleMana()
    {
        manaFilterIndex = (manaFilterIndex + 1) % ManaFilterNames.Length;
    }

    public void CycleSort()
    {
        sortModeIndex = (sortModeIndex + 1) % SortModes.Length;
    }

    public void SetSearchText(string newText)
    {
        searchText = newText == null ? string.Empty : newText.Trim();
    }

    public string TypeLabel =>
        typeFilterIndex == 0
            ? "계열: 전체"
            : $"계열: {MonsterTypeRules.GetDisplayName(TypeFilters[typeFilterIndex])}";

    public string RarityLabel =>
        rarityFilterIndex == 0
            ? "희귀도: 전체"
            : $"희귀도: {CardRarityRules.GetDisplayName(RarityFilters[rarityFilterIndex - 1])}";

    public string ManaLabel =>
        $"마나: {ManaFilterNames[manaFilterIndex]}";

    public string SortLabel =>
        $"정렬: {GetSortModeName(SortModes[sortModeIndex])}";

    public bool Passes(CardData cardData) // 필터 통과 여부
    {
        if (cardData == null) { return false; }

        if (typeFilterIndex > 0)
        {
            MonsterType filterType = TypeFilters[typeFilterIndex];

            if (cardData.SummonMonster == null) { return false; }
            if (!cardData.SummonMonster.HasType(filterType)) { return false; }
        }

        if (rarityFilterIndex > 0)
        {
            CardRarity filterRarity = RarityFilters[rarityFilterIndex - 1];

            if (cardData.Rarity != filterRarity) { return false; }
        }

        if (manaFilterIndex > 0)
        {
            int manaCost = cardData.ManaCost;

            bool inRange =
                (manaFilterIndex == 1 && manaCost <= 2) ||
                (manaFilterIndex == 2 && manaCost >= 3 && manaCost <= 5) ||
                (manaFilterIndex == 3 && manaCost >= 6);

            if (!inRange) { return false; }
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            if (cardData.CardName == null) { return false; }
            if (!cardData.CardName.Contains(searchText)) { return false; }
        }

        return true;
    }

    public void Sort(List<OwnedCardData> ownedCardList) // 보유 카드 정렬
    {
        if (ownedCardList == null) { return; }

        CardSortMode sortMode = SortModes[sortModeIndex];

        ownedCardList.Sort((left, right) =>
        {
            CardData leftCard = left.CardData;
            CardData rightCard = right.CardData;

            switch (sortMode)
            {
                case CardSortMode.EnhanceLevel:
                    return right.HighestEnhanceLevel.CompareTo(
                        left.HighestEnhanceLevel
                    ); // 강화 단계가 높은 카드부터

                case CardSortMode.ManaAsc:
                    return leftCard.ManaCost.CompareTo(rightCard.ManaCost);

                case CardSortMode.ManaDesc:
                    return rightCard.ManaCost.CompareTo(leftCard.ManaCost);

                case CardSortMode.Rarity:
                    return rightCard.Rarity.CompareTo(leftCard.Rarity);

                case CardSortMode.MonsterType:
                    return leftCard.MainType.CompareTo(rightCard.MainType);

                default:
                    return string.Compare(
                        leftCard.CardName,
                        rightCard.CardName,
                        System.StringComparison.Ordinal
                    );
            }
        });
    }

    private static string GetSortModeName(CardSortMode sortMode)
    {
        switch (sortMode)
        {
            case CardSortMode.ManaAsc:
                return "마나 낮은 순";

            case CardSortMode.ManaDesc:
                return "마나 높은 순";

            case CardSortMode.Rarity:
                return "희귀도순";

            case CardSortMode.MonsterType:
                return "계열순";

            case CardSortMode.EnhanceLevel:
                return "강화 단계순";

            default:
                return "이름순";
        }
    }
}
