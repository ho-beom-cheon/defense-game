# v0.88 UI Screenshot QA

## 목적

Windows Player에서 주요 화면을 자동 순회하고 PNG로 저장해, 정적 프레임 계산만으로 확인할 수 없는 실제 렌더링과 한글 표시를 점검한다.

## 실행 방식

- 실행 인자: `-runegateCaptureUi`
- 출력 경로: `-runegateCapturePath <path>`
- 격리 저장 경로: `-runegateSavePath <path>`
- 캡처 화면: Title, StageSelect, Battle, Tutorial, Defeat Result, Upgrade
- 성공 로그: `RUNEGATE_UI_CAPTURE_PASSED`

`RuneGateVisualCaptureRunner`는 테스트 저장을 실제 사용자 저장과 분리한다. PNG는 `RuneGatePngEncoder`가 Unity 모듈 구성에 의존하지 않고 RGB 데이터를 직접 인코딩한다.

## 2026-07-12 결과

| 요청 해상도 | 실제 캡처 해상도 | 결과 |
|---|---:|---|
| 720 x 1280 | 720 x 1280 | 6개 화면 생성, 통과 |
| 1080 x 1920 | 1080 x 1440 | 6개 화면 생성, 통과 |
| 1440 x 2560 | 1440 x 1440 | 6개 화면 생성, 통과 |

Windows 데스크톱의 최대 창 높이 제한 때문에 고해상도 두 실행은 높이가 1440으로 조정되었다. 목표 세로 비율의 정적 레이아웃은 `UIFrameValidator`의 1080 x 1920 및 1440 x 2560 케이스로 별도 통과했다.

## 확인 내용

- Battle HUD와 영웅 스킬 6개가 화면 안에 표시된다.
- 영웅 스킬은 3열 2행으로 배치되어 우측 잘림이 없다.
- 영웅 전투 수치는 가로 스크롤 없이 3열 2행으로 표시된다.
- StageSelect 목록과 상세 패널이 겹치지 않는다.
- Tutorial 및 Defeat Result 팝업이 중앙에 표시되고 버튼이 화면 안에 있다.
- Upgrade 이름, 설명, 비용이 한글로 표시된다.
- 핵심 캡처에서 `??`, 내부 stage id, rune effect key가 노출되지 않는다.

## 남은 수동 QA

- Android 실기기 1080 x 1920 계열 Safe Area와 터치 영역
- 글꼴 크기와 버튼 최소 터치 크기의 실제 기기 가독성
- Rune Selection의 실제 웨이브 종료 시점 캡처
- Victory Result와 장시간 전투 중 동적 텍스트 겹침
