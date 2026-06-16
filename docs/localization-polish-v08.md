# v0.8 Result Localization Polish

이 문서는 v0.9 Release Candidate로 넘어가기 전에 사용자에게 보이는 주요 UI 문구를 한국어로 정리한 기준을 기록한다.

## 원칙

- 내부 id와 표시 문자열을 분리한다.
- `stageId`, `heroId`, `runeId`, `skillId`, `effectKey`는 저장과 로직에만 사용한다.
- 사용자 화면에는 `displayName`, `displayNameKorean`, 설명 문구, 또는 `GameTextMapper`가 변환한 표시명을 사용한다.
- 디버그용 내부 값은 기본 UI에서 숨긴다.

## BattleState 표시명

| 내부 상태 | 표시명 |
| --- | --- |
| `Preparing` | 준비 중 |
| `WaveRunning` | 전투 중 |
| `RuneSelection` | 룬 선택 중 |
| `Victory` | 승리 |
| `Defeat` | 패배 |

## Difficulty 표시명

| 내부 id | 표시명 |
| --- | --- |
| `easy` | 쉬움 |
| `normal` | 보통 |
| `hard` | 어려움 |
| `nightmare` | 악몽 |

## Stage 표시명

StageData의 `displayNameKorean`을 우선 사용한다. 내부 id만 있는 경우 `stage_goblin_forest_10`은 `재문 숲 10`처럼 표시한다.

## Result UI 문구

Victory:

- 크리스탈 방어 성공!
- 획득 골드
- 스테이지 클리어: 예
- 다음 스테이지 해금: 재문 숲 N
- 클리어 웨이브: N/N
- 어려운 스테이지 전에 골드로 업그레이드하세요.

Defeat:

- 크리스탈이 파괴되었습니다.
- 획득 골드
- 스테이지 클리어: 아니오
- 다음 스테이지 해금: 아니오
- 클리어 웨이브: N/N
- 스테이지별 힌트 문구

## Rune Selection

룬 선택 화면은 다음만 표시한다.

- 룬 이름
- 등급
- 설명
- 선택 버튼

`effectKey`와 수치 값은 `showDebugEffectKey`가 켜진 경우에만 표시한다.

## 구현 위치

- `Assets/_Project/Scripts/UI/GameTextMapper.cs`
- `Assets/_Project/Scripts/UI/BattleHUD.cs`
- `Assets/_Project/Scripts/UI/StageResultUI.cs`
- `Assets/_Project/Scripts/UI/StageSelectUI.cs`
- `Assets/_Project/Scripts/UI/RuneSelectionUI.cs`
- `Assets/_Project/Scripts/UI/FormationSkillPanelUI.cs`
- `Assets/_Project/Scripts/UI/HeroSkillButton.cs`
