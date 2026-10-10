using System.Collections; // 코루틴 기능
using System.Collections.Generic; // 리스트 기능
using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능
using UnityEngine.UI; // Unity UI 기능

public partial class BattleManager // 분리된 전투 기능
{
    private void AddBattleLog(BattleLogCategory category, string message) // 전투 로그 추가
    {
        if (battleLogUI == null) { return; } // 전투 로그 UI 누락 차단

        battleLogUI.AddEntry(turnNumber, category, message); // 현재 턴 전투 로그 등록
    }


    private void SetAttackButtonsInteractable(bool canAttack)
    {
        bool hasReadyMonster = selectedMonster != null && selectedMonster.CanAttack;

        if (monsterAttackButton != null)
        {
            monsterAttackButton.interactable = canAttack && hasReadyMonster;
        }

        if (skillButton != null)
        {
            skillButton.interactable =
                canAttack &&
                selectedMonster != null &&
                selectedMonster.CanUseSkill; // 스킬 사용 가능 시에만 활성화
        }

        if (lustAttackButton != null)
        {
            lustAttackButton.interactable =
                canAttack &&
                hasReadyMonster &&
                selectedMonster.LustDamage > 0;
        }
    }
    private void ShowPlayerTurn() // 플레이어 턴 UI 표시
    {
        turnText.text = "플레이어 턴"; // 플레이어 턴 문구 설정
        endTurnButton.interactable = true; // 턴 종료 버튼 활성화
        SetAttackButtonsInteractable(false); // 공격 버튼 초기 비활성화

        if (skillButton != null)
        {
            skillButton.onClick.RemoveAllListeners();
            skillButton.onClick.AddListener(UseSelectedMonsterSkill); // 스킬 버튼 연결
        }
        SetHandInteractable(true); // 손패 버튼 활성화
        SetMonsterInteractable(true); // 공격 가능 마물 선택 활성화
    }

    private void SetHandInteractable(bool isInteractable) // 손패 버튼 활성 상태 변경
    {
        foreach (Button handButton in handButtons) // 모든 손패 버튼 반복
        {
            if (handButton != null) // 버튼 존재 확인
            {
                handButton.interactable = isInteractable; // 버튼 활성 상태 적용
            }
        }
    }

    
    private void ClearHand() // 손패 버튼 전체 제거
    {
        foreach (Button handButton in handButtons) // 모든 손패 버튼 반복
        {
            if (handButton != null) // 버튼 존재 확인
            {
                Destroy(handButton.gameObject); // 카드 버튼 제거
            }
        }

        handButtons.Clear(); // 손패 버튼 목록 초기화
        handCardCopies.Clear(); // 손패 카드 연결 초기화
    }

    
    
    private void UpdateBattleUI() // 전투 수치 UI 갱신
    {
        int currentHeroineDefense = GetHeroineCurrentDefense(); // 상태 효과 포함 히로인 방어력 계산
        turnNumberText.text = $"{turnNumber}턴"; // 턴 번호 표시
        // 아이콘 이미지가 있으면 이름 대신 아이콘을 쓴다. (UISkin.IconOr)
        playerHpText.text = $"{UISkin.IconOr(UIIcons.Hp, "HP")} {playerCurrentHp} / {PlayerMaxHp}"; // 플레이어 체력 표시
        playerShieldText.text = $"{UISkin.IconOr(UIIcons.Shield, "보호막")} {playerCurrentShield}"; // 플레이어 보호막 표시
        manaText.text = $"{UISkin.IconOr(UIIcons.Mana, "마나")} {currentMana} / {maximumMana}"; // 마나 표시
        heroineHpText.text = $"{UISkin.IconOr(UIIcons.Hp, "HP")} {heroineCurrentHp} / {heroineMaxHp}"; // 히로인 체력 표시
        heroineDefenseText.text = $"{UISkin.IconOr(UIIcons.Defense, "방어")} {currentHeroineDefense}"; // 현재 히로인 방어력 표시
        heroineShieldText.text = $"{UISkin.IconOr(UIIcons.Shield, "보호막")} {heroineCurrentShield} / {heroineMaxShield}"; // 히로인 현재 및 최대 보호막 표시

        if (playerHpGauge != null) { playerHpGauge.SetValue(playerCurrentHp, PlayerMaxHp); } // 플레이어 체력 게이지
        if (manaGauge != null) { manaGauge.SetValue(currentMana, maximumMana); } // 마나 게이지
        if (heroineHpGauge != null) { heroineHpGauge.SetValue(heroineCurrentHp, heroineMaxHp); } // 히로인 체력 게이지
        if (heroineLustGauge != null) { heroineLustGauge.SetValue(heroineLust, heroineMaxLust); } // 히로인 성욕 게이지
        if (heroineStatusText != null) { heroineStatusText.text = GetHeroineStatusDisplay(); } // 히로인 상태 효과 표시
        RefreshHeroineStatusIcons(); // 히로인 상태 효과 아이콘 갱신
        if (lustText != null)
        {
            string lustLabel = UISkin.IconOr(UIIcons.Lust, "성욕");

            lustText.text =
                $"{lustLabel} {heroineLust} / {heroineMaxLust} {GetLustStageName()}"; // 정상, 동요, 절정 (기획서 11.8.1)
        }

        if (heroineLustSlider != null)
        {
            heroineLustSlider.minValue = 0;
            heroineLustSlider.maxValue = Mathf.Max(1, heroineMaxLust);
            heroineLustSlider.value = heroineLust;
        }
        UpdateDeckStatusUI();
        UpdateHeroineIntentUI(); // 히로인 행동 예고 표시
        RefreshHeroineArtIdle(); // 히로인 그림의 평소 상태 (기본 또는 동요)
        UpdateSummonerUI(); // 소환사 스킬 버튼과 패시브 표시
        UpdateBattleItemUI(); // 소모성 아이템 버튼
    }
    





}