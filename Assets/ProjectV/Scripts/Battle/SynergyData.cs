using System;
using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

[CreateAssetMenu(
    fileName = "NewSynergyData",
    menuName = "Project V/타입 시너지 데이터"
)] // 타입 시너지 데이터 생성 메뉴
public class SynergyData : ScriptableObject // 타입별 시너지 정의
{
    [Serializable]
    public class SynergyStage // 시너지 단계
    {
        [SerializeField, Min(1)] private int requiredCount = 2; // 필요 마물 수

        [SerializeField]
        private SynergyEffectType effectType = SynergyEffectType.None; // 효과 종류

        [SerializeField, Min(0)] private int amount = 1; // 효과 수치
        [SerializeField] private bool applyToAllAllies; // 모든 아군 적용 여부

        [TextArea(1, 3)]
        [SerializeField] private string description; // 단계 설명

        public int RequiredCount => Mathf.Max(1, requiredCount); // 필요 마물 수 반환
        public SynergyEffectType EffectType => effectType;       // 효과 종류 반환
        public int Amount => Mathf.Max(0, amount);               // 효과 수치 반환
        public bool ApplyToAllAllies => applyToAllAllies;        // 전체 적용 여부 반환
        public string Description => description;                // 단계 설명 반환

        public bool IsContinuous => // 지속 보정 여부
            effectType == SynergyEffectType.ModifyMaxHp ||
            effectType == SynergyEffectType.ModifyAttack ||
            effectType == SynergyEffectType.ModifyDefense ||
            effectType == SynergyEffectType.ModifyLustDamage;
    }

    [Header("시너지 정보")]
    [SerializeField] private MonsterType monsterType = MonsterType.None; // 대상 타입

    [SerializeField]
    private List<SynergyStage> stages = new List<SynergyStage>(); // 단계 목록

    public MonsterType MonsterType => monsterType; // 대상 타입 반환

    public IReadOnlyList<SynergyStage> Stages => stages; // 단계 목록 반환

    public string DisplayName =>
        MonsterTypeRules.GetDisplayName(monsterType); // 표시 이름 반환

    // 현재 마물 수로 활성화된 단계 수를 센다.
    public int GetActiveStageCount(int monsterCount)
    {
        int activeCount = 0;

        foreach (SynergyStage stage in stages)
        {
            if (stage == null) { continue; }
            if (monsterCount < stage.RequiredCount) { continue; }

            activeCount += 1;
        }

        return activeCount;
    }

    // 다음 단계까지 필요한 마물 수를 반환한다. 모든 단계가 열렸다면 0을 반환한다.
    public int GetNextRequiredCount(int monsterCount)
    {
        int nextRequired = 0;

        foreach (SynergyStage stage in stages)
        {
            if (stage == null) { continue; }
            if (monsterCount >= stage.RequiredCount) { continue; }

            if (nextRequired == 0 || stage.RequiredCount < nextRequired)
            {
                nextRequired = stage.RequiredCount;
            }
        }

        return nextRequired;
    }
}
