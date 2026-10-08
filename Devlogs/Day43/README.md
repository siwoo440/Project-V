\---



\# Project V 43일차 개발일지



\---



\## 1. 작업 목표



42일차까지 골드와 마물의 정수는 전투 보상으로만 들어오고 마물 강화에만 쓰였다. 재화에 보유 한도가 없었고, 시작 재화는 강화와 그리모어를 바로 시험하려고 넣은 임시값이었다. 기획서 9장의 경제 규칙과 9.12의 상점을 구현해 골드의 두 번째 사용처를 만든다.



상점에서 파는 소모성 아이템을 전투에서 쓸 수 있게 하고, 상점 가격을 낮추는 상인의 감각 패시브를 추가한다.



\---



\## 2. 구현 내용



\- 재화 규칙과 보유 한도 추가

\- 기획서의 시작 재화와 시험용 시작값 분리

\- 전투 결과 화면에 한도 초과분 표시 추가

\- 상점 상품 데이터 구조와 상품 8종 추가

\- 소모성 아이템 데이터 구조와 아이템 5종 추가

\- 구매, 보유, 장착, 소모 처리 추가

\- 상점 화면 추가

\- 전투 화면에 소모성 아이템 버튼과 효과 5종 추가

\- 지역 선택에 소모성 아이템 장착 칸 추가

\- 메인 메뉴와 지역 선택에 상점 버튼 추가

\- 상인의 감각 패시브 추가

\- 목록 줄을 만드는 공용 코드 추가

\- 상점 화면용 이미지 요청문 작성과 이미지 적용



\---



\## 3. 재화 한도와 시작 재화



기획서 9.3과 부록 A.47의 보유 한도를 넣었다.



| 재화 | 보유 한도 | 새 게임 시작값 |

| --- | --- | --- |

| 골드 | 999,999 | 500 |

| 마물의 정수 | 9,999 | 5 |

| 욕망의 파편 | 9,999 | 3 |



한도를 넘는 양은 받지 못하고 다른 재화로 바꾸지 않는다. 전투 결과 화면은 받지 못한 양을 "보유 한도 초과로 받지 못함" 줄로 따로 보여 준다. 결과 화면의 예상 수령량도 한도를 반영한다.



42일차까지 골드와 정수를 더하는 코드가 전투 보상, 카드 획득, 그리모어에 흩어져 있었다. 재화를 더하는 곳을 한 군데로 모으고 그곳에서 한도를 적용했다.



시작 재화는 두 벌을 둔다. 진행 데이터의 시험값 사용 항목이 켜져 있으면 지금까지 쓰던 시험값으로, 꺼져 있으면 위 표의 기획서 값과 경험치 0으로 시작한다. 강화, 그리모어, 상점을 바로 확인할 수 있도록 지금은 켜 둔다.



\---



\## 4. 상점 상품



기획서 9.12의 상품 8종을 데이터 에셋으로 만들었다. 가격과 효과는 기획서 값이다.



| 탭 | 상품 | 가격 | 해금 (챕터 / 임시 레벨) |

| --- | --- | --- | --- |

| 마물의 정수 | 마물의 정수 1개 | 100 | 처음부터 / Lv.1 |

| 마물의 정수 | 마물의 정수 10개 | 900 | 챕터 2 / Lv.5 |

| 욕망의 파편 | 욕망의 파편 1개 | 1,000 | 챕터 3 / Lv.10 |

| 소모성 아이템 | 하급 회복 물약 | 250 | 처음부터 / Lv.1 |

| 소모성 아이템 | 마나 결정 | 350 | 챕터 2 / Lv.5 |

| 소모성 아이템 | 보호의 인장 | 400 | 챕터 3 / Lv.10 |

| 소모성 아이템 | 기억의 깃펜 | 450 | 챕터 4 / Lv.14 |

| 소모성 아이템 | 정화의 향 | 500 | 챕터 5 / Lv.20 |



구매는 골드가 충분하고, 받을 재화나 아이템이 한도에 닿지 않았고, 상품이 해금되어 있을 때만 된다. 한도 때문에 일부만 받게 되는 구매는 막는다.



마물의 정수 10개와 욕망의 파편은 구매 버튼을 한 번 더 눌러 확정한다. 나머지 상품은 한 번에 산다.



챕터가 아직 없어서 해금은 플레이어 레벨로 판정한다. 기획서의 챕터 조건도 데이터에 함께 넣어 두었다.



\---



\## 5. 소모성 아이템



기획서 10.13과 11.14의 규칙을 따른다.



| 규칙 | 내용 |

| --- | --- |

| 장착 | 전투에 하나만 가져간다 |

| 사용 횟수 | 전투마다 한 번 |

| 비용 | 마나와 행동을 쓰지 않는다 |

| 사용 시점 | 플레이어 턴 |

| 소모 | 사용하면 보유 수량이 1 줄어든다 |

| 보유 한도 | 종류마다 99개 |



| 아이템 | 효과 | 사용할 수 없는 경우 |

| --- | --- | --- |

| 하급 회복 물약 | 플레이어 HP 8 회복 | HP가 가득 참 |

| 마나 결정 | 마나 3 회복 | 마나가 가득 참 |

| 보호의 인장 | 아군 마물 하나에 보호막 14 | 아군 마물이 없음 |

| 기억의 깃펜 | 카드 2장 뽑기 | 손패가 가득 참 |

| 정화의 향 | 아군 전체의 해로운 상태를 하나씩 제거 | 해로운 상태가 없음 |



효과가 없는 상황에서는 사용을 막아 아이템이 헛되이 줄지 않게 했다.



보호의 인장은 버튼을 누른 뒤 대상 마물을 고른다. 버튼을 다시 누르면 취소된다. 소환사 스킬의 대상 선택과 겹치지 않도록 한쪽을 시작하면 다른 쪽을 취소한다.



전투 화면 왼쪽 아래, 전투 기록 버튼 위에 아이템 버튼을 두었다. 장착한 아이템이 없으면 버튼에 "소모품 없음"이 표시되고 눌리지 않는다. 전투가 시작될 때 장착한 아이템이 전투 기록에 남고, 사용하면 효과가 기록된다.



장착은 지역 선택 화면에서 한다. 전투 시작 버튼 위의 칸을 누를 때마다 "없음"과 보유한 아이템이 차례로 바뀐다. 가진 수량이 0이 되면 장착하지 않은 것으로 처리한다.



\---



\## 6. 상점 화면



새 씬 08 Shop을 추가했다. 메인 메뉴와 지역 선택에서 들어간다.



| 영역 | 내용 |

| --- | --- |

| 위쪽 | 보유 골드, 마물의 정수, 욕망의 파편 |

| 탭 | 마물의 정수, 욕망의 파편, 소모성 아이템 |

| 왼쪽 목록 | 상품 그림, 이름, 가격, 보유 수량, 잠긴 상품의 해금 조건 |

| 오른쪽 상세 | 효과, 가격, 구매 버튼, 구매할 수 없는 이유 |



상인의 감각으로 가격이 내려가면 목록과 상세에 내린 가격이 표시된다.



그리모어 강화 화면과 상점 화면의 목록 줄이 같은 모양이라 줄을 만드는 코드를 공용으로 빼고 두 화면이 함께 쓰게 했다.



메인 메뉴의 버튼이 8개가 되어 버튼 높이를 60에서 52로 줄였다.



\---



\## 7. 상인의 감각 패시브



소환사 패시브의 열 번째로 추가했다.



| 항목 | 값 |

| --- | --- |

| 공개 레벨 | Lv.5 |

| 1단계 | 상점 가격 5% 감소 |

| 2단계 | 상점 가격 10% 감소 |

| 3단계 | 상점 가격 15% 감소 |



패시브는 하나만 장착하므로 장착하고 있을 때만 가격이 내려간다. 내린 가격은 소수점 아래를 버리고 1골드보다 낮아지지 않는다.



기획서에 감소율이 없어 임시로 정한 값이다.



\---



\## 8. 이미지



상점 화면용 이미지를 GPT 요청문으로 받아 적용했다. 요청문은 Tools/UIThemeGenerator/GPT\_Request\_UI\_03.md에 있다.



| 원본 | 조각 | 쓰이는 곳 |

| --- | --- | --- |

| Icon\_Item\_Sheet\_01.png | 물약, 마나 결정, 인장, 깃펜, 향 | 상점 목록, 장착 칸, 전투의 아이템 버튼 |

| Icon\_Item\_Sheet\_01.png | 상점 가판대 | 메인 메뉴의 상점 버튼 |

| Icon\_Item\_Sheet\_01.png | 정수 주머니, 빈 칸, 가방 | 정수 10개 상품, 빈 장착 칸, 소모성 아이템 탭 |

| Shop\_BG\_01.png | 상점 배경 | 상점 화면 |



아이콘 9개는 글자 사이에 넣는 아이콘 묶음에는 넣지 않았다. 묶음 64칸 가운데 63칸을 이미 쓰고 있고, 아이템 그림은 글자 사이에 넣을 일이 없다.



테마에 연결된 이미지는 101개에서 111개가 되었다.



\---



\## 9. 임시 처리와 기획서 확인 사항



| 항목 | 지금 처리 | 정식 처리 시점 |

| --- | --- | --- |

| 상품 해금 조건 | 플레이어 레벨 (Lv.1, 5, 10, 14, 20) | 챕터가 생기는 일차 |

| 소모성 아이템 장착 위치 | 지역 선택 화면 | 전투 준비 화면 일차 |

| 소모성 아이템 보유 한도 | 종류마다 99개 | 밸런싱 |

| 상인의 감각 감소율 | 5, 10, 15% | 밸런싱 |

| 시작 재화 | 시험값 사용 | 저장 기능 뒤 새 게임 흐름 |

| 보상 배율 | 시험 전투 보상 고정 | 스테이지 일차 |



상품 해금 조건은 기획서 안에서 두 가지로 적혀 있다. 6.3.5의 레벨 해금 표는 Lv.5, 10, 14, 20, 25, 30에 상점 상품이 열린다고 하고, 9.12.2는 챕터 2, 3, 4, 5를 조건으로 든다. 챕터가 없는 지금은 챕터 2부터 5까지를 레벨 표의 Lv.5, 10, 14, 20에 차례로 맞췄다. Lv.25와 Lv.30에 열릴 상품은 기획서에 정해져 있지 않아 넣지 않았다.



기획서 9.4부터 9.8의 보상 배율(지역, 난이도, 반복 보정)은 스테이지 구조가 필요해 이번 일차에 넣지 않았다.



\---



\## 10. 생성한 파일



\- Assets/ProjectV/Scripts/Progress/CurrencyRules.cs

\- Assets/ProjectV/Scripts/Progress/ShopRewardType.cs

\- Assets/ProjectV/Scripts/Progress/ShopItemData.cs

\- Assets/ProjectV/Scripts/Progress/ShopRules.cs

\- Assets/ProjectV/Scripts/Progress/BattleItemType.cs

\- Assets/ProjectV/Scripts/Progress/BattleItemData.cs

\- Assets/ProjectV/Scripts/Progress/PlayerProgressManagerShop.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerItem.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerItemEffect.cs

\- Assets/ProjectV/Scripts/Flow/ShopFlow.cs

\- Assets/ProjectV/Scripts/Flow/ShopFlowList.cs

\- Assets/ProjectV/Scripts/Flow/ShopFlowDetail.cs

\- Assets/ProjectV/Scripts/Flow/StageSelectFlowItem.cs

\- Assets/ProjectV/Scripts/UI/UIListRow.cs

\- Assets/ProjectV/Editor/SceneUIBuilderShop.cs

\- Assets/ProjectV/Scenes/08\_Shop.unity

\- Assets/ProjectV/Data/ShopItems (상품 8종)

\- Assets/ProjectV/Data/BattleItems (아이템 5종)

\- Assets/ProjectV/Data/SummonerPassives/SPV-10\_MerchantSense.asset

\- Assets/ProjectV/Art/UI/Icons (아이템 아이콘 9종)

\- Assets/ProjectV/Art/Backgrounds/Common/Shop\_BG\_01.jpg

\- Tools/UIThemeGenerator/GPT\_Request\_UI\_03.md

\- Tools/UIThemeGenerator/raw/Icon\_Item\_Sheet\_01.png

\- Tools/UIThemeGenerator/raw/Shop\_BG\_01.png



\---



\## 11. 수정한 파일



\- Assets/ProjectV/Scripts/Progress/PlayerProgressManager.cs

\- Assets/ProjectV/Scripts/Progress/PlayerProgressManagerGrimoire.cs

\- Assets/ProjectV/Scripts/Progress/GrimoireRules.cs

\- Assets/ProjectV/Scripts/Battle/BattleManager.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerUI.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerResult.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerMonster.cs

\- Assets/ProjectV/Scripts/Battle/BattleManagerSummonerSkill.cs

\- Assets/ProjectV/Scripts/Battle/BattleResultData.cs

\- Assets/ProjectV/Scripts/Battle/BattleResultUI.cs

\- Assets/ProjectV/Scripts/Flow/SceneNames.cs

\- Assets/ProjectV/Scripts/Flow/SceneFlow.cs

\- Assets/ProjectV/Scripts/Flow/MainMenuFlow.cs

\- Assets/ProjectV/Scripts/Flow/StageSelectFlow.cs

\- Assets/ProjectV/Scripts/Flow/GrimoireUpgradeFlowRow.cs

\- Assets/ProjectV/Scripts/UI/UIKeys.cs

\- Assets/ProjectV/Editor/SceneUIBuilder.cs

\- Assets/ProjectV/Scenes/00\_Bootstrap.unity

\- Assets/ProjectV/Scenes/01\_MainMenu.unity

\- Assets/ProjectV/Scenes/03\_StageSelect.unity

\- Assets/ProjectV/Scenes/BattleScene.unity

\- Assets/ProjectV/Resources/UITheme.asset

\- Assets/ProjectV/Art/UI/UIThemeManifest.txt

\- Assets/ProjectV/Fonts/KoreanDynamic SDF.asset

\- ProjectSettings/EditorBuildSettings.asset

\- Tools/UIThemeGenerator/theme\_spec.txt

\- Tools/UIThemeGenerator/README.md



\---



\## 12. 확인한 내용



\- 전체 스크립트 컴파일 오류 없음

\- Unity 컴파일 통과와 씬 UI 다시 구성으로 씬 10개 구성

\- 씬 10개와 프리팹의 스크립트 참조에 빈 칸 없음

\- 진행 데이터에 상품 8종 연결

\- 테마에 아이템 아이콘 9종과 상점 배경 연결

\- 메인 메뉴에서 상점 화면 진입

\- 실행 중 Unity Console 오류 없음



\---



\## 13. 다음 실행에서 확인할 내용



이번 실행은 메인 메뉴에서 상점 화면에 들어간 데까지다. 구매 결과와 전투에서의 아이템 사용은 실행 기록으로 확인하지 못했다.



\- 구매 시 골드 차감과 보유 수량 증가

\- 확정이 필요한 상품의 두 번 누르기

\- 레벨 조건으로 잠긴 상품 표시

\- 지역 선택의 장착 칸 전환

\- 전투에서 아이템 사용, 수량 감소, 같은 전투에서 재사용 차단

\- 보호의 인장의 대상 선택과 취소

\- 상인의 감각 장착 시 가격 표시

\- 42일차에서 넘어온 항목: 그리모어 전투 효과, 승리 시 욕망의 파편 지급, 소환사 화면과 스킬 사용, 마물 판의 상태 효과 아이콘



\---



\## 14. 작업 결과



전투에서 번 골드를 정수, 욕망의 파편, 소모성 아이템으로 바꿀 수 있게 되었다. 골드가 마물 강화 말고도 쓰이면서 전투, 골드, 상점, 다음 전투 준비로 이어지는 고리가 생겼다.



상품과 아이템은 데이터 에셋이라 가격, 효과 수치, 해금 조건을 코드 없이 고칠 수 있다. 새 효과 종류를 추가할 때만 코드를 고친다.



재화를 더하는 곳이 한 군데로 모여, 저장 기능을 넣을 때 재화가 바뀌는 지점을 한 곳에서 잡을 수 있다.



보유 재화, 소모성 아이템 수량, 장착한 아이템은 저장 대상이다. 게임을 종료하면 초기화된다.



\---



\## 15. 다음 개발 방향



44일차에는 저장과 불러오기를 구현한다. 재화, 경험치, 보유 카드와 강화 단계, 덱 프리셋, 소환사 스킬과 패시브, 그리모어 강화, 소모성 아이템을 파일에 저장하고 게임을 다시 켰을 때 이어서 진행하게 한다.



\---



\## 16. 진행도



\- 현재 전투 프로토타입 진행: 43일차 완료

\- 지역 1 수직 슬라이스 목표: 43 / 48일차

\- 기획서 전체 기능 목표: 43 / 59일차

\- 별도 트랙 콘텐츠 제작 예상: 약 15\~25일
