using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterCollectionUI : MonoBehaviour
{
    [Header("Collection Panel")]
    [SerializeField] private GameObject collectionPanel;

    [Header("Collection UI")]
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
                    "Missing Player Progress Manager";
            }

            if (detailText != null)
            {
                detailText.text = "No Monster Data";
            }

            return;
        }

        if (summaryText != null)
        {
            summaryText.text =
                $"Gold: {progress.Gold} | " +
                $"Total EXP: {progress.TotalExperience} | " +
                $"Owned: {progress.OwnedMonsters.Count}";
        }

        if (progress.OwnedMonsters.Count == 0)
        {
            if (detailText != null)
            {
                detailText.text = "No Owned Monsters";
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
                $"x{ownedMonster.CopyCount}";
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
            detailText.text = "No Monster Data";
            return;
        }

        detailText.text =
            $"{ownedMonster.MonsterData.MonsterName}\n" +
            $"ID: {ownedMonster.MonsterData.MonsterId}\n" +
            $"Level: {ownedMonster.Level}\n" +
            $"EXP: {ownedMonster.CurrentExperience} / " +
            $"{ownedMonster.RequiredExperience}\n" +
            $"Copies: {ownedMonster.CopyCount}\n\n" +
            $"HP: {ownedMonster.MaxHp}\n" +
            $"ATK: {ownedMonster.Attack}\n" +
            $"LST: {ownedMonster.LustDamage}\n" +
            $"DEF: {ownedMonster.Defense}";
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