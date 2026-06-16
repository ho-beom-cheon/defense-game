# Save System

RuneGate Defense는 로컬 JSON 저장을 유지한다. 서버, 로그인, 클라우드 저장은 사용하지 않는다.

## SaveData 필드

- `saveVersion`: 저장 구조 버전
- `totalGold`: 보유 Gold
- `clearedStageIds`: 클리어한 Stage ID 목록
- `unlockedStageIds`: 해금된 Stage ID 목록
- `upgradeLevels`: 업그레이드별 레벨
- `formationSlots`: 현재 편성 슬롯
- `lastSelectedStageId`: 마지막 선택 Stage
- `selectedDifficultyId`: 마지막 선택 난이도
- `hasSeenIntro`: 인트로/튜토리얼 계열 호환 플래그
- `hasSeenTutorial`: 튜토리얼 완료 여부

## 안정화 정책

- 저장 파일이 없으면 기본 저장을 생성한다.
- 저장 파일이 비어 있거나 읽기에 실패하면 `.bak` 백업 후 기본 저장으로 복구한다.
- Gold는 음수가 되지 않게 보정한다.
- Stage ID 목록은 중복을 제거한다.
- 업그레이드 ID가 비어 있거나 중복된 항목은 정리한다.
- UpgradeScene 진입 시 저장된 업그레이드 레벨이 해당 `UpgradeData.MaxLevel`을 넘지 않게 보정한다.

## Reset Save

Reset Save는 TitleScene에서 2단계 확인 후 실행된다. Reset 후 기본 저장이 다시 생성되고 Stage 1만 해금된다.
