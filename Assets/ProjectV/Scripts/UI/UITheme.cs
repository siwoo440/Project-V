using System;
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능

// UI 이미지 묶음 (기획서 12.17 Art/UI)
// 에디터 도구 UIThemeSetup이 Art 폴더의 이미지를 읽어 채운다.
// 이미지가 없는 항목은 null을 돌려주며, 호출하는 쪽은 기존 단색 표시를 유지한다.
public class UITheme : ScriptableObject
{
    public const string ResourcePath = "UITheme"; // Resources 기준 경로

    [Serializable]
    public class SpriteEntry // 이름과 이미지 한 쌍
    {
        public string key;    // 이미지 파일 이름 (확장자 제외)
        public Sprite sprite; // 연결된 스프라이트
    }

    [SerializeField]
    private List<SpriteEntry> sprites = new List<SpriteEntry>(); // 이미지 목록

    [SerializeField] private TMP_SpriteAsset iconAsset; // 글자 사이에 넣는 아이콘 묶음

    [SerializeField]
    private List<string> iconNames = new List<string>(); // 아이콘 묶음에 들어 있는 이름

    private Dictionary<string, Sprite> spriteLookup; // 이름으로 찾는 표
    private HashSet<string> iconLookup;              // 아이콘 이름 표

    private static UITheme current; // 불러온 테마
    private static bool isLoaded;   // 불러오기 시도 여부

    public static UITheme Current // 현재 테마. 이미지가 없으면 null
    {
        get
        {
            if (!isLoaded)
            {
                current = Resources.Load<UITheme>(ResourcePath);
                isLoaded = true;
            }

            return current;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reload() // 다음 사용 때 테마를 다시 읽는다.
    {
        current = null;
        isLoaded = false;
    }

    public static void SetCurrent(UITheme theme) // 에디터 도구가 방금 만든 테마를 바로 쓰게 한다.
    {
        current = theme;
        isLoaded = true;
    }

    public string IconAssetName =>
        iconAsset == null ? string.Empty : iconAsset.name; // 아이콘 묶음 이름

    public int SpriteCount => sprites.Count; // 등록된 이미지 수

    public Sprite GetSprite(string key) // 이름으로 이미지 찾기
    {
        if (string.IsNullOrEmpty(key)) { return null; }

        if (spriteLookup == null)
        {
            spriteLookup = new Dictionary<string, Sprite>();

            foreach (SpriteEntry entry in sprites)
            {
                if (entry == null || entry.sprite == null) { continue; }
                if (string.IsNullOrEmpty(entry.key)) { continue; }

                spriteLookup[entry.key] = entry.sprite;
            }
        }

        return spriteLookup.TryGetValue(key, out Sprite sprite) ? sprite : null;
    }

    public bool HasIcon(string iconName) // 글자 사이 아이콘 보유 여부
    {
        if (iconAsset == null) { return false; }
        if (string.IsNullOrEmpty(iconName)) { return false; }

        if (iconLookup == null)
        {
            iconLookup = new HashSet<string>(iconNames);
        }

        return iconLookup.Contains(iconName);
    }

    // 에디터 도구가 내용을 채울 때 사용한다.
    public void SetContents(
        List<SpriteEntry> newSprites,
        TMP_SpriteAsset newIconAsset,
        List<string> newIconNames
    )
    {
        sprites = newSprites ?? new List<SpriteEntry>();
        iconAsset = newIconAsset;
        iconNames = newIconNames ?? new List<string>();

        spriteLookup = null;
        iconLookup = null;
    }
}
