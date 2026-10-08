using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class WorldMapFlow // 월드맵의 지역 표시 (기획서 4.3.3 / 11.6.3)
{
    private const float NodeSize = 96f;   // 지역 받침 크기
    private const float EmblemSize = 66f; // 지역 문양 크기
    private const float RingSize = 136f;  // 고른 지역의 고리 크기
    private const float BadgeSize = 48f;  // 클리어 표시와 알림 배지 크기

    private static readonly Color PlateColor = new Color(0.20f, 0.24f, 0.42f, 1f);       // 받침 이미지가 없을 때 색
    private static readonly Color LockedPlateColor = new Color(0.16f, 0.16f, 0.20f, 1f); // 받침 이미지가 없을 때 잠긴 지역 색
    private static readonly Color LockedTint = new Color(0.34f, 0.35f, 0.42f, 1f);       // 잠긴 지역의 문양을 어둡게

    private void BuildNodes(PlayerProgressManager progress)
    {
        if (mapArea == null) { return; }

        foreach (RegionData region in progress.Regions)
        {
            if (region == null) { continue; }

            CreateNode(progress, region);
        }
    }

    // 지역 하나: 받침, 문양, 상태 표시(자물쇠, 클리어, NEW, 위치 깃발), 이름판
    private void CreateNode(PlayerProgressManager progress, RegionData region)
    {
        RegionState state = progress.GetRegionState(region);
        bool isSelected = region == selectedRegion;
        bool isLocked = state == RegionState.Locked;

        GameObject nodeObject =
            new GameObject($"RegionNode_{region.RegionId}", typeof(RectTransform));

        nodeObject.transform.SetParent(mapArea, false);
        generatedNodes.Add(nodeObject);

        RectTransform nodeRect = nodeObject.GetComponent<RectTransform>();
        nodeRect.anchorMin = region.MapPosition; // 지도 그림 안의 비율 위치
        nodeRect.anchorMax = region.MapPosition;
        nodeRect.pivot = new Vector2(0.5f, 0.5f);
        nodeRect.anchoredPosition = Vector2.zero;
        nodeRect.sizeDelta = new Vector2(NodeSize, NodeSize);

        Image plate = nodeObject.AddComponent<Image>();

        string plateKey = isLocked
            ? UIKeys.NodeLocked
            : state == RegionState.Cleared ? UIKeys.NodeDone : UIKeys.NodeOpen; // 그리모어 노드 받침을 함께 쓴다.

        if (!UISkin.ApplySimple(plate, plateKey, true))
        {
            plate.color = isLocked ? LockedPlateColor : PlateColor;
        }

        Button nodeButton = nodeObject.AddComponent<Button>();
        nodeButton.targetGraphic = plate;

        RegionData targetRegion = region;

        nodeButton.onClick.AddListener(() => SelectRegion(targetRegion));

        if (isSelected)
        {
            Image ring = CreateMark(nodeObject.transform, "SelectRing", RingSize, Vector2.zero);

            bool hasRing = UISkin.ApplySimple(ring, UIKeys.MapMarkSelect, true);

            ring.enabled = hasRing;

            if (!hasRing) { nodeRect.localScale = Vector3.one * 1.14f; } // 고리 이미지가 없으면 크기로 표시
        }

        Image emblem = CreateMark(nodeObject.transform, "Emblem", EmblemSize, new Vector2(0f, 2f));

        bool hasEmblem = UISkin.ApplySimple(emblem, region.IconKey, true);

        emblem.enabled = hasEmblem; // 문양 이미지가 없으면 받침만 보여준다.

        if (hasEmblem && isLocked) { emblem.color = LockedTint; }

        if (isLocked)
        {
            Image lockIcon = CreateMark(
                nodeObject.transform, "LockIcon", 42f,
                hasEmblem ? new Vector2(28f, -28f) : Vector2.zero
            );

            lockIcon.enabled = UISkin.ApplySimple(lockIcon, UIKeys.IconLock, true);
        }

        if (state == RegionState.Cleared)
        {
            Image clearMark = CreateMark(
                nodeObject.transform, "ClearMark", BadgeSize, new Vector2(34f, 34f)
            );

            clearMark.enabled = UISkin.ApplySimple(clearMark, UIKeys.MapMarkClear, true);
        }

        if (state == RegionState.New)
        {
            CreateNewBadge(nodeObject.transform);
        }

        if (region == progress.CurrentRegion)
        {
            Image hereMark = CreateMark(
                nodeObject.transform, "HereMark", BadgeSize, new Vector2(-36f, 36f)
            );

            hereMark.enabled = UISkin.ApplySimple(hereMark, UIKeys.MapMarkHere, true);
        }

        CreateNamePlate(nodeObject.transform, region, isSelected, isLocked);
    }

    private static Image CreateMark(Transform parent, string objectName, float size, Vector2 center)
    {
        Vector2 middle = new Vector2(0.5f, 0.5f);
        Vector2 half = new Vector2(size * 0.5f, size * 0.5f);

        return CardEntryFactory.CreateImage(
            parent, objectName, Color.white, middle, middle, center - half, center + half
        );
    }
}
