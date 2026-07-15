# Android 전투 카메라 프레이밍 QA v0.9

## 목적

Android 세로 화면에서 BattleScene의 카메라 viewport 전체가 전투 배경으로 채워지고, Scene 전환 전 UI가 카메라 바깥 영역에 잔상으로 남지 않도록 정리한다.

## 수정 내용

- `LaneManager`가 전투 카메라의 실제 world bounds를 기준으로 전체 viewport용 어두운 숲 backdrop을 생성한다.
- 기존 `bg_goblin_forest_lanes.png`, 3개 라인, 유닛 좌표와 전투 safe bounds는 그대로 유지한다.
- `BattleHUD`가 카메라 viewport 바깥 영역을 매 IMGUI 프레임마다 불투명한 배경색으로 지운다.
- StageSelect에서 BattleScene으로 전환할 때 이전 `전투 시작` 버튼이 하단에 잔상으로 남지 않는다.
- ConceptSheets와 RuntimePixel 분리 정책, 스폰 위치, 전투 로직은 변경하지 않았다.

## Android 실제 터치 검증

- 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080x2400 Portrait
- 실행 경로: 앱 일반 실행 > 이어하기 > StageSelect > 전투 시작 > BattleScene
- 확인 결과:
  - TitleScene과 StageSelectScene 정상 표시
  - BattleScene 배경이 battlefield viewport 전체를 채움
  - 3개 라인, 영웅 6인, 몬스터, HP Bar 정상 표시
  - StageSelect UI 잔상 없음
  - 룬 선택 버튼 터치 후 다음 웨이브 진행
  - 핵심 한글 및 내부 ID 노출 이상 없음

## 회귀 검증

- Unity Validator를 포함한 Android APK 빌드 성공
- `-runegateSmokeFullChapter` 통과
- Normal Stage 1~10 승리
- 업그레이드 10회 구매
- Stage 10 그룸바르 3단계와 증원/공격 패턴 확인
- 치명적 예외, `NullReferenceException`, `MissingReferenceException` 없음

## 빌드 증거

- APK: `C:\workspace\defense-game-issue63-artifacts\RuneGateDefense-battle-camera-final.apk`
- 파일 크기: `72,039,587 bytes`
- SHA-256: `5B7F9AF05623335665364D1E61A72642F63CD7A1DD96C2CF191152340B227866`
- Unity 로그: `C:\workspace\defense-game-issue63-artifacts\unity-android-build-final.log`
- Android 회귀 로그: `C:\workspace\defense-game-issue63-artifacts\android-full-chapter.log`
- 실제 화면 캡처: `C:\workspace\defense-game-issue63-artifacts\issue63-final-battle.png`

## 남은 한계

- 실제 Android 기기의 display cutout과 제조사별 화면비는 아직 확인하지 않았다.
- RuntimePixel 배경과 유닛은 프로토타입 아트이며 최종 품질이 아니다.
- IMGUI 기반 화면은 장기적으로 Canvas/RectTransform 기반 UI로 교체할 필요가 있다.
- 세로 battlefield의 위아래 여백은 배경으로 채웠지만, 최종 아트 단계에서 환경 오브젝트와 조명 연출을 추가할 수 있다.
