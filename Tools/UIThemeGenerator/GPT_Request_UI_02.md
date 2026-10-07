# UI 이미지 생성 요청문 2 (GPT용): 그리모어 강화

- 작성일: 2026-10-08 (42일차 그리모어 영구 강화)
- 용도: 그리모어 강화 화면의 분기 문양, 욕망의 파편 아이콘, 노드 받침, 메뉴 아이콘, 배경. 모두 3장.
- 사용법: 새 대화를 열고 아래 [붙여 넣을 내용] 블록 전체를 한 번에 붙여 넣는다.
- 함께 첨부할 그림: `raw/UI_Panel_Sheet_01.png`, `raw/Icon_Type_Sheet_01.png`, `raw/Icon_Stat_Sheet_01.png` (그림체와 금색을 맞추기 위한 기준)
- 받은 뒤: `ui_theme_5.zip`을 이 폴더에 넣고 `process_theme.ps1`을 실행한 다음 Unity에서 `Project V > 씬 UI 다시 구성`을 실행한다.
- 그림이 없어도 화면은 동작한다. 문양과 받침 자리는 비어 보이고 배경은 서재 배경을 쓴다.

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

[이미지 목록: 묶음 5, zip 이름 ui_theme_5.zip]

1. Icon_Grimoire_Sheet_01.png (정사각형)
A 3x3 grid of nine icons in a square image, listed left to right, top to bottom. Icon style: a bold readable silhouette, a thick dark-brown outline, glossy hand-painted shading, each icon centered in its cell at a similar size, readable at 32 pixels.
(1) to (8) are round enamel medallions with gold rims, all the same size and with the same rim. Each medallion has its own background color and one symbol in a lighter tone of the same color.
(1) royal blue #3F6FD8, a rolled contract scroll with a wax seal
(2) violet #8A5BD6, a summoning circle with a paw print in the center
(3) crimson #C8463C, a commander's war banner on a pole
(4) cyan #2FB4D9, a chalice overflowing with liquid mana
(5) amber #D9A23A, an open book with three cards rising from its pages
(6) rose pink #E0609A, a flame shaped like a heart
(7) green #4FB36B, three gems linked in a triangle by thin lines
(8) bronze #A8703A, an open treasure chest with crystals inside
(9) is not a medallion and has no background plate: a single jagged rose-pink crystal shard shaped like half of a broken heart, faceted and glossy, with one small gold sparkle.

2. UI_Node_Sheet_01.png (정사각형)
A 2x2 grid of four pieces with wide transparent gaps.
Top-left, top-right and bottom-left are three round skill-tree node medallions seen from the front. All three are perfect circles of exactly the same size. Each has a flat, smooth, completely empty deep navy center (#16264F) that fills about 70% of the diameter, because a level number will be drawn on it later. All decoration stays on the rim and nothing sticks out beyond the circle.
Top-left (locked node): a dull dark-iron rim with small rivets, no gold, and a slightly greyer, dimmer center.
Top-right (available node): a bright polished gold rim with four small gold studs, clean and inviting.
Bottom-left (completed node): a thick ornate gold rim with filigree, three small blue gems evenly spaced on the rim and a small gold star at the very top of the rim, clearly the most decorated of the three.
Bottom-right (menu icon, not a medallion): a closed magic grimoire with a deep violet leather cover, gold corner fittings, a rose-pink heart-shaped gem clasp and a red ribbon bookmark, drawn in the same icon style as the first sheet, with no background plate.

3. Grimoire_BG_01.png (가로형 3:2, 배경)
An opaque landscape 3:2 background. It will be cropped to 16:9 from the top and bottom, so keep important things away from the top and bottom edges. Paint it slightly softer, lower in contrast and less saturated than the UI pieces. No characters, no creatures, no text, no UI elements.
A bright, mystical grimoire sanctum. A huge open magic book rests on a stone pedestal in the middle distance, with loose pages and soft light particles floating up from it. Tall arched windows with blue sky, pale marble walls, bookshelves fading into soft violet and gold light, and a faint circular star-chart pattern made of simple geometric shapes on the floor. Soft and slightly out of focus, calm and uncluttered in the center.

1번부터 시작해 줘.
```

## 그림이 쓰이는 곳

| 파일 | 조각 | 쓰이는 곳 |
| --- | --- | --- |
| Icon_Grimoire_Sheet_01.png | 분기 문양 8개 | 그리모어 강화 화면의 분기 목록과 분기 제목 |
| Icon_Grimoire_Sheet_01.png | 욕망의 파편 | 재화 표시, 전투 결과, 강화 비용 |
| UI_Node_Sheet_01.png | 노드 받침 3개 | 노드 목록의 잠김, 강화 가능, 최대 단계 표시 |
| UI_Node_Sheet_01.png | 그리모어 책 | 메인 메뉴의 그리모어 강화 버튼 |
| Grimoire_BG_01.png | 배경 | 그리모어 강화 화면 배경 |

## 수령 기록

- 2026-10-08: 3장을 모두 받았다. 다시 만든 그림은 없다.
- 세 장 모두 요청한 배치와 순서대로 나왔고 투명 배경도 실제 알파로 들어왔다.
- 노드 받침 3종은 가운데가 남색으로 비어 있어 단계 글자를 그 위에 올려 쓴다.
