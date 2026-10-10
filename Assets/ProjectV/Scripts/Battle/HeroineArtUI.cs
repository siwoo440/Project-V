using System.Collections; // 코루틴 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 전투 화면의 히로인 그림 (기획서 11.8.1 / 12.4 / 12.5)
// 평소에는 기본 자세나 동요 상태 그림을 보여 주고, 공격을 받으면 피격 그림으로 잠깐 바꿨다가 되돌린다.
// 기본 자세 그림이 없는 히로인은 그림 칸을 숨긴다.
public class HeroineArtUI : MonoBehaviour
{
    [SerializeField] private Image artImage; // 히로인 그림

    [SerializeField, Min(0.1f)]
    private float reactionSeconds = 0.7f; // 피격 그림을 보여 주는 시간

    private string artKey = string.Empty; // 그림 이름의 앞부분 (Aria 등)
    private HeroineArtState idleState = HeroineArtState.Normal; // 평소 상태 (기본 자세 또는 동요 상태)
    private Coroutine reactionRoutine;
    private bool isLocked; // 전투가 끝나 마지막 그림을 고정했는지 여부

    public bool IsShown => gameObject.activeSelf && !string.IsNullOrEmpty(artKey); // 그림을 보여 주고 있는지 여부

    // 이번 전투의 히로인 그림을 준비한다. 기본 자세 그림이 없으면 칸을 숨기고 false를 돌려준다.
    public bool Setup(string key)
    {
        StopReaction();

        isLocked = false;
        idleState = HeroineArtState.Normal;
        artKey = HeroineArtLibrary.Has(key) ? key : string.Empty;

        gameObject.SetActive(artKey.Length > 0);

        if (artKey.Length == 0) { return false; }

        Apply(idleState);

        return true;
    }

    public void Hide() // 히로인이 없는 전투 (일반전, 포획전)
    {
        StopReaction();

        artKey = string.Empty;
        gameObject.SetActive(false);
    }

    // 평소 그림을 정한다. 성욕이 동요 구간에 들어가면 동요 상태 그림으로 바뀐다.
    public void SetIdle(bool isShaken)
    {
        HeroineArtState nextIdle = isShaken ? HeroineArtState.Shaken : HeroineArtState.Normal;

        if (nextIdle == idleState) { return; }

        idleState = nextIdle;

        if (IsShown && !isLocked && reactionRoutine == null) { Apply(idleState); }
    }

    // 피격이나 공격 그림을 잠깐 보여 준 뒤 평소 그림으로 돌아간다.
    public void PlayReaction(HeroineArtState state)
    {
        if (!IsShown || isLocked) { return; }

        StopReaction();
        Apply(state);

        reactionRoutine = StartCoroutine(ReturnToIdle());
    }

    public void ShowFinal(HeroineArtState state) // 전투가 끝난 뒤 계속 보여 줄 그림
    {
        if (!IsShown) { return; }

        StopReaction();

        isLocked = true;
        Apply(state);
    }

    private IEnumerator ReturnToIdle()
    {
        yield return new WaitForSeconds(reactionSeconds);

        reactionRoutine = null;

        if (!isLocked) { Apply(idleState); }
    }

    private void StopReaction()
    {
        if (reactionRoutine == null) { return; }

        StopCoroutine(reactionRoutine);
        reactionRoutine = null;
    }

    private void Apply(HeroineArtState state)
    {
        if (artImage == null) { return; }

        Sprite sprite = HeroineArtLibrary.Get(artKey, state);

        if (sprite == null) { return; }

        artImage.sprite = sprite;
        artImage.preserveAspect = true;
        artImage.enabled = true;
    }
}
