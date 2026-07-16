# Android 결과·업그레이드 레이아웃 QA v0.9

## 목적

Android 세로 화면에서 전투 결과와 업그레이드 정보가 프레임 안에서 자연스럽게 읽히고, 핵심 행동 버튼을 안정적으로 터치할 수 있도록 정리한다.

## 수정 내용

### Result

- 결과 패널의 바깥 프레임과 내부 콘텐츠 영역을 분리했다.
- 모바일에서 좌우 18px, 상하 16px의 내부 여백을 적용했다.
- 모바일 세로 패널 높이를 940px에서 800px로 줄였다.
- ScrollView 높이는 내부 콘텐츠 영역을 기준으로 계산한다.
- 제목, 결과 본문, 다음 스테이지, 업그레이드, 스테이지 선택 버튼이 테두리와 겹치지 않는다.

### Upgrade

- ScrollView의 실제 폭을 기준으로 카드 폭을 계산한다.
- 아이콘을 제외한 텍스트 열이 카드의 남은 폭을 사용한다.
- 업그레이드 설명, 현재 효과, 다음 효과가 불필요하게 좁은 열에서 줄바꿈되지 않는다.
- 구매 버튼은 카드 폭을 사용해 모바일 터치 영역을 확보한다.

## Android 실제 터치 검증

- 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080x2400 Portrait
- 경로: Title > StageSelect > Stage 1 > Rune Selection > Victory > Upgrade
- 확인 결과:
  - Victory 결과 제목과 본문 padding 정상
  - 결과 행동 버튼 간격과 터치 정상
  - UpgradeScene 전환 정상
  - 업그레이드 4개 카드의 설명/효과 줄바꿈 정상
  - 핵심 한글과 내부 ID 노출 이상 없음

## 회귀 검증

- Unity Validator 포함 Android APK 빌드 성공
- `RUNEGATE_FULL_CHAPTER_E2E_PASSED`
- Normal Stage 1~10 승리
- 업그레이드 10회 구매
- 그룸바르 보스 3단계 및 패턴 확인
- 치명적 예외, null reference, missing reference 없음

## 빌드 증거

- APK: `C:\workspace\defense-game-issue65-artifacts\RuneGateDefense-result-layout.apk`
- 파일 크기: `72,039,875 bytes`
- SHA-256: `10A73484097D62AFABB2B9ED709D9AD32687026CD2EB7D0E40255248C777D598`
- Unity 로그: `C:\workspace\defense-game-issue65-artifacts\unity-android-build.log`
- Android 로그: `C:\workspace\defense-game-issue65-artifacts\android-full-chapter.log`
- Result 캡처: `C:\workspace\defense-game-issue65-artifacts\issue65-result.png`
- Upgrade 캡처: `C:\workspace\defense-game-issue65-artifacts\issue65-upgrade.png`

## 남은 한계

- Defeat 결과창은 자동 회귀 검증을 통과했지만 이번 수동 캡처는 Victory 기준이다.
- 실제 Android 기기의 컷아웃, 글꼴 배율, 제조사별 화면비는 추가 확인이 필요하다.
- Result와 Upgrade는 여전히 IMGUI 기반이며 최종 Canvas UI가 아니다.
