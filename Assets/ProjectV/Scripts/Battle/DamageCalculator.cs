using UnityEngine; // Unity 수학 기능

public static class DamageCalculator // 공통 피해 계산 관리
{ // 클래스 시작
    private const int MinimumDamage = 1; // 최소 피해량

    public static int CalculateDamage(int attackPower, int defense) // 최종 피해 계산
    { // 메서드 시작
        int safeAttackPower = Mathf.Max(0, attackPower); // 음수 공격력 차단
        int safeDefense = Mathf.Max(0, defense); // 음수 방어력 차단

        if (safeAttackPower <= 0) // 공격력 없음 확인
        { // 조건문 시작
            return 0; // 피해 없음 반환
        } // 조건문 끝

        int calculatedDamage = safeAttackPower - safeDefense; // 공격력과 방어력 차이 계산
        return Mathf.Max(MinimumDamage, calculatedDamage); // 최소 피해 적용
    } // 메서드 끝
} // 클래스 끝