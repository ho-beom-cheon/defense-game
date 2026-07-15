# Difficulty Design

RuneGate Defense의 난이도는 전투 수치, 보상, Chapter 1 진행 상태를 함께 관리한다. 내부 ID는 영문으로 유지하고 사용자 화면에는 한국어 표시명을 사용한다.

## 난이도와 해금

| ID | 표시 | 해금 조건 | 보상 배율 |
| --- | --- | --- | ---: |
| `easy` | 쉬움 | 기본 해금 | x0.9 |
| `normal` | 보통 | 기본 해금 | x1.0 |
| `hard` | 어려움 | 보통 `재문 숲 10` 클리어 | x1.2 |
| `nightmare` | 악몽 | 어려움 `재문 숲 10` 클리어 | x1.45 |

- 잠긴 난이도는 `StageSelectScene`의 난이도 순환에서 건너뛴다.
- 저장 계층에서도 잠긴 난이도 직접 선택을 거부해 UI 우회를 막는다.
- 상위 난이도는 Stage 1부터 시작하며, 같은 난이도의 이전 스테이지를 클리어해야 다음 스테이지가 열린다.
- Easy와 Normal은 기존 Stage 1~10 해금 진행을 공유해 이전 세이브와 호환한다.

## 전투 보정

| 난이도 | 몬스터 HP | 이동 속도 | 크리스탈 피해 | 크리스탈 HP |
| --- | ---: | ---: | ---: | ---: |
| 쉬움 | x0.85 | x0.92 | x0.8 | x1.15 |
| 보통 | x1.0 | x1.0 | x1.0 | x1.0 |
| 어려움 | x1.18 | x1.08 | x1.25 | x0.95 |
| 악몽 | x1.36 | x1.16 | x1.5 | x0.9 |

- 망각의 뼈병 부활은 어려움과 악몽에서만 활성화한다.
- 몬스터 골드와 Stage 1~3 최소 승리 보상에는 선택 난이도의 보상 배율을 적용한다.
- Result 화면은 선택 난이도, 보상 배율, 새로 열린 난이도를 표시한다.

## 저장

Save schema v5에서 다음 값을 로컬 JSON에 저장한다.

- `selectedDifficultyId`: 현재 선택 난이도
- `clearedDifficultyIds`: Chapter 1을 완료한 난이도
- `clearedDifficultyStageKeys`: `difficultyId:stageId` 형식의 상위 난이도별 클리어 기록

v4 이하 세이브에서 `재문 숲 10`이 이미 클리어된 경우 Normal 완료로 이관한다. 당시 선택 난이도가 Hard 또는 Nightmare였다면 해당 완료 상태도 보존한다. 손상되거나 알 수 없는 난이도 키는 sanitize 과정에서 제거한다.

## 검증

- Unity Project Validator 통과
- Unity Progression Smoke 통과
- Android 15 API 35 에뮬레이터 Stage 1~10 전체 승리 통과
- Normal 완료 후 Hard 해금 및 Result 표시 확인
- Hard Stage 1~10 순차 진행 후 Nightmare 해금 확인
- 난이도 완료와 선택 상태의 JSON 디스크 재로드 확인
