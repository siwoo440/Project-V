public readonly struct DamageResult // 피해 계산 결과
{ // 구조체 시작
    public int IncomingAttack { get; } // 최초 공격력
    public int ShieldAbsorbed { get; } // 보호막 흡수량
    public int HpDamage { get; } // 실제 HP 피해량
    public int RemainingShield { get; } // 피해 후 남은 보호막

    public DamageResult(int incomingAttack, int shieldAbsorbed, int hpDamage, int remainingShield) // 피해 결과 생성
    { // 생성자 시작
        IncomingAttack = incomingAttack; // 최초 공격력 저장
        ShieldAbsorbed = shieldAbsorbed; // 보호막 흡수량 저장
        HpDamage = hpDamage; // HP 피해량 저장
        RemainingShield = remainingShield; // 남은 보호막 저장
    } // 생성자 끝
} // 구조체 끝
