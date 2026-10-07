using UnityEngine; // Unity 기본 기능

// 그리모어 강화 노드 하나의 데이터 (기획서 6.6)
[CreateAssetMenu(
    fileName = "NewGrimoireNode",
    menuName = "Project V/그리모어 노드 데이터"
)]
public class GrimoireNodeData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string nodeId;      // 노드 ID
    [SerializeField] private string displayName; // 노드 이름
    [SerializeField] private GrimoireBranch branch; // 소속 분기
    [SerializeField, Min(1)] private int order = 1; // 분기 안의 순서 (앞 노드를 찍어야 열린다)

    [Header("효과")]
    [SerializeField] private GrimoireEffectType effectType; // 효과 종류
    [SerializeField] private int level1Amount = 1; // 단계별 수치
    [SerializeField] private int level2Amount = 2;
    [SerializeField] private int level3Amount = 3;

    [SerializeField, TextArea]
    private string description; // 효과 설명. {0} 자리에 단계별 수치가 들어간다.

    public string NodeId => nodeId;                   // 노드 ID 반환
    public string DisplayName => displayName;         // 노드 이름 반환
    public GrimoireBranch Branch => branch;           // 소속 분기 반환
    public int Order => Mathf.Max(1, order);          // 분기 안의 순서 반환
    public GrimoireEffectType EffectType => effectType; // 효과 종류 반환

    public int GetAmount(int level) // 단계별 수치 (0단계는 0)
    {
        if (level <= 0) { return 0; }
        if (level == 1) { return level1Amount; }
        if (level == 2) { return level2Amount; }

        return level3Amount;
    }

    public string GetEffectText(int level) // 수치를 넣은 효과 설명
    {
        if (string.IsNullOrEmpty(description)) { return string.Empty; }

        int shownLevel = Mathf.Clamp(level, 1, GrimoireRules.MaxLevel);

        return description.Replace("{0}", GetAmount(shownLevel).ToString());
    }
}
