# Android 그림자 계약 화면 QA

## 환경

- 날짜: 2026-07-16
- Unity: 6000.4.11f1
- 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080x2400 Portrait
- Package: `com.hobeomcheon.runegatedefense`

## 빌드 산출물

- APK: `C:\workspace\defense-game-issue93-artifacts\RuneGateDefense-pet-contract.apk`
- 크기: 72,149,659 bytes
- SHA-256: `98C536F63D3F4B7D63E3257C01832B164A02A1E92183E6DE614145AE8DAD7CDB`
- 빌드 마커: `RUNEGATE_ANDROID_BUILD_PASSED`

## 검증 결과

- Unity Project Validator: PASS
- Progression Smoke: PASS
- Android UI Capture: PASS, 7개 핵심 화면 생성
- 계약 팝업: 7종 카드, 한국어 이름, RuntimePixel 초상, 패시브, 조각 수와 행동 버튼 표시
- 조각 소비: 문틈 도깨비 `5 -> 0`
- 계약 저장: 계약 목록과 최초 자동 장착 저장
- 장착 흐름: 해제 후 재장착 성공
- JSON 재로드: 조각, 계약, 장착 상태 유지
- Android System Flow: `RUNEGATE_SYSTEM_FLOWS_E2E_PASSED`
- Android Full Chapter: Stage 1~10 Victory, 강화 10회, 그룸바르 3페이즈/지원군/패턴 검증
- 전체 챕터 마커: `RUNEGATE_FULL_CHAPTER_E2E_PASSED: upgrades=10, gold=1622`
- `FATAL EXCEPTION`, `NullReferenceException`, `MissingReferenceException` 없음

## 렌더 캡처

Android Player의 `RuneGateVisualCaptureRunner`가 다음 화면을 직접 캡처했다.

- `01-title-1080x2400.png`
- `02-stage-select-1080x2400.png`
- `02b-pet-contract-1080x2400.png`
- `03-battle-1080x2400.png`
- `04-tutorial-1080x2400.png`
- `05-defeat-result-1080x2400.png`
- `06-upgrade-1080x2400.png`

캡처 산출물은 Git에 포함하지 않고 로컬 `.utmp/issue93-ui-final2/`에 보관한다.

## 남은 확인

- 실제 Android 기기의 컷아웃과 시스템 글꼴 확대
- 빠른 연속 계약/장착 탭
- 장시간 스크롤 관성과 저사양 GPU 표시
- 패시브 수치의 실사용 밸런스
