\# Project V 개발 일지



\## 5일차: 마물 소환 대기 및 수동 공격 구현



\### 개발 목표



소환된 마물이 첫 턴에는 공격하지 못하도록 대기 상태를 적용하고, 다음 플레이어 턴부터 마물을 직접 선택하여 히로인을 공격하는 수동 공격 기능을 구현한다.



\### 구현 내용



\- MonsterActionState 행동 상태 열거형 생성

\- Summoning, Ready, Acted 상태 정의

\- MonsterUnit에 현재 행동 상태 저장 기능 추가

\- 소환된 마물의 초기 상태를 Summoning으로 설정

\- 소환된 턴에는 마물 선택과 공격 차단

\- 다음 플레이어 턴에 Ready 상태로 전환

\- Ready 상태의 마물 선택 기능 구현

\- 선택된 마물의 배경색 변경

\- 기존 마물 선택 후 다른 마물 선택 가능

\- MonsterAttackButton UI 생성

\- 선택한 마물의 수동 공격 기능 구현

\- 마물 공격력만큼 히로인 HP 감소

\- 공격한 마물을 Acted 상태로 변경

\- Acted 상태의 마물 재선택 및 중복 공격 차단

\- 다음 플레이어 턴에 Acted 상태를 Ready로 복구

\- 턴 종료 시 모든 마물이 자동으로 공격하던 기능 제거

\- 여러 마물을 플레이어가 각각 선택하여 공격 가능

\- 히로인 턴과 전투 종료 시 선택 상태 해제

\- 마물 사망 시 잘못된 선택 참조 제거

\- 마물 행동 상태를 UI 텍스트로 표시



\### 생성 및 수정 파일



\- `Assets/ProjectV/Scripts/Battle/MonsterActionState.cs`

\- `Assets/ProjectV/Scripts/Battle/MonsterUnit.cs`

\- `Assets/ProjectV/Scripts/Battle/BattleManager.cs`

\- `Assets/ProjectV/Prefabs/UI/MonsterUnit.prefab`

\- `Assets/ProjectV/Scenes/BattleScene.unity`

\- `Devlogs/Day05/README.md`



\### 마물 행동 상태



| 상태 | 설명 |

|---|---|

| Summoning | 해당 턴에 소환되어 공격할 수 없는 상태 |

| Ready | 플레이어가 선택하여 공격할 수 있는 상태 |

| Acted | 해당 턴의 공격을 완료한 상태 |



\### 현재 마물 공격 흐름



1\. 플레이어가 마물 카드를 사용한다.

2\. 마물이 Summoning 상태로 필드에 소환된다.

3\. 소환된 턴에는 해당 마물을 선택할 수 없다.

4\. 플레이어가 턴을 종료한다.

5\. 히로인이 마물 또는 플레이어를 공격한다.

6\. 다음 플레이어 턴에 기존 마물이 Ready 상태가 된다.

7\. 플레이어가 Ready 상태의 마물을 선택한다.

8\. 선택된 마물의 배경색이 변경된다.

9\. 플레이어가 Attack 버튼을 누른다.

10\. 마물 공격력만큼 히로인 HP가 감소한다.

11\. 공격한 마물은 Acted 상태가 된다.

12\. Acted 상태의 마물은 같은 턴에 다시 공격할 수 없다.

13\. 다음 플레이어 턴에 다시 Ready 상태가 된다.



\### 테스트 결과



\- 소환된 마물이 Waiting 상태로 생성되는 것을 확인했다.

\- 소환된 턴에는 마물을 선택할 수 없는 것을 확인했다.

\- 소환된 턴에는 마물이 공격할 수 없는 것을 확인했다.

\- 다음 플레이어 턴에 마물이 Ready 상태가 되는 것을 확인했다.

\- Ready 상태의 마물을 클릭하여 선택할 수 있는 것을 확인했다.

\- 선택한 마물의 배경색이 변경되는 것을 확인했다.

\- 마물을 선택하면 Attack 버튼이 활성화되는 것을 확인했다.

\- Attack 버튼을 누르면 선택한 마물만 공격하는 것을 확인했다.

\- 마물 공격력만큼 히로인 HP가 감소하는 것을 확인했다.

\- 공격한 마물이 Acted 상태로 변경되는 것을 확인했다.

\- Acted 상태의 마물이 같은 턴에 다시 공격할 수 없는 것을 확인했다.

\- 다음 플레이어 턴에 마물이 다시 Ready 상태가 되는 것을 확인했다.

\- 여러 마물이 각각 한 번씩 공격할 수 있는 것을 확인했다.

\- 턴 종료 시 추가 자동 공격이 발생하지 않는 것을 확인했다.

\- 전투 종료 후 카드, 마물, 공격 버튼이 비활성화되는 것을 확인했다.

\- Unity Console Error 0건을 확인했다.



\### 오류 수정



마물 소환 시 `MonsterUnit.Initialize()`에서 NullReferenceException이 발생했다.



원인은 `MonsterUnit.prefab`의 `Select Button` 필드에 Button 컴포넌트가 연결되지 않은 것이었다.



다음 Inspector 참조를 연결하여 수정했다.



\- Select Button: 최상위 MonsterUnit의 Button

\- Background Image: 최상위 MonsterUnit의 Image

\- Monster State Text: MonsterStateText



\### 미구현 항목



\- 마물 공격 대상 선택

\- 적 소환체

\- 히로인 행동 예고

\- 히로인 단일 및 광역 스킬

\- 마물 액티브 스킬

\- 마물 패시브 스킬

\- 마물 공격 애니메이션

\- 히로인 공격 애니메이션

\- 피해량 팝업

\- 카드 및 마물 툴팁

\- 전투 재시작



\### 다음 개발 방향



6일차에는 히로인의 다음 행동을 미리 표시하고, 단일 공격과 광역 공격을 구분하는 기본 히로인 행동 패턴을 구현한다.

