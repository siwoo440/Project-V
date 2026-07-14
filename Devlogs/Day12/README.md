\# Project V 개발 일지



\## 12일차: 히로인 방어 및 보호막 행동 구현



\### 개발 목표



히로인이 공격 행동뿐 아니라 자신의 보호막을 획득하는 방어 행동을 사용할 수 있도록 AI 행동 종류와 보호막 획득 구조를 확장한다.



\### 구현 내용



\- HeroineActionType에 GainShield 추가

\- HeroineTargetType에 Self 추가

\- HeroineActionData에 Shield Amount 추가

\- Magic Barrier 보호막 행동 데이터 생성

\- 히로인 최대 보호막 설정 추가

\- 시작 보호막의 최대치 제한

\- 보호막 획득 행동 실행 기능 구현

\- 실제 보호막 획득량 계산

\- 최대 보호막 초과 획득 차단

\- 최대 보호막 상태의 방어 행동 선택 차단

\- 방어 행동에 쿨타임 적용

\- 방어 행동에 연속 사용 제한 적용

\- 행동 예고에 보호막 획득량 표시

\- 행동 예고에 Self 대상 표시

\- 현재 및 최대 보호막 UI 표시

\- HP 50% 이하 방어 행동 후보 추가



\### 생성 및 수정 파일



\- `Assets/ProjectV/Scripts/Battle/HeroineActionType.cs`

\- `Assets/ProjectV/Scripts/Battle/HeroineTargetType.cs`

\- `Assets/ProjectV/Scripts/Battle/HeroineActionData.cs`

\- `Assets/ProjectV/Scripts/Battle/BattleManager.cs`

\- `Assets/ProjectV/Data/HeroineActions/HA007\_MagicBarrier.asset`

\- `Assets/ProjectV/Scenes/BattleScene.unity`

\- `Devlogs/Day12/README.md`



\### Magic Barrier 데이터



| 항목 | 설정값 |

|---|---|

| 사용 조건 | 히로인 HP 50% 이하 |

| 보호막 획득 | 5 |

| 최대 보호막 | 10 |

| 가중치 | 30 |

| 쿨타임 | 2턴 |

| 연속 사용 | 최대 1회 |

| 대상 | Self |



\### 테스트 결과



\- Magic Barrier가 행동 예고 UI에 표시되는 것을 확인했다.

\- Magic Barrier 사용 시 공격이 발생하지 않는 것을 확인했다.

\- 히로인 보호막이 5 증가하는 것을 확인했다.

\- 보호막이 최대치 10을 초과하지 않는 것을 확인했다.

\- 최대치에 가까운 경우 실제 증가량만 표시되는 것을 확인했다.

\- 최대 보호막 상태에서 Magic Barrier가 후보에서 제외되는 것을 확인했다.

\- Magic Barrier에 쿨타임이 적용되는 것을 확인했다.

\- Magic Barrier가 연속으로 사용되지 않는 것을 확인했다.

\- 증가한 보호막이 마물 공격을 흡수하는 것을 확인했다.

\- Unity Console Error 0건을 확인했다.



\### 미구현 항목



\- 히로인 HP 회복 행동

\- 보호막 지속 시간

\- 턴 종료 보호막 감소

\- 마물 보호막 획득 스킬

\- 방어력 증가 행동

\- 방어력 감소 상태

\- 행동 애니메이션

\- 보호막 생성 효과

\- 보호막 파괴 효과

\- 행동 아이콘



\### 다음 개발 방향



13일차에는 히로인의 HP 회복 행동을 추가하고, 현재 HP가 최대 HP일 때 회복 행동이 선택되지 않도록 AI 조건을 확장한다.

