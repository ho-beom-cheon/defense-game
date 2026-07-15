# Android All-Difficulty Campaign QA v0.9

## 목적

Normal, Hard, Nightmare의 Chapter 1을 같은 저장 데이터로 실제 연속 플레이해 상위 난이도가 수치 정의만 존재하는 상태가 아니라 끝까지 플레이 가능한지 검증한다.

## 환경

- Unity: 6000.4.11f1
- Device: Android 15 / API 35 emulator
- Orientation: Portrait
- Package: `com.hobeomcheon.runegatedefense`
- Runtime argument: `-runegateSmokeAllDifficulties`
- Isolated save: `runegate_issue60.json`
- APK: `RuneGateDefense-all-difficulties-final.apk`
- Size: `72,039,275 bytes`
- SHA-256: `CB9BAC68A06E239AD511A8CD626C762AFDC8D60189D70007D67A8A57B6B78B6C`

## 검증 결과

- Normal Stage 1~10: 10 Victory
- Hard Stage 1~10: 10 Victory
- Nightmare Stage 1~10: 10 Victory
- 총 실제 전투: 30
- 저장된 업그레이드 구매: 20회
- 최종 Gold: 7,304
- Normal 완료 후 Hard 해금: PASS
- Hard 완료 후 Nightmare 해금: PASS
- 난이도별 순차 스테이지 해금: PASS
- 세 난이도 완료 상태 디스크 재로드: PASS
- Fatal/NullReference/MissingReference: 0

## 보스 검증

각 난이도의 Stage 10에서 다음 항목이 각각 1회씩, 총 3회 통과했다.

- 그룸바르 스폰
- Phase 2 진입
- Phase 3 진입
- 지원군 5기 이상 스폰
- 한국어 보스 HUD 표시
- Phase 1/2/3 전용 공격 패턴이 실제 영웅에게 적중
- 그룸바르 처치 후 Victory

## 밸런스 판단

별도의 난이도 완화 없이 현재 배율과 정상적인 업그레이드 구매 흐름으로 Nightmare Stage 10까지 완주했다. 자동 스킬 사용과 웨이브별 룬 선택을 포함한 결과이므로 시스템 회귀와 플레이 가능성은 확인됐지만, 사람 플레이의 입력 실수와 룬 선택 편차까지 대표하지는 않는다.

## 재실행

Android에서는 Unity 실행 인자를 하나의 intent extra 문자열로 전달한다.

```powershell
$remote = "am start -S -n com.hobeomcheon.runegatedefense/com.unity3d.player.UnityPlayerGameActivity --es unity '-runegateSmokeAllDifficulties -runegateSavePath /data/user/0/com.hobeomcheon.runegatedefense/files/runegate-all-difficulties.json'"
& $adb shell $remote
```

성공 로그:

```text
RUNEGATE_ALL_DIFFICULTIES_E2E_PASSED: stages=30, upgrades=20, gold=7304
```

## 남은 수동 QA

- Android 실기기 30스테이지 장시간 발열/메모리 확인
- 수동 스킬과 다양한 룬 조합으로 Hard/Nightmare 난이도 체감 확인
- 무작위 몬스터 변종에 따른 극단 조합 반복 플레이
