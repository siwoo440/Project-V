// 테마 이미지 이름 모음. Art 폴더의 파일 이름(확장자 제외)과 같다. (기획서 12.16)
public static class UIKeys
{
    // 패널과 버튼
    public const string PanelParchment = "UI_Panel_Parchment_01"; // 밝은 양피지 패널
    public const string PanelNavy = "UI_Panel_Navy_01";           // 짙은 남색 패널
    public const string ButtonBlue = "UI_Button_Blue_01";         // 기본 버튼
    public const string ButtonGold = "UI_Button_Gold_01";         // 강조 버튼
    public const string ButtonRed = "UI_Button_Red_01";           // 경고 버튼

    // 띠와 판
    public const string RibbonTitle = "UI_Ribbon_Title_01"; // 화면 제목 리본
    public const string BarHeader = "UI_Bar_Header_01";     // 상단 띠
    public const string DividerGold = "UI_Divider_Gold_01"; // 구분선
    public const string RowNormal = "UI_Row_Normal_01";     // 목록 줄
    public const string RowSelected = "UI_Row_Selected_01"; // 선택한 목록 줄
    public const string InputField = "UI_Input_Field_01";   // 입력 칸
    public const string PlateLabel = "UI_Plate_Label_01";   // 짙은 글자 받침
    public const string PlateName = "UI_Plate_Name_01";     // 밝은 이름판

    // 카드와 전투
    public const string CardFrameCommon = "UI_CardFrame_Common_01";
    public const string CardFrameRare = "UI_CardFrame_Rare_01";
    public const string CardFrameSpecial = "UI_CardFrame_Special_01";
    public const string CardFrameLegendary = "UI_CardFrame_Legendary_01";
    public const string SlotUnit = "UI_Slot_Unit_01";   // 필드 마물 판
    public const string SlotEmpty = "UI_Slot_Empty_01"; // 빈 카드 자리
    public const string GaugeFrame = "UI_Gauge_Frame_01";
    public const string GaugeRed = "UI_Gauge_Red_01";
    public const string GaugePink = "UI_Gauge_Pink_01";
    public const string GaugeBlue = "UI_Gauge_Blue_01";
    public const string GaugeGreen = "UI_Gauge_Green_01";
    public const string EmblemCrest = "UI_Emblem_Crest_01";     // 게임 문장
    public const string EmblemVictory = "UI_Emblem_Victory_01"; // 승리 문장
    public const string EmblemDefeat = "UI_Emblem_Defeat_01";   // 패배 문장

    // 메뉴 아이콘
    public const string IconStory = "Icon_Menu_Story_01";
    public const string IconStage = "Icon_Menu_Stage_01";
    public const string IconDeck = "Icon_Menu_Deck_01";
    public const string IconEnhance = "Icon_Menu_Enhance_01";
    public const string IconQuit = "Icon_Menu_Quit_01";
    public const string IconSummoner = "Icon_Action_Skill_01"; // 소환사 메뉴는 마법 별 아이콘을 함께 쓴다.
    public const string IconLog = "Icon_Menu_Log_01";
    public const string IconCollection = "Icon_Menu_Collection_01";
    public const string IconLock = "Icon_Menu_Lock_01";
    public const string IconClose = "Icon_Misc_Close_01"; // 닫기
    public const string IconGrimoire = "Icon_Menu_Grimoire_01"; // 그리모어 강화 메뉴
    public const string IconShop = "Icon_Menu_Shop_01";         // 상점 메뉴

    // 전투 종류 아이콘
    public const string StageNormal = "Icon_Stage_Normal_01";
    public const string StageCapture = "Icon_Stage_Capture_01";
    public const string StageHeroine = "Icon_Stage_Heroine_01";

    // 배경
    public const string BgMainMenu = "MainMenu_BG_01";
    public const string BgDeckBuilder = "DeckBuilder_BG_01";
    public const string BgEnhance = "Enhance_BG_01";
    public const string BgStageSelect = "StageSelect_BG_01";
    public const string BgStory = "Prologue_Story_BG_01";
    public const string BgBattle = "Region01_Battle_BG_01";
    public const string BgGrimoire = "Grimoire_BG_01";
    public const string BgShop = "Shop_BG_01";
    public const string BgSaveLoad = "SaveLoad_BG_01";

    // 재화와 소모성 아이템
    public const string StatGold = "Icon_Stat_Gold_01";
    public const string StatEssence = "Icon_Stat_Essence_01";
    public const string StatShard = "Icon_Stat_Shard_01";
    public const string ItemPotion = "Icon_Item_Potion_01";           // 하급 회복 물약
    public const string ItemManaCrystal = "Icon_Item_ManaCrystal_01"; // 마나 결정
    public const string ItemSeal = "Icon_Item_Seal_01";               // 보호의 인장
    public const string ItemQuill = "Icon_Item_Quill_01";             // 기억의 깃펜
    public const string ItemIncense = "Icon_Item_Incense_01";         // 정화의 향
    public const string ItemEssenceBundle = "Icon_Item_EssenceBundle_01"; // 정수 묶음
    public const string ItemEmpty = "Icon_Item_Empty_01";             // 빈 아이템 칸
    public const string ItemBag = "Icon_Item_Bag_01";                 // 소모성 아이템 탭

    // 저장과 불러오기
    public const string SaveWrite = "Icon_Save_Write_01";       // 저장
    public const string SaveLoad = "Icon_Save_Load_01";         // 불러오기
    public const string SaveContinue = "Icon_Save_Continue_01"; // 이어하기
    public const string SaveNewGame = "Icon_Save_NewGame_01";   // 새 게임
    public const string SaveAuto = "Icon_Save_Auto_01";         // 자동 저장
    public const string SaveDelete = "Icon_Save_Delete_01";     // 저장 삭제
    public const string SaveWarning = "Icon_Save_Warning_01";   // 불러올 수 없는 저장
    public const string SaveTime = "Icon_Save_Time_01";         // 플레이 시간
    public const string SaveEmpty = "Icon_Save_Empty_01";       // 빈 저장 칸

    // 월드맵 (지역 문양의 이름은 지역 데이터에 적는다)
    public const string IconWorldMap = "Icon_Menu_Stage_01";    // 월드맵 메뉴는 지역 아이콘을 함께 쓴다.
    public const string MapMarkSelect = "UI_MapMark_Select_01"; // 고른 지역의 고리
    public const string MapMarkClear = "UI_MapMark_Clear_01";   // 클리어한 지역
    public const string MapMarkNew = "UI_MapMark_New_01";       // 새로 열린 지역의 배지
    public const string MapMarkHere = "UI_MapMark_Here_01";     // 마지막으로 들어간 지역의 깃발

    // 스테이지: 난이도와 보상 표시
    public const string DifficultyEasy = "Icon_Difficulty_Easy_01";     // 쉬움
    public const string DifficultyNormal = "Icon_Difficulty_Normal_01"; // 보통
    public const string DifficultyHard = "Icon_Difficulty_Hard_01";     // 어려움
    public const string RewardChestClosed = "Icon_Reward_ChestClosed_01"; // 최초 보상을 아직 받지 않음
    public const string RewardChestOpen = "Icon_Reward_ChestOpen_01";     // 최초 보상을 이미 받음
    public const string RewardRepeat = "Icon_Reward_Repeat_01";           // 반복 보상
    public const string StageCrown = "Icon_Stage_Crown_01";               // 이긴 스테이지
    public const string ArrowLeft = "Icon_Arrow_Left_01";                 // 앞으로 넘기기
    public const string ArrowRight = "Icon_Arrow_Right_01";               // 다음으로 넘기기
    public const string ArrowSkip = "Icon_Arrow_Skip_01";                 // 전투 속도
    public const string CaptureOrb = "Icon_Capture_Orb_01";               // 포획 콘텐츠
    public const string CaptureReroll = "Icon_Capture_Reroll_01";         // 포획 목록 다시 뽑기

    // 그리모어 강화: 분기 문양과 노드 받침
    public const string GrimoireContract = "Icon_Grimoire_Contract_01";
    public const string GrimoireSummon = "Icon_Grimoire_Summon_01";
    public const string GrimoireCommand = "Icon_Grimoire_Command_01";
    public const string GrimoireMana = "Icon_Grimoire_Mana_01";
    public const string GrimoireMemory = "Icon_Grimoire_Memory_01";
    public const string GrimoireDesire = "Icon_Grimoire_Desire_01";
    public const string GrimoireLineage = "Icon_Grimoire_Lineage_01";
    public const string GrimoireCapture = "Icon_Grimoire_Capture_01";
    public const string NodeLocked = "UI_Node_Locked_01"; // 잠긴 노드
    public const string NodeOpen = "UI_Node_Open_01";     // 강화할 수 있는 노드
    public const string NodeDone = "UI_Node_Done_01";     // 최대 단계 노드
}

// 글자 사이에 넣는 아이콘 이름 모음. Tools/UIThemeGenerator/theme_spec.txt의 이름과 같다.
public static class UIIcons
{
    public const string Hp = "hp";
    public const string Attack = "atk";
    public const string Defense = "def";
    public const string Shield = "shd";
    public const string Lust = "lust";
    public const string Mana = "mana";
    public const string Gold = "gold";
    public const string Essence = "ess";
    public const string Exp = "exp";
    public const string Shard = "shard"; // 욕망의 파편

    public const string Skill = "skill";
    public const string Cooldown = "cd";
    public const string Taunt = "taunt";

    public const string StateWait = "st_wait";
    public const string StateReady = "st_ready";
    public const string StateDone = "st_done";

    public const string Draw = "draw";
    public const string Discard = "discard";
    public const string Lock = "lock";
    public const string Search = "search";
    public const string Close = "close";
    public const string Next = "next";
    public const string Skip = "skip";
    public const string Log = "m_log";
    public const string Book = "m_book";
    public const string Deck = "m_deck";
    public const string Enhance = "m_enhance";
    public const string Stage = "m_stage";
    public const string Quit = "m_quit";
}
