using System;
using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 스토리 장면 하나의 데이터 (기획서 2.10 / 12.12 / F.3.1)
// 대사를 차례로 보여 준다. 줄마다 화자와 대사를 적고, 배경이나 서 있는 인물이 바뀌는 줄에만 그 내용을 적는다.
// 선택지가 있는 줄에서는 답을 고르고, 고른 답의 갈래 이름이 적힌 줄만 이어서 본 뒤 갈래 이름이 없는 줄에서 다시 합쳐진다.
[CreateAssetMenu(
    fileName = "NewStoryScene",
    menuName = "Project V/스토리 장면 데이터"
)]
public class StorySceneData : ScriptableObject
{
    [Serializable]
    public class Choice // 선택지 하나
    {
        [SerializeField] private string label; // 버튼에 적는 도윤의 대답
        [SerializeField] private StoryChoiceType type = StoryChoiceType.Serious; // 답변의 분위기
        [SerializeField] private string branch; // 이 답을 고르면 이어지는 갈래 이름 (A, B, C, D)

        public string Label => label ?? string.Empty;   // 대답 반환
        public StoryChoiceType Type => type;            // 답변의 분위기 반환
        public string Branch => branch ?? string.Empty; // 갈래 이름 반환

        public string TypeName // 버튼 앞에 붙이는 분위기 이름
        {
            get
            {
                switch (type)
                {
                    case StoryChoiceType.Playful: return "장난";
                    case StoryChoiceType.Honest: return "솔직";
                    case StoryChoiceType.Grimoire: return "그리모어";
                    default: return "진지";
                }
            }
        }
    }

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

        // 갈래 이름. 비우면 어떤 답을 골라도 나오는 줄이고, 적으면 앞의 선택지에서 그 갈래를 골랐을 때만 나온다.
        [SerializeField] private string branch;

        // 이 줄을 보여 준 뒤 고르는 선택지 (2~4개). 비어 있으면 평소처럼 다음 줄로 넘어간다.
        [SerializeField] private List<Choice> choices = new List<Choice>();

        public string SpeakerName => speakerName ?? string.Empty;     // 화자 이름 반환
        public string Text => text ?? string.Empty;                   // 대사 반환
        public StorySlot SpeakerSlot => speakerSlot;                  // 화자의 자리 반환
        public string BackgroundKey => backgroundKey ?? string.Empty; // 배경 이미지 이름 반환
        public string Branch => branch ?? string.Empty;               // 갈래 이름 반환

        public bool HasChoices => choices != null && choices.Count > 0; // 선택지가 있는 줄인지 여부

        public IReadOnlyList<Choice> Choices => choices ?? (choices = new List<Choice>()); // 선택지 반환

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
    [SerializeField] private string sceneId; // 장면 ID (R01-STORY-INTRO, R01-H1-STORY-BEFORE). 본 장면 기록에 쓴다.
    [SerializeField] private string title;   // 장면 제목

    [Header("대사")]
    [SerializeField]
    private List<Line> lines = new List<Line>(); // 대사 목록

    public string SceneId => sceneId;           // 장면 ID 반환
    public string Title => title;               // 장면 제목 반환
    public IReadOnlyList<Line> Lines => lines;  // 대사 목록 반환

    public int ChoiceCount // 선택지가 나오는 횟수 (회상 목록에 적는다)
    {
        get
        {
            int count = 0;

            foreach (Line line in lines)
            {
                if (line != null && line.HasChoices) { count += 1; }
            }

            return count;
        }
    }
}
