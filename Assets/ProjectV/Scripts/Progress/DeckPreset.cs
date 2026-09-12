using System;
using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

[Serializable]
public class DeckPreset // 덱 프리셋 (기획서 6.12.3)
{
    [SerializeField] private string presetName = "덱"; // 프리셋 이름

    [SerializeField]
    private List<CardData> cards = new List<CardData>(); // 편성 카드 목록

    public string PresetName => presetName; // 프리셋 이름 반환
    public List<CardData> Cards => cards;   // 편성 카드 목록 반환

    public DeckPreset(string name)
    {
        presetName = name;
        cards = new List<CardData>();
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
        cards.AddRange(source.Cards); // 참조가 아닌 값 복사
    }
}
