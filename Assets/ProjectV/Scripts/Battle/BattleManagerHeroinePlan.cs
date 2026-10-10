using System.Collections.Generic; // 리스트 기능
using System.Text; // 문자열 조립 기능
using UnityEngine; // Unity 기본 기능

// 히로인의 한 턴 행동 계획 (기획서 D.2.3 / D.2.4)
// 전투 단계에 따라 한 턴에 여러 번 행동하고, 행동은 내 턴이 시작될 때 모두 예고한다.
public partial class BattleManager
{
    private readonly List<HeroineActionData> plannedHeroineActions =
        new List<HeroineActionData>(); // 다음 히로인 턴에 예고한 행동 (앞에서부터 실행)

    private int heroineTurnsTaken; // 지금까지 진행한 히로인 턴 수

    private int GetHeroineActionCount() // 다음 히로인 턴의 행동 횟수
    {
        if (heroineBattle == null) { return 1; } // 시험 히로인은 한 번 행동한다.

        int count = heroineBattle.ActionsPerTurn;
        int interval = heroineBattle.ExtraActionInterval;

        if (interval > 0 && (heroineTurnsTaken + 1) % interval == 0)
        {
            count += 1; // 2차전은 세 번째 턴마다, 서브 히로인전은 네 번째 턴마다 한 번 더 행동한다.
        }

        return count;
    }

    // 다음 히로인 턴의 행동을 정한다. 기본 공격이 아닌 행동은 한 턴에 한 번만 고른다.
    private void PlanHeroineActions()
    {
        plannedHeroineActions.Clear();

        int actionCount = GetHeroineActionCount();

        for (int i = 0; i < actionCount; i++)
        {
            SelectNextHeroineAction();

            if (nextHeroineAction == null) { break; }

            plannedHeroineActions.Add(nextHeroineAction);
        }

        nextHeroineAction = plannedHeroineActions.Count > 0
            ? plannedHeroineActions[0]
            : null;
    }

    private bool IsAlreadyPlanned(HeroineActionData actionData) // 이번 턴에 이미 고른 행동인지 여부 (기본 공격은 겹쳐도 된다)
    {
        return actionData.CooldownTurns > 0 && plannedHeroineActions.Contains(actionData);
    }

    // 상황에 따라 보정한 행동 가중치. 0이면 고르지 않는다. (기획서 D.2.4의 공통 보정)
    private int GetHeroineActionWeight(HeroineActionData actionData)
    {
        int weight = actionData.Weight;

        if (heroineBattle == null) { return weight; } // 시험 히로인은 데이터의 가중치만 쓴다.

        bool isProtectAction =
            actionData.ActionType == HeroineActionType.Heal ||
            actionData.ActionType == HeroineActionType.GainShield;

        int monsterCount = GetLivingMonsterCandidates().Count;

        if (heroineCurrentHp * 2 <= heroineMaxHp && isProtectAction) { weight += 20; } // HP 절반 이하: 회복과 보호
        if (heroineCurrentShield <= 0 && actionData.ShieldAmount > 0) { weight += 10; } // 보호막 없음: 보호막 행동

        if (actionData.LustReduction > 0)
        {
            if (heroineLust * 100 >= heroineMaxLust * 50) { weight += 20; } // 성욕 50 이상: 성욕 감소
            if (heroineLust * 100 >= heroineMaxLust * 80) { weight += 20; } // 성욕 80 이상: 추가
        }

        if (monsterCount >= 5 && actionData.ActionType == HeroineActionType.AreaAttack) { weight += 20; } // 마물 5체 이상: 광역
        if (monsterCount <= 1 && actionData.TargetType == HeroineTargetType.Player) { weight += 15; } // 마물 1체 이하: 플레이어 대상

        if (isHeroinePhaseActive)
        {
            weight += heroineBattle.GetPhaseWeightBonus(actionData); // 페이즈 전환으로 달라진 가중치
        }

        return Mathf.Max(0, weight);
    }

    // 여러 번 행동하는 턴의 예고 문구. 지금 차례인 행동에는 대상을 함께 적는다.
    private string GetHeroinePlanText()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append($"다음 행동 {plannedHeroineActions.Count}회");

        for (int i = 0; i < plannedHeroineActions.Count; i++)
        {
            HeroineActionData actionData = plannedHeroineActions[i];

            builder.Append(
                $"\n{i + 1}. {actionData.DisplayName}: {GetHeroineActionEffectDisplay(actionData)}"
            );
        }

        builder.Append($"\n대상: {GetHeroineTargetPreviewText()}");

        return builder.ToString();
    }
}
