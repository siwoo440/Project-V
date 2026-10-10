using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 스토리 화면의 배경과 인물 그림 (기획서 12.12)
// 인물은 왼쪽, 가운데, 오른쪽 세 자리에 선다. 말하는 인물은 밝고 조금 크게, 나머지는 어둡게 보인다.
public partial class StoryFlow
{
    private const float SpeakerScale = 1.05f; // 말하는 인물의 크기

    [Header("배경과 인물")]
    [SerializeField] private Image backgroundImage; // 배경
    [SerializeField] private Image leftPortrait;    // 왼쪽 인물
    [SerializeField] private Image centerPortrait;  // 가운데 인물
    [SerializeField] private Image rightPortrait;   // 오른쪽 인물

    private static readonly Color SpeakerColor = Color.white;                         // 말하는 인물
    private static readonly Color ListenerColor = new Color(0.50f, 0.50f, 0.56f, 1f); // 듣는 인물

    private void ResetStage() // 장면을 시작할 때 세 자리를 비운다.
    {
        SetPortrait(leftPortrait, null);
        SetPortrait(centerPortrait, null);
        SetPortrait(rightPortrait, null);
    }

    // 대사 한 줄의 배경과 인물 지시를 적용한다. 지시가 없는 자리는 앞 줄의 상태를 그대로 둔다.
    private void ApplyStage(StorySceneData.Line line)
    {
        if (backgroundImage != null && UISkin.Has(line.BackgroundKey))
        {
            UISkin.ApplySimple(backgroundImage, line.BackgroundKey, false);
        }

        ApplyPortrait(leftPortrait, line.GetArt(StorySlot.Left));
        ApplyPortrait(centerPortrait, line.GetArt(StorySlot.Center));
        ApplyPortrait(rightPortrait, line.GetArt(StorySlot.Right));

        bool isNarration = line.SpeakerSlot == StorySlot.None; // 해설이나 화면에 없는 화자의 줄에는 모두 밝게 둔다.

        ApplyHighlight(leftPortrait, line.SpeakerSlot == StorySlot.Left, isNarration);
        ApplyHighlight(centerPortrait, line.SpeakerSlot == StorySlot.Center, isNarration);
        ApplyHighlight(rightPortrait, line.SpeakerSlot == StorySlot.Right, isNarration);
    }

    // 인물 지시: 비어 있으면 그대로, "-"는 자리를 비우고, "Aria"나 "Aria:Shaken"은 그 인물의 그림을 세운다.
    private static void ApplyPortrait(Image portrait, string art)
    {
        if (portrait == null || string.IsNullOrEmpty(art)) { return; }

        if (art == "-")
        {
            SetPortrait(portrait, null);
            return;
        }

        int separator = art.IndexOf(':');

        string artKey = separator > 0 ? art.Substring(0, separator) : art;
        string stateName = separator > 0 ? art.Substring(separator + 1) : string.Empty;

        SetPortrait(portrait, HeroineArtLibrary.GetStoryArt(artKey, stateName));
    }

    private static void SetPortrait(Image portrait, Sprite sprite) // 그림이 없으면 자리를 숨긴다.
    {
        if (portrait == null) { return; }

        portrait.sprite = sprite;
        portrait.preserveAspect = true;
        portrait.enabled = sprite != null;
    }

    private static void ApplyHighlight(Image portrait, bool isSpeaker, bool isNarration)
    {
        if (portrait == null) { return; }

        portrait.color = isSpeaker || isNarration ? SpeakerColor : ListenerColor;
        portrait.transform.localScale = isSpeaker ? Vector3.one * SpeakerScale : Vector3.one;
    }
}
