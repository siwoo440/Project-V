\# Project V 개발 일지



\## 10일차: 방어력 및 공통 피해 계산 구현



\### 개발 목표



마물과 히로인에게 방어력을 추가하고, 모든 단일 및 광역 공격이 공통 피해 계산식을 사용하도록 전투 계산 구조를 구현한다.



\### 피해 공식



최종 피해는 공격력에서 방어력을 뺀 값으로 계산한다.



\- 공격력이 0 이하이면 피해는 0이다.

\- 공격력이 1 이상이면 최소 1의 피해를 준다.

\- 방어력이 공격력보다 높아도 최소 1의 피해를 준다.



\### 구현 내용



\- DamageCalculator 공통 피해 계산 클래스 생성

\- 공격력과 방어력 기반 최종 피해 계산

\- 최소 피해 1 적용

\- 공격력 0 이하 피해 차단

\- MonsterData에 Defense 추가

\- 마물별 방어력 수치 설정

\- MonsterUnit 방어력 속성 추가

\- MonsterUnit UI에 DEF 표시

\- MonsterUnit TakeDamage 반환값을 실제 피해량으로 변경

\- 히로인 방어력 설정 추가

\- 히로인 방어력 UI 표시

\- 마물 공격에 히로인 방어력 적용

\- 히로인 단일 공격에 마물 방어력 적용

\- 히로인 광역 공격에 마물별 방어력 적용

\- 전투 결과 문구에 실제 피해량 표시



\### 생성 및 수정 파일



\- `Assets/ProjectV/Scripts/Battle/DamageCalculator.cs`

\- `Assets/ProjectV/Scripts/Battle/MonsterData.cs`

\- `Assets/ProjectV/Scripts/Battle/MonsterUnit.cs`

\- `Assets/ProjectV/Scripts/Battle/BattleManager.cs`

\- `Assets/ProjectV/Prefabs/UI/MonsterUnit.prefab`

\- `Assets/ProjectV/Scenes/BattleScene.unity`

\- `Devlogs/Day10/README.md`



\### 마물 기본 능력치



| 마물 | HP | ATK | DEF |

|---|---:|---:|---:|

| Slime | 6 | 2 | 0 |

| Goblin | 8 | 3 | 1 |

| Tentacle | 10 | 4 | 2 |

| Imp | 5 | 2 | 0 |

| Skeleton | 9 | 3 | 2 |



\### 테스트 결과



\- 마물 방어력이 UI에 정상적으로 표시되는 것을 확인했다.

\- 마물 방어력에 따라 히로인 공격 피해가 감소하는 것을 확인했다.

\- 히로인 방어력에 따라 마물 공격 피해가 감소하는 것을 확인했다.

\- 광역 공격에서 마물별 방어력이 개별 적용되는 것을 확인했다.

\- 방어력이 공격력보다 높아도 최소 1 피해가 적용되는 것을 확인했다.

\- 공격력이 0이면 피해가 발생하지 않는 것을 확인했다.

\- 결과 문구가 실제 적용된 피해량을 표시하는 것을 확인했다.

\- Unity Console Error 0건을 확인했다.



\### 미구현 항목



\- 피해 배율

\- 치명타

\- 회피

\- 보호막

\- 방어력 증가 및 감소 상태

\- 피해 증가 및 감소 상태

\- 속성 상성

\- 방어 무시

\- 고정 피해

\- 피해량 팝업과 애니메이션



\### 다음 개발 방향



11일차에는 보호막을 추가하고, 피해가 HP보다 보호막에 먼저 적용되는 피해 흡수 구조를 구현한다.

