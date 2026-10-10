using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 스토리 화면의 배경과 인물 그림 (기획서 12.12)
// 인물은 왼쪽, 가운데, 오른쪽 세 자리에 선다. 말하는 인물은 밝고 조금 크게, 나머지는 어둡게 보인다.
// 인물이 들어오고 나갈 때는 서서히 나타나거나 사라지면서 옆으로 조금 미끄러진다.
public partial class StoryFlow
{
    private const float SpeakerScale = 1.05f;         // 말하는 인물의 크기
    private const float PortraitMoveSeconds = 0.25f;  // 등장과 퇴장에 걸리는 시간
    private const float PortraitSlideDistance = 70f;  // 등장과 퇴장 때 옆으로 미끄러지는 거리

    [Header("배경과 인물")]
    [SerializeField] private Image backgroundImage; // 배경
    [SerializeField] private Image leftPortrait;    // 왼쪽 인물
    [SerializeField] private Image centerPortrait;  // 가운데 인물
    [SerializeField] private Image rightPortrait;   // 오른쪽 인물

    private static readonly Color SpeakerColor = Color.white;                         // 말하는 인물
    private static readonly Color ListenerColor = new Color(0.50f, 0.50f, 0.56f, 1f); // 듣는 인물

    private class PortraitSlot // 인물 자리 하나의 등장 상태
    {
        public Image Image;            // 그림
        public RectTransform Rect;     // 위치
        public Vector2 BasePosition;   // 다 들어왔을 때의 위치
        public float SlideDirection;   // 들어오는 쪽 (왼쪽 자리 -1, 가운데 0, 오른쪽 자리 1)
        public bool IsShown;           // 들어와 있어야 하는지 여부
        public float Progress;         // 0은 완전히 나간 상태, 1은 완전히 들어온 상태
        public Color Tint = Color.white; // 화자 강조 색
    }

    private readonly List<PortraitSlot> portraitSlots = new List<PortraitSlot>(); // 세 자리

    private PortraitSlot leftSlot;   // 왼쪽 자리
    private PortraitSlot centerSlot; // 가운데 자리
    private PortraitSlot rightSlot;  // 오른쪽 자리

    private void ResetStage() // 장면을 시작할 때 세 자리를 비운다.
    {
        portraitSlots.Clear();

        leftSlot = CreateSlot(leftPortrait, -1f);
        centerSlot = CreateSlot(centerPortrait, 0f);
        rightSlot = CreateSlot(rightPortrait, 1f);
    }

    private PortraitSlot CreateSlot(Image portrait, float slideDirection)
    {
        if (portrait == null) { return null; }

        PortraitSlot slot = new PortraitSlot
        {
            Image = portrait,
            Rect = portrait.rectTransform,
            BasePosition = portrait.rectTransform.anchoredPosition,
            SlideDirection = slideDirection,
        };

        portrait.sprite = null;
        portrait.preserveAspect = true;
        portrait.enabled = false;

        portraitSlots.Add(slot);

        return slot;
    }

    // 대사 한 줄의 배경과 인물 지시를 적용한다. 지시가 없는 자리는 앞 줄의 상태를 그대로 둔다.
    private void ApplyStage(StorySceneData.Line line)
    {
        if (backgroundImage != null && UISkin.Has(line.BackgroundKey))
        {
            UISkin.ApplySimple(backgroundImage, line.BackgroundKey, false);
        }

        ApplyPortrait(leftSlot, line.GetArt(StorySlot.Left));
        ApplyPortrait(centerSlot, line.GetArt(StorySlot.Center));
        ApplyPortrait(rightSlot, line.GetArt(StorySlot.Right));

        bool isNarration = line.SpeakerSlot == StorySlot.None; // 해설이나 화면에 없는 화자의 줄에는 모두 밝게 둔다.

        ApplyHighlight(leftSlot, line.SpeakerSlot == StorySlot.Left, isNarration);
        ApplyHighlight(centerSlot, line.SpeakerSlot == StorySlot.Center, isNarration);
        ApplyHighlight(rightSlot, line.SpeakerSlot == StorySlot.Right, isNarration);
    }

    // 인물 지시: 비어 있으면 그대로, "-"는 자리를 비우고, "Aria"나 "Aria:Shaken"은 그 인물의 그림을 세운다.
    private static void ApplyPortrait(PortraitSlot slot, string art)
    {
        if (slot == null || string.IsNullOrEmpty(art)) { return; }

        if (art == "-")
        {
            slot.IsShown = false; // 그림은 다 나갈 때까지 남겨 둔다.
            return;
        }

        int separator = art.IndexOf(':');

        string artKey = separator > 0 ? art.Substring(0, separator) : art;
        string stateName = separator > 0 ? art.Substring(separator + 1) : string.Empty;

        Sprite sprite = HeroineArtLibrary.GetStoryArt(artKey, stateName);

        if (sprite == null)
        {
            slot.IsShown = false; // 그림이 없는 인물은 자리를 비운다.
            return;
        }

        slot.Image.sprite = sprite;
        slot.Image.enabled = true;
        slot.IsShown = true;
    }

    private static void ApplyHighlight(PortraitSlot slot, bool isSpeaker, bool isNarration)
    {
        if (slot == null) { return; }

        slot.Tint = isSpeaker || isNarration ? SpeakerColor : ListenerColor;
        slot.Rect.localScale = isSpeaker ? Vector3.one * SpeakerScale : Vector3.one;

        RefreshSlot(slot);
    }

    private void UpdatePortraits(float deltaTime) // 등장과 퇴장을 시간에 따라 진행한다.
    {
        foreach (PortraitSlot slot in portraitSlots)
        {
            float target = slot.IsShown ? 1f : 0f;

            if (Mathf.Approximately(slot.Progress, target)) { continue; }

            slot.Progress = Mathf.MoveTowards(slot.Progress, target, deltaTime / PortraitMoveSeconds);

            RefreshSlot(slot);
        }
    }

    private static void RefreshSlot(PortraitSlot slot) // 진행 정도를 투명도와 위치에 반영한다.
    {
        float eased = slot.Progress * slot.Progress * (3f - 2f * slot.Progress); // 처음과 끝을 부드럽게

        Color color = slot.Tint;
        color.a = eased;
        slot.Image.color = color;

        slot.Rect.anchoredPosition = slot.BasePosition +
            new Vector2(slot.SlideDirection * PortraitSlideDistance * (1f - eased), 0f);

        if (!slot.IsShown && slot.Progress <= 0f)
        {
            slot.Image.enabled = false; // 다 나간 자리는 숨긴다.
        }
    }
}
