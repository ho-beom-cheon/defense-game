# UI/UX 명세

## 1. 목표

MVP UI는 화려함보다 정보 전달과 테스트 편의성이 우선이다. 플레이어가 다음을 즉시 이해해야 한다.

- Crystal HP가 얼마나 남았는가
- 현재 몇 번째 웨이브인가
- 전투 상태가 무엇인가
- 영웅 스킬을 사용할 수 있는가
- 룬 선택이 필요한가
- 승리했는가 패배했는가

## 2. BattleScene UI 구성

```text
Canvas
  BattleHUD
    TitleText
    CrystalHpText
    WaveText
    StateText
    GoldText
    HeroStatsText
  SkillButtonPanel
    KnightSkillButton
    ArcherSkillButton
  RuneSelectionPanel
    RuneCardButton1
    RuneCardButton2
    RuneCardButton3
  ResultPanel
    ResultTitleText
    RewardText
    RestartButton
    BackButtonPlaceholder
```

## 3. HUD

### 3.1 표시 항목

| 항목 | 예시 |
|---|---|
| 게임명 | RuneGate Defense |
| Crystal HP | Crystal HP 180/180 |
| Wave | Wave 1/2 |
| State | State WaveRunning |
| Gold | Gold 12 |
| Hero Stats | Knight ATK 20 SPD 1.00 |

### 3.2 MVP 표현 방식

- Text 기반이어도 충분하다.
- TextMeshPro가 프로젝트에 이미 있지 않으면 UnityEngine.UI.Text를 사용한다.
- 전투 로직과 UI는 이벤트 또는 public refresh method로 연결한다.

## 4. Rune Selection UI

### 4.1 표시 타이밍

- 비최종 웨이브 종료 후 표시
- 표시 중에는 전투 진행을 멈춘다.

### 4.2 카드 정보

각 카드에 다음을 표시한다.

- 룬 이름
- 룬 설명
- 수치

예:

```text
Sword Rune
All heroes ATK +20%
```

### 4.3 선택 후 처리

- 선택한 버튼을 누르면 즉시 룬 적용
- RuneSelectionPanel 숨김
- 다음 웨이브 시작

## 5. Skill Button UI

### 5.1 표시 항목

- 영웅 이름
- 스킬 이름
- 쿨타임 상태

예:

```text
Knight: Shield Bash
Ready
```

쿨타임 중:

```text
Knight: Shield Bash
5.2s
```

### 5.2 버튼 상태

| 상태 | 동작 |
|---|---|
| Ready | 클릭 가능 |
| Cooldown | 클릭 불가 또는 클릭해도 무시 |
| Battle Ended | 비활성화 |

## 6. Result UI

### 6.1 Victory

표시:

```text
Victory
Gold Earned: 50
[Restart]
[Back - Placeholder]
```

### 6.2 Defeat

표시:

```text
Defeat
Gold Earned: 12
[Restart]
[Back - Placeholder]
```

## 7. 화면 전환 플로우

```text
Preparing
→ HUD 표시
→ WaveRunning
→ RuneSelectionPanel 표시
→ WaveRunning
→ Victory/Defeat
→ ResultPanel 표시
→ Restart
```

## 8. 모바일 고려사항

- Portrait 고정
- 버튼은 손가락으로 누르기 충분한 크기
- 스킬 버튼은 하단 또는 좌측 하단
- 룬 카드 선택은 화면 중앙
- 전투 중 핵심 정보는 화면 상단 또는 좌측 상단

## 9. v0.2 UI 완료 기준

- Crystal HP가 실시간 갱신된다.
- Wave 번호가 갱신된다.
- Battle State가 갱신된다.
- 스킬 버튼 쿨타임이 보인다.
- 룬 선택 UI가 뜬다.
- 룬 선택 후 사라진다.
- 승리/패배 패널이 뜬다.
- Restart가 작동한다.

## 10. 후속 UI 계획

v0.5 이후:

- TitleScene
- StageSelectScene
- UpgradeScene
- Collection/Character screen
- Settings screen
- Sound toggle
- Language option placeholder

MVP에서는 BattleScene 중심으로 진행한다.
