\# Project V 개발 일지



\## 2일차: 카드 드로우 및 마나 소비 구현



\### 개발 목표



전투 화면에 임시 카드 덱과 손패를 추가하고, 카드 사용에 따른 마나 소비와 히로인 피해 기능을 구현한다.



\### 구현 내용



\- CardData ScriptableObject 구조 생성

\- 카드 ID, 카드 이름, 마나 비용, 피해량 데이터 구성

\- 임시 카드 데이터 5종 생성

\- CardButton UI 프리팹 생성

\- HandPanel 가로 레이아웃 구성

\- 전투 시작 시 시작 손패 3장 생성

\- 플레이어 턴 시작 시 카드 1장 드로우

\- 카드 클릭 시 마나 비용 확인

\- 마나 부족 시 카드 사용 차단

\- 카드 사용 시 히로인 HP 감소

\- 사용 카드 손패 제거

\- 사용 카드 버린 카드 더미 이동

\- 드로우 더미 소진 시 버린 카드 더미 재사용

\- 히로인 HP 0 도달 시 승리 처리

\- 승리 후 카드 버튼과 턴 종료 버튼 비활성화



\### 생성 및 수정 파일



\- `Assets/\_ProjectV/Scripts/Battle/CardData.cs`

\- `Assets/\_ProjectV/Scripts/Battle/BattleManager.cs`

\- `Assets/\_ProjectV/Data/Cards/C001\_SlimeStrike.asset`

\- `Assets/\_ProjectV/Data/Cards/C002\_GoblinRaid.asset`

\- `Assets/\_ProjectV/Data/Cards/C003\_TentacleWhip.asset`

\- `Assets/\_ProjectV/Data/Cards/C004\_ImpBolt.asset`

\- `Assets/\_ProjectV/Data/Cards/C005\_SkeletonSlash.asset`

\- `Assets/\_ProjectV/Prefabs/UI/CardButton.prefab`

\- `Devlogs/Day02/README.md`



\### 테스트 결과



\- 전투 시작 시 카드 3장이 손패에 생성되는 것을 확인했다.

\- 턴 시작 시 카드 1장이 추가되는 것을 확인했다.

\- 카드 사용 시 카드 비용만큼 마나가 감소하는 것을 확인했다.

\- 마나가 부족하면 카드 사용이 차단되는 것을 확인했다.

\- 카드 사용 시 히로인 HP가 감소하는 것을 확인했다.

\- 사용한 카드가 손패에서 제거되는 것을 확인했다.

\- 히로인 HP가 0이 되면 Victory가 표시되는 것을 확인했다.

\- 승리 후 카드와 턴 종료 버튼이 비활성화되는 것을 확인했다.

\- Unity Console Error 0건을 확인했다.



\### 미구현 항목



\- 실제 마물 소환

\- 마물 필드 슬롯

\- 마물 공격

\- 카드 타입 구분

\- 카드 드로우 애니메이션

\- 덱 셔플

\- 손패 최대 수 제한

\- 성욕 게이지 피해

\- 히로인 스킬 패턴



\### 다음 개발 방향



3일차에는 카드 사용으로 마물을 필드에 소환하고, 소환된 마물이 히로인을 공격하는 기본 마물 전투 기능을 구현한다.

