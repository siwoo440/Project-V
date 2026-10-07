using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 소환사 액티브 스킬과 패시브 (기획서 6.4 / 6.5)
{
    private const int StartingMaximumMana = 2; // 첫 턴 최대 마나 (기획서 5.5.1 / A.18)
    private const int MaximumManaLimit = 10;   // 최대 마나 상한

    private readonly Dictionary<Button, CardCopy> handCardCopies =
        new Dictionary<Button, CardCopy>(); // 손패 버튼과 카드 사본 연결

    private SummonerSkillData battleSkill;     // 이번 전투의 액티브 스킬
    private SummonerPassiveData battlePassive; // 이번 전투의 패시브
    private int battlePassiveRank;             // 이번 전투의 패시브 단계
    private MonsterType passiveFocusType;      // 계열 집중 대상 계열

    private bool summonerSkillUsedThisTurn; // 이번 턴 액티브 사용 여부 (기획서 6.4.1: 턴당 1회)
    private bool isSelectingSkillTarget;    // 액티브 대상 선택 중 여부
    private bool thriftySummonUsed;         // 절약 소환 사용 여부 (전투당 1회)
    private bool legionCommandActive;       // 군단 지휘 발동 상태
    private bool isDrawingReshuffleBonus;   // 재활용 지식 추가 드로우 처리 중
    private int pendingReshuffleBonus;      // 덱을 다시 섞어 생긴 추가 드로우
    private int playerMaxHpBonus;           // 패시브로 늘어난 플레이어 최대 HP
    private int turnLustBonus;              // 이번 턴 성욕 부여량 증가 (욕망 공명)

    private int PlayerMaxHp =>
        Mathf.Max(1, playerMaxHp + playerMaxHpBonus); // 보정을 포함한 플레이어 최대 HP

    // 전투 시작 시 장착한 스킬과 패시브를 읽는다.
    // 기획서 6.5.4 적용 순서: 기본 능력치 → 그리모어 강화(미구현) → 장착 패시브
    private void PrepareSummonerForBattle()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        battleSkill = progress == null ? null : progress.EquippedSkill;
        battlePassive = progress == null ? null : progress.EquippedPassive;
        battlePassiveRank = progress == null ? 0 : progress.EquippedPassiveRank;

        summonerSkillUsedThisTurn = false;
        isSelectingSkillTarget = false;
        thriftySummonUsed = false;
        legionCommandActive = false;
        isDrawingReshuffleBonus = false;
        pendingReshuffleBonus = 0;
        turnLustBonus = 0;

        playerMaxHpBonus =
            Grimoire(GrimoireEffectType.PlayerMaxHp) +
            GetPassiveAmount(SummonerPassiveType.LifeContract); // 그리모어 → 패시브 순으로 더한다.
        passiveFocusType = FindMostCommonDeckType();

        if (summonerSkillButton != null)
        {
            summonerSkillButton.onClick.RemoveAllListeners();
            summonerSkillButton.onClick.AddListener(OnSummonerSkillButton);
        }
    }

    private void LogSummonerLoadout() // 전투 시작 시 장착 내용 기록
    {
        string skillName = battleSkill == null ? "없음" : battleSkill.DisplayName;

        string passiveName = battlePassive == null
            ? "없음"
            : $"{battlePassive.DisplayName} {battlePassiveRank}단계";

        AddBattleLog(
            BattleLogCategory.System,
            $"소환사 스킬: {skillName} / 패시브: {passiveName}"
        );
    }

    // 장착한 패시브가 해당 종류일 때만 현재 단계의 수치를 돌려준다. (아니면 0)
    private int GetPassiveAmount(SummonerPassiveType passiveType)
    {
        if (battlePassive == null || battlePassiveRank <= 0) { return 0; }
        if (battlePassive.PassiveType != passiveType) { return 0; }

        return battlePassive.GetAmount(battlePassiveRank);
    }

    private MonsterType FindMostCommonDeckType() // 덱에 가장 많은 계열 (계열 집중 대상)
    {
        Dictionary<MonsterType, int> typeCounts = new Dictionary<MonsterType, int>();

        foreach (CardCopy deckCopy in battleDeck)
        {
            if (deckCopy == null || deckCopy.CardData == null) { continue; }

            MonsterType mainType = deckCopy.CardData.MainType;

            if (mainType == MonsterType.None) { continue; }

            typeCounts.TryGetValue(mainType, out int count);
            typeCounts[mainType] = count + 1;
        }

        MonsterType bestType = MonsterType.None;
        int bestCount = 0;

        foreach (KeyValuePair<MonsterType, int> typeCount in typeCounts)
        {
            bool isBetter =
                typeCount.Value > bestCount ||
                (typeCount.Value == bestCount && typeCount.Key < bestType);

            if (!isBetter) { continue; }

            bestType = typeCount.Key;
            bestCount = typeCount.Value;
        }

        return bestType;
    }

    // ---------- 패시브 적용 ----------

    // 모든 마물이 받는 공격 보정: 그리모어 강화와 군단 지휘 패시브
    private int GetSummonerAttackBonus()
    {
        int livingCount = 0;

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }

            livingCount += 1;
        }

        int grimoireBonus = GetGrimoireAttackBonus(livingCount);
        int amount = GetPassiveAmount(SummonerPassiveType.LegionCommand);

        if (amount <= 0) { return grimoireBonus; }

        bool isActive = livingCount >= battlePassive.ConditionValue;

        if (isActive != legionCommandActive)
        {
            legionCommandActive = isActive;

            AddBattleLog(
                BattleLogCategory.System,
                isActive
                    ? $"군단 지휘 발동: 모든 마물 공격 +{amount}"
                    : "군단 지휘 해제"
            );
        }

        return grimoireBonus + (isActive ? amount : 0);
    }

    private int GetSummonerLustBonus() // 그리모어 강화, 욕망 증폭 패시브, 욕망 공명 스킬의 성욕 보정
    {
        return GetGrimoireLustBonus() +
               GetPassiveAmount(SummonerPassiveType.LustAmplify) +
               turnLustBonus;
    }

    private int ReducePlayerHpDamage(int hpDamage) // 강인한 계약: 받는 HP 피해 감소 (최소 1)
    {
        int reduction = GetPassiveAmount(SummonerPassiveType.SturdyContract);

        if (reduction <= 0 || hpDamage <= 0) { return hpDamage; }

        return Mathf.Max(1, hpDamage - reduction);
    }

    private bool IsThriftySummonTarget(CardCopy cardCopy) // 절약 소환 대상 카드 여부
    {
        if (thriftySummonUsed || cardCopy == null) { return false; }
        if (GetPassiveAmount(SummonerPassiveType.ThriftySummon) <= 0) { return false; }
        if (cardCopy.ManaCost <= 0) { return false; } // 비용이 없는 카드에는 쓰지 않는다.

        return cardCopy.ManaCost <= battlePassive.ConditionValue;
    }

    private int GetThriftyDiscount(CardCopy cardCopy) // 절약 소환 패시브로 줄어드는 비용
    {
        if (!IsThriftySummonTarget(cardCopy)) { return 0; }

        int remainingCost =
            cardCopy.ManaCost - GetGrimoireCardDiscount(cardCopy); // 그리모어를 먼저 적용한 뒤의 비용

        return Mathf.Clamp(
            GetPassiveAmount(SummonerPassiveType.ThriftySummon),
            0,
            Mathf.Max(0, remainingCost)
        );
    }

    private int GetCardPlayCost(CardCopy cardCopy) // 그리모어와 패시브를 반영한 실제 카드 비용
    {
        if (cardCopy == null) { return 0; }

        return Mathf.Max(
            0,
            cardCopy.ManaCost -
            GetGrimoireCardDiscount(cardCopy) -
            GetThriftyDiscount(cardCopy)
        );
    }

    private void ClearSummonerTurnEffects() // 플레이어 턴 종료 시 이번 턴 한정 효과 제거
    {
        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null) { continue; }

            fieldMonster.ClearTurnBonus();
        }

        if (turnLustBonus == 0) { return; }

        turnLustBonus = 0;
        RefreshSynergies(); // 성욕 보정 다시 계산
    }
}
