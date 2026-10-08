using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class WorldMapFlow // 월드맵 지역 표시의 NEW 배지와 이름판
{
    private const float NameWidth = 180f; // 이름판 너비
    private const float NameHeight = 30f; // 이름판 높이

    private static readonly Color NamePlateColor = new Color(0.06f, 0.07f, 0.14f, 0.86f); // 이름판 이미지가 없을 때 색

    private static void CreateNewBadge(Transform parent) // 새로 열린 지역의 NEW 표시
    {
        Vector2 middle = new Vector2(0.5f, 0.5f);
        Vector2 center = new Vector2(36f, 36f);

        Image badge = CreateMark(parent, "NewBadge", BadgeSize + 8f, center);

        bool hasBadge = UISkin.ApplySimple(badge, UIKeys.MapMarkNew, true);

        badge.enabled = hasBadge;

        TextMeshProUGUI newLabel = CardEntryFactory.CreateLabel(
            parent, "NewText", "NEW",
            14f, hasBadge ? Color.white : UISkin.Gold, TextAlignmentOptions.Center,
            middle, middle,
            center - new Vector2(30f, 12f), center + new Vector2(30f, 12f)
        );

        newLabel.fontStyle = FontStyles.Bold;
    }

    private static void CreateNamePlate(
        Transform parent,
        RegionData region,
        bool isSelected,
        bool isLocked
    )
    {
        Vector2 middle = new Vector2(0.5f, 0.5f);
        float top = -NodeSize * 0.5f - 2f;

        Image namePlate = CardEntryFactory.CreateImage(
            parent, "NamePlate", Color.white, middle, middle,
            new Vector2(-NameWidth * 0.5f, top - NameHeight),
            new Vector2(NameWidth * 0.5f, top)
        );

        bool isLightPlate =
            isSelected && UISkin.ApplyBar(namePlate, UIKeys.PlateName); // 고른 지역은 밝은 이름판

        if (!isLightPlate && !UISkin.ApplyBar(namePlate, UIKeys.PlateLabel))
        {
            namePlate.color = NamePlateColor;
        }

        Color nameColor = isLightPlate
            ? UISkin.Ink
            : isLocked ? UIListRow.LockedTextColor : UISkin.Cream;

        CardEntryFactory.CreateLabel(
            namePlate.transform, "NameText", region.DisplayName,
            16f, nameColor, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one,
            new Vector2(8f, 0f), new Vector2(-8f, 0f)
        );
    }
}
