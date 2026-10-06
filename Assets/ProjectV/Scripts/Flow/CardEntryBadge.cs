using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 카드 위에 올라가는 배지(비용, 강화 단계)와 능력치 문구
public static partial class CardEntryFactory
{
    // 왼쪽 위 마나 비용. 마나 보석 이미지가 있으면 그 위에 숫자를 올린다.
    private static void BuildCostBadge(
        Transform face,
        int shownCost,
        bool isCostReduced,
        float scale,
        float dimRate
    )
    {
        float size = 48f * scale;
        float inset = 7f * scale;

        Vector2 corner = new Vector2(0f, 1f);
        Vector2 offsetMin = new Vector2(inset, -inset - size);
        Vector2 offsetMax = new Vector2(inset + size, -inset);

        Image gem = CreateImage(
            face, "ManaBadge", Dim(BadgeColor, 1f),
            corner, corner, offsetMin, offsetMax
        );

        bool hasGem = UISkin.ApplySimple(gem, "Icon_Stat_Mana_01", true);

        if (hasGem)
        {
            gem.color = new Color(dimRate, dimRate, dimRate, 1f);
        }

        Color costColor = isCostReduced
            ? ReducedCostColor
            : new Color(dimRate, dimRate, dimRate, 1f);

        float fontSize = Mathf.Max(15f, 23f * scale);
        float shadowShift = Mathf.Max(1f, 1.5f * scale);

        if (hasGem)
        {
            // 밝은 보석 위에서도 숫자가 읽히도록 어두운 그림자 글자를 먼저 깐다.
            TextMeshProUGUI shadow = CreateLabel(
                face, "ManaShadow", shownCost.ToString(),
                fontSize, new Color(0.04f, 0.07f, 0.2f, 0.9f), TextAlignmentOptions.Center,
                corner, corner,
                offsetMin + new Vector2(shadowShift, -shadowShift),
                offsetMax + new Vector2(shadowShift, -shadowShift)
            );

            shadow.fontStyle = FontStyles.Bold;
        }

        TextMeshProUGUI costLabel = CreateLabel(
            face, "ManaText", shownCost.ToString(),
            fontSize, costColor, TextAlignmentOptions.Center,
            corner, corner, offsetMin, offsetMax
        );

        costLabel.fontStyle = FontStyles.Bold;
    }

    // 오른쪽 위 강화 단계
    private static void BuildLevelBadge(
        Transform face,
        int enhanceLevel,
        Color levelColor,
        float scale
    )
    {
        float width = 62f * scale;
        float height = 28f * scale;
        float inset = 12f * scale;

        Vector2 corner = new Vector2(1f, 1f);
        Vector2 offsetMin = new Vector2(-inset - width, -inset - height);
        Vector2 offsetMax = new Vector2(-inset, -inset);

        Image plate = CreateImage(
            face, "LevelBadge", Dim(BadgeColor, 1f),
            corner, corner, offsetMin, offsetMax
        );

        UISkin.ApplyBar(plate, UIKeys.PlateLabel);

        CreateLabel(
            face, "LevelText", $"Lv.{enhanceLevel}",
            Mathf.Max(11f, 16f * scale), levelColor, TextAlignmentOptions.Center,
            corner, corner, offsetMin, offsetMax
        );
    }

    // 강화 단계를 반영한 능력치 두 줄. 아이콘이 없으면 글자 이름표를 쓴다.
    public static string BuildStatText(MonsterData monsterData, int enhanceLevel)
    {
        if (monsterData == null) { return string.Empty; }

        int maxHp = CardEnhanceRules.GetStatValue(
            monsterData.MaxHp, monsterData.HpGrowthPerLevel, enhanceLevel);

        int attack = CardEnhanceRules.GetStatValue(
            monsterData.Attack, monsterData.AttackGrowthPerLevel, enhanceLevel);

        int defense = CardEnhanceRules.GetStatValue(
            monsterData.Defense, monsterData.DefenseGrowthPerLevel, enhanceLevel);

        int lustDamage = CardEnhanceRules.GetStatValue(
            monsterData.LustDamage, monsterData.LustGrowthPerLevel, enhanceLevel);

        return
            $"{UISkin.IconOr(UIIcons.Hp, "체력")} {maxHp}   " +
            $"{UISkin.IconOr(UIIcons.Attack, "공격")} {attack}\n" +
            $"{UISkin.IconOr(UIIcons.Defense, "방어")} {defense}   " +
            $"{UISkin.IconOr(UIIcons.Lust, "성욕")} {lustDamage}";
    }
}
