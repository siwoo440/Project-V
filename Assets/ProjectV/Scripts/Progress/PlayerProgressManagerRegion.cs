using System.Collections.Generic; // 리스트 기능
using UnityEngine; // Unity 기본 기능

// 지역 해금과 진행 (기획서 4.4 / 4.14 / 8.3)
// 지역은 순서대로 하나씩 열린다. 앞 지역을 클리어하면 다음 지역이 열리고, 클리어한 지역은 언제든 다시 들어갈 수 있다.
// 열렸는지는 따로 저장하지 않고 앞 지역의 클리어 여부에서 계산한다.
public partial class PlayerProgressManager
{
    [Header("지역")]
    [SerializeField]
    private List<RegionData> regions =
        new List<RegionData>(); // 지역 목록 (해금 순서대로)

    private readonly HashSet<RegionData> visitedRegions =
        new HashSet<RegionData>(); // 한 번이라도 들어간 지역

    private readonly HashSet<RegionData> clearedRegions =
        new HashSet<RegionData>(); // 클리어한 지역

    private RegionData currentRegion;        // 마지막으로 들어간 지역
    private RegionData newlyUnlockedRegion;  // 방금 열린 지역. 월드맵이 한 번 안내하고 지운다. (저장하지 않는다)

    public IReadOnlyList<RegionData> Regions => regions; // 지역 목록 반환
    public RegionData CurrentRegion => currentRegion;    // 마지막으로 들어간 지역 반환

    public int ClearedRegionCount => clearedRegions.Count; // 클리어한 지역 수

    private void InitializeRegionProgress() // 시작 진행 데이터 구성: 첫 지역만 열려 있다.
    {
        visitedRegions.Clear();
        clearedRegions.Clear();
        currentRegion = null;
        newlyUnlockedRegion = null;
    }

    public RegionData GetPreviousRegion(RegionData region) // 바로 앞 순서의 지역 (첫 지역이면 없음)
    {
        if (region == null) { return null; }

        RegionData previous = null;

        foreach (RegionData other in regions)
        {
            if (other == null || other.Order >= region.Order) { continue; }

            if (previous == null || other.Order > previous.Order)
            {
                previous = other;
            }
        }

        return previous;
    }

    public RegionData GetNextRegion(RegionData region) // 바로 다음 순서의 지역 (마지막 지역이면 없음)
    {
        if (region == null) { return null; }

        RegionData next = null;

        foreach (RegionData other in regions)
        {
            if (other == null || other.Order <= region.Order) { continue; }

            if (next == null || other.Order < next.Order)
            {
                next = other;
            }
        }

        return next;
    }

    public RegionData GetFirstRegion() // 처음부터 열려 있는 지역
    {
        RegionData first = null;

        foreach (RegionData region in regions)
        {
            if (region == null) { continue; }

            if (first == null || region.Order < first.Order)
            {
                first = region;
            }
        }

        return first;
    }

    public bool IsRegionCleared(RegionData region) // 지역 클리어 여부
    {
        return region != null && clearedRegions.Contains(region);
    }

    public bool IsRegionUnlocked(RegionData region) // 지역 해금 여부 (기획서 4.4.1)
    {
        if (region == null) { return false; }

        RegionData previous = GetPreviousRegion(region);

        return previous == null || clearedRegions.Contains(previous);
    }

    public RegionState GetRegionState(RegionData region) // 월드맵에 보여줄 상태
    {
        if (!IsRegionUnlocked(region)) { return RegionState.Locked; }
        if (clearedRegions.Contains(region)) { return RegionState.Cleared; }

        return visitedRegions.Contains(region)
            ? RegionState.InProgress
            : RegionState.New;
    }

    public bool IsChapterCleared(int chapter) // 해당 챕터의 지역을 클리어했는지 여부 (상점 해금 조건)
    {
        foreach (RegionData region in clearedRegions)
        {
            if (region != null && region.Order == chapter) { return true; }
        }

        return false;
    }

    public bool TryEnterRegion(RegionData region, out string message) // 지역 진입
    {
        if (region == null)
        {
            message = "들어갈 지역을 선택하세요.";
            return false;
        }

        if (!IsRegionUnlocked(region))
        {
            RegionData previous = GetPreviousRegion(region);

            message = previous == null
                ? "아직 들어갈 수 없는 지역입니다."
                : $"앞 지역을 클리어하면 열립니다: {previous.DisplayName}";

            return false;
        }

        bool isFirstVisit = visitedRegions.Add(region);

        currentRegion = region;
        message = string.Empty;

        ProgressChanged?.Invoke(); // 진행 데이터 변경 알림

        if (isFirstVisit)
        {
            Debug.Log($"지역 첫 진입: {region.DisplayName}"); // 진입 기록
            AutoSave("새 지역 진입"); // 기획서 15.6
        }

        return true;
    }

    // 지역을 클리어 처리한다. 다음 지역이 있으면 열리고 월드맵이 한 번 안내한다. (기획서 4.14.2)
    private bool MarkRegionCleared(RegionData region)
    {
        if (region == null || !clearedRegions.Add(region)) { return false; }

        visitedRegions.Add(region);
        newlyUnlockedRegion = GetNextRegion(region);

        Debug.Log(
            newlyUnlockedRegion == null
                ? $"지역 클리어: {region.DisplayName}"
                : $"지역 클리어: {region.DisplayName} (다음 지역 해금: {newlyUnlockedRegion.DisplayName})"
        ); // 클리어 기록

        return true;
    }

    public RegionData TakeNewlyUnlockedRegion() // 방금 열린 지역을 한 번만 돌려준다.
    {
        RegionData unlockedRegion = newlyUnlockedRegion;

        newlyUnlockedRegion = null;

        return unlockedRegion;
    }

    private RegionData FindRegionById(string regionId)
    {
        if (string.IsNullOrEmpty(regionId)) { return null; }

        foreach (RegionData region in regions)
        {
            if (region != null && region.RegionId == regionId) { return region; }
        }

        return null;
    }

    public string GetRegionName(string regionId) // 저장 칸에 보여줄 지역 이름 (없으면 빈 문자열)
    {
        RegionData region = FindRegionById(regionId);

        return region == null ? string.Empty : region.DisplayName;
    }
}
