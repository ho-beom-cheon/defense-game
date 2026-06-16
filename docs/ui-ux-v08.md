# RuneGate Defense v0.8 UI/UX Polish

v0.8은 새 기능을 크게 늘리기보다 첫 플레이 흐름을 읽기 쉽게 만드는 단계다. 현재 UI는 IMGUI 기반 프로토타입을 유지하되, RuntimePixel UI 이미지를 일부 사용한다.

## 흐름

1. TitleScene에서 시작 또는 이어하기를 선택한다.
2. StageSelectScene에서 스테이지 placeholder와 Stage 1~10 해금 상태를 확인한다.
3. Stage를 선택하면 BattleScene으로 진입한다.
4. 첫 전투에서는 Tutorial Overlay가 표시된다.
5. Victory/Defeat 결과를 확인한다.
6. UpgradeScene에서 Gold로 업그레이드를 구매한다.
7. StageSelectScene으로 돌아가 다음 Stage를 선택한다.

## 적용한 UI 에셋

- Tutorial: `ui_tutorial_arrow.png`, `ui_tap_indicator.png`
- StageSelect: `ui_stage_node_unlocked.png`, `ui_stage_node_locked.png`, `ui_stage_node_cleared.png`
- Upgrade: `icon_upgrade_crystal_hp.png`, `icon_upgrade_hero_attack.png`, `icon_upgrade_attack_speed.png`, `icon_upgrade_skill_cooldown.png`
- System: `ui_icon_settings.png`, `ui_icon_reset_save.png`, `ui_icon_save.png`
- Difficulty: `badge_easy.png`, `badge_normal.png`, `badge_hard.png`, `badge_nightmare.png`

## 현재 구현

- StageSelect는 Stage 1~10, Gold, Formation Slot, 잠금/해금/클리어 상태를 표시한다.
- Easy/Normal 스테이지는 선택 가능하고 Hard/Nightmare는 잠금 placeholder로 표시한다.
- UpgradeScene은 현재 레벨, 다음 비용, 구매 가능/불가 상태, 업그레이드별 아이콘을 표시한다.
- Result UI는 Victory/Defeat, Gold, Stage clear, 다음 Stage 해금, Retry/Upgrade/Stage Select를 표시한다.
- Settings에는 튜토리얼 다시 보기와 Reset Save 확인 흐름이 있다.
- IMGUI 기반 한글 UI는 `KoreanFontManager`를 통해 NotoSansKR-Regular를 적용한다.
- v0.8 Korean Text Data Repair에서 ScriptableObject에 저장된 `??` 표시 문자열을 실제 한글 데이터로 복구했다.

## 한글 표시 데이터 기준

- StageSelect 표시명은 StageData의 한글 표시명을 사용한다.
- Battle HUD는 스테이지명, 크리스탈 HP, 웨이브, 상태, 골드를 한글 라벨로 표시한다.
- 스킬 버튼은 내부 `skillId`를 유지하고 표시명만 한글 스킬명으로 보여준다.
- Rune Selection은 RuneData의 한글 이름과 설명을 사용한다.
- 내부 ID, stageId, heroId, effectKey는 영문 key를 유지한다.
- Result UI는 내부 `stageId`나 영어 결과 메시지를 직접 표시하지 않고 한국어 결과 문구와 StageData 표시명을 사용한다.
- BattleState는 `Preparing`, `WaveRunning`, `RuneSelection`, `Victory`, `Defeat` 대신 준비 중, 전투 중, 룬 선택 중, 승리, 패배로 표시한다.
- Rune Selection의 `effectKey`는 기본 UI에서 숨기고 디버그 옵션에서만 표시한다.

## 남은 UI 과제

- IMGUI를 Canvas/uGUI 또는 UI Toolkit 기반 모바일 UI로 교체해야 한다.
- Safe area와 1080x1920 기준 레이아웃은 실제 기기 테스트 후 추가 조정한다.
- Tutorial arrow와 tap indicator는 아직 정적 overlay이며, 타겟 추적형 가이드는 다음 단계 과제다.
