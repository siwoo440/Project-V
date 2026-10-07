\---



\# Project V 42일차 개발일지



\---



\## 1. 작업 목표



41일차까지의 성장 수단은 마물 카드 강화, 플레이어 레벨, 소환사 패시브였다. 히로인전에서 이겨도 소환사 자신에게 남는 영구 보상은 없었다. 기획서 6.6의 그리모어 영구 강화를 구현해 히로인전 보상인 욕망의 파편과 그 사용처를 만든다.



그리모어 강화는 찍은 노드가 모두 겹쳐서 모든 덱과 모든 전투에 적용된다. 하나만 골라 장착하는 패시브와 다른 점이다.



\---



\## 2. 구현 내용



\- 욕망의 파편 재화 추가

\- 전투 보상과 결과 화면에 욕망의 파편 추가

\- 그리모어 노드 데이터 구조와 노드 40개 추가

\- 노드 해금, 강화, 선행 노드 판정 추가

\- 그리모어 강화의 전투 적용 추가

\- 그리모어 강화 화면 추가

\- 메인 메뉴와 지역 선택에 그리모어 강화 버튼 추가

\- 재화 표시에 욕망의 파편 추가

\- 그리모어 화면용 이미지 요청문 작성과 이미지 적용

\- 메인 메뉴 버튼 높이 수정

\- 비용 감소와 덱 재구성 전투 기록 문구 수정



\---



\## 3. 욕망의 파편



기획서 6.6.1에 따라 그리모어 강화에는 욕망의 파편만 사용하고 골드는 쓰지 않는다.



| 항목 | 내용 |

| --- | --- |

| 획득처 | 메인 히로인전, 서브 히로인전, 지역 클리어 |

| 지급하지 않는 전투 | 일반전, 포획전, 패배한 전투 |

| 보유 한도 | 9,999 |

| 한도 초과분 | 버림 |



전투 보상 데이터에 욕망의 파편 항목을 추가했다. 0이면 지급하지 않으므로 일반전과 포획전의 보상 데이터는 그대로 둔다.



시험 전투는 기획서 A.36의 지역 1 메인 히로인 1차전 보통 난이도 기준으로 3개를 지급한다. 재전투의 60% 지급과 난이도별 수량은 스테이지 일차에 보상 공식과 함께 구현한다.



전투 결과 화면은 골드와 경험치 아래 줄에 욕망의 파편과 마물의 정수를 표시한다. 메인 메뉴, 전투 화면 하단, 도감 요약에도 보유한 파편을 표시한다.



\---



\## 4. 강화 규칙



기획서 6.6.5, 9.9.5, 부록 A.46의 규칙을 적용했다.



| 항목 | 값 |

| --- | --- |

| 분기 | 8개 |

| 분기당 노드 | 5개 |

| 전체 노드 | 40개 |

| 노드 최대 단계 | Lv.3 |

| Lv.0 → Lv.1 비용 | 3 |

| Lv.1 → Lv.2 비용 | 6 |

| Lv.2 → Lv.3 비용 | 9 |

| 노드 하나 최대 강화 | 18 |

| 전체 최대 강화 | 720 |



같은 분기의 앞 노드를 Lv.1 이상 해금해야 다음 노드가 열린다. 분기 사이에는 제한이 없어 어느 분기를 먼저 골라도 다른 분기가 잠기지 않는다.



기획서 6.6.7에 따라 강화는 초기화할 수 없다. 실수를 막기 위해 강화 버튼을 누르면 확인 안내가 나오고 한 번 더 눌러야 실행된다.



| 강화할 수 없는 조건 | 안내 |

| --- | --- |

| 최대 단계 | 이미 최대 단계입니다 |

| 선행 노드 미해금 | 필요한 선행 노드 이름 표시 |

| 파편 부족 | 보유량과 필요량 표시 |



\---



\## 5. 노드 목록



기획서 6.6.6에는 6개 분기에 노드 3개씩, 모두 18개의 효과 방향만 있다. 계열 분기와 포획 분기, 각 분기의 나머지 노드, 모든 수치는 임시로 정했다. 3단계의 추가 효과도 수치 증가로 통일했다.



표의 N은 단계별 수치이며 Lv.1 / Lv.2 / Lv.3 순서이다. 주력 계열은 덱에 가장 많은 계열을 뜻한다.



계약 분기: 플레이어 HP와 보호막



| 노드 | 효과 | 수치 |

| --- | --- | --- |

| 생명 계약 | 플레이어 최대 HP +N | 2 / 4 / 6 |

| 보호의 문장 | 전투를 시작할 때 플레이어 보호막 +N | 1 / 2 / 3 |

| 재생 계약 | 전투마다 한 번, 플레이어 HP가 절반 이하가 되면 HP N 회복 | 3 / 5 / 8 |

| 굳건한 서약 | 내 턴을 시작할 때 보호막이 없으면 보호막 +N | 1 / 2 / 3 |

| 최후의 계약 | 전투마다 한 번, 쓰러질 피해를 받아도 버틴다 (남는 HP N) | 1 / 3 / 5 |



소환 분기: 소환 마물의 HP와 생존력



| 노드 | 효과 | 수치 |

| --- | --- | --- |

| 생명 공명 | 소환한 마물의 최대 HP +N | 1 / 2 / 3 |

| 안정된 소환 | 소환한 마물에게 보호막 +N | 1 / 2 / 3 |

| 죽음의 기록 | 아군 마물이 쓰러지면 플레이어 보호막 +N (턴마다 한 번) | 1 / 2 / 3 |

| 치유의 인장 | 내 턴을 시작할 때 가장 많이 다친 마물의 HP N 회복 | 2 / 4 / 6 |

| 선봉의 축복 | 전투마다 처음 소환한 마물의 최대 HP +N | 3 / 6 / 9 |



지휘 분기: 마물 공격과 행동 보조



| 노드 | 효과 | 수치 |

| --- | --- | --- |

| 공격 명령 | 모든 마물의 공격 +N | 1 / 2 / 3 |

| 신속 명령 | 집중 명령, 계약 보호막, 욕망 공명 스킬의 효과 +N | 1 / 2 / 3 |

| 군단 지휘 | 필드에 마물이 4체 이상이면 모든 마물의 공격 +N | 1 / 2 / 3 |

| 대기 명령 | 턴을 마칠 때 행동하지 않은 마물 1체마다 플레이어 보호막 +1 (최대 N) | 1 / 2 / 3 |

| 결전 명령 | 히로인의 HP가 절반 이하이면 모든 마물의 공격 +N | 1 / 2 / 3 |



마나 분기: 시작 마나와 임시 마나



| 노드 | 효과 | 수치 |

| --- | --- | --- |

| 마나 그릇 | 전투 첫 턴에 임시 마나 +N | 1 / 2 / 3 |

| 마나 회수 | 아군 마물이 쓰러지면 다음 내 턴에 임시 마나 +1 (턴마다 최대 N) | 1 / 2 / 3 |

| 저비용 계약 | 전투마다 처음 내는 비용 1 이상 카드의 비용 -N | 1 / 2 / 3 |

| 마나 비축 | 쓰지 않은 마나를 다음 턴의 임시 마나로 넘긴다 (최대 N) | 1 / 2 / 3 |

| 여분의 마력 | 내 턴을 시작할 때 손패가 N장 이하이면 임시 마나 +1 | 2 / 3 / 4 |



기억 분기: 드로우와 손패 관리



| 노드 | 효과 | 수치 |

| --- | --- | --- |

| 기억 확장 | 시작 손패 +N장 | 1 / 2 / 3 |

| 재기록 | N턴마다 카드를 1장 더 뽑는다 | 4 / 3 / 2 |

| 순환 기록 | 덱을 다시 섞을 때 카드를 N장 더 뽑는다 | 1 / 2 / 3 |

| 잔류 기억 | 아군 마물이 쓰러지면 카드 1장 드로우 (전투마다 N번) | 1 / 2 / 3 |

| 지휘의 기억 | 소환사 스킬을 쓰면 카드 1장 드로우 (전투마다 N번) | 1 / 2 / 3 |



욕망 분기: 성욕 부여와 절정 공략



| 노드 | 효과 | 수치 |

| --- | --- | --- |

| 욕망 공명 | 모든 마물의 성욕 부여량 +N | 1 / 2 / 3 |

| 절정 추적 | 히로인이 동요 상태(성욕 절반 이상)이면 모든 마물의 성욕 부여량 +N | 1 / 2 / 3 |

| 유혹의 기록 | 주력 계열(덱에 가장 많은 계열) 마물의 성욕 부여량 +N | 1 / 2 / 3 |

| 첫 유혹 | 턴마다 처음 하는 성욕 공격의 성욕 +N | 2 / 4 / 6 |

| 여운 | 턴을 마칠 때 히로인의 성욕이 1 이상이면 성욕 +N | 1 / 2 / 3 |



계열 분기: 마물 시너지 효과



| 노드 | 효과 | 수치 |

| --- | --- | --- |

| 계열 숙련 | 주력 계열(덱에 가장 많은 계열) 마물의 공격 +N | 1 / 2 / 3 |

| 계열 결속 | 주력 계열(덱에 가장 많은 계열) 마물의 최대 HP +N | 2 / 4 / 6 |

| 계열 각성 | 시너지 단계가 새로 켜지면 카드 1장 드로우 (전투마다 N번) | 1 / 2 / 3 |

| 혼성 편성 | 필드에 서로 다른 계열이 3종 이상이면 내 턴을 시작할 때 플레이어 HP N 회복 | 1 / 2 / 3 |

| 계열 공명 | 필드에 주력 계열(덱에 가장 많은 계열) 마물이 N체 이상이면 그 계열의 시너지 계산 수 +1 | 3 / 2 / 1 |



포획 분기: 중복 포획과 재료 획득



| 노드 | 효과 | 수치 |

| --- | --- | --- |

| 정수 추출 | 중복 포획으로 얻는 마물의 정수 +N | 1 / 2 / 3 |

| 포획술 | 포획 확률 +N% | 5 / 10 / 15 |

| 전리품 감정 | 전투 승리로 얻는 골드 +N% | 10 / 20 / 30 |

| 전투 기록 | 전투 승리로 얻는 경험치 +N% | 10 / 20 / 30 |

| 정수 응축 | 전투에서 승리하면 마물의 정수 +N | 1 / 2 / 3 |



기획서에 이름이 있는 노드는 계약, 소환, 지휘, 마나, 기억, 욕망 분기의 앞 세 노드이다. 생명 계약, 군단 지휘, 욕망 공명은 소환사 패시브나 액티브와 이름이 같지만 별개의 효과이며 함께 적용된다.



\---



\## 6. 전투 적용



전투를 시작할 때 노드별 단계를 읽어 효과 수치를 정하고, 전투 도중에는 바꾸지 않는다. 적용한 강화 단계의 합을 전투 기록에 남긴다.



기획서 6.5.4의 적용 순서는 기본 능력치, 그리모어 강화, 장착 패시브이다. 41일차에 만든 패시브 적용 지점에 그리모어 수치를 먼저 더하도록 했다.



| 적용 시점 | 노드 |

| --- | --- |

| 전투 시작 | 생명 계약, 보호의 문장, 마나 그릇, 기억 확장 |

| 마물 소환 | 생명 공명, 안정된 소환, 선봉의 축복, 계열 결속 |

| 필드 변화 | 공격 명령, 군단 지휘, 결전 명령, 계열 숙련, 욕망 공명, 절정 추적, 유혹의 기록, 계열 공명 |

| 카드 사용 | 저비용 계약 |

| 마물 공격 | 첫 유혹 |

| 소환사 스킬 사용 | 신속 명령, 지휘의 기억 |

| 마물 사망 | 죽음의 기록, 마나 회수, 잔류 기억 |

| 플레이어 피해 | 재생 계약, 최후의 계약 |

| 내 턴 시작 | 굳건한 서약, 치유의 인장, 혼성 편성, 재기록, 여분의 마력 |

| 내 턴 종료 | 대기 명령, 마나 비축, 여운 |

| 시너지 변화 | 계열 각성 |

| 덱 재구성 | 순환 기록 |

| 전투 승리 | 포획술, 전리품 감정, 전투 기록, 정수 응축 |

| 중복 포획 | 정수 추출 |



결전 명령과 절정 추적은 히로인의 HP와 성욕에 따라 달라지므로 마물의 공격과 스킬 사용 뒤에 보정을 다시 계산한다.



저비용 계약과 절약 소환 패시브는 함께 적용된다. 그리모어를 먼저 적용하고 남은 비용에 패시브를 적용하며, 비용이 0이 되어 패시브가 줄일 것이 없으면 패시브는 소모하지 않는다.



여운으로 히로인의 성욕이 최대가 되면 턴 종료 시점에 성욕 승리로 끝난다.



마나 회수와 마나 비축으로 얻은 임시 마나는 다음 내 턴에 마나를 채운 뒤 더한다.



\---



\## 7. 그리모어 강화 화면



07\_Grimoire 씬을 추가했다. 메인 메뉴와 지역 선택 화면에서 들어간다.



| 영역 | 구성 |

| --- | --- |

| 상단 | 제목, 보유한 욕망의 파편, 전체 강화 단계 |

| 왼쪽 | 분기 8개와 분기별 진행도 |

| 가운데 | 선택한 분기의 노드 5개, 단계와 상태 |

| 오른쪽 | 선택한 노드의 단계별 효과, 비용, 선행 노드, 강화 버튼 |

| 하단 | 안내 문구, 돌아가기 |



노드의 상태는 잠김, 해금 가능, 강화 가능, 파편 부족, 최대 단계로 표시한다. 기획서 11.14에 따라 잠긴 노드도 효과와 잠금 조건을 보여준다.



분기를 고르면 지금 올릴 수 있는 첫 노드를 자동으로 선택한다.



기획서 11.14의 전체 트리 화면과 드래그 이동, 확대와 축소는 넣지 않았다. 화면 정식화 일차에 처리한다.



\---



\## 8. 이미지



그리모어 화면에 필요한 이미지 3장의 요청문을 작성했고, 같은 날 이미지를 받아 적용했다. 다시 만든 이미지는 없다.



| 시트 | 조각 |

| --- | --- |

| Icon\_Grimoire\_Sheet\_01 | 분기 문양 8개, 욕망의 파편 아이콘 |

| UI\_Node\_Sheet\_01 | 노드 받침 3종, 메뉴용 그리모어 책 |

| Grimoire\_BG\_01 | 그리모어 강화 화면 배경 |



노드 받침은 잠김, 강화 가능, 최대 단계의 3종이며 가운데의 빈 자리에 현재 단계를 적는다.



욕망의 파편 아이콘은 글자 사이에 넣는 아이콘 묶음에 추가했다. 묶음의 아이콘은 54개에서 63개가 되었고 최대 64개까지 들어간다.



이미지가 없어도 화면이 동작하도록 만들었다. 문양과 받침은 숨기고, 파편은 글자로 표시하며, 배경은 서재 그림을 쓴다.



\---



\## 9. 함께 수정한 내용



| 항목 | 내용 |

| --- | --- |

| 메인 메뉴 버튼 | 버튼 7개가 패널에 들어가도록 높이를 68에서 60으로 수정 |

| 비용 감소 기록 | 절약 소환과 저비용 계약의 전투 기록을 비용 감소 한 줄로 통합 |

| 덱 재구성 기록 | 재활용 지식과 순환 기록의 추가 드로우 기록을 덱 재구성으로 통합 |

| 소환사 스킬 설명 | 전투 화면의 스킬 효과 설명에 신속 명령 수치를 반영 |

| 후처리 도구 안내 | 일부 zip만 있어도 실행되는 점을 안내문에 반영 |



\---



\## 10. 개발 중 문제



화면 스크립트를 만드는 동안 Unity가 컴파일 중이었고, 그때 만들어진 파일 하나가 Unity의 컴파일 목록에서 빠졌다. 같은 클래스를 나눠 쓴 나머지 파일들이 변수를 찾지 못해 오류 42개가 났다.



Unity는 컴파일 목록을 스크립트가 추가되거나 삭제될 때만 다시 만든다. 파일 내용을 바꾸는 것으로는 해결되지 않아, 화면 스크립트 4개를 새 이름의 파일로 다시 만들었다. 화면 클래스 이름은 GrimoireUpgradeFlow이다.



같은 증상이 다시 나오면 Unity가 만든 프로젝트 파일에 해당 스크립트가 들어 있는지 먼저 확인한다.



\---



\## 11. 임시값과 기획서 확인 사항



| 항목 | 값 | 비고 |

| --- | --- | --- |

| 노드 40개의 효과와 수치 | 5절 표 | 기획서에 수치가 없어 임시로 정함 |

| 시작 욕망의 파편 | 30 | 강화 확인용, 기획서의 시작 재화는 3 |

| 시험 전투 파편 보상 | 3 | 기획서 A.36 기준 |

| 군단 지휘 발동 조건 | 필드 마물 4체 | 패시브와 같은 기준 |

| 동요 상태 기준 | 성욕 절반 이상 | 기획서 5.14의 50 이상 |



골드와 마물의 정수에는 아직 보유 한도가 없다. 기획서 A.47의 한도와 한도 초과분 표시는 경제 일차에 함께 넣는다.



전투 화면에는 적용된 그리모어 강화를 따로 표시하지 않고 전투 기록에만 남긴다. 기획서의 전투 준비 화면에 있는 그리모어 강화 요약은 전투 준비 화면 일차에 넣는다.



\---



\## 12. 생성한 파일



\- Assets/ProjectV/Scripts/Progress/GrimoireBranch.cs

\- Assets/ProjectV/Scripts/Progress/GrimoireEffectType.cs

\- Assets/ProjectV/Scripts/Progress/GrimoireNodeData.cs

\- Assets/ProjectV/Scripts/Progress/GrimoireRules.cs

\- Assets/ProjectV/Scripts/Progress/PlayerProgressManagerGrimoire.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerGrimoire.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerGrimoireEvent.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerGrimoireTurn.cs

\- Assets/ProjectV/Scripts/Flow/GrimoireUpgradeFlow.cs

\- Assets/ProjectV/Scripts/Flow/GrimoireUpgradeFlowList.cs

\- Assets/ProjectV/Scripts/Flow/GrimoireUpgradeFlowRow.cs

\- Assets/ProjectV/Scripts/Flow/GrimoireUpgradeFlowDetail.cs

\- Assets/ProjectV/Editor/SceneUIBuilderGrimoire.cs

\- Assets/ProjectV/Scenes/07\_Grimoire.unity

\- Assets/ProjectV/Data/GrimoireNodes 노드 데이터 40개

\- Assets/ProjectV/Art/UI/Icons 분기 문양 8개와 욕망의 파편 아이콘

\- Assets/ProjectV/Art/UI/Frames 노드 받침 3개와 그리모어 메뉴 아이콘

\- Assets/ProjectV/Art/Backgrounds/Common/Grimoire\_BG\_01.jpg

\- Tools/UIThemeGenerator/GPT\_Request\_UI\_02.md

\- Tools/UIThemeGenerator/raw 원본 시트 3장



\---



\## 13. 수정한 파일



\- Assets/ProjectV/Scripts/Progress/PlayerProgressManager.cs

\- Assets/ProjectV/Scripts/Progress/SummonerSkillData.cs

\- Assets/ProjectV/Scripts/Progress/MonsterCollectionUI.cs

\- Assets/ProjectV/Scripts/Battle/BattleManager.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerDeck.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerEffect.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerHeroine.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerMonster.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerResult.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerSummoner.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerSummonerEffect.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerSynergy.cs

\- Assets/ProjectV/Scripts/Battle/BattleResultData.cs

\- Assets/ProjectV/Scripts/Battle/BattleResultUI.cs

\- Assets/ProjectV/Scripts/Battle/BattleRewardData.cs

\- Assets/ProjectV/Scripts/Flow/BattleReturnFlow.cs

\- Assets/ProjectV/Scripts/Flow/MainMenuFlow.cs

\- Assets/ProjectV/Scripts/Flow/SceneFlow.cs

\- Assets/ProjectV/Scripts/Flow/SceneNames.cs

\- Assets/ProjectV/Scripts/Flow/StageSelectFlow.cs

\- Assets/ProjectV/Scripts/UI/UIKeys.cs

\- Assets/ProjectV/Editor/SceneUIBuilder.cs

\- Assets/ProjectV/Editor/SceneUIBuilderSummonerBattle.cs

\- Assets/ProjectV/Scenes/00\_Bootstrap.unity

\- Assets/ProjectV/Scenes/01\_MainMenu.unity

\- Assets/ProjectV/Scenes/03\_StageSelect.unity

\- Assets/ProjectV/Scenes/BattleScene.unity

\- Assets/ProjectV/Data/BattleRewards/BR001\_TestBattleReward.asset

\- Assets/ProjectV/Resources/UITheme.asset

\- Assets/ProjectV/Resources/Sprite Assets/UIIcons.asset

\- Assets/ProjectV/Art/UI/Atlases/UI\_IconAtlas\_01.png

\- Assets/ProjectV/Art/UI/UIThemeManifest.txt

\- Assets/ProjectV/Fonts/KoreanDynamic SDF.asset

\- ProjectSettings/EditorBuildSettings.asset

\- Tools/UIThemeGenerator/theme\_spec.txt

\- Tools/UIThemeGenerator/README.md



\---



\## 14. 확인한 내용



\- 전체 스크립트 컴파일 오류 없음

\- Unity 컴파일 통과와 씬 UI 다시 구성으로 씬 9개 구성

\- 씬 9개와 프리팹의 스크립트 참조에 빈 칸 없음

\- 진행 데이터에 노드 40개 연결

\- 테마 이미지 101개 연결, 글자용 아이콘 63개 등록

\- 메인 메뉴에서 강화, 덱 편성, 그리모어 강화 화면 진입

\- 그리모어 강화 화면에서 공격 명령, 신속 명령, 군단 지휘, 마나 그릇 해금

\- 실행 중 Unity Console 오류 없음



\---



\## 15. 다음 실행에서 확인할 내용



이번 실행에서는 덱을 28장으로 줄인 상태로 전투에 들어가 전투가 시작되지 않았다. 전투에서의 동작은 실행으로 확인하지 못했다.



\- 전투 기록의 그리모어 강화 적용 표시

\- 해금한 노드의 전투 효과

\- 승리 시 욕망의 파편 지급과 결과 화면 표시

\- 소환사 화면과 소환사 스킬 사용

\- 마물 판의 상태 효과 아이콘



\---



\## 16. 작업 결과



히로인전 승리, 욕망의 파편, 그리모어 강화, 더 강한 소환사로 이어지는 영구 성장 고리가 생겼다. 마물 카드 강화가 카드 한 장을 키우는 수단이라면 그리모어 강화는 모든 덱에 적용되는 수단이다.



노드 효과는 데이터 에셋의 수치와 설명만 고치면 바꿀 수 있다. 새 효과 종류를 추가할 때만 코드를 고친다.



보유한 욕망의 파편과 노드별 단계는 저장 대상이다. 게임을 종료하면 초기화되며 저장은 저장 및 불러오기 일차에서 처리한다.



\---



\## 17. 다음 개발 방향



43일차에는 경제와 상점을 구현한다. 시작 재화와 시험 보상의 임시값을 기획서 수치로 바꾸고, 재화 보유 한도, 상점, 상인의 감각 패시브를 넣는다.



\---



\## 18. 진행도



\- 현재 전투 프로토타입 진행: 42일차 완료

\- 지역 1 수직 슬라이스 목표: 42 / 48일차

\- 기획서 전체 기능 목표: 42 / 59일차

\- 별도 트랙 콘텐츠 제작 예상: 약 15\~25일
