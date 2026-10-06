\---



\# Project V 41일차 개발일지



\---



\## 1. 작업 목표



40일차까지 모든 화면은 단색 패널과 기본 버튼으로만 이루어져 있었다. 41일차는 UI 디자인 일차로, GPT로 만든 UI 이미지 20장을 받아 전 화면에 적용한다. UI 그림은 코드로 그리지 않고 이미지로 받으며, 시트를 자르고 화면에 연결하는 작업만 직접 처리한다.



이미지를 기다리는 동안 42일차로 잡아 두었던 소환사 액티브 스킬과 패시브를 당겨서 구현했다. 기획서 6.4와 6.5의 내용이며, 40일차에 문구만 표시하던 레벨별 액티브와 패시브 해금이 실제 기능으로 연결된다.



\---



\## 2. 구현 내용



\- UI 이미지 시트 후처리 도구 추가

\- UI 테마 에셋과 화면 연결 구조 추가

\- 배경, 패널, 버튼, 입력 칸 이미지 적용

\- 카드 표시를 2:3 카드 틀로 공용화

\- 체력, 마나, 성욕 게이지 추가

\- 글자 사이 아이콘 표시 추가

\- 필드 마물 판과 상태 효과 아이콘 재구성

\- 전투 결과 승패 문장 표시 추가

\- 소환사 액티브 스킬 7종 추가

\- 소환사 패시브 9종과 패시브 포인트 추가

\- 소환사 화면 추가

\- 전투 화면 소환사 패널과 스킬 사용 추가

\- 패시브 전투 효과 적용

\- 레벨업 시 새 스킬과 패시브 안내 추가

\- 첫 턴 최대 마나 수정

\- 씬 구성 도구의 세로 배치 높이 처리 수정

\- 마물 상태 효과 아이콘 칸 연결 수정



\---



\## 3. UI 이미지 후처리



요청한 시트 20장을 모두 받았고 다시 만든 이미지는 없다. 시트 한 장에 여러 조각이 들어 있어 그대로는 쓸 수 없으므로 Tools/UIThemeGenerator의 후처리 도구로 조각을 잘라 낸다.



| 단계 | 처리 |

| --- | --- |

| 배경 제거 | 투명 배경, 단색 배경, 체크무늬 배경을 구분해 제거 |

| 조각 분리 | 빈 줄을 기준으로 시트를 조각으로 분리 |

| 끝 장식 계산 | 늘어나면 안 되는 테두리 폭을 조각마다 계산 |

| 크기 정리 | 가운데를 줄여 패널은 한 변 512, 막대는 높이 160 이하로 축소 |

| 아이콘 정리 | 256 크기로 맞추고 글자용 아이콘 묶음 1장 생성 |

| 배경 정리 | 16:9 비율의 1920x1080 JPG로 변환 |

| 목록 작성 | 조각 이름과 테두리 값을 목록 파일로 기록 |



| 종류 | 수량 |

| --- | --- |

| 패널, 틀, 막대 | 24 |

| 버튼 | 3 |

| 아이콘 | 54 |

| 글자용 아이콘 묶음 | 1 |

| 배경 | 6 |



전설 카드 틀은 안쪽이 일부 투명하게 나와 안쪽 색으로 메웠다. 제목 리본은 조금 휘어 있어 양 끝의 고정 폭을 넓게 잡았다.



원본 시트는 Tools/UIThemeGenerator/raw에 보관한다. 이미지를 다시 받으면 도구를 다시 실행해 같은 이름으로 덮어쓴다.



\---



\## 4. UI 테마 구조



화면 코드가 이미지 파일을 직접 알지 않도록 이름으로만 찾는 구조를 두었다.



| 파일 | 역할 |

| --- | --- |

| UITheme | 이름과 이미지 목록, 글자용 아이콘 묶음을 담는 에셋 |

| UIKeys | 이미지 이름과 아이콘 이름 상수 |

| UISkin | 이미지 적용, 글자색, 아이콘 표시 문구 생성 |

| UISkinMap | 희귀도, 계열, 행동, 상태 효과에 맞는 이미지 이름 선택 |

| UISliceFitter | 막대 높이가 달라져도 끝 장식 비율 유지 |

| UIGauge | 채움 비율로 게이지 표시 |

| UITextPlate | 글자가 비면 받침 이미지도 숨김 |



이미지가 없으면 적용 함수가 실패를 돌려주고 화면은 기존 단색 모습을 유지한다. 이미지가 일부만 있어도 화면이 깨지지 않는다.



글자색은 글자가 놓인 바탕으로 정한다. 양피지처럼 밝은 바탕 위에서는 어두운 글자를, 남색 패널 위에서는 밝은 글자를 쓴다.



Unity 메뉴의 씬 UI 다시 구성을 실행하면 이미지 가져오기 설정, 테마 에셋, 글자용 아이콘 묶음, 상태 효과 아이콘 연결까지 한 번에 처리한다.



\---



\## 5. 화면별 적용



| 화면 | 적용 내용 |

| --- | --- |

| 공통 | 배경 그림, 양피지와 남색 패널, 파랑·금색·빨강 버튼, 상단 띠, 제목 리본, 입력 칸 |

| 메인 메뉴 | 게임 문장, 제목 리본, 메뉴 아이콘 |

| 덱 편성 | 카드 틀, 하단 띠, 선택한 프리셋 강조 |

| 강화 | 카드 틀, 빈 강화대 자리, 재화 아이콘 |

| 지역 선택 | 전투 종류 아이콘, 자물쇠, 선택 줄 강조 |

| 스토리 | 양피지 대사 창, 이름판, 진행 화살표 |

| 전투 | 게이지, 수치 아이콘, 행동 예고 아이콘, 마물 판, 손패 카드, 승패 문장 |

| 소환사 | 스킬 목록과 패시브 목록 |



전투 화면의 체력, 마나, 성욕은 숫자와 함께 게이지로 표시한다. 공격과 성욕, 방어와 보호막은 아이콘을 붙여 한 줄씩 묶었다.



\---



\## 6. 카드 표시



덱 편성, 강화, 강화대, 전투 손패가 같은 카드 표시를 사용한다. 카드는 2:3 비율이며 희귀도에 따라 틀이 달라진다.



| 위치 | 표시 |

| --- | --- |

| 왼쪽 위 | 마나 비용 |

| 오른쪽 위 | 강화 단계 |

| 가운데 | 계열 문양 |

| 이름 줄 | 카드 이름, 계열과 희귀도 |

| 능력치 줄 | 강화 단계를 반영한 체력, 공격, 방어, 성욕 |

| 아래 줄 | 보유 수량 등 화면별 안내 |



가운데의 계열 문양은 마물 일러스트가 생기기 전까지의 자리 표시이다.



전투 손패는 패시브로 비용이 줄어든 카드의 비용을 다른 색으로 표시한다.



\---



\## 7. 소환사 액티브 스킬



기획서 6.4에 따라 액티브 스킬은 하나만 장착하고, 마나를 써서 플레이어 턴마다 한 번 사용한다.



| 스킬 | 해금 | 마나 | 효과 |

| --- | --- | --- | --- |

| 집중 명령 | Lv.1 | 1 | 행동 가능한 마물 하나의 이번 턴 공격 +3 |

| 긴급 드로우 | Lv.5 | 2 | 카드 2장 드로우 |

| 마나 순환 | Lv.10 | 1 | 이번 턴 임시 마나 2 |

| 계약 보호막 | Lv.14 | 2 | 플레이어 보호막 +5 |

| 긴급 귀환 | Lv.20 | 1 | 아군 마물 하나를 손패로 반환 |

| 욕망 공명 | Lv.25 | 3 | 이번 턴 모든 아군의 성욕 부여량 +3 |

| 절대 명령 | Lv.30 | 4 | 행동을 마친 마물 하나가 다시 행동 |



스킬을 따로 장착하지 않으면 해금된 스킬 가운데 해금 레벨이 가장 낮은 스킬을 사용한다.



대상이 필요한 스킬은 버튼을 누르면 대상 후보가 표시되고, 마물을 누르면 발동한다. 버튼을 다시 누르거나 턴을 종료하면 취소된다.



긴급 귀환은 마물을 사망 처리하지 않고 필드에서 치운 뒤 소환에 쓴 카드를 손패로 돌려준다. 카드 없이 나온 토큰 마물은 대상이 될 수 없다.



\---



\## 8. 소환사 패시브



기획서 6.5에 따라 패시브는 하나만 장착한다. 패시브 포인트로 해금하고 단계를 올리며, 포인트는 되돌릴 수 없다.



| 패시브 | 공개 | 단계별 수치 | 효과 |

| --- | --- | --- | --- |

| 생명 계약 | Lv.2 | 3 / 6 / 10 | 최대 HP 증가 |

| 빠른 이해 | Lv.2 | 1 / 2 / 3 | 첫 턴 추가 드로우 |

| 절약 소환 | Lv.2 | 1 / 2 / 3 | 전투마다 처음 내는 비용 3 이하 마물의 비용 감소 |

| 강인한 계약 | Lv.2 | 1 / 2 / 3 | 받는 HP 피해 감소, 최소 1 |

| 욕망 증폭 | Lv.10 | 1 / 2 / 3 | 모든 마물의 성욕 부여량 증가 |

| 포획 기록 | Lv.10 | 1 / 2 / 3 | 중복 포획 시 정수 추가 |

| 군단 지휘 | Lv.14 | 1 / 2 / 3 | 필드 마물이 4체 이상이면 모든 마물 공격 증가 |

| 재활용 지식 | Lv.20 | 1 / 2 / 3 | 덱을 다시 섞을 때 추가 드로우 |

| 계열 집중 | Lv.25 | 1 / 2 | 덱에 가장 많은 계열의 시너지 계산 수 증가 |



패시브 포인트는 Lv.3부터 레벨마다 1개를 받는다. 단계 하나에 1포인트가 들며, Lv.30까지 28개를 받아 9종을 모두 최대 단계로 올릴 수 있다.



처음 해금한 패시브는 바로 장착된다.



\---



\## 9. 전투 적용



전투를 시작할 때 장착한 스킬과 패시브를 읽어 전투 기록에 남긴다. 기획서 6.5.4의 적용 순서는 기본 능력치, 그리모어 강화, 장착 패시브이다. 그리모어 강화는 아직 없으므로 기본 능력치 다음에 패시브를 적용한다.



| 패시브 | 적용 지점 |

| --- | --- |

| 생명 계약 | 전투 시작 시 플레이어 최대 HP |

| 빠른 이해 | 시작 손패 드로우 |

| 절약 소환 | 카드 비용 계산 |

| 강인한 계약 | 플레이어 HP 피해 계산 |

| 욕망 증폭 | 마물 성욕 부여량 |

| 포획 기록 | 중복 포획 정수 지급 |

| 군단 지휘 | 필드 마물 수가 바뀔 때의 공격 보정 |

| 재활용 지식 | 버린 더미를 덱으로 다시 섞을 때 |

| 계열 집중 | 시너지 계열 수 계산 |



전투 화면 왼쪽에 소환사 패널을 추가해 스킬 버튼, 스킬 효과, 장착한 패시브를 표시한다. 마나가 부족하거나 이번 턴에 이미 사용했으면 버튼이 비활성화된다.



\---



\## 10. 소환사 화면



06\_Summoner 씬을 추가했다. 메인 메뉴와 지역 선택 화면에서 들어간다.



| 영역 | 구성 |

| --- | --- |

| 상단 | 제목, 플레이어 레벨, 남은 패시브 포인트 |

| 왼쪽 | 액티브 스킬 목록, 눌러서 장착 |

| 오른쪽 위 | 패시브 목록, 눌러서 선택 |

| 오른쪽 아래 | 현재 단계와 다음 단계 효과, 장착, 해금 또는 강화 |

| 하단 | 안내 문구, 돌아가기 |



아직 열리지 않은 스킬과 패시브도 목록에 보이며 필요한 레벨을 표시한다.



전투 결과 화면은 레벨이 오른 구간에서 새로 열린 스킬과 패시브, 받은 패시브 포인트를 함께 표시한다.



\---



\## 11. 함께 수정한 내용



| 항목 | 내용 |

| --- | --- |

| 첫 턴 최대 마나 | 1에서 2로 수정, 기획서 A.18 기준 |

| 세로 배치 | 씬 구성 도구가 정한 줄 높이가 무시되던 문제 수정 |

| 전투 화면 하단 | 돌아가기 버튼과 진행 정보가 손패와 겹치지 않도록 이동 |

| 전투 로그 버튼 | 버튼 글자를 전투 로그로 수정 |

| 마물 상태 효과 아이콘 | 마물 판의 아이콘 칸이 연결되지 않아 표시되지 않던 문제 수정 |



마물 상태 효과 아이콘은 33일차에 프리팹을 다시 구성하면서 연결이 빠져 있었다.



40일차에는 씬 구성을 실행하지 않아 전투 결과의 레벨 변화 줄과 강화 화면의 강화 상한 표시가 씬에 없었다. 이번 씬 구성으로 함께 들어갔다.



\---



\## 12. 임시값과 기획서 확인 사항



기획서 6.4와 6.5에는 이름, 해금 레벨, 마나 비용과 효과 방향만 있다. 다음 값은 임시로 정했으며 밸런싱 때 다시 정한다.



| 항목 | 임시값 |

| --- | --- |

| 집중 명령 공격 증가 | 3 |

| 계약 보호막 | 5 |

| 욕망 공명 성욕 증가 | 3 |

| 패시브 단계별 수치 | 8절 표의 수치 |

| 패시브 포인트 지급 | Lv.3부터 레벨마다 1 |

| 패시브 단계당 필요 포인트 | 1 |



긴급 드로우의 2장과 마나 순환의 임시 마나 2는 기획서 수치이다.



기획서와 구현이 다른 항목과 미구현 항목은 다음과 같다.



| 항목 | 내용 |

| --- | --- |

| 플레이어 최대 HP | A.5는 레벨 5마다 1 증가, 6.2.2는 레벨로 증가 없음. 현재 6.2.2 기준 |

| UI 분위기 | 11.1과 11.4.1은 어두운 마도서 분위기, 구현은 밝은 판타지 |

| 상인의 감각 패시브 | 상점이 없어 제외, 상점 일차에 추가 |

| 시작 손패 교환 | A.20 미구현 |

| 손패 초과 드로우 | A.21의 버린 더미 처리 미구현 |

| 소환 시 초기 쿨타임 | A.24 미구현 |



\---



\## 13. 생성한 파일



\- Assets/ProjectV/Scripts/UI/UITheme.cs

\- Assets/ProjectV/Scripts/UI/UIKeys.cs

\- Assets/ProjectV/Scripts/UI/UISkin.cs

\- Assets/ProjectV/Scripts/UI/UISkinMap.cs

\- Assets/ProjectV/Scripts/UI/UISliceFitter.cs

\- Assets/ProjectV/Scripts/UI/UIGauge.cs

\- Assets/ProjectV/Scripts/UI/UITextPlate.cs

\- Assets/ProjectV/Scripts/Progress/SummonerSkillType.cs

\- Assets/ProjectV/Scripts/Progress/SummonerSkillData.cs

\- Assets/ProjectV/Scripts/Progress/SummonerPassiveType.cs

\- Assets/ProjectV/Scripts/Progress/SummonerPassiveData.cs

\- Assets/ProjectV/Scripts/Progress/SummonerRules.cs

\- Assets/ProjectV/Scripts/Progress/PlayerProgressManagerSummoner.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerSummoner.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerSummonerSkill.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerSummonerEffect.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerHand.cs

\- Assets/ProjectV/Scripts/Flow/SummonerFlow.cs

\- Assets/ProjectV/Scripts/Flow/SummonerFlowList.cs

\- Assets/ProjectV/Scripts/Flow/SummonerFlowRow.cs

\- Assets/ProjectV/Scripts/Flow/CardEntryFace.cs

\- Assets/ProjectV/Scripts/Flow/CardEntryBadge.cs

\- Assets/ProjectV/Editor/UIThemeSetup.cs

\- Assets/ProjectV/Editor/UIThemeSetupImport.cs

\- Assets/ProjectV/Editor/SceneUIBuilderTheme.cs

\- Assets/ProjectV/Editor/SceneUIBuilderThemeParts.cs

\- Assets/ProjectV/Editor/SceneUIBuilderGauge.cs

\- Assets/ProjectV/Editor/SceneUIBuilderBattleParts.cs

\- Assets/ProjectV/Editor/SceneUIBuilderPrefabs.cs

\- Assets/ProjectV/Editor/SceneUIBuilderPrefabParts.cs

\- Assets/ProjectV/Editor/SceneUIBuilderStatusIcon.cs

\- Assets/ProjectV/Editor/SceneUIBuilderSummoner.cs

\- Assets/ProjectV/Editor/SceneUIBuilderSummonerBattle.cs

\- Assets/ProjectV/Scenes/06\_Summoner.unity

\- Assets/ProjectV/Data/SummonerSkills 스킬 데이터 7개

\- Assets/ProjectV/Data/SummonerPassives 패시브 데이터 9개

\- Assets/ProjectV/Resources/UITheme.asset

\- Assets/ProjectV/Resources/Sprite Assets/UIIcons.asset

\- Assets/ProjectV/Art/UI 이미지 82개와 조각 목록

\- Assets/ProjectV/Art/Backgrounds 배경 5개

\- Assets/ProjectV/Art/WorldMap 배경 1개

\- Tools/UIThemeGenerator 후처리 도구와 원본 시트 20장



\---



\## 14. 수정한 파일



\- Assets/ProjectV/Editor/SceneUIBuilder.cs

\- Assets/ProjectV/Scripts/Battle/BattleManager.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerDeck.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerEffect.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerHeroine.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerMonster.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerResult.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerSynergy.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerUI.cs

\- Assets/ProjectV/Scripts/Battle/BattleResultUI.cs

\- Assets/ProjectV/Scripts/Battle/MonsterUnit.cs

\- Assets/ProjectV/Scripts/Battle/StatusEffectIconUI.cs

\- Assets/ProjectV/Scripts/Flow/BattleReturnFlow.cs

\- Assets/ProjectV/Scripts/Flow/CardEntryFactory.cs

\- Assets/ProjectV/Scripts/Flow/DeckBuilderFlow.cs

\- Assets/ProjectV/Scripts/Flow/EnhanceFlow.cs

\- Assets/ProjectV/Scripts/Flow/MainMenuFlow.cs

\- Assets/ProjectV/Scripts/Flow/SceneFlow.cs

\- Assets/ProjectV/Scripts/Flow/SceneNames.cs

\- Assets/ProjectV/Scripts/Flow/StageSelectFlow.cs

\- Assets/ProjectV/Scripts/Flow/StoryFlow.cs

\- Assets/ProjectV/Scripts/Progress/MonsterCollectionUI.cs

\- Assets/ProjectV/Scripts/Progress/PlayerProgressManager.cs

\- Assets/ProjectV/Scenes 기존 씬 7개

\- Assets/ProjectV/Prefabs/UI 프리팹 4개

\- Assets/ProjectV/Data/StatusEffects 상태 효과 데이터 8개

\- Assets/ProjectV/Fonts/KoreanDynamic SDF.asset

\- ProjectSettings/EditorBuildSettings.asset

\- .gitignore



\---



\## 15. 확인한 내용



\- 전체 스크립트 컴파일 오류 없음

\- Unity에서 씬 UI 다시 구성 실행, 씬 8개와 프리팹 4개 구성 완료

\- 구성과 실행 중 Unity Console 오류 없음

\- 테마 에셋의 이미지 87개가 모두 연결됨

\- 글자용 아이콘 묶음에 아이콘 54개 등록

\- 씬 8개의 스크립트 참조에 빈 칸 없음

\- 진행 데이터에 스킬 7종과 패시브 9종 등록

\- 00\_Bootstrap에서 메인 메뉴, 스토리, 지역 선택, 전투 화면까지 진입



\---



\## 16. 다음 실행에서 확인할 내용



소환사 화면은 이번 실행에서 열어 보지 않았다. 마물 상태 효과 아이콘 연결은 씬 구성 뒤에 수정했다. 다음 항목은 42일차 작업을 확인할 때 함께 확인한다.



\- 소환사 화면에서 스킬 장착과 패시브 해금, 강화, 장착

\- 전투에서 스킬 사용과 턴당 한 번 제한

\- 대상 선택과 취소

\- 패시브별 전투 효과

\- 마물 판의 상태 효과 아이콘 표시

\- 글자가 패널 밖으로 나가는 곳과 아이콘 크기



\---



\## 17. 작업 결과



모든 화면이 같은 그림 체계를 쓰게 되었다. 화면 코드는 이미지 이름만 알기 때문에 이미지를 교체해도 코드를 고치지 않는다.



플레이어 레벨이 스킬과 패시브를 열어 주면서 40일차의 해금 표 가운데 액티브와 패시브 항목이 실제 기능이 되었다. 상점 항목은 상점 일차에 연결한다.



장착한 스킬, 장착한 패시브, 패시브 단계, 사용한 패시브 포인트는 저장 대상이다. 게임을 종료하면 초기화되며 저장은 저장 및 불러오기 일차에서 처리한다.



마물 일러스트, 히로인 그림, 지역별 배경은 이번 범위가 아니다. 전투 전에 스킬과 패시브를 바꾸는 기능은 전투 준비 화면 일차에 연결한다.



\---



\## 18. 다음 개발 방향



42일차에는 그리모어 영구 강화를 구현한다. 욕망의 파편 재화와 기획서 6.6의 강화 트리를 만들고, 이번에 만든 패시브 적용 지점에 그리모어 보정을 함께 연결한다.



소환사 스킬을 한 일차 당겨 구현했으므로 이후 일정이 하루씩 앞당겨진다.



\---



\## 19. 진행도



\- 현재 전투 프로토타입 진행: 41일차 완료

\- 지역 1 수직 슬라이스 목표: 41 / 48일차

\- 기획서 전체 기능 목표: 41 / 59일차

\- 별도 트랙 콘텐츠 제작 예상: 약 15\~25일
