# Battle uGUI QA v1

## 범위

- 이슈: #99 `Stage 1 전투 UI를 봉문 전술 기록서 스타일의 uGUI로 전환`
- 브랜치: `codex/issue-99-battle-ugui-refresh`
- 대상: Stage 1 기본 전투, 튜토리얼 1/4/7단계, 일시정지, 룬 선택, 승리·패배 결과
- 기기 기준: Android 15 / API 35 에뮬레이터, 1080×2400 Portrait

## 자동 검증

- C# Player 컴파일 및 Android APK 빌드: 통과
- Project Validator: 통과
- Progression Smoke Test: 통과
- Game Frame Validator: `PASS 86`, `WARNING 0`, `FAIL 0`
- Android System Flows E2E: `RUNEGATE_SYSTEM_FLOWS_E2E_PASSED`
- Android Full Chapter E2E: `RUNEGATE_FULL_CHAPTER_E2E_PASSED: upgrades=10, gold=1650, difficulties=normal|hard|nightmare`
- `git diff --check`: 통과

## 화면 캡처

캡처는 커밋 대상이 아닌 로컬 QA 산출물로 `.utmp/issue99-captures/` 아래에 보관한다.

| 해상도 | 환경 | 경로 | 결과 |
| --- | --- | --- | --- |
| 720×1280 | Windows Player | `720x1280-v3/` | 통과 |
| 1080×1920 | Android 15 | `android-1080x1920/issue99-captures-1080x1920/` | 통과 |
| 1080×2400 | Android 15 | `android-1080x2400/issue99-captures-v2/` | 통과 |
| 1600×900 | Windows Player fallback | `1600x900/` | 통과 |

각 세트에서 기본 전투, 튜토리얼 1/4/7단계, 일시정지, 룬 선택, 승리·패배 결과 화면을 확인했다. 세이프 에어리어, 텍스트 잘림, 프레임 이탈, 모달 배경 입력 차단, 스킬 카드 6개 노출, 수정체·균열·영웅 실루엣 구분을 사람이 검토했다.

## Android 빌드

- APK: `.utmp/issue99-android/RuneGateDefense.apk`
- 크기: `73,835,367 bytes`
- SHA-256: `489CF5D28A859A7CF89D68FB75A6C3629F1794423AF907CC89944CFCF3AFC9EB`

## 남은 한계

- Android 에뮬레이터에서 검증했으며 제조사별 노치·펀치홀과 실제 기기 글꼴 배율은 별도 확인이 필요하다.
- 신규 최종 일러스트와 캐릭터 모션시트는 범위에서 제외했고 기존 RuntimePixel 자산만 재구성했다.
- 타이틀, 스테이지 선택, 펫 계약, 강화 화면의 uGUI 전면 전환은 후속 마일스톤이다.
