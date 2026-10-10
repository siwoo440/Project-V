using System.Collections.Generic; // 딕셔너리 기능
using UnityEngine; // Unity 기본 기능

// 히로인 그림 불러오기 (기획서 12.14 / 12.16)
// 그림은 Resources/Characters/<이름>/<이름>_Battle_<상태>.png에 두고, 전투에 나온 히로인의 그림만 불러온다.
// 없는 상태는 비슷한 상태의 그림으로 대신한다. 기본 자세도 없으면 그림을 쓰지 않는다.
public static class HeroineArtLibrary
{
    private static readonly Dictionary<string, Sprite> cache =
        new Dictionary<string, Sprite>(); // 불러온 그림 (없는 그림은 null로 기억한다)

    private static string GetFileState(HeroineArtState state) // 파일 이름의 상태 부분
    {
        switch (state)
        {
            case HeroineArtState.Hit: return "Hit_01";
            case HeroineArtState.HeavyHit: return "Hit_02";
            case HeroineArtState.Guard: return "Guard_01";
            case HeroineArtState.LustHit: return "LustHit_01";
            case HeroineArtState.Shaken: return "Shaken_01";
            case HeroineArtState.ShakenLustHit: return "LustHit_02";
            case HeroineArtState.Defeat: return "Defeat_01";
            case HeroineArtState.Climax: return "Climax_01";
            case HeroineArtState.Attack: return "Attack_01";
            default: return "Normal_01";
        }
    }

    private static HeroineArtState GetFallback(HeroineArtState state) // 그림이 없을 때 대신 쓰는 상태
    {
        switch (state)
        {
            case HeroineArtState.HeavyHit: return HeroineArtState.Hit;
            case HeroineArtState.LustHit: return HeroineArtState.Hit;
            case HeroineArtState.ShakenLustHit: return HeroineArtState.LustHit;
            case HeroineArtState.Defeat: return HeroineArtState.HeavyHit;
            case HeroineArtState.Climax: return HeroineArtState.ShakenLustHit;
            default: return HeroineArtState.Normal; // 가벼운 피격, 방어 성공, 동요 상태, 공격
        }
    }

    private static Sprite LoadExact(string artKey, HeroineArtState state)
    {
        string path = $"Characters/{artKey}/{artKey}_Battle_{GetFileState(state)}";

        if (cache.TryGetValue(path, out Sprite cached)) { return cached; }

        Sprite sprite = Resources.Load<Sprite>(path);

        cache[path] = sprite;

        return sprite;
    }

    public static bool Has(string artKey) // 기본 자세 그림이 있는 히로인인지 여부
    {
        return !string.IsNullOrEmpty(artKey) && LoadExact(artKey, HeroineArtState.Normal) != null;
    }

    // 그 상태의 그림을 돌려준다. 없으면 대신 쓰는 상태를 차례로 찾고, 끝내 없으면 null이다.
    public static Sprite Get(string artKey, HeroineArtState state)
    {
        if (string.IsNullOrEmpty(artKey)) { return null; }

        while (true)
        {
            Sprite sprite = LoadExact(artKey, state);

            if (sprite != null || state == HeroineArtState.Normal) { return sprite; }

            state = GetFallback(state);
        }
    }
}
