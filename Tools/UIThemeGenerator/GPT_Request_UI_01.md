# UI 이미지 생성 요청문 (GPT용)

- 작성일: 2026-10-06 (41일차 UI 디자인)
- 용도: 밝은 판타지 UI 세트. 기획서 12.18에 따라 사용한 요청문을 기록한다.
- 사용법: [공통] 블록 아래에 [묶음] 블록 하나를 붙여 한 번에 보낸다. 묶음마다 새 대화를 쓴다.
- 묶음 2부터는 묶음 1에서 받은 UI_Panel_Sheet_01.png와 UI_Button_Sheet_01.png를 함께 첨부한다.

## 공통

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
```

## 묶음 1: 기본 틀 (4장)

```text
[이미지 목록: 묶음 1, zip 이름 ui_theme_1.zip]

1. UI_Panel_Sheet_01.png (가로형 3:2)
Two empty square UI panels side by side with a wide transparent gap, both made for 9-slice scaling and identical in shape, frame and corner ornaments. Each is a perfect square with slightly rounded corners and a thin ornate gold frame; filigree and one small blue gem only at each of the four corners. Left panel: flat, smooth warm cream parchment interior (#FBF2D8). Right panel: flat, smooth, fully opaque deep navy interior (#16264F). Both interiors are completely empty.

2. UI_Button_Sheet_01.png (정사각형)
Three empty game buttons stacked vertically with wide transparent gaps: top sapphire blue, middle golden yellow, bottom ruby red. All three have exactly the same shape: a wide rounded rectangle about 3.5:1, glossy, with a thin gold rim. Small ornaments are allowed only at the left and right ends; the long middle section must be plain and uniform so the button can be stretched. No text, no icons.

3. UI_Strip_Sheet_01.png (가로형 3:2)
Three horizontal pieces stacked vertically with wide transparent gaps, each perfectly straight and horizontally symmetric, spanning about 85% of the image width.
Top: a title banner ribbon of crimson red cloth with gold trim along its top and bottom edges and forked tails at both ends. Its long middle section is plain and uniform, with no emblem.
Middle: a header bar, a flat royal navy bar with thin gold trim lines along its top and bottom edges and small gold end caps. Its long middle section is plain and uniform.
Bottom: a thin gold filigree divider line with one small blue gem in the center.

4. UI_Bar_Sheet_01.png (정사각형)
Five wide rounded-rectangle bars (about 5:1) stacked vertically with wide transparent gaps, all the same size, each with a plain uniform middle section so it can be stretched. From top to bottom:
(1) list row: a dark navy bar with a thin gold rim
(2) selected list row: a brighter royal blue bar with a thicker bright gold rim
(3) input field: a recessed cream parchment bar with a thin brown rim and a soft inner shadow
(4) label plate: a very dark navy pill with a thin gold rim
(5) name plate: a cream parchment plate with a thin dark-brown rim

1번부터 시작해 줘.
```

## 묶음 2: 카드와 전투 틀 (4장)

```text
[이미지 목록: 묶음 2, zip 이름 ui_theme_2.zip]

1. UI_CardFrame_Sheet_01.png (세로형 2:3)
Four empty trading-card frames in a 2x2 grid. All four have exactly the same outer shape and size: a portrait 2:3 rectangle with slightly rounded corners. The interior of every card is one flat deep navy color (#16264F) and completely empty: no picture, no plates, no boxes, no sockets. The frames differ by rarity.
Top-left (common): a simple plain silver-grey steel frame.
Top-right (rare): a sapphire blue frame with a double border.
Bottom-left (special): an amethyst purple frame with an engraved pattern along the border.
Bottom-right (legendary): an ornate gold frame with elaborate filigree corners and jewel accents.

2. UI_Slot_Sheet_01.png (가로형 3:2)
Two empty portrait 2:3 plates side by side with a wide transparent gap, same size, slightly rounded corners, both made for 9-slice scaling.
Left (battlefield unit plate): a flat light cream parchment interior with a thin bronze-gold frame.
Right (empty card slot): a flat dark navy recessed stone plate with a thin dull-gold border.
Both interiors are completely empty.

3. UI_Gauge_Sheet_01.png (정사각형)
Five horizontal gauge pieces stacked vertically with wide transparent gaps, all the same length (about 85% of the image width), capsule shaped with rounded ends, plain and uniform along their length. From top to bottom:
(1) empty gauge frame: a dark navy recessed groove inside a thin gold frame
(2) a solid glossy red bar with no frame
(3) a solid glossy pink bar with no frame
(4) a solid glossy blue bar with no frame
(5) a solid glossy green bar with no frame

4. UI_Emblem_Sheet_01.png (가로형 3:2)
Three decorative emblems in one row with wide transparent gaps, similar in size.
Left (game crest): an open magic grimoire with a bright summoning circle above it made of simple geometric star shapes, framed by golden wings and a blue gem.
Middle (victory): a golden laurel wreath with a star, crossed swords and short red ribbon tails.
Right (defeat): a cracked dull-steel shield with a broken sword and a torn grey banner.

1번부터 시작해 줘.
```

## 묶음 3: 아이콘 (6장)

```text
[이미지 목록: 묶음 3, zip 이름 ui_theme_3.zip]

Icon style for every sheet in this group: a bold readable silhouette, a thick dark-brown outline, glossy hand-painted shading, no background plate unless stated, each icon centered in its cell at a similar size, readable at 32 pixels. Every sheet is a 3x3 grid in a square image, listed left to right, top to bottom.

1. Icon_Stat_Sheet_01.png
(1) a red heart  (2) a steel sword pointing up  (3) a round steel shield with a gold rim
(4) a blue crystal barrier shield  (5) a pink heart with a small sparkle  (6) a round blue mana gem set in a gold ring
(7) a gold coin with an embossed star  (8) a cluster of purple crystal shards  (9) a golden star medal

2. Icon_Type_Sheet_01.png
Nine round enamel medallions with gold rims. Each medallion has its own background color and a symbol in a lighter tone of the same color.
(1) magenta #C94FA3, a curled tentacle  (2) yellow-green #8ED34F, a slime droplet  (3) olive #8B9A3B, a dagger and a pointed goblin ear
(4) crimson #C63D4F, a pair of horns  (5) blue-grey #718A9E, a skull  (6) orange #D7803C, three claw marks
(7) teal #38B9AE, an elemental crystal  (8) steel blue #7087A8, a cogwheel  (9) pale gold #E7C96A, a pair of wings with a halo

3. Icon_Action_Sheet_01.png
(1) a sword with one red slash arc  (2) three red slash arcs spreading out  (3) a blue shield with a small plus-shaped sparkle
(4) a green heart with a small white cross  (5) a purple swirling curse orb with a small skull  (6) a pale blue water drop with sparkles
(7) a golden four-pointed magic star burst  (8) a blue-grey hourglass  (9) a shield with a red bullseye target on it

4. Icon_Status_Sheet_01.png
(1) a sword with a gold upward arrow  (2) a sword with a purple downward arrow  (3) a shield with a gold upward arrow
(4) a cracked shield with a purple downward arrow  (5) a green poison droplet with a small skull  (6) two crossed grey iron chains
(7) a round blue badge with a white spiral  (8) a round green badge with a white upright sword  (9) a round grey badge with a white check mark

5. Icon_Menu_Sheet_01.png
(1) an open storybook with a ribbon bookmark  (2) a rolled parchment map with a red location pin  (3) a fan of three cards
(4) an anvil with a hammer and a sparkle  (5) a wooden arched door slightly open  (6) a scroll with a quill pen
(7) a thick leather book with a claw-mark emblem and a clasp  (8) a golden padlock  (9) a magnifying glass

6. Icon_Misc_Sheet_01.png
(1) two crossed swords  (2) a crystal orb wrapped in golden chains  (3) a jeweled tiara
(4) a neat stack of face-down cards with blue backs  (5) a loose pile of cards with a curved arrow above it  (6) a red X cross mark
(7) a gold arrow pointing left  (8) a gold arrow pointing right  (9) a gold double arrow pointing right

1번부터 시작해 줘.
```

## 묶음 4: 배경 (6장)

```text
[이미지 목록: 묶음 4, zip 이름 ui_theme_4.zip]

All six are opaque landscape 3:2 backgrounds. They will be cropped to 16:9 from the top and bottom, so keep important things away from the top and bottom edges. Paint them slightly softer, lower in contrast and less saturated than the UI pieces. No characters, no creatures, no text, no UI elements.

1. MainMenu_BG_01.png
A bright royal-capital fantasy landscape in green and gold tones. A vast blue sky with soft fluffy clouds, sunlit rolling green meadows with small flowers, distant pale-blue mountains, and a white castle town with blue roofs and golden spires far away on the left third. Calm and uncluttered in the center.

2. DeckBuilder_BG_01.png
A bright, cozy summoner's study. Tall arched windows with sunlight and blue sky outside, wooden bookshelves full of books, a large wooden desk with a closed grimoire and scattered blank cards, potted plants, warm golden light. Soft and slightly out of focus, calm in the center.

3. Enhance_BG_01.png
A bright magic workshop. A round stone altar with a large blue crystal in the middle distance, shelves with potions and crystals, hanging lanterns, sunlight through a high window, floating light particles. Soft and slightly out of focus, calm in the center.

4. StageSelect_BG_01.png
A hand-painted fantasy world map seen from directly above on aged cream parchment: green plains, forests, mountain ranges, rivers, a coastline with blue sea, small castle and village markers, winding roads, and a compass rose without letters. No labels anywhere.

5. Prologue_Story_BG_01.png
A grand bright summoning hall seen from the front. White marble pillars, tall stained-glass windows casting blue and gold light, a large summoning circle made of simple geometric shapes on the polished floor, plain banners. Open space in the center and on both sides for characters.

6. Region01_Battle_BG_01.png
A fantasy battlefield seen from the front at eye level. A sunny grassy plain with a wide, flat, ancient stone-paved arena floor across the lower-middle of the image, a few broken stone pillars only at the far left and right edges, mountains and a bright sky with clouds in the upper half. The middle band stays open and empty because monster cards will be placed there.

1번부터 시작해 줘.
```

## 수령 기록

- 2026-10-06: 묶음 1~4의 20장을 모두 받았다. 다시 만든 그림은 없다.
- 모든 시트가 실제 투명 배경으로 나왔다. 시트 분할과 9분할 경계는 `process_theme.ps1`이 처리했다.
- 후처리로 고친 것: 전설 카드 틀 안쪽이 일부 투명해 안쪽 색으로 메웠다.
- 요청과 다르게 나온 것: 제목 리본이 곧지 않고 살짝 휘어 있다. 양 끝을 넓게 잡아 가운데만 늘리는 방식으로 쓴다.
