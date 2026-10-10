# UI 테마 이미지 가공 도구

GPT로 받은 UI 시트(zip)를 Unity에서 바로 쓸 수 있는 조각으로 바꾼다. 그림을 새로 그리지는 않고, 받은 그림을 자르고 다듬기만 한다.

## 쓰는 순서

1. 받은 `ui_theme_1.zip`, `ui_theme_2.zip`처럼 이름이 `ui_theme_`으로 시작하는 zip을 이 폴더에 넣는다. 일부만 있어도 되고, 없는 시트는 건너뛴다.
2. 아래 명령을 실행한다. (Unity는 켜 둔 채로 실행해도 된다)

```bash
powershell -ExecutionPolicy Bypass -File Tools/UIThemeGenerator/process_theme.ps1
```

3. `preview` 폴더의 `*_preview.png`에서 잘린 모양과 늘어난 모양을 확인한다.
4. Unity에서 `Project V > 씬 UI 다시 구성`을 실행한다. 이미지 가져오기 설정, 테마 에셋, 씬 적용이 한 번에 처리된다.

## 하는 일

| 단계 | 내용 |
| --- | --- |
| 배경 제거 | 투명 배경은 그대로 쓰고, 마젠타·단색·체크무늬 배경은 지운다 |
| 시트 분할 | 투명한 틈을 기준으로 줄과 칸을 나눈다 |
| 늘어나는 구간 계산 | 패널은 네 변, 막대는 좌우 끝 장식의 길이를 구해 9분할 경계로 쓴다 |
| 크기 정리 | 아이콘 256, 카드 틀 400x600, 배경 1920x1080(JPG) |
| 카드 틀 안쪽 정리 | 안쪽이 비쳐 보이는 부분을 안쪽 색으로 메운다 |
| 아이콘 묶음 | 글자 사이에 넣을 아이콘(현재 63개, 최대 64개)을 한 장으로 모은다 |
| 목록 파일 | `Assets/ProjectV/Art/UI/UIThemeManifest.txt`에 조각 이름과 경계를 적는다 |

## 파일

| 파일 | 역할 |
| --- | --- |
| `theme_spec.txt` | 시트별 칸 수, 종류, 조각 이름. 경계를 직접 정하려면 `border` 줄을 추가한다 |
| `process_theme.ps1` | 실행 스크립트 |
| `Process*.cs` | 가공 코드 (실행할 때 PowerShell이 컴파일한다) |
| `raw/` | 받은 원본 시트 보관 (기획서 12.14: 원본과 게임 적용 파일 분리) |
| `preview/` | 확인용 그림과 `report.txt` (git 제외) |
| `GPT_Request_UI_01.md` | GPT에 보낸 요청문 기록 (기획서 12.18) |
| `GPT_Request_UI_02.md` | 그리모어 강화 화면용 요청문 (묶음 5, `ui_theme_5.zip`) |
| `GPT_Request_UI_03.md` | 상점과 소모품용 요청문 (묶음 6, `ui_theme_6.zip`) |
| `GPT_Request_UI_04.md` | 저장과 불러오기용 요청문 (묶음 7, `ui_theme_7.zip`) |
| `GPT_Request_UI_05.md` | 월드맵용 요청문 (묶음 8, `ui_theme_8.zip`) |
| `GPT_Request_UI_06.md` | 아직 받지 못한 그림 2장(저장 화면 배경, 월드맵 표시)을 다시 쓴 요청문 (`ui_theme_9.zip`) |

## 그림을 바꾸고 싶을 때

같은 파일명으로 다시 받아 `raw` 폴더에 덮어쓰거나 zip을 다시 넣고 2번부터 반복한다. 조각 이름이 같으면 Unity 쪽 연결은 그대로 유지된다.
