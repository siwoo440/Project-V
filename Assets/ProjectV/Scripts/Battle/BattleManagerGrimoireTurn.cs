using UnityEngine; // Unity 기본 기능

public partial class BattleManager // 그리모어 강화: 내 턴 시작과 종료
{
    // 내 턴 시작. 마나를 채우고 턴 드로우를 마친 뒤에 호출한다.
    private void ApplyGrimoireTurnStart()
    {
        grimoireDeathShieldUsed = false; // 턴마다 한 번 쓰는 효과 초기화
        grimoireFirstLustUsed = false;
        grimoireDeathManaCount = 0;

        if (pendingTemporaryMana > 0)
        {
            currentMana += pendingTemporaryMana; // 마나 회수, 마나 비축

            AddBattleLog(
                BattleLogCategory.System,
                $"그리모어: 임시 마나 +{pendingTemporaryMana}"
            );

            pendingTemporaryMana = 0;
        }

        int turnShield = Grimoire(GrimoireEffectType.TurnShield);

        if (turnShield > 0 && playerCurrentShield <= 0)
        {
            playerCurrentShield += turnShield;

            AddBattleLog(
                BattleLogCategory.System,
                $"굳건한 서약: 플레이어 보호막 +{turnShield}"
            );
        }

        HealMostDamagedMonster(Grimoire(GrimoireEffectType.TurnHealMonster));

        int mixedHeal = Grimoire(GrimoireEffectType.MixedHeal);

        if (mixedHeal > 0 && synergyCounts.Count >= 3 && playerCurrentHp < PlayerMaxHp)
        {
            int previousHp = playerCurrentHp;

            playerCurrentHp = Mathf.Min(PlayerMaxHp, playerCurrentHp + mixedHeal);

            AddBattleLog(
                BattleLogCategory.System,
                $"혼성 편성: 플레이어 HP +{playerCurrentHp - previousHp}"
            );
        }

        int rewriteInterval = Grimoire(GrimoireEffectType.Rewrite);

        if (rewriteInterval > 0 && turnNumber % rewriteInterval == 0)
        {
            DrawCards(1);
            AddBattleLog(BattleLogCategory.System, "재기록: 카드 1장 추가 드로우");
        }

        int spareHandLimit = Grimoire(GrimoireEffectType.SpareMana);

        if (spareHandLimit > 0)
        {
            handButtons.RemoveAll(handButton => handButton == null);

            if (handButtons.Count <= spareHandLimit)
            {
                currentMana += 1;
                AddBattleLog(BattleLogCategory.System, "여분의 마력: 임시 마나 +1");
            }
        }
    }

    // 내 턴 종료. 여운으로 성욕이 가득 차서 전투가 끝나면 true를 돌려준다.
    private bool ApplyGrimoireTurnEnd()
    {
        int standbyLimit = Grimoire(GrimoireEffectType.StandbyOrder);

        if (standbyLimit > 0)
        {
            int readyCount = 0;

            foreach (MonsterUnit fieldMonster in fieldMonsters)
            {
                if (fieldMonster != null && fieldMonster.CanAttack) { readyCount += 1; }
            }

            int standbyShield = Mathf.Min(readyCount, standbyLimit);

            if (standbyShield > 0)
            {
                playerCurrentShield += standbyShield;

                AddBattleLog(
                    BattleLogCategory.System,
                    $"대기 명령: 플레이어 보호막 +{standbyShield}"
                );
            }
        }

        int reservedMana = Mathf.Min(
            Mathf.Max(0, currentMana),
            Grimoire(GrimoireEffectType.ManaReserve)
        );

        if (reservedMana > 0)
        {
            pendingTemporaryMana += reservedMana;

            AddBattleLog(
                BattleLogCategory.System,
                $"마나 비축: 다음 내 턴에 임시 마나 +{reservedMana}"
            );
        }

        int afterglow = Grimoire(GrimoireEffectType.Afterglow);

        if (afterglow <= 0 || heroineLust <= 0) { return false; }

        int appliedLust = AddHeroineLust(afterglow);

        if (appliedLust > 0)
        {
            AddBattleLog(
                BattleLogCategory.System,
                $"여운: 히로인 성욕 +{appliedLust}"
            );
        }

        if (heroineLust < heroineMaxLust) { return false; }

        UpdateBattleUI();
        EndBattle(BattleOutcome.VictoryLust); // 성욕 승리

        return true;
    }

    private void HealMostDamagedMonster(int healAmount) // 치유의 인장
    {
        if (healAmount <= 0) { return; }

        MonsterUnit targetMonster = null;
        int largestMissingHp = 0;

        foreach (MonsterUnit fieldMonster in fieldMonsters)
        {
            if (fieldMonster == null || fieldMonster.IsDead) { continue; }

            int missingHp = fieldMonster.MaxHp - fieldMonster.CurrentHp;

            if (missingHp <= largestMissingHp) { continue; }

            largestMissingHp = missingHp;
            targetMonster = fieldMonster;
        }

        if (targetMonster == null) { return; }

        int healedHp = targetMonster.Heal(healAmount);

        AddBattleLog(
            BattleLogCategory.System,
            $"치유의 인장: {targetMonster.MonsterName} HP +{healedHp}"
        );
    }
}
