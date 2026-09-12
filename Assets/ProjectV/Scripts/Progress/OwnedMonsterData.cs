using System; // 직렬화 특성
using UnityEngine; // Unity 기본 기능

[Serializable]
public class OwnedMonsterData // 보유 마물 도감 및 성장 기록
{
    [SerializeField] private MonsterData monsterData; // 보유 마물

    // 기획서 6.9의 성장 수단은 강화 하나뿐이므로 보유 사본의 최고 강화 단계를 따른다.
    [SerializeField, Min(1)] private int enhanceLevel = 1;

    public MonsterData MonsterData => monsterData; // 보유 마물 반환

    public int Level =>
        CardEnhanceRules.ClampLevel(enhanceLevel); // 강화 단계 반환

    public int MaxHp =>
        monsterData == null
            ? 0
            : CardEnhanceRules.GetStatValue(
                monsterData.MaxHp,
                monsterData.HpGrowthPerLevel,
                Level
            ); // 강화 반영 최대 체력

    public int Attack =>
        monsterData == null
            ? 0
            : CardEnhanceRules.GetStatValue(
                monsterData.Attack,
                monsterData.AttackGrowthPerLevel,
                Level
            ); // 강화 반영 공격력

    public int LustDamage =>
        monsterData == null
            ? 0
            : CardEnhanceRules.GetStatValue(
                monsterData.LustDamage,
                monsterData.LustGrowthPerLevel,
                Level
            ); // 강화 반영 성욕 피해

    public int Defense =>
        monsterData == null
            ? 0
            : CardEnhanceRules.GetStatValue(
                monsterData.Defense,
                monsterData.DefenseGrowthPerLevel,
                Level
            ); // 강화 반영 방어력

    public OwnedMonsterData(MonsterData data)
    {
        monsterData = data;
        enhanceLevel = CardEnhanceRules.MinLevel;
    }

    public bool RaiseToLevel(int level) // 보유 사본의 최고 단계를 반영
    {
        int targetLevel = CardEnhanceRules.ClampLevel(level);

        if (targetLevel <= Level) { return false; } // 하향 갱신 차단

        enhanceLevel = targetLevel;

        return true;
    }
}
