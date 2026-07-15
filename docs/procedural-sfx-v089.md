# v0.89 절차형 SFX 폴백

## 목표

외부 오디오 에셋 없이도 전투와 주요 UI 행동에 최소한의 청각 피드백을 제공한다. 최종 WAV 파일이 준비되면 기존 `SfxKey` 매핑에 연결해 절차형 폴백을 자동으로 대체한다.

## 런타임 구조

- Unity 내장 `com.unity.modules.audio`만 사용한다.
- `AudioManager`는 첫 씬 로드 전에 전역 인스턴스로 생성되고 씬 전환 후에도 유지된다.
- 직렬화된 실제 `AudioClip`이 있으면 해당 클립을 우선 사용한다.
- 실제 클립이 없는 `SfxKey`는 `ProceduralSfxFactory`가 22,050Hz 모노 클립을 런타임에 생성한다.
- 생성 실패 또는 키 누락 시 게임 흐름은 중단하지 않는다.

## 지원 키

| SfxKey | 절차형 피드백 |
|---|---|
| `ButtonClick` | 짧은 UI 클릭음 |
| `HeroAttack` | 공격 스윙음 |
| `MonsterHit` | 노이즈가 섞인 피격음 |
| `MonsterDeath` | 하강하는 사망음 |
| `CrystalHit` | 낮은 충격음 |
| `RuneSelect` | 상승하는 선택음 |
| `Victory` | 상승 아르페지오 |
| `Defeat` | 하강 음형 |
| `UpgradePurchase` | 짧은 구매 아르페지오 |

## 사용자 설정

- TitleScene 설정에서 `SFX: 켜짐/꺼짐`을 전환한다.
- 설정 키는 `RuneGate.SfxEnabled`이며 `PlayerPrefs`에 즉시 저장한다.
- 앱을 종료하고 다시 실행해도 설정이 유지된다.
- BGM과 SFX는 각각 켜기/끄기와 25/50/75/100% 음량 단계를 제공한다.
- BGM 설정은 `RuneGate.BgmEnabled`, `RuneGate.BgmVolume`, SFX 음량은 `RuneGate.SfxVolume`에 즉시 저장한다.
- 장면별 BGM 구조는 `docs/scene-bgm-audio-settings-v090.md`를 참고한다.

## 검증 결과

- Unity 프로젝트 Validator: 통과
- Progression Smoke Test: 통과
- Android APK 빌드: 통과
- Android 15 / API 35 / 1080x2400 시스템 플로우: 통과
- 런타임 로그: 절차형 클립 9개 생성 확인
- 수동 확인: SFX 끄기 후 앱 재실행 시 꺼짐 상태 유지
- APK: `C:\workspace\defense-game-issue44-artifacts\RuneGateDefense-issue44-procedural-sfx.apk`
- APK 크기: `71,973,099 bytes`
- SHA-256: `DFABE6ACD94709BCA2C3F8CDABCDBD8E2F6A0101231CF0A05923422C9E3D6D3A`

## 남은 작업

- 각 키에 맞는 최종 WAV 파일 제작과 음량 정규화
- 실기기 스피커와 이어폰으로 체감 음량 확인
- 최종 Battle/Menu 작곡 음원과 Result 징글 제작
- 실제 음원 기준 BGM/SFX 믹싱과 덕킹 조정
