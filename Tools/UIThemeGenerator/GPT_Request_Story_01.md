# 스토리 인물 이미지 생성 요청문 1 (GPT용): 도윤과 그리모어

- 작성일: 2026-10-10 (53일차 스토리 시스템)
- 용도: 스토리 화면에 세우는 도윤과 그리모어의 대화용 그림. 도윤 5장, 그리모어 4장.
- 사용법: 인물마다 새 대화를 열고 그 인물의 [붙여 넣을 내용] 블록 전체를 한 번에 붙여 넣는다.
- 함께 첨부할 그림: `Aria_Battle_Normal_01.png`을 첨부해 그림체를 맞춘다.
- 받은 뒤: 그림을 채팅에 붙여 넣거나 내려받은 위치를 알려 주면, 배경을 지우고 `Assets/ProjectV/Resources/Characters/<이름>/`에 넣는다.
- 그림이 없어도 스토리는 진행된다. 그림이 없는 인물은 자리가 비고 이름과 대사만 나온다.
- 히로인은 대화용 그림이 생기기 전까지 전투 그림을 대신 쓴다.

## 그림 목록

파일 이름은 `<이름>_Talk_<표정>_01.png`이다. 대사 데이터에는 `Doyoon:Bluff`처럼 이름과 표정을 적는다.

| 인물 | 표정 | 파일 이름 |
| --- | --- | --- |
| 도윤 | 기본 | `Doyoon_Talk_Normal_01.png` |
| 도윤 | 웃음 | `Doyoon_Talk_Smile_01.png` |
| 도윤 | 당황 | `Doyoon_Talk_Flustered_01.png` |
| 도윤 | 진지 | `Doyoon_Talk_Serious_01.png` |
| 도윤 | 허세 | `Doyoon_Talk_Bluff_01.png` |
| 그리모어 | 기본 | `Grimoire_Talk_Normal_01.png` |
| 그리모어 | 웃음 | `Grimoire_Talk_Smile_01.png` |
| 그리모어 | 놀람 | `Grimoire_Talk_Surprised_01.png` |
| 그리모어 | 화남 | `Grimoire_Talk_Angry_01.png` |

없는 표정은 기본 표정으로 대신 보여 준다. 기본 한 장만 받아도 된다.

## 붙여 넣을 내용: 도윤

```text
Unity로 만드는 판타지 카드 배틀 게임의 대화 화면에 세울 캐릭터 일러스트를 만들어 줘. 한 인물의 그림 5장이고, 모두 같은 인물, 같은 옷, 같은 그림체여야 해. 아래 [공통 스타일], [인물], [공통 규칙]을 모든 이미지에 똑같이 적용해 줘. 이미지가 첨부되어 있으면 그 그림체, 외곽선 굵기, 채색 방식을 그대로 따라 줘. 첨부한 그림의 인물을 그리라는 뜻은 아니야.

[공통 스타일]
2D anime-style game character illustration for a bright high-fantasy card battle game. Body proportion about 7.5 heads tall. A bold, clean dark-brown outer outline with thinner inner lines. Cel shading with soft gradients, bright saturated colors, and separate clear highlights for cloth and leather. Soft light from the top-left. A lively, slightly comedic adventure mood with an expressive anime face.

[인물]
Do-yoon, a 22-year-old human man, a newly made monster summoner from the slums. An ordinary build, not flashy or heroic looking. Short black hair, brown eyes, a friendly and cheeky face. A simple cream shirt under a short brown travel jacket, dark trousers, leather boots, and a leather card pouch on his belt. Under his left arm he carries an old leather-bound grimoire with a gold sigil on the cover (no letters).

[공통 규칙]
1. 세로형 2:3 이미지 한 장에 인물 한 명만 그려 줘. 무릎 위부터 머리끝까지 나오는 구도이고, 몸은 화면 오른쪽을 살짝 향한 각도야.
2. 모든 그림에서 인물의 크기와 위치를 똑같이 맞춰 줘. 인물은 좌우 가운데에 두고, 머리끝은 이미지 위에서 10% 지점에 맞춰 줘.
3. 배경은 실제 알파 채널이 있는 투명 PNG로 만들어 줘. 체크무늬를 그려 넣으면 안 돼. 투명 배경이 안 되면 단색 마젠타(#FF00FF) 배경으로 만들고, 그림 안에는 마젠타를 쓰지 마.
4. 바닥, 바닥 그림자, 후광, 배경 소품은 그리지 마. 머리카락과 손이 이미지 가장자리에서 잘리면 안 돼.
5. 글자, 숫자, 말풍선, 로고, 워터마크를 절대 넣지 마.
6. 1번을 먼저 만들어 줘. 2번부터는 1번 그림을 기준으로 얼굴 생김새, 머리 모양, 옷, 색, 그림체를 그대로 두고 표정과 팔 동작만 바꿔 줘.
7. 한 번에 하나씩 만들어 줘. 하나를 만든 뒤 파일명을 말하고 멈춰. 내가 "다음"이라고 하면 다음 이미지를, "다시"라고 하면 같은 이미지를 다시 만들어 줘.
8. 크기 조정이나 자르기는 하지 마.

[이미지 목록: 5장]

1. Doyoon_Talk_Normal_01.png (기본)
Standing relaxed with a small easy smile, free hand in his jacket pocket.

2. Doyoon_Talk_Smile_01.png (웃음)
A wide, bright laugh with his eyes closed, free hand scratching the back of his head.

3. Doyoon_Talk_Flustered_01.png (당황)
Flustered and caught off guard: wide eyes, a nervous crooked smile, one sweat drop, free hand raised with the palm out.

4. Doyoon_Talk_Serious_01.png (진지)
Serious and determined: steady eyes, a closed firm mouth, free hand clenched into a fist at his side.

5. Doyoon_Talk_Bluff_01.png (허세)
Bragging with a smug grin, chin raised, free hand pointing at himself with his thumb.

1번부터 시작해 줘.
```

## 붙여 넣을 내용: 그리모어

```text
Unity로 만드는 판타지 카드 배틀 게임의 대화 화면에 세울 마스코트 캐릭터 일러스트를 만들어 줘. 한 캐릭터의 그림 4장이고, 모두 같은 캐릭터, 같은 그림체여야 해. 아래 [공통 스타일], [캐릭터], [공통 규칙]을 모든 이미지에 똑같이 적용해 줘. 이미지가 첨부되어 있으면 그 그림체, 외곽선 굵기, 채색 방식을 그대로 따라 줘. 첨부한 그림의 인물을 그리라는 뜻은 아니야.

[공통 스타일]
2D anime-style game mascot illustration for a bright high-fantasy card battle game. A bold, clean dark-brown outer outline with thinner inner lines. Cel shading with soft gradients and bright saturated colors. Soft light from the top-left. A lively, comedic mood.

[캐릭터]
Grimoire, a tiny palm-sized imp mascot who is the spirit of an ancient spellbook. Drawn in a chibi, super-deformed cartoon style, about 2.5 heads tall, like a game mascot. Two short horns, small bat-like wings, a thin long tail with an arrow tip, glowing gold eyes and a mischievous grin. He wears a little dark-violet tunic with simple gold sigils that match the book cover (no letters). He floats in the air beside an old leather-bound grimoire that also floats, with a gold sigil on its dark-violet cover and a faint gold glow.

[공통 규칙]
1. 세로형 2:3 이미지 한 장에 이 캐릭터와 떠 있는 마도서만 그려 줘. 캐릭터는 작게 그려 줘. 캐릭터와 마도서를 합친 크기가 이미지 높이의 40% 정도이고, 이미지의 위쪽 절반 가운데에 떠 있어야 해. 사람 옆에 세웠을 때 손바닥만 한 크기로 보이게 하려는 거야.
2. 모든 그림에서 캐릭터의 크기와 위치를 똑같이 맞춰 줘.
3. 귀엽고 코믹한 마스코트야. 선정적인 요소는 넣지 마.
4. 배경은 실제 알파 채널이 있는 투명 PNG로 만들어 줘. 체크무늬를 그려 넣으면 안 돼. 투명 배경이 안 되면 단색 마젠타(#FF00FF) 배경으로 만들고, 그림 안에는 마젠타를 쓰지 마.
5. 바닥, 그림자, 큰 후광, 배경 소품은 그리지 마. 날개와 꼬리가 이미지 가장자리에서 잘리면 안 돼.
6. 글자, 숫자, 말풍선, 로고, 워터마크를 절대 넣지 마.
7. 1번을 먼저 만들어 줘. 2번부터는 1번 그림을 기준으로 생김새와 색, 그림체를 그대로 두고 표정과 몸짓만 바꿔 줘.
8. 한 번에 하나씩 만들어 줘. 하나를 만든 뒤 파일명을 말하고 멈춰. 내가 "다음"이라고 하면 다음 이미지를, "다시"라고 하면 같은 이미지를 다시 만들어 줘.
9. 크기 조정이나 자르기는 하지 마.

[이미지 목록: 4장]

1. Grimoire_Talk_Normal_01.png (기본)
Floating with arms crossed and a mischievous one-sided grin, the book hovering at his side.

2. Grimoire_Talk_Smile_01.png (웃음)
Laughing with his eyes closed and both hands on his belly, wings flapping.

3. Grimoire_Talk_Surprised_01.png (놀람)
Startled: huge round eyes, open mouth, wings and tail sticking straight up, the book tilting.

4. Grimoire_Talk_Angry_01.png (화남)
Comically angry: puffed cheeks, a small steam puff over his head, shaking a tiny fist.

1번부터 시작해 줘.
```

## 수령 기록

- 아직 받은 그림이 없다.
