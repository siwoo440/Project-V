using UnityEngine; // Unity 기본 기능

public partial class StageSelectFlow // 지역 화면의 지역 표시 (기획서 4.5)
{
    private RegionData region; // 이 화면이 보여 주는 지역

    // 월드맵에서 고른 지역을 화면에 표시한다. 월드맵을 거치지 않고 들어왔으면 첫 지역을 쓴다.
    private void ShowRegion()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        if (progress == null) { return; }

        region = progress.CurrentRegion;

        if (region == null)
        {
            RegionData firstRegion = progress.GetFirstRegion();

            if (progress.TryEnterRegion(firstRegion, out _))
            {
                region = firstRegion;
            }
        }

        if (region == null) { return; } // 지역 데이터가 없으면 기존 표시를 그대로 둔다.

        if (titleText != null)
        {
            titleText.text = region.DisplayName;
        }

        if (backgroundImage != null && UISkin.Has(region.BackgroundKey))
        {
            UISkin.ApplySimple(backgroundImage, region.BackgroundKey, false); // 지역 배경 그림이 없으면 지도 그림을 그대로 둔다.
        }
    }

    // 스테이지 이름. 스테이지 데이터가 지역별로 나뉘기 전까지는 지역 이름과 전투 종류로 만든다.
    private string GetStageTitle(StageEntry stage)
    {
        if (stage == null) { return string.Empty; }

        return region == null
            ? stage.StageName
            : $"{region.DisplayName} {stage.StageType}";
    }
}
