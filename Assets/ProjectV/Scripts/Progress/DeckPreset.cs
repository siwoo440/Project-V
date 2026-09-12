using System; // 직렬화 특성
using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

[Serializable]
public class DeckPreset // 덱 프리셋 (기획서 6.12.3)
{
    [SerializeField] private string presetName = "덱"; // 프리셋 이름

    [SerializeField]
    private List<CardCopy> cards = new List<CardCopy>(); // 편성 사본 목록

    public string PresetName => presetName; // 프리셋 이름 반환
    public List<CardCopy> Cards => cards;   // 편성 사본 목록 반환

    public DeckPreset(string name)
    {
        presetName = name;
        cards = new List<CardCopy>();
    }

    public void SetName(string name) // 프리셋 이름 변경
    {
        if (string.IsNullOrWhiteSpace(name)) { return; }

        presetName = name.Trim();
    }

    public void CopyFrom(DeckPreset source) // 다른 프리셋 구성 복사
    {
        if (source == null) { return; }

        cards.Clear();
        cards.AddRange(source.Cards); // 같은 사본을 그대로 편성
    }

    public int CountCard(CardData cardData) // 같은 카드 편성 수량
    {
        if (cardData == null) { return 0; }

        int count = 0;

        foreach (CardCopy copy in cards)
        {
            if (copy == null) { continue; }
            if (!copy.IsSameCard(cardData)) { continue; }

            count += 1;
        }

        return count;
    }

    public bool Contains(CardCopy targetCopy) // 해당 사본 편성 여부
    {
        if (targetCopy == null) { return false; }

        return cards.Contains(targetCopy);
    }
}
