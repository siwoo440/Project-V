# UI 이미지 생성 요청문 3 (GPT용): 상점과 소모품

- 작성일: 2026-10-08 (43일차 경제와 상점)
- 용도: 상점 화면의 소모품 아이콘, 상점 메뉴 아이콘, 상점 배경. 모두 2장.
- 사용법: 새 대화를 열고 아래 [붙여 넣을 내용] 블록 전체를 한 번에 붙여 넣는다.
- 함께 첨부할 그림: `raw/UI_Panel_Sheet_01.png`, `raw/Icon_Stat_Sheet_01.png`, `raw/Icon_Menu_Sheet_01.png` (그림체와 금색을 맞추기 위한 기준)
- 받은 뒤: `ui_theme_6.zip`을 이 폴더에 넣고 `process_theme.ps1`을 실행한 다음 Unity에서 `Project V > 씬 UI 다시 구성`을 실행한다.
- 그림이 없어도 화면은 동작한다. 아이콘 자리는 비어 보이고 배경은 덱 편성 화면의 서재 그림을 쓴다.

## 붙여 넣을 내용

```text
Unity로 만드는 판타지 카드 배틀 게임의 UI 이미지 세트를 만들어 줘. 아래 [공통 스타일]과 [공통 규칙]을 모든 이미지에 똑같이 적용해 줘. 이미지가 첨부되어 있으면 그 그림체, 금색, 외곽선 굵기를 그대로 따라 줘.

[공통 스타일]
Bright, cheerful high-fantasy JRPG mobile game UI in an anime game style. Hand-painted look with clean shapes, soft gradients and crisp dark-brown outlines. Polished warm gold trim (around #D4AF55), warm cream parchment (#FBF2D8), sky blue and royal navy (#16264F) as the main colors, small gemstone accents. Soft light from the top-left. Every asset must look like part of the same UI kit: same gold tone, same outline thickness, same rendering style, same level of detail.

[공통 규칙]
1. 글자, 숫자, 룬 문자, 로고, 워터마크를 절대 넣지 마.
2. 파일명에 BG가 들어간 배경 그림을 뺀 나머지는 실제 알파 채널이 있는 투명 배경 PNG로 만들어 줘. 체크무늬를 그림으로 그려 넣으면 안 돼. 투명 배경이 안 되면 단색 마젠타(#FF00FF) 배경으로 만들고, 그림 안에는 마젠타를 쓰지 마.
3. 투명 배경 이미지는 정면에서 본 평면 그림이어야 해. 원근, 기울임, 회전 없이 반듯하게 그려 줘.
4. 그림이 이미지 가장자리에서 잘리면 안 돼. 사방에 여백을 남기고, 바깥으로 퍼지는 그림자와 후광은 넣지 마.
5. 여러 개를 한 장에 그리는 시트는 지정한 칸 수대로 반듯한 격자에 배치해 줘. 한 칸에 하나씩, 서로 닿지 않게 넓은 투명 간격을 두고, 칸 구분선이나 번호는 그리지 마.
6. "9-slice"라고 적힌 조각은 늘려서 쓰는 거야. 네 변은 처음부터 끝까지 똑같은 굵기의 곧은 선이어야 하고, 장식은 네 모서리에만 넣고, 안쪽은 무늬 없이 평평하게 비워 줘.
7. 한 번에 하나씩 만들어 줘. 하나를 만든 뒤 파일명을 말하고 멈춰. 내가 "다음"이라고 하면 다음 이미지를, "다시"라고 하면 같은 이미지를 다시 만들어 줘.
8. 크기 조정이나 자르기는 하지 마. 생성된 원본 그대로 두면 돼.
9. 목록이 끝나면 아래 파일명 그대로 zip 하나로 묶어 줘.

[이미지 목록: 묶음 6, zip 이름 ui_theme_6.zip]

1. Icon_Item_Sheet_01.png (정사각형)
A 3x3 grid of nine icons in a square image, listed left to right, top to bottom. Icon style: a bold readable silhouette, a thick dark-brown outline, glossy hand-painted shading, no background plate, each icon centered in its cell at a similar size, readable at 32 pixels.
(1) a round glass flask filled with red healing potion, with a cork stopper and a small heart-shaped charm tied to its neck
(2) a cluster of three bright blue mana crystals growing from a small stone base
(3) a round golden seal stamp with a blue shield emblem embossed on its face and a short red ribbon
(4) a white feather quill pen with a gold nib and a small swirl of sparkles
(5) a small bronze incense burner with a thin curl of pale green smoke
(6) a small merchant stall with a red-and-white striped awning and a round gold coin sign
(7) a drawstring leather pouch overflowing with purple crystal shards
(8) an empty round socket: a flat dark navy disc inside a thin dull-gold ring, with nothing inside
(9) a brown leather adventurer's satchel with a gold buckle

2. Shop_BG_01.png (가로형 3:2, 배경)
An opaque landscape 3:2 background. It will be cropped to 16:9 from the top and bottom, so keep important things away from the top and bottom edges. Paint it slightly softer, lower in contrast and less saturated than the UI pieces. No characters, no creatures, no text, no signs with writing, no UI elements.
A bright, cheerful fantasy item shop interior seen from the front. A long wooden counter in the middle distance, wooden shelves filled with potion bottles, crystals and rolled scrolls on both sides, hanging lanterns, a window with blue sky and a striped awning outside, crates and barrels at the far left and right edges. Warm golden light. Soft and slightly out of focus, calm and uncluttered in the center.

1번부터 시작해 줘.
```

## 그림이 쓰이는 곳

| 조각 | 쓰이는 곳 |
| --- | --- |
| 물약, 마나 결정, 인장, 깃펜, 향 | 상점 상품 목록, 지역 선택의 소모품 칸, 전투 화면의 소모품 버튼 |
| 상점 가판대 | 메인 메뉴의 상점 버튼 |
| 정수 주머니 | 상점의 마물의 정수 10개 상품 |
| 빈 칸 | 소모품을 장착하지 않았을 때 |
| 가방 | 상점의 소모품 탭 |
| Shop_BG_01.png | 상점 화면 배경 |

## 수령 기록

- 2026-10-08: `Icon_Item_Sheet_01.png` 수령 (zip 없이 원본 PNG, 1254x1254, 투명 배경). 9조각 모두 정상으로 잘림.
- 2026-10-08: `Shop_BG_01.png` 수령 (`ui_theme_6.zip`, 1536x1024). 16:9로 잘라 `Backgrounds/Common/Shop_BG_01.jpg`로 저장.
