using TMPro; // TextMeshPro 기능
using UnityEngine; // Unity 기본 기능

public class MonsterUnit : MonoBehaviour // 필드 마물 UI 관리
{ // 클래스 시작
    [SerializeField] private TMP_Text monsterNameText; // 마물 이름 텍스트
    [SerializeField] private TMP_Text monsterHpText; // 마물 체력 텍스트
    [SerializeField] private TMP_Text monsterAttackText; // 마물 공격력 텍스트

    private MonsterData monsterData; // 연결 마물 데이터
    private int currentHp; // 현재 마물 체력

    public int Attack => monsterData != null ? monsterData.Attack : 0; // 현재 마물 공격력 반환

    public void Initialize(MonsterData data) // 마물 정보 초기화
    { // 메서드 시작
        monsterData = data; // 마물 데이터 저장
        currentHp = monsterData.MaxHp; // 현재 체력 초기화
        UpdateMonsterUI(); // 마물 UI 갱신
    } // 메서드 끝

    private void UpdateMonsterUI() // 마물 UI 표시 갱신
    { // 메서드 시작
        monsterNameText.text = monsterData.MonsterName; // 마물 이름 표시
        monsterHpText.text = $"HP: {currentHp} / {monsterData.MaxHp}"; // 마물 체력 표시
        monsterAttackText.text = $"ATK: {monsterData.Attack}"; // 마물 공격력 표시
    } // 메서드 끝
} // 클래스 끝