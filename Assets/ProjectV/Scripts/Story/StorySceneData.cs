using System;
using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 스토리 장면 하나의 데이터 (기획서 12.12 / F.3.1)
// 대사를 차례로 보여 준다. 줄마다 화자와 대사를 적고, 배경이나 서 있는 인물이 바뀌는 줄에만 그 내용을 적는다.
[CreateAssetMenu(
    fileName = "NewStoryScene",
    menuName = "Project V/스토리 장면 데이터"
)]
public class StorySceneData : ScriptableObject
{
    [Serializable]
    public class Line // 대사 한 줄
    {
        [SerializeField] private string speakerName; // 화자 이름 (비우면 해설)

        [SerializeField, TextArea(2, 4)]
        private string text; // 대사

        [SerializeField] private StorySlot speakerSlot = StorySlot.None; // 말하는 인물의 자리. 그 자리의 인물이 밝게 보인다.

        // 인물 그림 지시: 비우면 그대로 두고, "-"는 자리를 비우고, "Aria"나 "Aria:Shaken"은 그 인물의 그림을 세운다.
        [SerializeField] private string leftArt;
        [SerializeField] private string centerArt;
        [SerializeField] private string rightArt;

        [SerializeField] private string backgroundKey; // 배경 이미지 이름 (비우면 그대로)

        public string SpeakerName => speakerName ?? string.Empty;     // 화자 이름 반환
        public string Text => text ?? string.Empty;                   // 대사 반환
        public StorySlot SpeakerSlot => speakerSlot;                  // 화자의 자리 반환
        public string BackgroundKey => backgroundKey ?? string.Empty; // 배경 이미지 이름 반환

        public Line() { } // 에셋에 저장된 줄용

        public Line(string speaker, string dialogue) // 씬에 적힌 예전 대사를 옮길 때 쓴다.
        {
            speakerName = speaker;
            text = dialogue;
        }

        public string GetArt(StorySlot slot) // 그 자리의 인물 그림 지시 반환
        {
            switch (slot)
            {
                case StorySlot.Left: return leftArt ?? string.Empty;
                case StorySlot.Center: return centerArt ?? string.Empty;
                case StorySlot.Right: return rightArt ?? string.Empty;
                default: return string.Empty;
            }
        }
    }

    [Header("장면 정보")]
    [SerializeField] private string sceneId; // 장면 ID (R01-STORY-INTRO, R01-STORY-END). 본 장면 기록에 쓴다.
    [SerializeField] private string title;   // 장면 제목

    [Header("대사")]
    [SerializeField]
    private List<Line> lines = new List<Line>(); // 대사 목록

    public string SceneId => sceneId;           // 장면 ID 반환
    public string Title => title;               // 장면 제목 반환
    public IReadOnlyList<Line> Lines => lines;  // 대사 목록 반환
}
