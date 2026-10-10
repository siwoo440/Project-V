using System.Text; // 문자열 조립 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능

public partial class WorldMapFlow // 월드맵의 지역 정보 창 (기획서 4.3.4 / 11.6.3)
{
    private static readonly Color WarningTextColor = new Color(1f, 0.58f, 0.52f, 1f); // 들어갈 수 없는 사유

    // 기획서 10.17에 따라 권장 레벨과 위험도는 적지 않는다.
    private void UpdateDetail(PlayerProgressManager progress)
    {
        if (regionIconImage != null)
        {
            regionIconImage.enabled =
                selectedRegion != null &&
                UISkin.ApplySimple(regionIconImage, selectedRegion.IconKey, true);
        }

        if (selectedRegion == null)
        {
            SetText(regionNameText, "지역 데이터 없음");
            SetText(regionInfoText, "씬 UI 다시 구성을 실행하면 지역 목록이 연결됩니다.");
            SetEnterButton("지역 진입", false);
            return;
        }

        RegionState state = progress.GetRegionState(selectedRegion);
        string warningHex = ColorUtility.ToHtmlStringRGB(WarningTextColor);
        StringBuilder builder = new StringBuilder();

        builder.Append($"{selectedRegion.ChapterName}\n");
        builder.Append($"주요 히로인  {selectedRegion.HeroineName}\n");
        builder.Append($"상태  {RegionRules.GetStateName(state)}\n\n");
        builder.Append($"전투 주제\n{selectedRegion.BattleTheme}\n");

        if (!string.IsNullOrEmpty(selectedRegion.Description))
        {
            builder.Append($"\n{selectedRegion.Description}\n");
        }

        if (state == RegionState.Locked)
        {
            RegionData previous = progress.GetPreviousRegion(selectedRegion);

            builder.Append(previous == null
                ? $"\n<color=#{warningHex}>아직 들어갈 수 없는 지역입니다.</color>"
                : $"\n<color=#{warningHex}>앞 지역을 클리어하면 열립니다: {previous.DisplayName}</color>");
        }
        else if (state == RegionState.Cleared)
        {
            builder.Append("\n클리어한 지역입니다. 언제든 다시 들어갈 수 있습니다.");
        }
        else
        {
            builder.Append($"\n{RegionRules.TemporaryClearHint}");
        }

        SetText(regionNameText, selectedRegion.DisplayName);
        SetText(regionInfoText, builder.ToString());

        SetEnterButton(
            state == RegionState.Locked
                ? "잠김"
                : state == RegionState.Cleared ? "다시 방문" : "지역 진입",
            state != RegionState.Locked
        );
    }

    private void SetText(TMP_Text targetText, string content)
    {
        if (targetText == null) { return; }

        targetText.text = content;
    }

    private void SetEnterButton(string label, bool isInteractable)
    {
        if (enterButton == null) { return; }

        enterButton.interactable = isInteractable;

        TMP_Text buttonLabel = enterButton.GetComponentInChildren<TMP_Text>(true);

        if (buttonLabel != null)
        {
            buttonLabel.text = label;
        }
    }
}
