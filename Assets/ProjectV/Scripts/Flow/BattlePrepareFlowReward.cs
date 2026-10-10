using System.Text; // 문자열 조립 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

// 전투 준비 화면의 난이도 선택과 보상 안내 (기획서 4.17 / 9.4 ~ 9.9 / 10.18)
public partial class BattlePrepareFlow
{
    [Header("난이도")]
    [SerializeField] private Button easyButton;   // 쉬움
    [SerializeField] private Button normalButton; // 보통
    [SerializeField] private Button hardButton;   // 어려움

    [Header("보상")]
    [SerializeField] private TMP_Text rewardText;   // 받을 보상과 승리 기록
    [SerializeField] private Image rewardIconImage; // 최초 보상 표시 (닫힌 상자: 남아 있음, 열린 상자: 받음)

    private static readonly Color DifficultyColor = new Color(0.20f, 0.16f, 0.31f, 1f);         // 버튼 이미지가 없을 때 기본 색
    private static readonly Color DifficultySelectedColor = new Color(0.42f, 0.34f, 0.16f, 1f); // 버튼 이미지가 없을 때 강조 색

    private void StartDifficultyButtons()
    {
        AddListener(easyButton, () => SelectDifficulty(BattleDifficulty.Easy));
        AddListener(normalButton, () => SelectDifficulty(BattleDifficulty.Normal));
        AddListener(hardButton, () => SelectDifficulty(BattleDifficulty.Hard));
    }

    private void SelectDifficulty(BattleDifficulty difficulty)
    {
        BattleSetup.SetDifficulty(difficulty);

        lastMessage = $"난이도를 {StageRules.GetDifficultyName(difficulty)}으로 바꿨습니다.";

        RefreshAll(); // 상대 능력치와 보상을 다시 적는다.
    }

    // 난이도를 고를 수 있는 전투에서만 버튼을 보여 주고, 고른 난이도는 금색 버튼으로 표시한다.
    private void RefreshDifficultyButtons()
    {
        bool isShown = BattleSetup.HasBattle && BattleSetup.CanChangeDifficulty;

        ApplyDifficultyButton(easyButton, BattleDifficulty.Easy, isShown);
        ApplyDifficultyButton(normalButton, BattleDifficulty.Normal, isShown);
        ApplyDifficultyButton(hardButton, BattleDifficulty.Hard, isShown);
    }

    private void ApplyDifficultyButton(Button button, BattleDifficulty difficulty, bool isShown)
    {
        if (button == null) { return; }

        button.gameObject.SetActive(isShown);

        if (!isShown) { return; }

        Image buttonImage = button.GetComponent<Image>();
        bool isSelected = difficulty == BattleSetup.Difficulty;

        bool hasSkin =
            buttonImage != null &&
            UISkin.ApplySelectable(
                buttonImage, isSelected,
                UIKeys.ButtonBlue, UIKeys.ButtonGold,
                DifficultyColor, DifficultySelectedColor
            );

        TMP_Text buttonLabel = button.GetComponentInChildren<TMP_Text>(true);

        if (hasSkin && buttonLabel != null)
        {
            buttonLabel.color = isSelected ? UISkin.ButtonInk : UISkin.Cream; // 금색 버튼 위에는 어두운 글자
        }
    }

    private void RefreshReward()
    {
        PlayerProgressManager progress = PlayerProgressManager.Instance;

        bool hasRecord =
            BattleSetup.HasBattle && progress != null && !string.IsNullOrEmpty(BattleSetup.StageId);

        bool isFirstClear = !hasRecord || !progress.IsStageCleared(BattleSetup.StageId);

        SetText(rewardText, BattleSetup.HasBattle ? GetRewardText(progress, isFirstClear) : string.Empty);

        if (rewardIconImage == null) { return; }

        rewardIconImage.enabled =
            hasRecord &&
            UISkin.ApplySimple(
                rewardIconImage,
                isFirstClear ? UIKeys.RewardChestClosed : UIKeys.RewardChestOpen,
                true
            ); // 상자 그림이 없으면 글자로만 알린다.
    }

    // 받을 보상. 전투 종류마다 주는 재화가 다르다. 그리모어 강화로 늘어나는 양은 넣지 않은 기본 값이다.
    private string GetRewardText(PlayerProgressManager progress, bool isFirstClear)
    {
        int regionOrder = BattleSetup.RegionOrder;
        BattleDifficulty difficulty = BattleSetup.Difficulty;

        string gold = UISkin.IconOr(UIIcons.Gold, "골드");
        string experience = UISkin.IconOr(UIIcons.Exp, "경험치");
        string essence = UISkin.IconOr(UIIcons.Essence, "정수");
        string shard = UISkin.IconOr(UIIcons.Shard, "파편");

        StringBuilder builder = new StringBuilder();

        builder.Append(
            isFirstClear
                ? "최초 승리 보상:  "
                : $"반복 승리 보상 (최초의 {StageRules.RepeatRewardPercent}%):  "
        );

        if (BattleSetup.Kind == BattleKind.Normal)
        {
            int stage = BattleSetup.StageNumber;

            builder.Append(
                $"{gold} {StageRules.GetNormalGold(stage, regionOrder, difficulty, isFirstClear)}    " +
                $"{experience} {StageRules.GetNormalExperience(stage, regionOrder, difficulty, isFirstClear)}    " +
                $"{essence} {StageRules.GetNormalEssence(stage, regionOrder, difficulty, isFirstClear)}\n"
            );
        }
        else if (BattleSetup.Kind == BattleKind.Capture)
        {
            builder.Append(
                $"{experience} {StageRules.GetCaptureExperience(regionOrder, isFirstClear)}    " +
                $"{essence} {StageRules.GetCaptureEssence(BattleSetup.Enemies.Count, isFirstClear)}    골드 없음\n" +
                "이기면 쓰러뜨린 마물을 모두 얻고, 지면 얻지 못합니다.\n"
            );
        }
        else if (BattleSetup.Heroine != null)
        {
            HeroineBattleData heroine = BattleSetup.Heroine;

            builder.Append(
                $"{gold} {HeroineBattleRules.GetGold(heroine, regionOrder, difficulty, isFirstClear)}    " +
                $"{experience} {HeroineBattleRules.GetExperience(heroine, regionOrder, difficulty, isFirstClear)}    " +
                $"{shard} {HeroineBattleRules.GetShards(heroine, difficulty, isFirstClear)}\n"
            );

            bool givesClearReward =
                BattleSetup.ClearsRegion &&
                progress != null &&
                !progress.IsRegionCleared(progress.CurrentRegion);

            if (givesClearReward)
            {
                builder.Append(
                    $"지역 클리어 보상:  {gold} {HeroineBattleRules.GetRegionClearGold(regionOrder)}    " +
                    $"{shard} {HeroineBattleRules.GetRegionClearShards(regionOrder)}\n"
                );
            }
        }
        else
        {
            builder.Append("시험 보상\n");
        }

        if (BattleSetup.CanChangeDifficulty && progress != null)
        {
            builder.Append("승리 기록:");

            foreach (BattleDifficulty each in StageRules.Difficulties)
            {
                bool isCleared = progress.IsStageCleared(BattleSetup.StageId, each);

                builder.Append($"  {StageRules.GetDifficultyName(each)} {(isCleared ? "승리" : "없음")}");
            }

            builder.Append("\n");
        }

        builder.Append($"패배하면 승리 경험치의 {StageRules.DefeatExperiencePercent}%만 받습니다.");

        return builder.ToString();
    }
}
