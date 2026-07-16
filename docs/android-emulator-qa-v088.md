# Android Emulator QA v0.88

## 검증 환경

- 날짜: 2026-07-12
- 브랜치: `codex/issue-36-android-emulator-qa`
- Unity: `6000.4.11f1`
- AVD: `RuneGate_API35_Portrait`
- 기기 이미지: Android 15 / API 35 / Google APIs x86_64
- 화면: 1080 x 2400 Portrait, density 420
- 패키지: `com.hobeomcheon.runegatedefense`
- 버전: `0.9.0` (`versionCode 9`)

## 최종 APK

- 출력: `C:\workspace\defense-game-issue36-artifacts\RuneGateDefense-emulator-release-final.apk`
- 크기: `71,892,795 bytes`
- SHA-256: `040BBDF2F3F5A7F8BAEF537CD5177860BA286DCF734E30F00AD2AD6C7C5E96B3`
- 설치: `adb install -r` 성공
- 서명: Android Debug 인증서

APK와 캡처 파일은 검증 산출물이므로 Git에 포함하지 않는다.

## 실제 확인 결과

- [x] 앱 설치 및 `UnityPlayerGameActivity` 실행
- [x] Portrait 방향 유지
- [x] Title 화면 버튼 터치
- [x] StageSelect 진입 및 Stage 1 선택
- [x] StageSelect 목록, 상세 정보, 난이도 버튼이 화면 안에 표시
- [x] BattleScene 진입
- [x] 튜토리얼 표시 및 건너뛰기 터치
- [x] 영웅 6인과 몬스터가 3개 라인 안에 표시
- [x] 몬스터가 오른쪽 경계에서 잘리지 않음
- [x] HP Bar와 전투 상태 표시
- [x] 룬 선택 카드 3장과 선택 버튼이 화면 안에 표시
- [x] Stage 1 Victory 및 Gold 110 지급
- [x] Stage 2 해금 표시
- [x] Result의 다음 스테이지, 업그레이드, 스테이지 선택 버튼 표시
- [x] 핵심 화면에서 `??`, 내부 id, `hook`, `placeholder` 문구 미노출
- [x] 앱 프로세스 유지 및 `FATAL EXCEPTION`, `NullReferenceException` 없음
- [x] 음원 미할당 상태의 잘못된 `AudioListener` 생성 경고 제거

## 이번 QA에서 수정한 항목

- 모바일 DPI와 해상도를 반영한 IMGUI 글자 및 터치 높이
- Title 대형 세로 화면 프레임
- StageSelect 헤더, 목록 행, 상세 설명, 하단 버튼 높이
- Battle 전용 카메라 viewport와 world width 자동 맞춤
- 모바일 Battle HUD에서 잘리던 개발용 영웅 수치 줄 제거
- Rune Selection과 Result 팝업의 모바일 크기 및 버튼 예약 공간
- 사용자에게 노출되던 룬 `hook` 문구 제거 및 6개 룬 런타임 효과 연결

## 재현 절차

```powershell
$adb = "C:\Users\cjs41\AppData\Local\Android\SdkRuneGate\platform-tools\adb.exe"
$apk = "C:\workspace\defense-game-issue36-artifacts\RuneGateDefense-emulator-release-final.apk"

& $adb wait-for-device
& $adb install -r $apk
& $adb shell am start -n com.hobeomcheon.runegatedefense/com.unity3d.player.UnityPlayerGameActivity
```

앱에서 `이어하기` 또는 `새 게임`을 선택하고 `재문 숲 1 > 전투 시작`으로 진입한다.

## 남은 검증

- 실제 Android 휴대폰 설치 및 노치/펀치홀 Safe Area 확인
- 저사양 ARM 기기의 프레임, 발열, 메모리 확인
- 30분 이상 장시간 세션
- 릴리스 키스토어 서명 APK/AAB
- Google Play 내부 테스트 트랙 업로드
