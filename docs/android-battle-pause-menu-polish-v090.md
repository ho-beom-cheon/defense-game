# Android Battle Pause Menu Polish v0.9.0

## 목적

전투 중 일시정지 화면을 단순 이동 버튼 모음에서 실제 게임 메뉴로 정리한다. 플레이어가 현재 전투 상태를 확인하고 음향을 조절한 뒤 안전하게 전투를 계속하거나 이탈할 수 있어야 한다.

## 구현 내용

- Safe Area 중앙에 어두운 dim overlay와 게임형 일시정지 패널을 표시한다.
- 현재 전선, 웨이브, 크리스탈 HP, 전투 골드를 2x2 요약 영역에 표시한다.
- BGM/SFX 켜기·끄기와 단계별 음량 조절을 제공한다.
- 음향 변경은 기존 `AudioManager`와 `PlayerPrefs`를 통해 즉시 저장하고 현재 AudioSource에 반영한다.
- `계속하기`를 단일 주 동작으로 강조한다.
- `전투 재시작`과 `스테이지 선택`은 즉시 실행하지 않고 별도 확인 모달을 거친다.
- 앱 백그라운드 전환으로 멈춘 경우 일반 수동 일시정지와 다른 안내 문구를 표시한다.

## 안전한 이동 규칙

- 전투 재시작 확인은 현재 전투 골드와 룬 효과가 초기화됨을 알린다.
- 스테이지 이탈 확인은 현재 전투 진행이 저장되지 않지만 기존 해금과 업그레이드는 유지됨을 알린다.
- 확인 모달이 열린 동안 뒤의 음향 및 이동 버튼은 입력되지 않는다.
- 취소하면 같은 일시정지 화면으로 돌아오며 전투 시간은 계속 멈춰 있다.
- 계속하기는 `BattlePauseController`가 보관한 이전 `Time.timeScale`을 복원한다.

## 반응형 레이아웃

`BattleHUD.CalculatePauseMenuLayoutForSize`가 패널, 요약, 음향, 피드백, 동작 버튼, 확인 모달의 Rect를 계산한다.

검증 화면 크기:

- 720x1280 Portrait
- 1080x1920 Portrait
- 1440x2560 Portrait
- 1600x900 Landscape fallback
- 2048x1152 Landscape fallback

`RuneGateProgressionSmokeTest`는 모든 영역이 Safe Area와 패널 내부에 있는지, 섹션과 버튼이 겹치지 않는지, 주 동작과 확인 버튼이 최소 터치 높이를 만족하는지 검사한다.

## 검증 결과

- Unity 6 컴파일: 통과
- Unity Project Validator: 통과
- Unity Progression Smoke Test: 통과
- Android System Flows E2E: 통과
  - 튜토리얼 완료 및 저장
  - 전투 일시정지와 생명주기 일시정지
  - Retry, Upgrade, Stage Select 이동
  - BGM/SFX 독립 저장
- Android Full Chapter E2E: 통과
  - Normal Stage 1~10
  - 업그레이드 10회
  - Stage 10 그룸바르 3페이즈와 전용 패턴
- Android 15 / API 35 에뮬레이터 1080x2400 수동 확인:
  - 전투 상태 요약과 버튼 배치
  - BGM 음량 변경
  - SFX 끄기와 즉시 피드백
  - 재시작 확인 및 취소
  - 스테이지 선택 확인 및 취소
  - 계속하기 후 전투 재개

## Android 빌드

- APK: `RuneGateDefense-pause-menu-polish.apk`
- 크기: `72,113,553 bytes`
- SHA-256: `AF084D719DF5B46CA923DC433DC063CB89EACFFBB9E18CBA5FD3133673E2B471`
- APK와 수동 캡처는 저장소 밖 검증 산출물 경로에 생성했다.

## 남은 항목

- 실제 Android 기기의 전화 수신, 화면 잠금, 멀티윈도우 복귀 검증
- 노치와 디스플레이 컷아웃이 있는 실기기 Safe Area 확인
- 최종 버튼 아이콘과 음향 슬라이더 아트
- 접근성 글자 크기 확대와 화면 읽기 기능 검증
