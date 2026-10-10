# 히로인 이미지 생성 요청문 1 (GPT용): 지역 1 전투 일러스트와 피격 이미지

- 작성일: 2026-10-10 (49일차 히로인전)
- 용도: 지역 1 히로인 4명(아리아, 실리아, 리엔, 프라나야)의 전투 화면 그림. 한 명에 10장이고, 기본 자세 1장과 피격 5장, 상태 1장, 패배 2장, 공격 1장이다.
- 사용법: 히로인 한 명마다 새 대화를 열고, 그 히로인의 [붙여 넣을 내용] 블록 전체를 한 번에 붙여 넣는다.
- 함께 첨부할 그림: 아리아는 첨부 없이 시작한다. 나머지 세 명은 완성된 `Aria_Battle_Normal_01.png`을 첨부해 그림체를 맞춘다.
- 받은 뒤: 그림을 채팅에 붙여 넣거나 내려받은 위치를 알려 주면, 배경을 지우고 `Assets/ProjectV/Resources/Characters/<이름>/`에 넣는다.
- 그림이 없어도 화면은 동작한다. 그림이 없는 히로인은 지금처럼 글자와 게이지만 보인다.
- 필수 6장만 받아도 된다. 선택 그림이 없으면 비슷한 그림으로 대신 보여 준다. (아래 표 참고)
- 서브 히로인 세 명의 외형은 기획서에 없어 직위와 성격에 맞춰 정했다. 바꾸고 싶으면 [인물] 문단만 고치면 된다.

## 그림 목록과 쓰이는 때

파일 이름은 `<이름>_Battle_<상태>.png`이다. 이름은 Aria, Silia, Rien, Pranaya를 쓴다.

| 번호 | 상태 | 파일 이름의 상태 부분 | 쓰이는 때 | 구분 | 없을 때 대신 쓰는 그림 |
| --- | --- | --- | --- | --- | --- |
| 1 | 기본 자세 | `Normal_01` | 평소 (성욕 49 이하) | 필수 | 없음 (그림 칸을 숨긴다) |
| 2 | 가벼운 피격 | `Hit_01` | HP 피해를 조금 받았을 때 | 필수 | 기본 자세 |
| 3 | 강한 피격 | `Hit_02` | HP 피해를 크게 받았을 때 (최대 HP의 10% 이상) | 필수 | 가벼운 피격 |
| 4 | 방어 성공 | `Guard_01` | 보호막이 피해를 모두 막았을 때 | 선택 | 기본 자세 |
| 5 | 성욕 공격 피격 | `LustHit_01` | 성욕 공격을 받았을 때 (성욕 49 이하) | 필수 | 가벼운 피격 |
| 6 | 동요 상태 | `Shaken_01` | 성욕이 50 이상일 때의 평소 모습 | 선택 | 기본 자세 |
| 7 | 성욕 공격 피격 (동요) | `LustHit_02` | 성욕 50 이상에서 성욕 공격을 받았을 때 | 선택 | 성욕 공격 피격 |
| 8 | HP 패배 | `Defeat_01` | HP가 0이 되어 졌을 때 | 필수 | 강한 피격 |
| 9 | 성욕 최대 | `Climax_01` | 성욕이 100이 되어 졌을 때 | 필수 | 성욕 공격 피격 (동요) |
| 10 | 공격 | `Attack_01` | 히로인이 공격 행동을 할 때 | 선택 | 기본 자세 |

피격 그림은 0.7초 동안 보인 뒤 평소 그림으로 돌아간다. 패배 그림은 전투가 끝난 뒤 계속 보인다.

## 붙여 넣을 내용: 아리아 (주요 히로인, 엘프 / 왕국 기사단장)

```text
Unity로 만드는 판타지 카드 배틀 게임의 캐릭터 전투 일러스트를 만들어 줘. 한 인물의 그림 10장이고, 모두 같은 인물, 같은 옷, 같은 그림체여야 해. 아래 [공통 스타일], [인물], [공통 규칙]을 모든 이미지에 똑같이 적용해 줘. 이미지가 첨부되어 있으면 그 그림체, 외곽선 굵기, 채색 방식을 그대로 따라 줘. 첨부한 그림의 인물을 그리라는 뜻은 아니야.

[공통 스타일]
2D anime-style game character illustration for a bright high-fantasy card battle game. Body proportion about 7.5 heads tall. A bold, clean dark-brown outer outline with thinner inner lines. Cel shading with soft gradients, bright saturated colors, and separate clear highlights for metal, cloth and leather. Soft light from the top-left. A lively, slightly comedic adventure mood with an expressive anime face.

[인물]
Aria, an adult elf woman, commander of the royal knights. Tall, dignified, mature adult proportions. Long silver hair tied in a high ponytail, long pointed elf ears, sharp emerald-green eyes, a strict and cool face. Polished silver plate armor (breastplate, pauldrons, gauntlets, tassets) with gold trim and forest-green cloth accents, and a short forest-green cape. A straight longsword in her right hand and a silver kite shield with a simple gold-and-green leaf crest (no letters) on her left arm.

[공통 규칙]
1. 인물은 성인 여성이야. 옷과 장비를 모두 갖춘 모습으로 그리고, 노출이나 찢어진 옷, 선정적인 자세는 넣지 마. 피와 상처도 그리지 마. 반응은 만화적인 표정과 효과(땀방울, 홍조, 충격 표시, 김, 하트 반짝임)로만 표현해 줘.
2. 세로형 2:3 이미지 한 장에 인물 한 명만 그려 줘. 무릎 위부터 머리끝까지 나오는 구도이고, 몸은 정면에서 살짝만 돌린 각도야.
3. 모든 그림에서 인물의 크기를 똑같이 맞춰 줘. 인물은 좌우 가운데에 두고, 머리끝은 이미지 위에서 10% 지점에 맞춰 줘. 게임에서 같은 자리에 그림을 바꿔 끼우기 때문이야. 자세가 낮아지는 그림도 머리는 이미지 위쪽 3분의 1 안에 있어야 해.
4. 배경은 실제 알파 채널이 있는 투명 PNG로 만들어 줘. 체크무늬를 그려 넣으면 안 돼. 투명 배경이 안 되면 단색 마젠타(#FF00FF) 배경으로 만들고, 그림 안에는 마젠타를 쓰지 마.
5. 바닥, 바닥 그림자, 후광, 배경 소품은 그리지 마. 무기, 머리카락, 효과가 이미지 가장자리에서 잘리면 안 돼. 사방에 여백을 남겨 줘.
6. 글자, 숫자, 말풍선, 로고, 워터마크를 절대 넣지 마.
7. 1번을 먼저 만들어 줘. 2번부터는 1번 그림을 기준으로 삼아 얼굴 생김새, 머리 모양, 옷, 장비, 색, 그림체를 그대로 두고 표정과 자세만 바꿔 줘.
8. 한 번에 하나씩 만들어 줘. 하나를 만든 뒤 파일명을 말하고 멈춰. 내가 "다음"이라고 하면 다음 이미지를, "다시"라고 하면 같은 이미지를 다시 만들어 줘.
9. 크기 조정이나 자르기는 하지 마. 생성된 원본 그대로 두면 돼.

[이미지 목록: 10장]

1. Aria_Battle_Normal_01.png (기본 자세)
Standing battle-ready stance, sword lowered at her side, shield held in front of her body. Calm, confident expression, looking at the viewer.

2. Aria_Battle_Hit_01.png (가벼운 피격)
She has just taken a light hit. Flinching slightly, one eye shut, teeth clenched, upper body leaning back a little, still holding her sword and shield firmly. One small white impact star near her shoulder.

3. Aria_Battle_Hit_02.png (강한 피격)
She has just taken a heavy hit. Staggering backward with her upper body bent, both eyes squeezed shut, mouth open in a shout, hair and cloth whipping from the impact, her sword and shield knocked out of line. Two or three white impact stars and one sweat drop.

4. Aria_Battle_Guard_01.png (방어 성공)
She blocks an attack squarely with her raised kite shield, sword held back and ready. Sharp focused eyes, a firm braced stance, a few small sparks where the attack was stopped.

5. Aria_Battle_LustHit_01.png (성욕 공격 피격)
She has just been hit by a charm spell and is flustered. Surprised wide eyes, a light blush across her cheeks, one arm pulled in front of her chest in a startled gesture, one sweat drop, a few small pink heart-shaped sparkles popping near her head.

6. Aria_Battle_Shaken_01.png (동요 상태)
Still in her battle stance but visibly shaken and embarrassed. A clear blush across her cheeks, knitted eyebrows, eyes glancing away to the side, a slightly wobbly guard with her sword and shield, two small sweat drops.

7. Aria_Battle_LustHit_02.png (성욕 공격 피격 (동요))
Hit by a charm spell again while already shaken. Her whole face is deep red, eyes shut tight, a wavy embarrassed mouth, shoulders hunched, small puffs of steam rising from her head, several pink heart-shaped sparkles around her, and she almost lets her sword and shield slip from her hand.

8. Aria_Battle_Defeat_01.png (HP 패배)
Defeated and out of strength but still on her feet. Slumped forward and propped up on her sword and shield, head hanging low, eyes closed with a tired, frustrated expression, breathing hard, several sweat drops. No injuries.

9. Aria_Battle_Climax_01.png (성욕 최대)
Completely overheated and dizzy, in a comedic way. Wobbling on her feet with her knees turned inward, comedic spiral eyes, a bright red face, big puffs of steam rising from her head, a dazed wobbly mouth, arms limp and her sword and shield slipping from her hand. A funny knocked-out-by-embarrassment look.

10. Aria_Battle_Attack_01.png (공격)
Attacking: a strong horizontal slash with her longsword, shield pulled in close. A fierce, determined expression, a dynamic twist of the upper body, and one short clean motion arc following the attack.

1번부터 시작해 줘.
```

## 붙여 넣을 내용: 실리아 (서브 히로인, 인간 / 마법학교 수습생)

```text
Unity로 만드는 판타지 카드 배틀 게임의 캐릭터 전투 일러스트를 만들어 줘. 한 인물의 그림 10장이고, 모두 같은 인물, 같은 옷, 같은 그림체여야 해. 아래 [공통 스타일], [인물], [공통 규칙]을 모든 이미지에 똑같이 적용해 줘. 이미지가 첨부되어 있으면 그 그림체, 외곽선 굵기, 채색 방식을 그대로 따라 줘. 첨부한 그림의 인물을 그리라는 뜻은 아니야.

[공통 스타일]
2D anime-style game character illustration for a bright high-fantasy card battle game. Body proportion about 7.5 heads tall. A bold, clean dark-brown outer outline with thinner inner lines. Cel shading with soft gradients, bright saturated colors, and separate clear highlights for metal, cloth and leather. Soft light from the top-left. A lively, slightly comedic adventure mood with an expressive anime face.

[인물]
Silia, an adult human woman in her early twenties, an apprentice mage of the imperial magic academy. Bright, curious and a little clumsy. Clearly an adult with mature adult proportions, not childlike. Shoulder-length wavy chestnut-brown hair with a small side braid, amber eyes, a cheerful face. A navy-blue apprentice robe with cream trim over a white blouse, a short capelet, a brown leather belt with a spellbook holster, and a soft pointed mage hat tilted back. A wooden staff topped with a small glowing blue crystal in her right hand.

[공통 규칙]
1. 인물은 성인 여성이야. 옷과 장비를 모두 갖춘 모습으로 그리고, 노출이나 찢어진 옷, 선정적인 자세는 넣지 마. 피와 상처도 그리지 마. 반응은 만화적인 표정과 효과(땀방울, 홍조, 충격 표시, 김, 하트 반짝임)로만 표현해 줘.
2. 세로형 2:3 이미지 한 장에 인물 한 명만 그려 줘. 무릎 위부터 머리끝까지 나오는 구도이고, 몸은 정면에서 살짝만 돌린 각도야.
3. 모든 그림에서 인물의 크기를 똑같이 맞춰 줘. 인물은 좌우 가운데에 두고, 머리끝은 이미지 위에서 10% 지점에 맞춰 줘. 게임에서 같은 자리에 그림을 바꿔 끼우기 때문이야. 자세가 낮아지는 그림도 머리는 이미지 위쪽 3분의 1 안에 있어야 해.
4. 배경은 실제 알파 채널이 있는 투명 PNG로 만들어 줘. 체크무늬를 그려 넣으면 안 돼. 투명 배경이 안 되면 단색 마젠타(#FF00FF) 배경으로 만들고, 그림 안에는 마젠타를 쓰지 마.
5. 바닥, 바닥 그림자, 후광, 배경 소품은 그리지 마. 무기, 머리카락, 효과가 이미지 가장자리에서 잘리면 안 돼. 사방에 여백을 남겨 줘.
6. 글자, 숫자, 말풍선, 로고, 워터마크를 절대 넣지 마.
7. 1번을 먼저 만들어 줘. 2번부터는 1번 그림을 기준으로 삼아 얼굴 생김새, 머리 모양, 옷, 장비, 색, 그림체를 그대로 두고 표정과 자세만 바꿔 줘.
8. 한 번에 하나씩 만들어 줘. 하나를 만든 뒤 파일명을 말하고 멈춰. 내가 "다음"이라고 하면 다음 이미지를, "다시"라고 하면 같은 이미지를 다시 만들어 줘.
9. 크기 조정이나 자르기는 하지 마. 생성된 원본 그대로 두면 돼.

[이미지 목록: 10장]

1. Silia_Battle_Normal_01.png (기본 자세)
Standing battle-ready stance, staff held upright in her right hand, left hand resting on the spellbook at her belt. Calm, confident expression, looking at the viewer.

2. Silia_Battle_Hit_01.png (가벼운 피격)
She has just taken a light hit. Flinching slightly, one eye shut, teeth clenched, upper body leaning back a little, still holding her staff firmly. One small white impact star near her shoulder.

3. Silia_Battle_Hit_02.png (강한 피격)
She has just taken a heavy hit. Staggering backward with her upper body bent, both eyes squeezed shut, mouth open in a shout, hair and cloth whipping from the impact, her staff knocked out of line. Two or three white impact stars and one sweat drop.

4. Silia_Battle_Guard_01.png (방어 성공)
She blocks an attack with a round translucent blue magic barrier projected from her staff. Sharp focused eyes, a firm braced stance, a few small sparks where the attack was stopped.

5. Silia_Battle_LustHit_01.png (성욕 공격 피격)
She has just been hit by a charm spell and is flustered. Surprised wide eyes, a light blush across her cheeks, one arm pulled in front of her chest in a startled gesture, one sweat drop, a few small pink heart-shaped sparkles popping near her head.

6. Silia_Battle_Shaken_01.png (동요 상태)
Still in her battle stance but visibly shaken and embarrassed. A clear blush across her cheeks, knitted eyebrows, eyes glancing away to the side, a slightly wobbly guard with her staff, two small sweat drops.

7. Silia_Battle_LustHit_02.png (성욕 공격 피격 (동요))
Hit by a charm spell again while already shaken. Her whole face is deep red, eyes shut tight, a wavy embarrassed mouth, shoulders hunched, small puffs of steam rising from her head, several pink heart-shaped sparkles around her, and she almost lets her staff slip from her hand.

8. Silia_Battle_Defeat_01.png (HP 패배)
Defeated and out of strength but still on her feet. Slumped forward and propped up on her staff, head hanging low, eyes closed with a tired, frustrated expression, breathing hard, several sweat drops. No injuries.

9. Silia_Battle_Climax_01.png (성욕 최대)
Completely overheated and dizzy, in a comedic way. Wobbling on her feet with her knees turned inward, comedic spiral eyes, a bright red face, big puffs of steam rising from her head, a dazed wobbly mouth, arms limp and her staff slipping from her hand. A funny knocked-out-by-embarrassment look.

10. Silia_Battle_Attack_01.png (공격)
Attacking: thrusting her staff forward and firing a glowing blue magic arrow. A fierce, determined expression, a dynamic twist of the upper body, and one short clean motion arc following the attack.

1번부터 시작해 줘.
```

## 붙여 넣을 내용: 리엔 (서브 히로인, 인간 / 마법학교 상급 조교)

```text
Unity로 만드는 판타지 카드 배틀 게임의 캐릭터 전투 일러스트를 만들어 줘. 한 인물의 그림 10장이고, 모두 같은 인물, 같은 옷, 같은 그림체여야 해. 아래 [공통 스타일], [인물], [공통 규칙]을 모든 이미지에 똑같이 적용해 줘. 이미지가 첨부되어 있으면 그 그림체, 외곽선 굵기, 채색 방식을 그대로 따라 줘. 첨부한 그림의 인물을 그리라는 뜻은 아니야.

[공통 스타일]
2D anime-style game character illustration for a bright high-fantasy card battle game. Body proportion about 7.5 heads tall. A bold, clean dark-brown outer outline with thinner inner lines. Cel shading with soft gradients, bright saturated colors, and separate clear highlights for metal, cloth and leather. Soft light from the top-left. A lively, slightly comedic adventure mood with an expressive anime face.

[인물]
Rien, an adult human woman in her late twenties, a senior teaching assistant at the imperial magic academy. Cold, composed and strict about rules. Mature adult proportions. Long straight black hair with blunt bangs, narrow violet eyes behind thin rectangular silver glasses. A fitted dark-violet instructor's long coat with silver buttons and a high collar, white gloves, and a silver chain with a small key at her waist. A slim silver wand like a pointer in her right hand and a thick closed ledger (blank cover, no letters) held against her side with her left arm.

[공통 규칙]
1. 인물은 성인 여성이야. 옷과 장비를 모두 갖춘 모습으로 그리고, 노출이나 찢어진 옷, 선정적인 자세는 넣지 마. 피와 상처도 그리지 마. 반응은 만화적인 표정과 효과(땀방울, 홍조, 충격 표시, 김, 하트 반짝임)로만 표현해 줘.
2. 세로형 2:3 이미지 한 장에 인물 한 명만 그려 줘. 무릎 위부터 머리끝까지 나오는 구도이고, 몸은 정면에서 살짝만 돌린 각도야.
3. 모든 그림에서 인물의 크기를 똑같이 맞춰 줘. 인물은 좌우 가운데에 두고, 머리끝은 이미지 위에서 10% 지점에 맞춰 줘. 게임에서 같은 자리에 그림을 바꿔 끼우기 때문이야. 자세가 낮아지는 그림도 머리는 이미지 위쪽 3분의 1 안에 있어야 해.
4. 배경은 실제 알파 채널이 있는 투명 PNG로 만들어 줘. 체크무늬를 그려 넣으면 안 돼. 투명 배경이 안 되면 단색 마젠타(#FF00FF) 배경으로 만들고, 그림 안에는 마젠타를 쓰지 마.
5. 바닥, 바닥 그림자, 후광, 배경 소품은 그리지 마. 무기, 머리카락, 효과가 이미지 가장자리에서 잘리면 안 돼. 사방에 여백을 남겨 줘.
6. 글자, 숫자, 말풍선, 로고, 워터마크를 절대 넣지 마.
7. 1번을 먼저 만들어 줘. 2번부터는 1번 그림을 기준으로 삼아 얼굴 생김새, 머리 모양, 옷, 장비, 색, 그림체를 그대로 두고 표정과 자세만 바꿔 줘.
8. 한 번에 하나씩 만들어 줘. 하나를 만든 뒤 파일명을 말하고 멈춰. 내가 "다음"이라고 하면 다음 이미지를, "다시"라고 하면 같은 이미지를 다시 만들어 줘.
9. 크기 조정이나 자르기는 하지 마. 생성된 원본 그대로 두면 돼.

[이미지 목록: 10장]

1. Rien_Battle_Normal_01.png (기본 자세)
Standing battle-ready stance, wand pointed down at her side, ledger tucked under her left arm. Calm, confident expression, looking at the viewer.

2. Rien_Battle_Hit_01.png (가벼운 피격)
She has just taken a light hit. Flinching slightly, one eye shut, teeth clenched, upper body leaning back a little, still holding her wand and ledger firmly. One small white impact star near her shoulder.

3. Rien_Battle_Hit_02.png (강한 피격)
She has just taken a heavy hit. Staggering backward with her upper body bent, both eyes squeezed shut, mouth open in a shout, hair and cloth whipping from the impact, her wand and ledger knocked out of line. Two or three white impact stars and one sweat drop.

4. Rien_Battle_Guard_01.png (방어 성공)
She blocks an attack with a flat hexagonal violet magic barrier raised with her wand. Sharp focused eyes, a firm braced stance, a few small sparks where the attack was stopped.

5. Rien_Battle_LustHit_01.png (성욕 공격 피격)
She has just been hit by a charm spell and is flustered. Surprised wide eyes, a light blush across her cheeks, one arm pulled in front of her chest in a startled gesture, one sweat drop, a few small pink heart-shaped sparkles popping near her head.

6. Rien_Battle_Shaken_01.png (동요 상태)
Still in her battle stance but visibly shaken and embarrassed. A clear blush across her cheeks, knitted eyebrows, eyes glancing away to the side, a slightly wobbly guard with her wand and ledger, two small sweat drops.

7. Rien_Battle_LustHit_02.png (성욕 공격 피격 (동요))
Hit by a charm spell again while already shaken. Her whole face is deep red, eyes shut tight, a wavy embarrassed mouth, shoulders hunched, small puffs of steam rising from her head, several pink heart-shaped sparkles around her, and she almost lets her wand and ledger slip from her hand.

8. Rien_Battle_Defeat_01.png (HP 패배)
Defeated and out of strength but still on her feet. Slumped forward and propped up on her wand and ledger, head hanging low, eyes closed with a tired, frustrated expression, breathing hard, several sweat drops. No injuries.

9. Rien_Battle_Climax_01.png (성욕 최대)
Completely overheated and dizzy, in a comedic way. Wobbling on her feet with her knees turned inward, comedic spiral eyes, a bright red face, big puffs of steam rising from her head, a dazed wobbly mouth, arms limp and her wand and ledger slipping from her hand. A funny knocked-out-by-embarrassment look.

10. Rien_Battle_Attack_01.png (공격)
Attacking: snapping her wand forward to cast glowing violet binding chains. A fierce, determined expression, a dynamic twist of the upper body, and one short clean motion arc following the attack.

1번부터 시작해 줘.
```

## 붙여 넣을 내용: 프라나야 (서브 히로인, 인간 / 제국 마법 기사단 마도검사)

```text
Unity로 만드는 판타지 카드 배틀 게임의 캐릭터 전투 일러스트를 만들어 줘. 한 인물의 그림 10장이고, 모두 같은 인물, 같은 옷, 같은 그림체여야 해. 아래 [공통 스타일], [인물], [공통 규칙]을 모든 이미지에 똑같이 적용해 줘. 이미지가 첨부되어 있으면 그 그림체, 외곽선 굵기, 채색 방식을 그대로 따라 줘. 첨부한 그림의 인물을 그리라는 뜻은 아니야.

[공통 스타일]
2D anime-style game character illustration for a bright high-fantasy card battle game. Body proportion about 7.5 heads tall. A bold, clean dark-brown outer outline with thinner inner lines. Cel shading with soft gradients, bright saturated colors, and separate clear highlights for metal, cloth and leather. Soft light from the top-left. A lively, slightly comedic adventure mood with an expressive anime face.

[인물]
Pranaya, an adult human woman in her mid twenties, a spellblade of the imperial magic knights. Direct, bold and happy to fight. Athletic, mature adult proportions. Short messy flame-red hair, golden-orange eyes, a confident grin. A crimson-and-black knight uniform with light steel armor (one pauldron, bracers, greaves) and a short half-cape. A one-handed sword whose blade is wrapped in bright orange flame in her right hand, and a small ball of fire floating above her open left hand.

[공통 규칙]
1. 인물은 성인 여성이야. 옷과 장비를 모두 갖춘 모습으로 그리고, 노출이나 찢어진 옷, 선정적인 자세는 넣지 마. 피와 상처도 그리지 마. 반응은 만화적인 표정과 효과(땀방울, 홍조, 충격 표시, 김, 하트 반짝임)로만 표현해 줘.
2. 세로형 2:3 이미지 한 장에 인물 한 명만 그려 줘. 무릎 위부터 머리끝까지 나오는 구도이고, 몸은 정면에서 살짝만 돌린 각도야.
3. 모든 그림에서 인물의 크기를 똑같이 맞춰 줘. 인물은 좌우 가운데에 두고, 머리끝은 이미지 위에서 10% 지점에 맞춰 줘. 게임에서 같은 자리에 그림을 바꿔 끼우기 때문이야. 자세가 낮아지는 그림도 머리는 이미지 위쪽 3분의 1 안에 있어야 해.
4. 배경은 실제 알파 채널이 있는 투명 PNG로 만들어 줘. 체크무늬를 그려 넣으면 안 돼. 투명 배경이 안 되면 단색 마젠타(#FF00FF) 배경으로 만들고, 그림 안에는 마젠타를 쓰지 마.
5. 바닥, 바닥 그림자, 후광, 배경 소품은 그리지 마. 무기, 머리카락, 효과가 이미지 가장자리에서 잘리면 안 돼. 사방에 여백을 남겨 줘.
6. 글자, 숫자, 말풍선, 로고, 워터마크를 절대 넣지 마.
7. 1번을 먼저 만들어 줘. 2번부터는 1번 그림을 기준으로 삼아 얼굴 생김새, 머리 모양, 옷, 장비, 색, 그림체를 그대로 두고 표정과 자세만 바꿔 줘.
8. 한 번에 하나씩 만들어 줘. 하나를 만든 뒤 파일명을 말하고 멈춰. 내가 "다음"이라고 하면 다음 이미지를, "다시"라고 하면 같은 이미지를 다시 만들어 줘.
9. 크기 조정이나 자르기는 하지 마. 생성된 원본 그대로 두면 돼.

[이미지 목록: 10장]

1. Pranaya_Battle_Normal_01.png (기본 자세)
Standing battle-ready stance, flaming sword resting on her right shoulder, left hand on her hip. Calm, confident expression, looking at the viewer.

2. Pranaya_Battle_Hit_01.png (가벼운 피격)
She has just taken a light hit. Flinching slightly, one eye shut, teeth clenched, upper body leaning back a little, still holding her flaming sword firmly. One small white impact star near her shoulder.

3. Pranaya_Battle_Hit_02.png (강한 피격)
She has just taken a heavy hit. Staggering backward with her upper body bent, both eyes squeezed shut, mouth open in a shout, hair and cloth whipping from the impact, her flaming sword knocked out of line. Two or three white impact stars and one sweat drop.

4. Pranaya_Battle_Guard_01.png (방어 성공)
She parries an attack with the flat of her flaming sword, sparks flying. Sharp focused eyes, a firm braced stance, a few small sparks where the attack was stopped.

5. Pranaya_Battle_LustHit_01.png (성욕 공격 피격)
She has just been hit by a charm spell and is flustered. Surprised wide eyes, a light blush across her cheeks, one arm pulled in front of her chest in a startled gesture, one sweat drop, a few small pink heart-shaped sparkles popping near her head.

6. Pranaya_Battle_Shaken_01.png (동요 상태)
Still in her battle stance but visibly shaken and embarrassed. A clear blush across her cheeks, knitted eyebrows, eyes glancing away to the side, a slightly wobbly guard with her flaming sword, two small sweat drops.

7. Pranaya_Battle_LustHit_02.png (성욕 공격 피격 (동요))
Hit by a charm spell again while already shaken. Her whole face is deep red, eyes shut tight, a wavy embarrassed mouth, shoulders hunched, small puffs of steam rising from her head, several pink heart-shaped sparkles around her, and she almost lets her flaming sword slip from her hand.

8. Pranaya_Battle_Defeat_01.png (HP 패배)
Defeated and out of strength but still on her feet. Slumped forward and propped up on her flaming sword, head hanging low, eyes closed with a tired, frustrated expression, breathing hard, several sweat drops. No injuries.

9. Pranaya_Battle_Climax_01.png (성욕 최대)
Completely overheated and dizzy, in a comedic way. Wobbling on her feet with her knees turned inward, comedic spiral eyes, a bright red face, big puffs of steam rising from her head, a dazed wobbly mouth, arms limp and her flaming sword slipping from her hand. A funny knocked-out-by-embarrassment look.

10. Pranaya_Battle_Attack_01.png (공격)
Attacking: a wide downward slash with her flaming sword, trailing fire. A fierce, determined expression, a dynamic twist of the upper body, and one short clean motion arc following the attack.

1번부터 시작해 줘.
```

## 수령 기록

- 2026-10-10: `project_v_heroine_battle.zip` 수령. 네 명 모두 10장씩 40장, 모두 1024x1536 투명 배경 PNG.
- 배경 처리 결과: 40장 모두 투명 배경으로 인식되어 가장자리만 정리했고, 크기와 위치는 그대로 두었다.
- 저장 위치: `Assets/ProjectV/Resources/Characters/<이름>/`. 원본은 `raw/Heroines/`에 두고 저장소에는 넣지 않는다.
- 그림 안의 인물 범위: 머리 위 효과를 포함한 윗선이 이미지 위에서 2~13% 사이, 얼굴 중심은 20~28% 사이에 있다. 그림 칸은 이 범위에 맞춰 잡았다.
