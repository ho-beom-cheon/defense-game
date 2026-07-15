# 그림자 계약 시스템

## 목적

전투에서 획득한 적성 조각을 장기 진행 보상으로 연결한다. StageSelect의 `그림자 계약` 버튼에서 적성 기록을 열고, 같은 적의 조각 5개로 계약한 뒤 한 종을 장착해 전투 패시브를 받는다.

## 플레이 흐름

1. 전투에서 몬스터를 처치하면 확률에 따라 해당 적성 조각을 얻는다.
2. StageSelect 헤더의 `그림자 계약`을 누른다.
3. 카드에서 보유 조각과 필요 수량을 확인한다.
4. 조각 5개를 소비해 계약한다.
5. 최초 계약은 자동 장착된다. 이후 계약한 적성끼리 교체하거나 장착을 해제할 수 있다.
6. 장착한 적성의 패시브는 다음 전투부터 적용된다.

## 계약 대상과 패시브

| 적성 기록 | 패시브 |
| --- | --- |
| 문틈 도깨비 | 승리 Gold +5% |
| 재갑 돌격병 | Crystal 최대 HP +5% |
| 부식 늑대 | 영웅 공격 속도 +3% |
| 균열 까마귀 | 모든 몬스터 이동 속도 -3% |
| 룬핵 점액 | Crystal 최대 HP +5% |
| 망각의 뼈병 | 영웅 공격력 +3% |
| 그룸바르 | 영웅 공격력 +4% |

## 저장 규칙

- `SaveData.monsterShardCounts`: 적성별 남은 조각 수
- `SaveData.contractedPetIds`: 계약 완료 적성 ID
- `SaveData.equippedPetId`: 현재 장착한 단일 적성 ID
- `SaveData.hasSeenPetTutorial`: 계약서 최초 안내 확인
- 계약은 조각 차감, 계약 목록 추가와 최초 자동 장착을 한 번의 저장으로 처리한다.
- 내부 `monsterId`는 저장 키로만 사용하고 UI에는 한국어 표시명을 사용한다.

## UI 구조

- StageSelect 헤더: `그림자 계약` 진입 버튼
- 중앙 Safe Area 팝업: 장착 요약, 7종 카드 ScrollView, 피드백, 장착 해제
- 넓은 Portrait: 2열 카드
- 720 Portrait와 Landscape fallback: 1열 스크롤
- 카드: RuntimePixel 적 초상, 패시브 아이콘, 조각 진행 바, 계약/장착/해제 상태
- `ConceptSheets` 이미지는 사용하지 않고 `RuntimePixel`과 전용 PetContract UI 자산만 사용한다.

## 자동 검증

- Project Validator가 필수 폴더, 스크립트, 9개 런타임 카탈로그 연결을 검사한다.
- Progression Smoke가 적성 7종 정의와 5개 화면비의 팝업/카드/터치 영역을 검사한다.
- Android System Flow가 조각 `5 -> 0`, 계약, 자동 장착, 해제, 재장착과 JSON 재로드를 확인한다.
- UI Capture가 1080x2400에서 실제 Android 렌더 결과를 PNG로 남긴다.

## 현재 한계

- 계약 비용과 패시브 수치는 1차 밸런스다.
- 한 번에 한 종만 장착한다.
- 펫이 BattleScene에 별도 유닛으로 따라다니지는 않는다.
- 전용 펫 애니메이션과 성장 레벨은 후속 범위다.
