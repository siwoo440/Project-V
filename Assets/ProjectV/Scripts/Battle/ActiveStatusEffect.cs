using UnityEngine; // Unity 수학 기능

public class ActiveStatusEffect // 전투 중 활성 상태 효과
{
    public StatusEffectData Data { get; } // 원본 상태 효과 데이터
    public int RemainingTurns { get; private set; } // 남은 지속 턴

    public bool IsExpired => Data == null || RemainingTurns <= 0; // 상태 효과 만료 여부

    public ActiveStatusEffect(StatusEffectData data) // 활성 상태 효과 생성
    {
        Data = data; // 원본 데이터 저장
        RemainingTurns = data == null ? 0 : Mathf.Max(1, data.DurationTurns); // 초기 지속시간 설정
    }

    public void ReduceDuration() // 지속시간 감소
    {
        RemainingTurns = Mathf.Max(0, RemainingTurns - 1); // 남은 지속시간 감소
    }
    public void RefreshDuration() // 상태 효과 지속시간 갱신
    {
        RemainingTurns = Data == null ? 0 : Mathf.Max(1, Data.DurationTurns); // 원본 지속시간 재설정
    }
}