using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterCollectionUI : MonoBehaviour
{
    [Header("도감 패널")]
    [SerializeField] private GameObject collectionPanel;

    [Header("도감 UI")]
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private Transform listContainer;
    [SerializeField] private Button monsterButtonPrefab;
    [SerializeField] private TMP_Text detailText;

    private readonly List<Button> generatedButtons =
        new List<Button>();

    private void Start()
    {
        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.ProgressChanged +=
                Refresh;
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.ProgressChanged -=
                Refresh;
        }
    }

    public void OpenPanel()
    {
        if (collectionPanel != null)
        {
            collectionPanel.SetActive(true);
        }

        Refresh();
    }

    public void ClosePanel()
    {
        if (collectionPanel != null)
        {
            collectionPanel.SetActive(false);
        }
    }

    public void Refresh()
    {
        ClearButtons();

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null)
        {
            if (summaryText != null)
            {
                summaryText.text =
                    "진행 데이터가 없습니다";
            }

            if (detailText != null)
            {
                detailText.text = "마물 데이터가 없습니다";
            }

            return;
        }

        if (summaryText != null)
        {
            summaryText.text =
                $"{UISkin.IconOr(UIIcons.Gold, "골드")} {progress.Gold} | " +
                $"{UISkin.IconOr(UIIcons.Essence, "정수")} {progress.MonsterEssence} | " +
                $"플레이어 {progress.PlayerLevelText} | " +
                $"보유 마물 {progress.OwnedMonsters.Count}";
        }

        if (progress.OwnedMonsters.Count == 0)
        {
            if (detailText != null)
            {
                detailText.text = "보유한 마물이 없습니다";
            }

            return;
        }

        foreach (OwnedMonsterData ownedMonster in progress.OwnedMonsters)
        {
            if (ownedMonster == null ||
                ownedMonster.MonsterData == null)
            {
                continue;
            }

            CreateMonsterButton(ownedMonster);
        }

        ShowMonsterDetail(progress.OwnedMonsters[0]);
    }

    private void CreateMonsterButton(
        OwnedMonsterData ownedMonster
    )
    {
        if (monsterButtonPrefab == null ||
            listContainer == null)
        {
            return;
        }

        Button newButton = Instantiate(
            monsterButtonPrefab,
            listContainer
        );

        TMP_Text buttonText =
            newButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.text =
                $"{ownedMonster.MonsterData.MonsterName} " +
                $"Lv.{ownedMonster.Level} " +
                $"x{GetOwnedCardCount(ownedMonster)}";
        }

        OwnedMonsterData targetMonster = ownedMonster;

        newButton.onClick.RemoveAllListeners();
        newButton.onClick.AddListener(
            () => ShowMonsterDetail(targetMonster)
        );

        generatedButtons.Add(newButton);
    }

    private void ShowMonsterDetail(
        OwnedMonsterData ownedMonster
    )
    {
        if (detailText == null) { return; }

        if (ownedMonster == null ||
            ownedMonster.MonsterData == null)
        {
            detailText.text = "마물 데이터가 없습니다";
            return;
        }

        detailText.text =
            $"{ownedMonster.MonsterData.MonsterName}\n" +
            $"ID {ownedMonster.MonsterData.MonsterId}\n" +
            $"희귀도 " +
            $"{CardRarityRules.GetDisplayName(ownedMonster.MonsterData.Rarity)}\n" +
            $"강화 단계 Lv.{ownedMonster.Level} / " +
            $"Lv.{CardEnhanceRules.MaxLevel}\n" +
            $"보유 {GetOwnedCardCount(ownedMonster)} / " +
            $"{ownedMonster.MonsterData.MaxOwnedCopies}\n\n" +
            $"HP {ownedMonster.MaxHp}\n" +
            $"공격 {ownedMonster.Attack}\n" +
            $"성욕 {ownedMonster.LustDamage}\n" +
            $"방어 {ownedMonster.Defense}";
    }

    private int GetOwnedCardCount(
        OwnedMonsterData ownedMonster
    )
    {
        if (ownedMonster == null ||
            ownedMonster.MonsterData == null)
        {
            return 0; // 빈 데이터 차단
        }

        PlayerProgressManager progress =
            PlayerProgressManager.Instance;

        if (progress == null) { return 0; } // 진행 데이터 누락 차단

        return progress.GetOwnedCardCount(
            ownedMonster.MonsterData.CaptureRewardCard
        ); // 보유 카드 수량 반환
    }

    private void ClearButtons()
    {
        foreach (Button generatedButton in generatedButtons)
        {
            if (generatedButton == null) { continue; }

            generatedButton.gameObject.SetActive(false);
            Destroy(generatedButton.gameObject);
        }

        generatedButtons.Clear();
    }
}