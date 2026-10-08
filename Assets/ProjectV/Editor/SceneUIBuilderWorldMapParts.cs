using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 월드맵 상단 메뉴 버튼과 지역 목록 연결
public static partial class SceneUIBuilder
{
    // 월드맵 상단 메뉴의 버튼 하나. 왼쪽부터 순서대로 놓는다.
    private static Button EnsureWorldMapMenuButton(
        string objectName,
        Transform parent,
        string label,
        int index,
        string iconKey)
    {
        Button button = EnsureButton(objectName, parent, label, ButtonColor);

        SetAnchored(button.gameObject,
            new Vector2(0f, 1f), new Vector2(158f + index * 250f, -158f), new Vector2(236f, 54f));

        StyleButtonByName(objectName, ButtonColor, 21f);
        EnsureButtonIcon(button, iconKey);

        return button;
    }

    // 프로젝트의 지역 데이터를 해금 순서대로 진행 데이터에 연결한다.
    private static void ApplyRegionList(PlayerProgressManager progress)
    {
        if (progress == null) { return; }

        List<RegionData> regions = LoadAssets<RegionData>();

        regions.Sort((left, right) =>
            left.Order != right.Order
                ? left.Order.CompareTo(right.Order)
                : string.CompareOrdinal(left.RegionId, right.RegionId));

        SerializedObject serializedProgress = new SerializedObject(progress);

        AssignList(serializedProgress, "regions", regions);

        serializedProgress.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"지역 {regions.Count}곳을 연결했습니다.");
    }
}
