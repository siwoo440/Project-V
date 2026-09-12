using UnityEngine; // Unity 기본 기능

public static class MonsterTypeRules // 마물 타입 규칙 정의
{
    public static Color GetDisplayColor(MonsterType monsterType) // 타입 표시 색상
    {
        switch (monsterType)
        {
            case MonsterType.Tentacle:
                return new Color(0.85f, 0.25f, 0.65f, 1f); // 자홍색

            case MonsterType.Slime:
                return new Color(0.56f, 0.86f, 0.36f, 1f); // 연두색

            case MonsterType.Goblin:
                return new Color(0.58f, 0.60f, 0.28f, 1f); // 올리브색

            case MonsterType.Demon:
                return new Color(0.82f, 0.20f, 0.24f, 1f); // 진홍색

            case MonsterType.Undead:
                return new Color(0.53f, 0.62f, 0.69f, 1f); // 청회색

            case MonsterType.Beast:
                return new Color(0.95f, 0.55f, 0.20f, 1f); // 주황색

            case MonsterType.Spirit:
                return new Color(0.28f, 0.78f, 0.75f, 1f); // 청록색

            case MonsterType.Machine:
                return new Color(0.66f, 0.70f, 0.74f, 1f); // 강철색

            case MonsterType.Angel:
                return new Color(1f, 0.96f, 0.80f, 1f); // 금백색

            default:
                return Color.white;
        }
    }

    public static string GetDisplayName(MonsterType monsterType) // 타입 표시 이름
    {
        switch (monsterType)
        {
            case MonsterType.Tentacle:
                return "촉수";

            case MonsterType.Slime:
                return "슬라임";

            case MonsterType.Goblin:
                return "고블린";

            case MonsterType.Demon:
                return "악마";

            case MonsterType.Undead:
                return "언데드";

            case MonsterType.Beast:
                return "괴수";

            case MonsterType.Spirit:
                return "정령";

            case MonsterType.Machine:
                return "기계";

            case MonsterType.Angel:
                return "천사";

            default:
                return "없음";
        }
    }
}
