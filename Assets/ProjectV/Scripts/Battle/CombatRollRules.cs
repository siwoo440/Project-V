using UnityEngine; // Unity 기본 기능

// 치명타와 회피 규칙 (기획서 5.12 / A.6 / A.7 / A.9 / A.10)
// 수치는 부록 A를 따른다. 본문 5.12.1에는 치명타 피해가 140%로 적혀 있지만 부록 A.6과 A.9는 150%다.
public static class CombatRollRules
{
    public const int BaseCritPercent = 10;    // 기본 치명타 확률
    public const int CritDamagePercent = 150; // 치명타 피해 배율
    public const int BaseEvasionPercent = 10; // 기본 회피 확률
    public const int MaxEvasionPercent = 75;  // 회피 확률의 상한

    public static int ClampCrit(int percent) // 치명타 확률은 0 ~ 100%
    {
        return Mathf.Clamp(percent, 0, 100);
    }

    public static int ClampEvasion(int percent) // 회피 확률은 0 ~ 75%
    {
        return Mathf.Clamp(percent, 0, MaxEvasionPercent);
    }

    public static bool Roll(int percent) // 확률 판정
    {
        return Random.Range(0, 100) < percent;
    }

    public static int ApplyCrit(int attackPower) // 치명타 피해: 기본 피해의 150%, 소수점은 버린다.
    {
        return Mathf.Max(0, attackPower) * CritDamagePercent / 100;
    }
}

// 공격 한 번의 판정 결과. 회피하면 피해와 딸린 효과가 모두 없다.
public readonly struct AttackRoll
{
    public readonly bool IsEvaded;   // 회피됐는지 여부
    public readonly bool IsCritical; // 치명타인지 여부
    public readonly int AttackPower; // 치명타를 반영한 공격 피해 (보호막과 방어력을 적용하기 전)

    public AttackRoll(bool isEvaded, bool isCritical, int attackPower)
    {
        IsEvaded = isEvaded;
        IsCritical = isCritical;
        AttackPower = attackPower;
    }
}
