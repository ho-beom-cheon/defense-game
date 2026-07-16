# Android v1.0 공개 테스트 패키지

## 후보 정보

- 작업 이슈: `#95`
- 브랜치: `codex/issue-95-v1-release-package`
- Unity: `6000.4.11f1`
- 패키지: `com.hobeomcheon.runegatedefense`
- 버전: `1.0.0` (`versionCode 10`)
- 방향: Portrait
- 최소 SDK: 25
- 대상 SDK: 36

## 산출물

산출물, 키스토어, manifest, 로그와 화면 캡처는 저장소 밖 `C:\workspace\defense-game-issue95-artifacts`에 보관한다.

| 파일 | 크기 | SHA-256 | 용도 |
|---|---:|---|---|
| `RuneGateDefense-v1.0.0.apk` | 72,149,651 bytes | `C6CB618106A7AD7F2CA9382B3334050FEB535FD9277D03D11FB21E3A2189B310` | Android 기기 QA |
| `RuneGateDefense-v1.0.0-qa-signed.aab` | 72,089,252 bytes | `7936ECFD04D726BA24CA807EFC288D7C8BD01663A31032D3970E3AB5C1D06FAB` | 서명 파이프라인 QA |

두 산출물 모두 `<artifact>.manifest.json`을 생성했다. AAB manifest의 SHA-256을 PowerShell `Get-FileHash`로 독립 계산해 일치함을 확인했고 `jarsigner -verify`가 종료 코드 0으로 통과했다. AAB는 저장소 밖 일회성 자체 서명 QA 인증서를 사용했으므로 Play Console 제출용이 아니다.

## Unity 검증

- `RuneGateProjectValidator.ValidateV10FromCommandLine`: PASS
- `RuneGateProgressionSmokeTest.RunFromCommandLine`: PASS
- v1.0 Validator가 Product Name, package, Portrait, `1.0.0 (10)`을 검사한다.

## Android 검증

- 기기: Android 15 / API 35 에뮬레이터
- 화면: 1080x2400 Portrait
- `adb install -r`: PASS
- Package Manager: `versionName=1.0.0`, `versionCode=10`
- UI Capture: Title, StageSelect, 그림자 계약, Battle, Tutorial, Defeat Result, Upgrade 7장 PASS
- System Flows: 편성 저장, 계약 조각 차감, 장착 저장, 튜토리얼, 일시정지, 저장 복구, 패배, BGM/SFX PASS
- Normal Stage 1~10: Victory 10회, 강화 10회, 그룸바르 3페이즈, 지원군 5기, 보스 패턴 PASS
- 최종 자동 회귀 골드: 1,584

첫 캡처 시 일반 `/sdcard` 경로는 Android 15 scoped storage가 거부했다. 최종 캡처는 앱 전용 외부 파일 경로 `/sdcard/Android/data/com.hobeomcheon.runegatedefense/files`를 사용했다.

## 공개 제출 전 남은 작업

- 보호된 운영 키스토어로 최종 AAB 서명
- Play App Signing과 Play Console 업로드
- 실제 Android 기기 설치, 컷아웃/Safe Area, 시스템 글꼴 확대 확인
- 30스테이지 장시간 발열, 메모리, 배터리와 터치 반복 검증
- 스토어 정책, Target SDK, Data Safety와 개인정보 문구 최종 검토
