# UI 이미지 생성 요청문 6 (GPT용): 아직 받지 못한 그림 2장

- 작성일: 2026-10-08 (46일차)
- 용도: 요청문 4의 저장 화면 배경과 요청문 5의 월드맵 표시 4종. 두 장만 따로 받기 위해 다시 썼다.
- 사용법: 새 대화를 열고 아래 [붙여 넣을 내용] 블록 전체를 한 번에 붙여 넣는다.
- 함께 첨부할 그림: `raw/UI_Node_Sheet_01.png`, `raw/Icon_Save_Sheet_01.png`, `raw/Icon_Region_Sheet_01.png` (그림체와 금색을 맞추기 위한 기준)
- 받은 뒤: `ui_theme_9.zip`을 이 폴더에 넣고 `process_theme.ps1`을 실행한 다음 Unity에서 `Project V > 씬 UI 다시 구성`을 실행한다. zip 없이 그림만 받아도 된다.
- 파일 이름은 아래와 똑같아야 한다. 후처리 목록(`theme_spec.txt`)에 이미 이 이름으로 들어 있다.

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

[이미지 목록: 2장, zip 이름 ui_theme_9.zip]

1. UI_MapMark_Sheet_01.png (정사각형)
A 2x2 grid of four map markers in a square image, listed left to right, top to bottom. One marker per cell, each centered in its cell with wide empty space around it. Thick dark-brown outline, glossy hand-painted shading, no background plate, no ground shadow.
(1) top-left: a selection ring. A thick ornate golden ring with four small diamond-shaped studs at the top, bottom, left and right. The inside of the ring is completely empty: only the background shows through, nothing is drawn there.
(2) top-right: a cleared mark. A golden laurel wreath with a bright green check mark in its center.
(3) bottom-left: a notice badge. A red starburst badge with a gold rim. Its face is plain red and completely blank.
(4) bottom-right: a location marker. A small royal-blue banner flag on a short gold pole with a round gold base.

2. SaveLoad_BG_01.png (가로형 3:2, 배경)
An opaque landscape 3:2 background. It will be cropped to 16:9 from the top and bottom, so keep important things away from the top and bottom edges. Paint it slightly softer, lower in contrast and less saturated than the UI pieces. No characters, no creatures, no text, no signs with writing, no UI elements.
A bright, quiet fantasy record room seen from the front in warm morning light. A wide wooden writing desk in the lower middle distance with an open blank journal, an inkwell with a white quill and a small brass lantern. Tall wooden shelves filled with closed ledgers and rolled scrolls on the far left and right. A large arched window in the center with blue sky and soft white clouds, pale blue curtains and a big brass hourglass on the windowsill. Cream walls, warm wood and royal-blue cloth with gold trim. Soft and slightly out of focus, calm and uncluttered in the center.

1번부터 시작해 줘.
```

## 그림이 쓰이는 곳

| 조각 | 쓰이는 곳 |
| --- | --- |
| 선택 고리 | 월드맵에서 고른 지역 |
| 클리어 표시 | 클리어한 지역 |
| 알림 배지 | 새로 열린 지역 (위에 NEW 글자를 얹는다) |
| 위치 깃발 | 마지막으로 들어간 지역 |
| SaveLoad_BG_01.png | 저장 화면 배경 |

## 수령 기록

- 2026-10-08: `UI_MapMark_Sheet_01.png` 수령 (원본 PNG, 1254x1254, 투명 배경). 4조각 모두 정상, 고리 가운데도 투명.
- `SaveLoad_BG_01.png`은 아직 받지 않음.
