using UnityEngine; // Unity 기본 기능

public partial class BattleManager // 그리모어 강화: 소환, 사망, 피해에 발동하는 효과
{
    // 마물을 소환한 직후: 최대 HP와 보호막을 더한다.
    private void ApplyGrimoireSummonBonus(MonsterUnit summonedMonster)
    {
        if (summonedMonster == null) { return; }

        int maxHpBonus = Grimoire(GrimoireEffectType.SummonMaxHp); // 생명 공명

        if (IsMainTypeMonster(summonedMonster))
        {
            maxHpBonus += Grimoire(GrimoireEffectType.MainTypeHp); // 계열 결속
        }

        if (!grimoireVanguardUsed)
        {
            grimoireVanguardUsed = true;
            maxHpBonus += Grimoire(GrimoireEffectType.VanguardHp); // 선봉의 축복
        }

        summonedMonster.IncreaseMaxHp(maxHpBonus);
        summonedMonster.AddShield(Grimoire(GrimoireEffectType.SummonShield)); // 안정된 소환
    }

    // 아군 마물이 쓰러진 직후
    private void ApplyGrimoireMonsterDefeated()
    {
        int shieldAmount = Grimoire(GrimoireEffectType.DeathShield);

        if (shieldAmount > 0 && !grimoireDeathShieldUsed)
        {
            grimoireDeathShieldUsed = true;
            playerCurrentShield += shieldAmount;

            AddBattleLog(
                BattleLogCategory.System,
                $"죽음의 기록: 플레이어 보호막 +{shieldAmount}"
            );
        }

        if (grimoireDeathManaCount < Grimoire(GrimoireEffectType.ManaRecovery))
        {
            grimoireDeathManaCount += 1;
            pendingTemporaryMana += 1;

            AddBattleLog(
                BattleLogCategory.System,
                "마나 회수: 다음 내 턴에 임시 마나 +1"
            );
        }

        if (grimoireFarewellDrawCount < Grimoire(GrimoireEffectType.FarewellDraw))
        {
            grimoireFarewellDrawCount += 1;
            DrawCards(1);

            AddBattleLog(
                BattleLogCategory.System,
                "잔류 기억: 카드 1장 드로우"
            );
        }
    }

    // 플레이어가 HP 피해를 받은 직후: 최후의 계약, 재생 계약
    private void ApplyGrimoirePlayerDamageReactions()
    {
        int lastStandHp = Grimoire(GrimoireEffectType.LastStand);

        if (playerCurrentHp <= 0 && lastStandHp > 0 && !grimoireLastStandUsed)
        {
            grimoireLastStandUsed = true;
            playerCurrentHp = Mathf.Min(PlayerMaxHp, lastStandHp);

            AddBattleLog(
                BattleLogCategory.System,
                $"최후의 계약: 쓰러지지 않고 버텼습니다 (HP {playerCurrentHp})"
            );
        }

        int regenAmount = Grimoire(GrimoireEffectType.RegenContract);

        bool isBelowHalf =
            playerCurrentHp > 0 && playerCurrentHp * 2 <= PlayerMaxHp;

        if (isBelowHalf && regenAmount > 0 && !grimoireRegenUsed)
        {
            int previousHp = playerCurrentHp;

            grimoireRegenUsed = true;
            playerCurrentHp = Mathf.Min(PlayerMaxHp, playerCurrentHp + regenAmount);

            AddBattleLog(
                BattleLogCategory.System,
                $"재생 계약: 플레이어 HP +{playerCurrentHp - previousHp}"
            );
        }
    }

    private void ApplyGrimoireSkillDraw() // 지휘의 기억: 소환사 스킬을 쓴 직후
    {
        if (grimoireSkillDrawCount >= Grimoire(GrimoireEffectType.SkillDraw)) { return; }

        grimoireSkillDrawCount += 1;
        DrawCards(1);

        AddBattleLog(BattleLogCategory.System, "지휘의 기억: 카드 1장 드로우");
    }

    private void ApplyGrimoireSynergyDraw(int activatedCount) // 계열 각성: 시너지 단계가 새로 켜진 직후
    {
        for (int i = 0; i < activatedCount; i++)
        {
            if (grimoireSynergyDrawCount >= Grimoire(GrimoireEffectType.SynergyDraw)) { return; }

            grimoireSynergyDrawCount += 1;
            DrawCards(1);

            AddBattleLog(BattleLogCategory.System, "계열 각성: 카드 1장 드로우");
        }
    }
}
