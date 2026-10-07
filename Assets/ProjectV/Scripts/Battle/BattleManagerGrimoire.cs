using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 그리모어 영구 강화의 전투 적용 (기획서 6.6)
// 기획서 6.5.4 적용 순서: 기본 능력치 → 그리모어 강화 → 장착 패시브
public partial class BattleManager
{
    private readonly Dictionary<GrimoireEffectType, int> grimoireAmounts =
        new Dictionary<GrimoireEffectType, int>(); // 전투를 시작할 때 읽은 효과별 수치

    private int grimoireTotalLevel;        // 이번 전투에 적용된 강화 단계 합
    private bool grimoireRegenUsed;        // 재생 계약 사용 여부 (전투당 1회)
    private bool grimoireLastStandUsed;    // 최후의 계약 사용 여부 (전투당 1회)
    private bool grimoireVanguardUsed;     // 선봉의 축복 사용 여부 (전투당 1회)
    private bool grimoireCheapCardUsed;    // 저비용 계약 사용 여부 (전투당 1회)
    private bool grimoireDeathShieldUsed;  // 죽음의 기록 사용 여부 (턴당 1회)
    private bool grimoireFirstLustUsed;    // 첫 유혹 사용 여부 (턴당 1회)
    private int grimoireDeathManaCount;    // 이번 턴 마나 회수 횟수
    private int grimoireFarewellDrawCount; // 잔류 기억 드로우 횟수
    private int grimoireSkillDrawCount;    // 지휘의 기억 드로우 횟수
    private int grimoireSynergyDrawCount;  // 계열 각성 드로우 횟수
    private int pendingTemporaryMana;      // 다음 내 턴에 받을 임시 마나

    private int Grimoire(GrimoireEffectType effectType) // 효과 수치 (찍지 않았으면 0)
    {
        return grimoireAmounts.TryGetValue(effectType, out int amount)
            ? amount
            : 0;
    }

    // 전투 시작 시 그리모어 강화 수치를 읽는다. 전투 도중에는 바뀌지 않는다.
    private void PrepareGrimoireForBattle()
    {
        grimoireAmounts.Clear();
        grimoireTotalLevel = 0;
        grimoireRegenUsed = false;
        grimoireLastStandUsed = false;
        grimoireVanguardUsed = false;
        grimoireCheapCardUsed = false;
        grimoireDeathShieldUsed = false;
        grimoireFirstLustUsed = false;
        grimoireDeathManaCount = 0;
        grimoireFarewellDrawCount = 0;
        grimoireSkillDrawCount = 0;
        grimoireSynergyDrawCount = 0;
        pendingTemporaryMana = 0;

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; } // 단독 전투

        foreach (GrimoireNodeData node in progress.GrimoireNodes)
        {
            if (node == null) { continue; }

            int level = progress.GetGrimoireLevel(node);

            if (level <= 0) { continue; }

            grimoireAmounts.TryGetValue(node.EffectType, out int currentAmount);
            grimoireAmounts[node.EffectType] = currentAmount + node.GetAmount(level);
            grimoireTotalLevel += level;
        }
    }

    private void LogGrimoireLoadout() // 전투 시작 시 적용된 강화 기록
    {
        if (grimoireTotalLevel <= 0) { return; }

        AddBattleLog(
            BattleLogCategory.System,
            $"그리모어 강화 {grimoireTotalLevel}단계 적용"
        );
    }

    // ---------- 능력치 보정 ----------

    private bool IsMainTypeMonster(MonsterUnit monsterUnit) // 주력 계열(덱에 가장 많은 계열) 마물 여부
    {
        return monsterUnit != null &&
               monsterUnit.Data != null &&
               passiveFocusType != MonsterType.None &&
               monsterUnit.Data.HasType(passiveFocusType);
    }

    // 공격 명령, 군단 지휘, 결전 명령의 공격 보정
    private int GetGrimoireAttackBonus(int livingMonsterCount)
    {
        int attackBonus = Grimoire(GrimoireEffectType.AttackOrder);

        if (livingMonsterCount >= GrimoireRules.LegionMonsterCount)
        {
            attackBonus += Grimoire(GrimoireEffectType.LegionOrder);
        }

        if (heroineCurrentHp * 2 <= heroineMaxHp)
        {
            attackBonus += Grimoire(GrimoireEffectType.FinishOrder);
        }

        return attackBonus;
    }

    // 욕망 공명, 절정 추적의 성욕 보정. 성욕이 절반 이상이면 동요 상태다. (기획서 5.14)
    private int GetGrimoireLustBonus()
    {
        int lustBonus = Grimoire(GrimoireEffectType.LustResonance);

        if (heroineLust * 2 >= heroineMaxLust)
        {
            lustBonus += Grimoire(GrimoireEffectType.ClimaxChase);
        }

        return lustBonus;
    }

    private int GetGrimoireSkillBonus(SummonerSkillType skillType) // 신속 명령: 수치형 소환사 스킬 강화
    {
        switch (skillType)
        {
            case SummonerSkillType.FocusCommand:
            case SummonerSkillType.ContractShield:
            case SummonerSkillType.LustResonance:
                return Grimoire(GrimoireEffectType.SwiftOrder);

            default:
                return 0;
        }
    }

    private int GetGrimoireCardDiscount(CardCopy cardCopy) // 저비용 계약: 전투마다 처음 내는 카드
    {
        if (grimoireCheapCardUsed || cardCopy == null) { return 0; }
        if (cardCopy.ManaCost <= 0) { return 0; } // 비용이 없는 카드에는 쓰지 않는다.

        return Mathf.Min(
            cardCopy.ManaCost,
            Grimoire(GrimoireEffectType.CheapContract)
        );
    }

    private int ConsumeGrimoireFirstLustBonus() // 첫 유혹: 턴마다 처음 하는 성욕 공격
    {
        if (grimoireFirstLustUsed) { return 0; }

        grimoireFirstLustUsed = true;

        return Grimoire(GrimoireEffectType.FirstTemptation);
    }

    // 전리품 감정, 전투 기록: 승리 보상을 비율로 늘린다.
    private int ApplyGrimoirePercent(int baseValue, GrimoireEffectType effectType)
    {
        int percent = Grimoire(effectType);

        if (percent <= 0 || baseValue <= 0) { return baseValue; }

        return Mathf.RoundToInt(baseValue * (100 + percent) / 100f);
    }
}
