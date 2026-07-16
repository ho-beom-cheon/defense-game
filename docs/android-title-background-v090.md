# Android 타이틀 배경 QA v0.9.0

## 작업 범위

- 기존 `Assets/_Project/Art/RuntimePixel/App/splash_background_1080x1920.png`를 런타임 비주얼 카탈로그에 연결했다.
- `TitleScene`이 해당 이미지를 전체 화면 배경으로 사용하고, 세로 모바일 화면에서는 메뉴 프레임을 아래로 이동해 중앙 크리스탈을 먼저 보여 주도록 조정했다.
- 설정이나 새 게임 확인 영역을 펼쳤을 때는 더 작은 오프셋을 사용하고 Safe Area 아래 경계를 넘지 않도록 제한했다.
- 배경을 찾지 못하면 기존 단색 타이틀 화면을 유지한다.

## 검증 환경

- 일자: 2026-07-16
- 브랜치: `codex/issue-67-android-title-background`
- 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080x2400 Portrait, density 420
- 패키지: `com.hobeomcheon.runegatedefense`

## 수동 확인 결과

- 앱을 일반 실행했을 때 크리스탈 배경과 타이틀 메뉴가 함께 표시됐다.
- 새 게임 확인 영역이 Safe Area 안에 표시되고 확인/취소 버튼이 잘리지 않았다.
- `이어하기` 터치 후 `StageSelectScene`으로 정상 전환됐다.
- 한글 깨짐, 내부 ID 노출, 치명적 런타임 오류는 확인되지 않았다.

## 자동 회귀 결과

- Unity Project Validator를 포함한 Android APK 빌드가 통과했다.
- Normal Stage 1~10 실제 전투가 모두 Victory로 종료됐다.
- 업그레이드 10회, 그룸바르 3페이즈와 전용 공격 패턴, 진행 데이터 저장을 확인했다.
- 최종 마커: `RUNEGATE_FULL_CHAPTER_E2E_PASSED`

## 빌드 산출물

- APK: `C:\workspace\defense-game-issue67-artifacts\RuneGateDefense-title-background.apk`
- 파일 크기: `72,059,881 bytes`
- SHA-256: `B2EAD6111A76EC0CCC4B862D6A3CBD82A1257F2C9F233C84DB396A026920FA36`
- 빌드 로그: `C:\workspace\defense-game-issue67-artifacts\unity-android-build.log`
- 전체 챕터 로그: `C:\workspace\defense-game-issue67-artifacts\android-full-chapter.log`
- 타이틀 캡처: `C:\workspace\defense-game-issue67-artifacts\issue67-title.png`
- 확인창 캡처: `C:\workspace\defense-game-issue67-artifacts\issue67-title-confirm.png`
- 스테이지 선택 캡처: `C:\workspace\defense-game-issue67-artifacts\issue67-stage-select.png`

## 남은 확인

- 현재 스플래시 배경은 첫 공개 테스트용 후보 자산이며 최종 타이틀 아트가 아니다.
- 실제 Android 기기의 노치, 펀치홀, 다양한 화면비에서 추가 Safe Area 확인이 필요하다.
- 새 게임 확인 상태의 빈 공간과 메뉴 세부 타이포그래피는 Canvas UI 전환 단계에서 다시 다듬는다.
