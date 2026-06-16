# Difficulty Design

v0.8에서는 난이도 UI와 저장 구조만 준비한다. 실제 난이도별 수치 적용은 v0.9 이후 작업으로 남긴다.

## 난이도

| ID | 표시 | 상태 |
| --- | --- | --- |
| `easy` | Easy | 선택 가능 |
| `normal` | Normal | 기본 선택 |
| `hard` | Hard | 잠금 placeholder |
| `nightmare` | Nightmare | 잠금 placeholder |

## 저장

선택한 난이도는 `SaveData.selectedDifficultyId`에 저장된다. `GameSession.SelectedDifficultyId`는 BattleScene 시작 데이터에 전달할 수 있도록 준비되어 있다.

## 다음 단계

- 난이도별 몬스터 HP/속도/보상 계수 정의
- StageSelect에서 Hard/Nightmare 해금 조건 추가
- Result 화면에 난이도별 보상 배율 표시
