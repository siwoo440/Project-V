\# Project V 개발 일지



\## 9일차: 히로인 행동 대상 선택 규칙 구현



\### 개발 목표



히로인 행동 데이터에 공격 대상 규칙을 추가하고, 행동마다 첫 번째 마물, 무작위 마물, 최저 HP 마물, 모든 마물 또는 플레이어를 선택할 수 있는 대상 결정 기능을 구현한다.



\### 구현 내용



\- HeroineTargetType 열거형 생성

\- HeroineActionData에 Target Type 추가

\- MonsterUnit 현재 HP 외부 조회 기능 추가

\- 첫 번째 마물 대상 선택 구현

\- 무작위 마물 대상 선택 구현

\- 현재 HP가 가장 낮은 마물 대상 선택 구현

\- 전체 마물 대상 선택 구현

\- 플레이어 직접 대상 선택 구현

\- 마물 대상 행동의 대상 부재 처리

\- 필드에 마물이 없으면 플레이어 대체 공격

\- 대상 마물 사망 시 필드 목록 제거

\- 선택된 마물 사망 시 선택 상태 해제

\- 행동 예고 UI에 대상 규칙 표시

\- Chaotic Strike 무작위 대상 행동 생성

\- Soul Pierce 플레이어 직접 공격 행동 생성



\### 생성 및 수정 파일



\- `Assets/ProjectV/Scripts/Battle/HeroineTargetType.cs`

\- `Assets/ProjectV/Scripts/Battle/HeroineActionData.cs`

\- `Assets/ProjectV/Scripts/Battle/MonsterUnit.cs`

\- `Assets/ProjectV/Scripts/Battle/BattleManager.cs`

\- `Assets/ProjectV/Data/HeroineActions/HA001\_PreciseStrike.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA002\_WideSweep.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA003\_DesperateSweep.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA004\_DesperateStrike.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA005\_ChaoticStrike.asset`

\- `Assets/ProjectV/Data/HeroineActions/HA006\_SoulPierce.asset`

\- `Assets/ProjectV/Scenes/BattleScene.unity`

\- `Devlogs/Day09/README.md`



\### 대상 선택 규칙



| 대상 규칙 | 설명 |

|---|---|

| FirstMonster | 가장 먼저 소환된 마물 공격 |

| RandomMonster | 필드의 무작위 마물 공격 |

| LowestHpMonster | 현재 HP가 가장 낮은 마물 공격 |

| AllMonsters | 필드의 모든 마물 공격 |

| Player | 마물을 무시하고 플레이어 직접 공격 |



\### 테스트 결과



\- FirstMonster 행동이 첫 번째 마물을 공격하는 것을 확인했다.

\- RandomMonster 행동이 필드의 무작위 마물을 공격하는 것을 확인했다.

\- LowestHpMonster 행동이 현재 HP가 가장 낮은 마물을 공격하는 것을 확인했다.

\- AllMonsters 행동이 모든 마물을 공격하는 것을 확인했다.

\- Player 행동이 마물을 무시하고 플레이어를 공격하는 것을 확인했다.

\- 마물 대상 행동에서 필드가 비어 있으면 플레이어가 피해를 받는 것을 확인했다.

\- 대상 마물 사망 시 필드에서 제거되는 것을 확인했다.

\- 행동 예고와 실제 공격 대상 규칙이 일치하는 것을 확인했다.

\- Unity Console Error 0건을 확인했다.



\### 미구현 항목



\- 최고 공격력 마물 대상

\- 무작위 대상 고정 시드

\- 공격 대상 미리 강조

\- 히로인 방어 행동

\- 히로인 회복 행동

\- 상태 이상 공격

\- 도발 마물 우선 공격

\- 대상 규칙 우선순위

\- 행동 아이콘

\- 공격 애니메이션



\### 다음 개발 방향



10일차에는 마물의 방어력과 피해 계산식을 추가하고, 히로인 공격 피해가 마물 방어력에 따라 감소하도록 기본 전투 계산 구조를 구현한다.

