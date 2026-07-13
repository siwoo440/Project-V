\# Project V 개발 일지



\## 3일차: 마물 소환 및 기본 공격 구현



\### 개발 목표



카드 사용 결과를 직접 피해에서 마물 소환으로 변경하고, 소환된 마물이 플레이어 턴 종료 시 히로인을 공격하는 기본 전투 구조를 구현한다.



\### 구현 내용



\- MonsterData ScriptableObject 구조 생성

\- 마물 ID, 이름, 최대 HP, 공격력 데이터 구성

\- 임시 마물 데이터 5종 생성

\- CardData와 MonsterData 연결

\- 카드별 소환 마물 설정

\- MonsterUnit UI 프리팹 생성

\- 마물 이름, HP, 공격력 UI 표시

\- MonsterFieldContainer 가로 배치 구조 생성

\- 카드 사용 시 마나 소비

\- 카드 사용 시 지정된 마물 소환

\- 소환 후 사용 카드 손패 제거

\- 사용 카드 버린 카드 더미 이동

\- 플레이어 턴 종료 시 모든 마물의 공격력 합산

\- 마물 공격력 합계만큼 히로인 HP 감소

\- 필드 최대 마물 수 8체 제한

\- 필드가 가득 찬 경우 추가 소환 차단

\- 마물 데이터가 없는 카드 사용 차단

\- 마물 공격으로 히로인 HP가 0이 되면 승리 처리

\- 전투 종료 후 카드와 턴 종료 버튼 비활성화



\### 생성 및 수정 파일



\- `Assets/\_ProjectV/Scripts/Battle/MonsterData.cs`

\- `Assets/\_ProjectV/Scripts/Battle/MonsterUnit.cs`

\- `Assets/\_ProjectV/Scripts/Battle/CardData.cs`

\- `Assets/\_ProjectV/Scripts/Battle/BattleManager.cs`

\- `Assets/\_ProjectV/Data/Monsters/M001\_Slime.asset`

\- `Assets/\_ProjectV/Data/Monsters/M002\_Goblin.asset`

\- `Assets/\_ProjectV/Data/Monsters/M003\_Tentacle.asset`

\- `Assets/\_ProjectV/Data/Monsters/M004\_Imp.asset`

\- `Assets/\_ProjectV/Data/Monsters/M005\_Skeleton.asset`

\- `Assets/\_ProjectV/Prefabs/UI/MonsterUnit.prefab`

\- `Assets/\_ProjectV/Scenes/BattleScene.unity`

\- `Devlogs/Day03/README.md`



\### 테스트 결과



\- 카드 사용 시 지정된 마물이 필드에 생성되는 것을 확인했다.

\- 카드 비용만큼 현재 마나가 감소하는 것을 확인했다.

\- 사용한 카드가 손패에서 제거되는 것을 확인했다.

\- 소환된 마물의 이름, HP, 공격력이 표시되는 것을 확인했다.

\- 여러 마물을 필드에 동시에 소환할 수 있는 것을 확인했다.

\- 턴 종료 시 모든 마물의 공격력이 합산되는 것을 확인했다.

\- 합산된 공격력만큼 히로인 HP가 감소하는 것을 확인했다.

\- 필드에 마물 8체가 존재하면 추가 소환이 차단되는 것을 확인했다.

\- 필드가 가득 찬 경우 `Field Full` 문구가 표시되는 것을 확인했다.

\- 히로인 HP가 0이 되면 `Victory`가 표시되는 것을 확인했다.

\- 승리 후 카드와 턴 종료 버튼이 비활성화되는 것을 확인했다.

\- Unity Console Error 0건을 확인했다.



\### 현재 전투 흐름



1\. 전투 시작 시 카드 3장을 드로우한다.

2\. 플레이어가 마나를 사용하여 카드를 선택한다.

3\. 카드와 연결된 마물이 필드에 소환된다.

4\. 사용한 카드는 손패에서 제거된다.

5\. 플레이어가 턴 종료 버튼을 누른다.

6\. 필드의 모든 마물이 히로인을 공격한다.

7\. 히로인이 플레이어에게 고정 피해를 준다.

8\. 다음 플레이어 턴에 최대 마나가 증가한다.

9\. 새로운 카드 1장을 드로우한다.

10\. 플레이어 또는 히로인 HP가 0이 될 때까지 반복한다.



\### 미구현 항목



\- 히로인의 마물 대상 공격

\- 마물의 피해 및 사망

\- 히로인 행동 패턴

\- 마물별 액티브 스킬

\- 마물별 패시브 스킬

\- 카드 덱 셔플

\- 손패 최대 수 제한

\- 마물 소환 애니메이션

\- 마물 공격 애니메이션

\- 마물 계열과 시너지

\- 성욕 게이지 공격

\- 전투 재시작



\### 다음 개발 방향



4일차에는 히로인이 필드의 마물을 공격하고, 피해를 받은 마물의 HP 감소와 사망 처리를 구현한다.

