// 게임 데이터 종류를 테마 이미지 이름으로 바꾼다.
public static partial class UISkin
{
    public static string CardFrameKey(CardRarity rarity) // 희귀도별 카드 틀 (기획서 11.4.2)
    {
        switch (rarity)
        {
            case CardRarity.Rare: return UIKeys.CardFrameRare;
            case CardRarity.Special: return UIKeys.CardFrameSpecial;
            case CardRarity.Legendary: return UIKeys.CardFrameLegendary;
            default: return UIKeys.CardFrameCommon;
        }
    }

    private static string TypeName(MonsterType monsterType)
    {
        switch (monsterType)
        {
            case MonsterType.Tentacle: return "Tentacle";
            case MonsterType.Slime: return "Slime";
            case MonsterType.Goblin: return "Goblin";
            case MonsterType.Demon: return "Demon";
            case MonsterType.Undead: return "Undead";
            case MonsterType.Beast: return "Beast";
            case MonsterType.Spirit: return "Spirit";
            case MonsterType.Machine: return "Machine";
            case MonsterType.Angel: return "Angel";
            default: return string.Empty;
        }
    }

    public static string TypeIconKey(MonsterType monsterType) // 계열 문양 이미지 (기획서 11.4.3)
    {
        string typeName = TypeName(monsterType);

        return string.IsNullOrEmpty(typeName)
            ? string.Empty
            : $"Icon_Type_{typeName}_01";
    }

    public static string TypeIconName(MonsterType monsterType) // 글자 사이 계열 아이콘
    {
        string typeName = TypeName(monsterType);

        return string.IsNullOrEmpty(typeName)
            ? string.Empty
            : $"t_{typeName.ToLowerInvariant()}";
    }

    public static string ActionIconKey(HeroineActionType actionType) // 히로인 행동 예고 아이콘
    {
        switch (actionType)
        {
            case HeroineActionType.SingleAttack: return "Icon_Action_Single_01";
            case HeroineActionType.AreaAttack: return "Icon_Action_Area_01";
            case HeroineActionType.GainShield: return "Icon_Action_Shield_01";
            case HeroineActionType.Heal: return "Icon_Action_Heal_01";
            case HeroineActionType.ApplyStatus: return "Icon_Action_Status_01";
            case HeroineActionType.Cleanse: return "Icon_Action_Cleanse_01";
            default: return string.Empty;
        }
    }

    public static string StatusIconKey(StatusEffectType statusType) // 상태 효과 아이콘 (기획서 12.16)
    {
        switch (statusType)
        {
            case StatusEffectType.AttackUp: return "Status_AttackUp_01";
            case StatusEffectType.AttackDown: return "Status_AttackDown_01";
            case StatusEffectType.DefenseUp: return "Status_DefenseUp_01";
            case StatusEffectType.DefenseDown: return "Status_DefenseDown_01";
            case StatusEffectType.Poison: return "Status_Poison_01";
            default: return string.Empty;
        }
    }

    public static string StateIconName(MonsterActionState actionState) // 마물 행동 상태 아이콘
    {
        switch (actionState)
        {
            case MonsterActionState.Summoning: return UIIcons.StateWait;
            case MonsterActionState.Ready: return UIIcons.StateReady;
            case MonsterActionState.Acted: return UIIcons.StateDone;
            default: return string.Empty;
        }
    }

    public static string StageIconKey(string stageType) // 전투 종류 아이콘
    {
        if (string.IsNullOrEmpty(stageType)) { return UIKeys.StageNormal; }
        if (stageType.Contains("포획")) { return UIKeys.StageCapture; }
        if (stageType.Contains("히로인")) { return UIKeys.StageHeroine; }

        return UIKeys.StageNormal;
    }
}
