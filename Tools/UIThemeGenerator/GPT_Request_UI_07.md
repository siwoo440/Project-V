# UI 이미지 생성 요청문 7 (GPT용): 스테이지와 난이도

- 작성일: 2026-10-10 (47일차 스테이지 구조와 보상)
- 용도: 난이도 표시 3종, 보상 표시 3종, 포획과 히로인전 표시 3종(48~49일차에 사용), 아직 받지 못한 저장 화면 배경. 모두 2장.
- 사용법: 새 대화를 열고 아래 [붙여 넣을 내용] 블록 전체를 한 번에 붙여 넣는다.
- 함께 첨부할 그림: `raw/Icon_Misc_Sheet_01.png`, `raw/Icon_Item_Sheet_01.png`, `raw/Icon_Save_Sheet_01.png` (그림체와 금색을 맞추기 위한 기준)
- 받은 뒤: `ui_theme_10.zip`을 이 폴더에 넣고 `process_theme.ps1`을 실행한 다음 Unity에서 `Project V > 씬 UI 다시 구성`을 실행한다. zip 없이 그림만 받아도 된다.
- 그림이 없어도 화면은 동작한다. 난이도와 보상은 글자로만 표시된다.

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

[이미지 목록: 2장, zip 이름 ui_theme_10.zip]

1. Icon_Battle_Sheet_01.png (정사각형)
A 3x3 grid of nine icons in a square image, listed left to right, top to bottom. Icon style: a bold readable silhouette, a thick dark-brown outline, glossy hand-painted shading, no background plate, no ground shadow, each icon centered in its cell at a similar size, readable at 32 pixels.
(1) easy difficulty: a round green shield with a gold rim and one small white feather on its face
(2) normal difficulty: a round royal-blue shield with a gold rim and one upright silver sword on its face
(3) hard difficulty: a round crimson-red shield with a gold rim and two crossed silver swords on its face
(4) a closed wooden treasure chest with gold bands and a blue gem lock
(5) the same treasure chest wide open and completely empty inside
(6) a single gold coin with two thick gold curved arrows circling around it
(7) a glowing violet crystal orb held in a gold claw mount, with a small monster paw print visible inside the orb
(8) two ivory dice with royal-blue pips and gold edges
(9) a golden crown with three small red gems sitting on a small royal-blue cushion

2. SaveLoad_BG_01.png (가로형 3:2, 배경)
An opaque landscape 3:2 background. It will be cropped to 16:9 from the top and bottom, so keep important things away from the top and bottom edges. Paint it slightly softer, lower in contrast and less saturated than the UI pieces. No characters, no creatures, no text, no signs with writing, no UI elements.
A bright, quiet fantasy record room seen from the front in warm morning light. A wide wooden writing desk in the lower middle distance with an open blank journal, an inkwell with a white quill and a small brass lantern. Tall wooden shelves filled with closed ledgers and rolled scrolls on the far left and right. A large arched window in the center with blue sky and soft white clouds, pale blue curtains and a big brass hourglass on the windowsill. Cream walls, warm wood and royal-blue cloth with gold trim. Soft and slightly out of focus, calm and uncluttered in the center.

1번부터 시작해 줘.
```

## 그림이 쓰이는 곳

| 조각 | 쓰이는 곳 |
| --- | --- |
| 초록, 파랑, 빨강 방패 | 지역 화면의 난이도 버튼 (쉬움, 보통, 어려움) |
| 닫힌 상자 | 최초 보상을 아직 받지 않은 스테이지 |
| 열린 상자 | 최초 보상을 이미 받은 스테이지 |
| 동전과 화살표 | 반복 보상 표시 |
| 수정 구슬 | 포획 콘텐츠 (48일차) |
| 주사위 | 포획 목록 다시 뽑기 (48일차) |
| 왕관 | 히로인전 단계 표시 (49일차) |
| SaveLoad_BG_01.png | 저장 화면 배경 |

## 수령 기록

- 2026-10-10: `Icon_Battle_Sheet_01.png` 수령 (원본 PNG, 1254x1254, 투명 배경). 9조각 모두 정상으로 잘림.
- `SaveLoad_BG_01.png`은 아직 받지 않음.
