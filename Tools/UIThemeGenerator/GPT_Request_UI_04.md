# UI 이미지 생성 요청문 4 (GPT용): 저장과 불러오기

- 작성일: 2026-10-08 (44일차 저장과 불러오기)
- 용도: 저장 화면의 칸 아이콘, 메인 메뉴의 이어하기·새 게임·불러오기 아이콘, 자동 저장 표시, 저장 화면 배경. 모두 2장.
- 사용법: 새 대화를 열고 아래 [붙여 넣을 내용] 블록 전체를 한 번에 붙여 넣는다.
- 함께 첨부할 그림: `raw/UI_Panel_Sheet_01.png`, `raw/Icon_Menu_Sheet_01.png`, `raw/Icon_Item_Sheet_01.png` (그림체와 금색을 맞추기 위한 기준)
- 받은 뒤: `ui_theme_7.zip`을 이 폴더에 넣고 `process_theme.ps1`을 실행한 다음 Unity에서 `Project V > 씬 UI 다시 구성`을 실행한다.
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

[이미지 목록: 묶음 7, zip 이름 ui_theme_7.zip]

1. Icon_Save_Sheet_01.png (정사각형)
A 3x3 grid of nine icons in a square image, listed left to right, top to bottom. Icon style: a bold readable silhouette, a thick dark-brown outline, glossy hand-painted shading, no background plate, each icon centered in its cell at a similar size, readable at 32 pixels.
(1) an open leather-bound journal with a white feather quill resting on its page, the page is blank
(2) an open book with a glowing golden bookmark ribbon and a few small sparkles rising from its blank pages
(3) a round royal-blue medallion with a gold rim and a bold gold triangle pointing to the right in its center
(4) a rolled cream parchment scroll tied with a blue ribbon, with a single gold four-point star above it
(5) two thick gold curved arrows chasing each other in a circle, with a small blue gem in the center
(6) a small wooden waste bin with a gold band and a crumpled paper ball inside
(7) a yellow triangular warning sign with a thick dark-brown outline and one bold dark exclamation mark in the middle (this single symbol is allowed as an exception to rule 1)
(8) a round gold pocket watch with a plain white face and two dark hands, no numbers on the face
(9) a closed plain grey-blue book with no decoration, looking faded and unused

2. SaveLoad_BG_01.png (가로형 3:2, 배경)
An opaque landscape 3:2 background. It will be cropped to 16:9 from the top and bottom, so keep important things away from the top and bottom edges. Paint it slightly softer, lower in contrast and less saturated than the UI pieces. No characters, no creatures, no text, no signs with writing, no UI elements.
A bright, quiet fantasy record room seen from the front. A wide wooden writing desk in the lower middle distance with an open blank journal, an inkwell and a quill, a small brass lantern. Tall shelves of closed ledgers and rolled scrolls on the far left and right. A large arched window in the center with blue sky and soft clouds, pale blue curtains, a big brass hourglass on the windowsill. Warm morning light. Soft and slightly out of focus, calm and uncluttered in the center.

1번부터 시작해 줘.
```

## 그림이 쓰이는 곳

| 조각 | 쓰이는 곳 |
| --- | --- |
| 일지와 깃펜 | 지역 선택의 저장 버튼, 저장 화면의 저장 버튼 |
| 빛나는 책 | 메인 메뉴의 불러오기 버튼, 저장 화면의 불러오기 버튼 |
| 파란 메달 | 메인 메뉴의 이어하기 버튼 |
| 두루마리와 별 | 메인 메뉴의 새 게임 버튼 |
| 도는 화살표 | 자동 저장 칸, 화면 오른쪽 위의 자동 저장 표시 |
| 휴지통 | 저장 화면의 삭제 버튼 |
| 경고 표지 | 손상되었거나 불러올 수 없는 칸 |
| 회중시계 | 저장 칸의 플레이 시간 |
| 빈 책 | 비어 있는 저장 칸 |
| SaveLoad_BG_01.png | 저장 화면 배경 |

## 수령 기록

- 2026-10-08: `Icon_Save_Sheet_01.png` 수령 (zip 없이 원본 PNG, 1254x1254, 투명 배경). 9조각 모두 정상으로 잘림.
- `SaveLoad_BG_01.png`은 아직 받지 않음. 받기 전까지 저장 화면은 서재 배경을 쓴다.
