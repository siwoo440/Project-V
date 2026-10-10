using UnityEngine; // Unity 기본 기능

// 전투 화면의 히로인 그림 연결 (기획서 11.8.1 / 12.4 / 12.5)
// 히로인이 공격을 받으면 피해의 종류와 크기에 맞는 피격 그림을 잠깐 보여 주고, 전투가 끝나면 패배 그림을 남긴다.
public partial class BattleManager
{
    private const int HeavyHitPercent = 10;            // 한 번에 최대 HP의 이 비율 이상을 잃으면 강한 피격
    private const float HeroineArtTurnPanelShift = -400f; // 그림이 위쪽 가운데를 쓸 때 턴 표시를 옮기는 거리

    [Header("히로인 그림")]
    [SerializeField] private HeroineArtUI heroineArtUI; // 히로인 그림 칸

    private bool IsHeroineArtShown => heroineArtUI != null && heroineArtUI.IsShown; // 그림을 보여 주는 전투인지 여부

    private bool IsHeroineShaken => heroineLust * 2 >= heroineMaxLust; // 동요 상태: 성욕이 절반 이상 (기획서 5.14)

    // 전투를 시작할 때 이번 히로인의 그림을 준비한다. 그림이 없는 히로인과 적 마물 전투에서는 칸을 숨긴다.
    private void PrepareHeroineArt()
    {
        if (heroineArtUI == null) { return; }

        if (isEnemyBattle || heroineBattle == null)
        {
            heroineArtUI.Hide();
        }
        else
        {
            heroineArtUI.Setup(heroineBattle.ArtKey);
        }

        PlaceTurnPanel(); // 그림이 위쪽 가운데를 쓰면 턴 표시를 왼쪽으로 옮긴다.
    }

    // 공격을 받은 결과에 맞는 그림: 보호막이 모두 막으면 방어, HP가 깎이면 피격
    private void ShowHeroineDamageArt(DamageResult damageResult)
    {
        if (!IsHeroineArtShown) { return; }

        if (damageResult.HpDamage > 0)
        {
            ShowHeroineHpLossArt(damageResult.HpDamage);
        }
        else if (damageResult.ShieldAbsorbed > 0)
        {
            heroineArtUI.PlayReaction(HeroineArtState.Guard);
        }
    }

    private void ShowHeroineHpLossArt(int hpDamage) // HP를 잃었을 때: 크게 잃으면 강한 피격
    {
        if (!IsHeroineArtShown || hpDamage <= 0) { return; }

        bool isHeavy = hpDamage * 100 >= heroineMaxHp * HeavyHitPercent;

        heroineArtUI.PlayReaction(isHeavy ? HeroineArtState.HeavyHit : HeroineArtState.Hit);
    }

    private void ShowHeroineLustArt(int previousLust) // 성욕이 올랐을 때: 이미 동요 상태였으면 더 강한 반응
    {
        if (!IsHeroineArtShown) { return; }

        bool wasShaken = previousLust * 2 >= heroineMaxLust;

        heroineArtUI.PlayReaction(wasShaken ? HeroineArtState.ShakenLustHit : HeroineArtState.LustHit);
    }

    private void RefreshHeroineArtIdle() // 평소 그림: 동요 상태면 동요 그림
    {
        if (IsHeroineArtShown) { heroineArtUI.SetIdle(IsHeroineShaken); }
    }

    private void ShowHeroineAttackArt(HeroineActionData actionData) // 히로인이 공격 행동을 할 때
    {
        if (!IsHeroineArtShown || actionData == null) { return; }

        if (actionData.ActionType == HeroineActionType.SingleAttack ||
            actionData.ActionType == HeroineActionType.AreaAttack)
        {
            heroineArtUI.PlayReaction(HeroineArtState.Attack);
        }
    }

    private void ShowHeroineResultArt(BattleOutcome outcome) // 전투가 끝난 뒤 남기는 그림
    {
        if (!IsHeroineArtShown) { return; }

        if (outcome == BattleOutcome.VictoryHp) { heroineArtUI.ShowFinal(HeroineArtState.Defeat); }
        if (outcome == BattleOutcome.VictoryLust) { heroineArtUI.ShowFinal(HeroineArtState.Climax); }
    }
}
