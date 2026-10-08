using System; // 직렬화 특성
using System.Collections.Generic; // 리스트 기능

// 저장 파일에 적는 진행 데이터 (기획서 15.2)
// 카드, 노드, 아이템은 에셋 대신 ID 문자열로 적고 불러올 때 ID로 다시 찾는다.
// 항목 이름은 저장 파일에 그대로 남으므로 바꾸려면 저장 버전을 올리고 변환을 넣어야 한다.

[Serializable]
public class SaveCopyEntry // 카드 사본 하나
{
    public int number; // 같은 카드 안의 사본 번호
    public int level;  // 강화 단계
}

[Serializable]
public class SaveCardEntry // 보유 카드 한 종류
{
    public string cardId;      // 카드 ID
    public int nextCopyNumber; // 다음에 받을 사본의 번호
    public List<SaveCopyEntry> copies = new List<SaveCopyEntry>(); // 보유 사본
}

[Serializable]
public class SaveDeckCardEntry // 덱에 편성한 사본 하나
{
    public string cardId;  // 카드 ID
    public int copyNumber; // 사본 번호
}

[Serializable]
public class SaveDeckEntry // 덱 프리셋 하나
{
    public string name; // 프리셋 이름
    public List<SaveDeckCardEntry> cards = new List<SaveDeckCardEntry>(); // 편성한 사본
}

[Serializable]
public class SaveCountEntry // ID 하나에 딸린 수치 (패시브 단계, 그리모어 단계, 아이템 수량)
{
    public string id; // 데이터 ID
    public int value; // 단계 또는 수량
}

[Serializable]
public class SaveData
{
    public int version;            // 저장 데이터 버전
    public string gameVersion;     // 저장한 게임 버전
    public string savedAt;         // 저장 일시
    public double playTimeSeconds; // 누적 플레이 시간 (초)

    public int gold;            // 골드
    public int monsterEssence;  // 마물의 정수
    public int desireShards;    // 욕망의 파편
    public int totalExperience; // 누적 경험치 (레벨은 여기에서 계산한다)

    public List<SaveCardEntry> cards = new List<SaveCardEntry>(); // 보유 카드와 강화 단계
    public List<SaveDeckEntry> decks = new List<SaveDeckEntry>(); // 덱 프리셋 5개
    public int selectedDeckIndex; // 사용 중인 프리셋 번호

    public string equippedSkillId;   // 장착한 소환사 액티브 스킬
    public string equippedPassiveId; // 장착한 소환사 패시브
    public List<SaveCountEntry> passiveRanks = new List<SaveCountEntry>(); // 패시브별 단계

    public List<SaveCountEntry> grimoireLevels = new List<SaveCountEntry>(); // 그리모어 노드별 단계

    public List<SaveCountEntry> battleItems = new List<SaveCountEntry>(); // 소모성 아이템 수량
    public string equippedBattleItemId; // 전투에 가져갈 소모성 아이템

    // 저장 파일에 없는 항목은 빈 값으로 읽힌다. (파일을 읽을 때는 위의 초기값이 적용되지 않는다)
    // 예전 버전의 파일을 읽어도 안전하도록, 읽은 뒤에 빈 목록과 빈 문자열로 채워 둔다.
    public void FillMissing()
    {
        if (gameVersion == null) { gameVersion = string.Empty; }
        if (savedAt == null) { savedAt = string.Empty; }
        if (equippedSkillId == null) { equippedSkillId = string.Empty; }
        if (equippedPassiveId == null) { equippedPassiveId = string.Empty; }
        if (equippedBattleItemId == null) { equippedBattleItemId = string.Empty; }

        if (cards == null) { cards = new List<SaveCardEntry>(); }
        if (decks == null) { decks = new List<SaveDeckEntry>(); }
        if (passiveRanks == null) { passiveRanks = new List<SaveCountEntry>(); }
        if (grimoireLevels == null) { grimoireLevels = new List<SaveCountEntry>(); }
        if (battleItems == null) { battleItems = new List<SaveCountEntry>(); }

        cards.RemoveAll(entry => entry == null);
        decks.RemoveAll(entry => entry == null);
        passiveRanks.RemoveAll(entry => entry == null);
        grimoireLevels.RemoveAll(entry => entry == null);
        battleItems.RemoveAll(entry => entry == null);

        foreach (SaveCardEntry cardEntry in cards)
        {
            if (cardEntry.copies == null) { cardEntry.copies = new List<SaveCopyEntry>(); }

            cardEntry.copies.RemoveAll(entry => entry == null);
        }

        foreach (SaveDeckEntry deckEntry in decks)
        {
            if (deckEntry.name == null) { deckEntry.name = string.Empty; }
            if (deckEntry.cards == null) { deckEntry.cards = new List<SaveDeckCardEntry>(); }

            deckEntry.cards.RemoveAll(entry => entry == null);
        }
    }
}
