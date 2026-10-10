using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 씬 UI 구성 도구의 그림 버튼: 글자 대신 아이콘만 보이는 둥근 버튼
public static partial class SceneUIBuilder
{
    private const float IconButtonSize = 68f; // 전투 화면 왼쪽 아래 그림 버튼의 크기

    // 전투 화면의 보조 버튼(소모품, 전투 로그, 도감, 지역으로)과 닫기 버튼을 그림 버튼으로 바꾼다.
    // 왼쪽 아래에 세로로 한 줄로 놓는다. 전투 로그는 시작할 때 닫아 둔다.
    private static void BuildBattleIconButtons()
    {
        Vector2 bottomLeft = new Vector2(0f, 0f);

        ApplyIconButton("ReturnButton", UIKeys.IconWorldMap, bottomLeft, new Vector2(56f, 50f), IconButtonSize);
        ApplyIconButton("OpenCollectionButton", UIKeys.IconCollection, bottomLeft, new Vector2(56f, 126f), IconButtonSize);
        ApplyIconButton("BattleLogOpenButton", UIKeys.IconLog, bottomLeft, new Vector2(56f, 202f), IconButtonSize);
        ApplyIconButton("BattleItemButton", UIKeys.ItemEmpty, bottomLeft, new Vector2(56f, 278f), IconButtonSize); // 아이콘은 실행 중에 장착한 아이템으로 바뀐다.

        ApplyIconButton("BattleLogCloseButton", UIKeys.IconClose, new Vector2(1f, 1f), new Vector2(-44f, -38f), 52f);
        ApplyIconButton("CollectionCloseButton", UIKeys.IconClose, new Vector2(1f, 1f), new Vector2(-52f, -46f), 52f);

        // 패널은 씬에서 켜 둔 채로 두고, 실행이 시작될 때 전투 로그가 스스로 닫는다.
        // 씬에서 꺼 두면 처음 열 때 초기화가 실행되어 그때까지 쌓인 기록이 지워진다.
        BattleLogUI logUI = Object.FindFirstObjectByType<BattleLogUI>(FindObjectsInactive.Include);

        if (logUI != null)
        {
            SerializedObject serializedLog = new SerializedObject(logUI);
            SerializedProperty startOpened = serializedLog.FindProperty("startOpened");

            if (startOpened != null)
            {
                startOpened.boolValue = false; // 전투에 들어갔을 때 전투 로그는 닫혀 있다.
                serializedLog.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    // 글자 버튼 하나를 그림 버튼으로 바꾼다. 받침은 둥근 노드 그림을 쓰고 가운데에 아이콘을 놓는다.
    // 아이콘 그림이 없으면 글자 버튼을 그대로 둔다.
    private static void ApplyIconButton(
        string objectName,
        string iconKey,
        Vector2 anchor,
        Vector2 position,
        float size)
    {
        GameObject target = FindInScene(objectName);

        if (target == null) { return; }
        if (!UISkin.Has(iconKey)) { return; }

        SetAnchored(target, anchor, position, new Vector2(size, size));

        Image plate = target.AddComponentIfMissing<Image>();
        plate.raycastTarget = true;

        if (!UISkin.ApplySimple(plate, UIKeys.NodeOpen, true))
        {
            plate.sprite = null;
            plate.color = ButtonColor; // 받침 그림이 없으면 단색
        }

        Transform existingIcon = target.transform.Find("Icon");

        GameObject icon = existingIcon != null
            ? existingIcon.gameObject
            : new GameObject("Icon", typeof(RectTransform));

        icon.transform.SetParent(target.transform, false);

        Image iconImage = icon.AddComponentIfMissing<Image>();
        iconImage.raycastTarget = false;

        SetAnchored(icon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 0.6f, size * 0.6f));

        iconImage.enabled = UISkin.ApplySimple(iconImage, iconKey, true);

        TextMeshProUGUI label = target.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
        {
            label.gameObject.SetActive(false); // 글자는 숨긴다. (실행 중에 글자를 바꾸는 코드는 그대로 동작한다)
        }
    }
}
