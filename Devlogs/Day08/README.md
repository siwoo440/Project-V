\# Project V 개발 일지



\## 8일차: 히로인 행동 쿨타임 및 반복 제한 구현



\### 개발 목표



히로인 행동 데이터에 쿨타임과 최대 연속 사용 횟수를 추가하고, 같은 행동이 지나치게 반복되는 것을 방지하는 AI 제약 조건을 구현한다.



\### 구현 내용



\- HeroineActionData에 Cooldown Turns 추가

\- HeroineActionData에 Max Consecutive Uses 추가

\- 행동별 남은 쿨타임 런타임 저장

\- 마지막 실행 행동 저장

\- 같은 행동의 연속 사용 횟수 저장

\- 쿨타임 중인 행동을 선택 후보에서 제외

\- 최대 연속 사용 횟수에 도달한 행동 제외

\- 행동 실행 후 쿨타임 적용

\- 다음 행동 선택 후 쿨타임 감소

\- 전투 시작 시 모든 AI 상태 초기화

\- Desperate Strike 위기 단일 공격 데이터 생성

\- HP 50% 이하 행동 후보 확장

\- 기본 행동과 위기 행동의 반복 제한 적용

\- 행동 데이터 누락 시 예외 방지



\### 생성 및 수정 파일



\- `Assets/ProjectV/Scripts/Battle/HeroineActionData.cs`

\- `Assets/ProjectV/Scripts/Battle/BattleManager.cs`

\- `Assets/ProjectV/Data/HeroineActions/HA001\_PreciseStrike.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA002\_WideSweep.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA003\_DesperateSweep.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA004\_DesperateStrike.asset`

\- `Assets/ProjectV/Scenes/BattleScene.unity`

\- `Devlogs/Day08/README.md`



\### 행동별 AI 제약



| 행동 | HP 조건 | 쿨타임 | 최대 연속 사용 |

|---|---|---:|---:|

| Precise Strike | HP 50% 초과 | 0 | 2 |

| Wide Sweep | HP 50% 초과 | 1 | 1 |

| Desperate Sweep | HP 50% 이하 | 1 | 1 |

| Desperate Strike | HP 50% 이하 | 1 | 1 |



\### 테스트 결과



\- 쿨타임 중인 행동이 다음 행동 후보에서 제외되는 것을 확인했다.

\- Wide Sweep이 바로 연속 선택되지 않는 것을 확인했다.

\- Precise Strike가 세 번 연속 선택되지 않는 것을 확인했다.

\- 마지막 행동과 연속 사용 횟수가 정상적으로 기록되는 것을 확인했다.

\- HP 50% 이하에서 위기 행동 두 종류가 선택되는 것을 확인했다.

\- Desperate Sweep과 Desperate Strike가 연속으로 반복되지 않는 것을 확인했다.

\- 새 전투 시작 시 쿨타임이 초기화되는 것을 확인했다.

\- 새 전투 시작 시 연속 사용 횟수가 초기화되는 것을 확인했다.

\- 행동 데이터가 부족해도 NullReferenceException이 발생하지 않는 것을 확인했다.

\- Unity Console Error 0건을 확인했다.



\### 미구현 항목



\- 행동 대상 선택 규칙 데이터

\- 무작위 마물 대상

\- 최저 HP 마물 대상

\- 최고 공격력 마물 대상

\- 플레이어 직접 공격 행동

\- 히로인 방어 행동

\- 히로인 회복 행동

\- 상태 이상 행동

\- 강제 행동

\- 행동별 아이콘과 애니메이션



\### 다음 개발 방향



9일차에는 히로인 행동 데이터에 공격 대상 규칙을 추가하고, 첫 번째 마물, 무작위 마물, 최저 HP 마물, 전체 마물을 구분하는 대상 선택 기능을 구현한다.

