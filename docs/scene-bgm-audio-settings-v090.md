# v0.90 장면별 BGM과 오디오 설정

## 목표

외부 음원이나 패키지를 추가하지 않고 Title, StageSelect, Upgrade, Battle 흐름에 실제 반복 재생되는 배경 음악을 제공한다. 최종 작곡 음원이 준비되면 같은 `BgmTheme` 매핑에 연결해 절차형 폴백을 교체할 수 있다.

## 런타임 구조

- `AudioManager`는 SFX용 AudioSource와 음악 교차 페이드용 AudioSource 두 개를 분리해 관리한다.
- `TitleScene`, `StageSelectScene`, `UpgradeScene`은 `BgmTheme.Menu`를 사용한다.
- `BattleScene`은 `BgmTheme.Battle`을 사용한다.
- 실제 `AudioClip`이 연결되지 않은 테마는 `ProceduralBgmFactory`가 22,050Hz 모노 루프를 생성한다.
- 장면 전환 시 0.35초 동안 이전 테마와 다음 테마를 교차 페이드한다.
- 같은 메뉴 테마를 사용하는 장면 사이에서는 트랙을 다시 시작하지 않는다.
- BGM이 꺼져 있어도 현재 장면 테마는 기억하며 다시 켜면 즉시 올바른 테마를 재생한다.

## 사용자 설정

TitleScene의 설정 패널에서 다음 항목을 독립적으로 변경한다.

- `BGM: 켜짐/꺼짐`
- `BGM 음량: 25/50/75/100%`
- `SFX: 켜짐/꺼짐`
- `SFX 음량: 25/50/75/100%`

설정은 `PlayerPrefs`의 `RuneGate.BgmEnabled`, `RuneGate.BgmVolume`, `RuneGate.SfxEnabled`, `RuneGate.SfxVolume`에 즉시 저장한다. 진행 데이터 초기화와 오디오 환경설정은 서로 분리한다.

## 검증 결과

- Unity 6000.4.11f1 Project Validator: 통과
- Unity Progression Smoke: 통과
- Android APK 빌드: 통과
- Android 15 / API 35 / 1080x2400 System Flow: 통과
- 메뉴 BGM 생성 및 재생: 통과
- BattleScene 전투 BGM 전환: 통과
- UpgradeScene과 StageSelectScene 메뉴 BGM 복귀: 통과
- BGM/SFX 음소거와 음량 독립 저장: 통과
- Android writer/reader 프로세스 간 오디오 설정 복원: 통과
- Android Stage 1~10 Full Chapter 회귀: 통과
- APK: `RuneGateDefense-issue54-bgm-final.apk`
- APK 크기: `72,024,311 bytes`
- SHA-256: `CE1C491B268429B2C95BD96128B7EDF02EB77EA9E2577777A0B6462138E8AECA`

## 남은 범위

- 절차형 BGM은 기능 완성용 폴백이며 최종 작곡, 악기 구성, 마스터링을 대체하지 않는다.
- 실기기 스피커와 이어폰에서 장시간 체감 음량을 확인해야 한다.
- Victory/Defeat 징글과 BGM 덕킹의 최종 믹싱은 실제 음원 제작 단계에서 조정한다.
