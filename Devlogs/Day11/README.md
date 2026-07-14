\# Project V 개발 일지



\## 11일차: 보호막 및 피해 흡수 순서 구현



\### 개발 목표



플레이어, 히로인과 마물에게 보호막을 추가하고, 공격 피해가 보호막에 먼저 흡수된 후 남은 공격력에 방어력이 적용되는 피해 처리 구조를 구현한다.



\### 피해 처리 순서



1\. 공격력 확인

2\. 현재 보호막으로 공격력 흡수

3\. 남은 공격력 계산

4\. 남은 공격력에 방어력 적용

5\. 최종 HP 피해 계산

6\. 변경된 보호막과 HP 저장



\### 구현 내용



\- DamageResult 피해 결과 구조체 생성

\- 보호막 흡수량 저장

\- 실제 HP 피해량 저장

\- 피해 후 남은 보호막 저장

\- DamageCalculator 보호막 계산 기능 추가

\- MonsterData 시작 보호막 추가

\- 마물별 시작 보호막 설정

\- MonsterUnit 현재 보호막 관리

\- MonsterUnit 보호막 UI 표시

\- 플레이어 시작 보호막 추가

\- 플레이어 현재 보호막 관리

\- 플레이어 보호막 UI 표시

\- 히로인 시작 보호막 추가

\- 히로인 현재 보호막 관리

\- 히로인 보호막 UI 표시

\- 마물 공격에 히로인 보호막 적용

\- 히로인 단일 공격에 마물 보호막 적용

\- 히로인 광역 공격에 마물별 보호막 적용

\- 플레이어 직접 공격에 플레이어 보호막 적용

\- 전투 결과에 보호막 흡수량과 HP 피해량 표시



\### 생성 및 수정 파일



\- `Assets/ProjectV/Scripts/Battle/DamageResult.cs`

\- `Assets/ProjectV/Scripts/Battle/DamageCalculator.cs`

\- `Assets/ProjectV/Scripts/Battle/MonsterData.cs`

\- `Assets/ProjectV/Scripts/Battle/MonsterUnit.cs`

\- `Assets/ProjectV/Scripts/Battle/BattleManager.cs`

\- `Assets/ProjectV/Prefabs/UI/MonsterUnit.prefab`

\- `Assets/ProjectV/Scenes/BattleScene.unity`

\- `Devlogs/Day11/README.md`



\### 테스트 결과



\- 보호막이 방어력보다 먼저 피해를 흡수하는 것을 확인했다.

\- 보호막이 공격력을 모두 흡수하면 HP 피해가 0인 것을 확인했다.

\- 보호막을 통과한 공격에 방어력이 적용되는 것을 확인했다.

\- 플레이어 보호막이 직접 공격을 흡수하는 것을 확인했다.

\- 히로인 보호막이 마물 공격을 흡수하는 것을 확인했다.

\- 마물 보호막이 단일 공격을 흡수하는 것을 확인했다.

\- 광역 공격이 각 마물의 보호막에 개별 적용되는 것을 확인했다.

\- 보호막이 다음 턴에 자동으로 회복되지 않는 것을 확인했다.

\- 전투 결과에 보호막과 HP 피해량이 표시되는 것을 확인했다.

\- Unity Console Error 0건을 확인했다.



\### 미구현 항목



\- 보호막 획득 카드

\- 히로인 보호막 행동

\- 보호막 최대치

\- 보호막 지속 시간

\- 턴 종료 보호막 감소

\- 보호막 파괴 효과

\- 방어 무시 피해

\- 고정 피해

\- 피해량 팝업

\- 보호막 애니메이션



\### 다음 개발 방향



12일차에는 히로인의 방어 및 보호막 행동을 추가하고, 공격 행동뿐 아니라 자신의 보호막을 생성하는 AI 행동을 구현한다.

