using UnityEngine; // Unity 기본 기능

public static class PlayerLevelRules // 플레이어 레벨 규칙 (기획서 6.3 / A.41 / A.42)
{
    public const int StartLevel = 1; // 시작 레벨
    public const int MaxLevel = 30;  // 최대 레벨 (기획서 6.3.1)

    private const int BaseRequiredExperience = 100;  // Lv.1→2 필요 경험치
    private const int RequiredExperienceGrowth = 30; // 레벨마다 늘어나는 필요 경험치

    // 기획서 6.3.5 레벨업 해금 표
    private static readonly int[] UnlockLevels =
    {
        1, 2, 3, 5, 6, 10, 11, 14, 16, 20, 21, 25, 30,
    };

    private static readonly string[] UnlockDescriptions =
    {
        "기본 소환사 스킬 / 덱 편성",
        "패시브 시스템",
        "첫 패시브 포인트",
        "신규 액티브 스킬 / 상점 상품",
        "마물 강화 Lv.2",
        "신규 액티브·패시브 / 상점 상품",
        "마물 강화 Lv.3",
        "신규 액티브·패시브 / 상점 상품",
        "마물 강화 Lv.4",
        "신규 액티브·패시브 / 상점 상품",
        "마물 강화 Lv.5",
        "상위 액티브·패시브 / 상점 상품",
        "최종 액티브·패시브 / 최종 상점 상품",
    };

    // 마물 강화 단계가 해금되는 플레이어 레벨 (기획서 6.3.5 / 6.3.6)
    // 6.3.6 표에는 Lv.15가 빠져 있어 6.3.5의 해금 레벨(11, 16)을 기준으로 삼는다.
    private static readonly int[] EnhanceUnlockLevels =
    {
        1, 6, 11, 16, 21,
    }; // 마물 Lv.1, Lv.2, Lv.3, Lv.4, Lv.5

    public static int ClampLevel(int level) // 레벨 범위 보정
    {
        return Mathf.Clamp(level, StartLevel, MaxLevel);
    }

    public static bool IsMaxLevel(int level) // 최대 레벨 도달 여부
    {
        return level >= MaxLevel;
    }

    // 다음 레벨에 필요한 경험치 = 100 + 30 × (현재 레벨 - 1) (기획서 A.41)
    public static int GetRequiredExperience(int level)
    {
        int safeLevel = ClampLevel(level);

        if (IsMaxLevel(safeLevel)) { return 0; } // 최대 레벨은 다음 단계가 없다.

        return BaseRequiredExperience +
               RequiredExperienceGrowth * (safeLevel - StartLevel);
    }

    // 해당 레벨에 도달하기까지 필요한 누적 경험치 (기획서 A.42)
    public static int GetCumulativeExperience(int level)
    {
        int steps = ClampLevel(level) - StartLevel; // 지나온 레벨업 횟수

        return BaseRequiredExperience * steps +
               RequiredExperienceGrowth * steps * (steps - 1) / 2;
    }

    public static int MaxTotalExperience =>
        GetCumulativeExperience(MaxLevel); // 최대 누적 경험치 15,080

    public static int ClampTotalExperience(int totalExperience) // 누적 경험치 범위 보정
    {
        return Mathf.Clamp(totalExperience, 0, MaxTotalExperience);
    }

    public static int GetLevel(int totalExperience) // 누적 경험치로 레벨 계산
    {
        int safeExperience = ClampTotalExperience(totalExperience);
        int level = StartLevel;

        while (level < MaxLevel &&
               safeExperience >= GetCumulativeExperience(level + 1))
        {
            level += 1;
        }

        return level;
    }

    public static int GetExperienceIntoLevel(int totalExperience) // 현재 레벨에서 쌓은 경험치
    {
        int safeExperience = ClampTotalExperience(totalExperience);
        int level = GetLevel(safeExperience);

        if (IsMaxLevel(level)) { return 0; }

        return safeExperience - GetCumulativeExperience(level);
    }

    public static string GetLevelText(int totalExperience) // 레벨과 경험치 표시 문구
    {
        int level = GetLevel(totalExperience);

        if (IsMaxLevel(level)) { return $"Lv.{level} (최대)"; }

        return
            $"Lv.{level} " +
            $"({GetExperienceIntoLevel(totalExperience)} / " +
            $"{GetRequiredExperience(level)})";
    }

    public static int GetEnhanceCap(int playerLevel) // 마물 강화 상한 (기획서 6.3.6)
    {
        int safeLevel = ClampLevel(playerLevel);
        int enhanceCap = CardEnhanceRules.MinLevel;

        for (int i = 0; i < EnhanceUnlockLevels.Length; i++)
        {
            if (safeLevel < EnhanceUnlockLevels[i]) { break; }

            enhanceCap = CardEnhanceRules.MinLevel + i;
        }

        return CardEnhanceRules.ClampLevel(enhanceCap);
    }

    public static int GetEnhanceUnlockLevel(int enhanceLevel) // 해당 강화 단계가 열리는 플레이어 레벨
    {
        int index =
            CardEnhanceRules.ClampLevel(enhanceLevel) - CardEnhanceRules.MinLevel;

        return EnhanceUnlockLevels[
            Mathf.Clamp(index, 0, EnhanceUnlockLevels.Length - 1)
        ];
    }

    public static string GetUnlockDescription(int level) // 해당 레벨의 해금 내용 (없으면 빈 문자열)
    {
        for (int i = 0; i < UnlockLevels.Length; i++)
        {
            if (UnlockLevels[i] == level)
            {
                return UnlockDescriptions[i];
            }
        }

        return string.Empty;
    }

    // 레벨이 오른 구간(이전 레벨 초과 ~ 현재 레벨 이하)의 해금 내용을 줄 단위로 모은다.
    public static string GetUnlockSummary(int levelBefore, int levelAfter)
    {
        string summary = string.Empty;

        for (int level = levelBefore + 1; level <= levelAfter; level++)
        {
            string description = GetUnlockDescription(level);

            if (string.IsNullOrEmpty(description)) { continue; }

            if (!string.IsNullOrEmpty(summary)) { summary += "\n"; }

            summary += $"Lv.{level} 해금: {description}";
        }

        return summary;
    }
}
