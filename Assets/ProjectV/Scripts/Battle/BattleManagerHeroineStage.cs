using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능

// 히로인전의 전투 데이터 적용 (기획서 D.2 / D.3 / D.4)
// 지역 화면에서 고른 히로인 전투의 능력치와 행동을 쓴다. 데이터가 없으면 전투 씬에 적힌 시험 히로인과 싸운다.
public partial class BattleManager
{
    [Header("히로인전")]
    [SerializeField] private TMP_Text heroineNameText; // 히로인 이름

    private HeroineBattleData heroineBattle;    // 이번 전투의 히로인 데이터 (없으면 시험 히로인)
    private int heroineAttack;                  // 난이도를 반영한 히로인 공격력
    private int heroineLustDamagePercent = 100; // 성욕 저항에 따른 성욕 피해 배율

    // 전투를 시작할 때 히로인의 능력치와 행동을 고른 전투의 값으로 바꾼다. 난이도는 HP와 공격력에만 적용한다.
    private void PrepareHeroineBattle()
    {
        heroineBattle = BattleSetup.IsEnemyBattle ? null : BattleSetup.Heroine;
        heroineAttack = 0;
        heroineLustDamagePercent = 100;

        if (heroineBattle == null) { return; }

        BattleDifficulty difficulty = BattleSetup.Difficulty;

        heroineMaxHp = HeroineBattleRules.GetMaxHp(heroineBattle, difficulty);
        heroineAttack = HeroineBattleRules.GetAttack(heroineBattle, difficulty);
        heroineDefense = heroineBattle.Defense;
        heroineMaxShield = heroineBattle.MaxShield;
        heroineStartingShield = heroineBattle.StartingShield;

        heroineLustDamagePercent =
            HeroineBattleRules.GetLustDamagePercent(heroineBattle.LustResistance);

        heroineActions.Clear();
        heroineActions.AddRange(heroineBattle.Actions);

        heroineNameText = SceneUIBinder.Bind(heroineNameText, "HeroineNameText");

        if (heroineNameText != null)
        {
            heroineNameText.text = heroineBattle.HeroineName;
        }
    }

    private void LogHeroineBattle() // 전투 로그에 상대와 난이도를 남긴다.
    {
        if (heroineBattle == null) { return; }

        AddBattleLog(
            BattleLogCategory.System,
            $"{BattleSetup.StageTitle} [{StageRules.GetDifficultyName(BattleSetup.Difficulty)}]: " +
            $"HP {heroineMaxHp}, 공격 {heroineAttack}, 방어 {heroineDefense}, " +
            $"성욕 저항 {HeroineBattleRules.GetLustResistanceName(heroineBattle.LustResistance)}"
        );
    }

    // 행동의 기본 피해량. 공격력 배율이 있는 행동은 히로인 공격력에 곱하고 반올림한다. (기획서 D.4)
    private int GetHeroineActionDamage(HeroineActionData actionData)
    {
        if (actionData == null) { return 0; }

        if (heroineBattle == null || actionData.AttackPercent <= 0) { return actionData.Damage; }

        int attackPercent = actionData.AttackPercent;

        if (isHeroinePhaseActive && heroineBattle.GetPhaseAttackPercent(actionData) > 0)
        {
            attackPercent = heroineBattle.GetPhaseAttackPercent(actionData); // 페이즈 전환으로 바뀐 배율
        }

        return HeroineBattleRules.ScalePercent(heroineAttack, attackPercent);
    }

    // 공격하면서 보호막도 얻는 행동의 보호막 처리 (방패 전진 등)
    private void ApplyHeroineAttackShield()
    {
        if (nextHeroineAction == null || nextHeroineAction.ShieldAmount <= 0) { return; }

        int previousShield = heroineCurrentShield;

        heroineCurrentShield = Mathf.Min(
            heroineMaxShield,
            heroineCurrentShield + nextHeroineAction.ShieldAmount
        );

        int gainedShield = heroineCurrentShield - previousShield;

        if (gainedShield <= 0) { return; }

        resultText.text += $" / 보호막 +{gainedShield}";

        AddBattleLog(
            BattleLogCategory.HeroineAction,
            $"{nextHeroineAction.DisplayName}: 히로인 보호막 +{gainedShield}"
        );
    }

    private int ReduceHeroineLust(int amount) // 히로인이 스스로 성욕을 낮춘다. 실제로 줄어든 양을 반환한다.
    {
        int reducedAmount = Mathf.Clamp(amount, 0, heroineLust);

        heroineLust -= reducedAmount;
        UpdateHeroineClimaxState(); // 성욕이 최대에서 내려오면 절정 상태에서 벗어난다.

        return reducedAmount;
    }
}
