\# Project V 개발 일지



\## 7일차: 히로인 행동 데이터와 조건부 AI 구현



\### 개발 목표



히로인의 행동 정보를 ScriptableObject 데이터로 분리하고, 현재 HP 조건과 행동 가중치에 따라 다음 행동을 선택하는 기본 AI 구조를 구현한다.



\### 구현 내용



\- HeroineActionData ScriptableObject 생성

\- 히로인 행동 ID와 표시 이름 데이터화

\- 히로인 행동 종류 데이터화

\- 행동 피해량 데이터화

\- 행동 선택 가중치 데이터화

\- 최소 및 최대 HP 조건 데이터화

\- Precise Strike 단일 공격 데이터 생성

\- Wide Sweep 기본 광역 공격 데이터 생성

\- Desperate Sweep 위기 광역 공격 데이터 생성

\- 현재 히로인 HP 비율 계산

\- HP 조건을 충족하는 행동 후보 생성

\- 사용 가능한 행동의 전체 가중치 계산

\- 가중치 기반 무작위 행동 선택

\- 선택된 행동 데이터의 피해량 적용

\- 행동 데이터 이름과 피해량 예고 UI 표시

\- HP 50% 이하 위기 행동 선택

\- 행동 데이터 누락 시 예외 방지



\### 생성 및 수정 파일



\- `Assets/ProjectV/Scripts/Battle/HeroineActionData.cs`

\- `Assets/ProjectV/Scripts/Battle/BattleManager.cs`

\- `Assets/ProjectV/Data/HeroineActions/HA001\_PreciseStrike.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA002\_WideSweep.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA003\_DesperateSweep.asset`

\- `Assets/ProjectV/Scenes/BattleScene.unity`

\- `Devlogs/Day07/README.md`



\### 히로인 행동 데이터



| 행동 | HP 조건 | 가중치 | 효과 |

|---|---|---:|---|

| Precise Strike | HP 50% 초과 | 60 | 첫 번째 마물에게 3 피해 |

| Wide Sweep | HP 50% 초과 | 40 | 모든 마물에게 2 피해 |

| Desperate Sweep | HP 50% 이하 | 100 | 모든 마물에게 4 피해 |



\### 테스트 결과



\- HP 50% 초과 상태에서 기본 행동이 선택되는 것을 확인했다.

\- Precise Strike와 Wide Sweep이 가중치에 따라 선택되는 것을 확인했다.

\- Precise Strike가 첫 번째 마물만 공격하는 것을 확인했다.

\- Wide Sweep이 모든 마물을 공격하는 것을 확인했다.

\- HP 50% 이하에서 Desperate Sweep이 선택되는 것을 확인했다.

\- Desperate Sweep이 모든 마물에게 4 피해를 주는 것을 확인했다.

\- 행동 데이터의 표시 이름이 예고 UI에 표시되는 것을 확인했다.

\- 예고된 행동과 실제 실행 행동이 일치하는 것을 확인했다.

\- 행동 데이터가 없을 때 예외 없이 처리되는 것을 확인했다.

\- Unity Console Error 0건을 확인했다.



\### 미구현 항목



\- 히로인별 행동 목록 분리

\- 행동 쿨타임

\- 연속 사용 제한

\- 강제 행동

\- 행동 대상 선택 규칙 데이터

\- 방어 및 회복 행동

\- 상태 이상 행동

\- 마물 공격 우선순위

\- 고정 난수 시드

\- 행동 애니메이션

\- 피해량 팝업



\### 다음 개발 방향



8일차에는 히로인 행동에 쿨타임과 연속 사용 제한을 추가하고, 특정 행동이 반복 선택되는 것을 방지하는 AI 제약 조건을 구현한다.

