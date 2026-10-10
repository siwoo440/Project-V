using UnityEngine; // Unity 기본 기능

// 전투 속도 (기획서 4.18.1 / 10.22 / 11.11)
// 1배, 1.5배, 2배 가운데 고른다. 행동 사이의 대기 시간과 그림 연출에만 적용하고 전투 판정에는 영향을 주지 않는다.
// 고른 속도는 기기에 저장해 다음 전투에도 쓴다. 옵션 화면이 생기면 그쪽 설정으로 옮긴다.
public static class BattleSpeed
{
    private const string PrefsKey = "ProjectV.BattleSpeed"; // 저장 이름

    private static readonly float[] Options = { 1f, 1.5f, 2f };

    private static int index = -1; // 고른 속도의 순번 (-1이면 아직 읽지 않음)

    private static int Index
    {
        get
        {
            if (index < 0)
            {
                index = Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, 0), 0, Options.Length - 1);
            }

            return index;
        }
    }

    public static float Current => Options[Index]; // 현재 배속

    public static string Label => $"x{Current:0.#}"; // 버튼에 적는 문구 (x1, x1.5, x2)

    public static void Next() // 다음 속도로 바꾼다. 2배 다음은 1배다.
    {
        index = (Index + 1) % Options.Length;

        PlayerPrefs.SetInt(PrefsKey, index);
        PlayerPrefs.Save();
    }

    public static float Scale(float seconds) // 배속을 적용한 대기 시간
    {
        return Mathf.Max(0f, seconds) / Current;
    }

    public static WaitForSeconds Wait(float seconds) // 배속을 적용한 대기
    {
        return new WaitForSeconds(Scale(seconds));
    }
}
