# v0.9 Difficulty Progression

## 구현 범위

- 새 세이브에서 Easy와 Normal만 선택 가능
- Normal Chapter 1 완료 시 Hard 해금
- Hard Chapter 1 완료 시 Nightmare 해금
- Hard/Nightmare는 난이도별 Stage 1~10 순차 진행
- 잠긴 난이도 선택 차단
- Result의 새 난이도 해금 안내
- StageSelect의 선택 난이도, 보상 배율, 다음 해금 조건 표시
- Save schema v5 마이그레이션

## 런타임 흐름

1. `StageSelectUI`가 현재 세이브에서 선택 가능한 다음 난이도를 구한다.
2. `GameSession.SelectDifficulty`는 `SaveManager`를 통해 잠금 상태를 다시 검증한다.
3. 승리 결과는 골드, 일반 스테이지 진행, 난이도별 스테이지 키를 한 번만 저장한다.
4. `재문 숲 10` 승리 시 해당 난이도 완료를 기록한다.
5. 완료 기록에 따라 다음 난이도가 즉시 열리고 Result에 한국어 안내를 표시한다.

## 자동 검증 결과

- Branch: `codex/issue-58-difficulty-unlock-rewards`
- Unity 6.4.11f1 compile: PASS
- `Tools/RuneGate/Validate Project`: PASS
- `Tools/RuneGate/Run Progression Smoke Test`: PASS
- Android full chapter smoke: PASS
- Device: Android 15 / API 35 emulator
- APK: `RuneGateDefense-issue58-difficulty-final.apk`
- Size: `72,037,083 bytes`
- SHA-256: `58A8CDA1FE642635DAEE4766B6C5B2446CCF3A945F8C9B508807712D74E903EF`

Android smoke는 새 세이브에서 Hard 선택이 거부되는지, Normal Stage 1~10 승리 후 Hard가 열리는지, Hard Stage 1~10 순차 결과 후 Nightmare가 열리는지, 디스크 재로드 뒤 완료 상태가 유지되는지 확인했다.

## 남은 수동 QA

- 실기기에서 난이도 버튼 연속 탭과 텍스트 가독성 확인
- Hard/Nightmare 장시간 밸런스와 최종 보상량 조정
- Nightmare Stage 1~10 실제 전투 완주 테스트
