# Task v0.3 — Progression Loop

## 0. 목적

`v0.2 Playable Battle Prototype`은 전투 한 판이 돌아가는 상태다.  
`v0.3`의 목표는 전투 바깥의 게임 루프를 붙여서, 프로젝트를 **전투 데모**에서 **플레이 가능한 게임 쉘**로 올리는 것이다.

최종 흐름:

```text
TitleScene
→ StageSelectScene
→ BattleScene
→ Result Flow
→ UpgradeScene
→ Save
→ StageSelectScene
```

## 1. 현재 상태 가정

현재 프로젝트에는 다음이 이미 존재한다고 가정한다.

```text
Assets/
  _Project/
    Scripts/
      Battle/
      Hero/
      Monster/
      Rune/
      Skill/
      Wave/
      Data/
      UI/
      Editor/
    Data/
    Scenes/
README.md
RUNEGATE_MASTER_PLAN.md
docs/
```

현재 BattleScene 기능:

- 3라인 전투
- Crystal HP
- Knight / Archer placeholder
- Goblin / Orc placeholder
- Wave spawning
- Hero auto attack
- Manual skill buttons
- Rune selection after wave clear
- Rune effects:
  - `hero_attack_percent`
  - `hero_attack_speed_percent`
  - `crystal_heal_flat`
- Victory / Defeat state
- placeholder visuals
- no server, no ads, no IAP

## 2. 이번 작업의 핵심 목표

v0.3은 아래 기능이 완성되면 성공이다.

```text
게임 시작 화면
→ 스테이지 선택
→ 선택한 스테이지로 전투 시작
→ 승리 시 골드 획득
→ 다음 스테이지 해금
→ 업그레이드 구매
→ 저장
→ Unity Play Mode를 껐다 켜도 진행상태 유지
```

## 3. 브랜치 전략

작업 전:

```powershell
cd C:\workspace\defense-game

git switch main
git pull
git switch -c codex/progression-v03
```

작업 후 검증 성공 시:

```powershell
git status
git add .
git commit -m "feat(progression): add stage, save, and upgrade loop"
git push -u origin codex/progression-v03
```

## 4. 구현 범위

### 4.1 Scene Flow

생성 또는 갱신할 씬:

```text
Assets/_Project/Scenes/TitleScene.unity
Assets/_Project/Scenes/StageSelectScene.unity
Assets/_Project/Scenes/BattleScene.unity
Assets/_Project/Scenes/UpgradeScene.unity
```

#### TitleScene

필수 UI:

- 게임 제목: `RuneGate Defense`
- `Start` 버튼
- `Continue` 버튼
- `Reset Save` 버튼

동작:

| 버튼 | 동작 |
|---|---|
| Start | 저장 데이터가 없으면 기본 저장 생성 후 StageSelectScene 이동 |
| Continue | 저장 데이터 로드 후 StageSelectScene 이동 |
| Reset Save | 개발 테스트용 저장 삭제/초기화 |

주의:

- `Continue`는 저장 데이터가 없으면 비활성화하거나 `Start`와 동일하게 동작해도 된다.
- TextMeshPro를 강제로 추가하지 않는다.
- 현재 프로젝트가 `OnGUI` 기반이면 그 방식을 유지해도 된다.

#### StageSelectScene

필수 UI:

- Stage 1
- Stage 2
- Stage 3
- Back to Title
- Go to Upgrade

스테이지 해금 규칙:

```text
Stage 1: 기본 해금
Stage 2: Stage 1 클리어 후 해금
Stage 3: Stage 2 클리어 후 해금
```

동작:

| 조건 | 동작 |
|---|---|
| 해금된 스테이지 클릭 | GameSession에 선택 스테이지 저장 후 BattleScene 이동 |
| 잠긴 스테이지 클릭 | 아무 일도 하지 않거나 "Locked" 표시 |
| Upgrade 클릭 | UpgradeScene 이동 |
| Back 클릭 | TitleScene 이동 |

#### BattleScene

기존 BattleScene은 유지하되, 다음을 반영한다.

- 선택된 StageData를 기반으로 전투 시작
- 선택된 StageData가 없으면 fallback으로 Stage 1 사용
- Victory / Defeat 이후 결과 흐름으로 이동
- Victory 시 SaveData에 클리어/해금/골드 반영
- Defeat 시 최소한 획득 골드 반영 여부를 정책화

추천 정책:

```text
Victory: earnedGold 전액 지급 + stage cleared + next stage unlocked
Defeat: earnedGold 50% 지급
```

단순화를 원하면 Defeat 보상은 0이어도 된다.  
단, 정책은 README에 명시한다.

#### Result Flow

별도 `ResultScene`을 만들지 않고 BattleScene 내부 결과 패널로 처리해도 된다.

필수 표시:

- Victory / Defeat
- Earned Gold
- Stage Clear 여부
- Retry
- Upgrade
- Stage Select

동작:

| 버튼 | 동작 |
|---|---|
| Retry | 같은 StageData로 BattleScene 재시작 |
| Upgrade | UpgradeScene 이동 |
| Stage Select | StageSelectScene 이동 |

#### UpgradeScene

필수 UI:

- 현재 총 골드
- 업그레이드 목록
- 각 업그레이드 레벨
- 각 업그레이드 비용
- 구매 버튼
- Back to Stage Select

구매 조건:

```text
totalGold >= upgradeCost
currentLevel < maxLevel
```

구매 시:

```text
totalGold -= cost
upgradeLevel += 1
Save
UI refresh
```

## 5. Save System

### 5.1 생성 파일

```text
Assets/_Project/Scripts/Save/SaveData.cs
Assets/_Project/Scripts/Save/SaveManager.cs
Assets/_Project/Scripts/Save/SerializableUpgradeLevel.cs
```

필요하면 다음도 생성:

```text
Assets/_Project/Scripts/Core/GameSession.cs
Assets/_Project/Scripts/Core/SceneNames.cs
```

### 5.2 저장 위치

```csharp
Application.persistentDataPath
```

파일명 예:

```text
runegate_save.json
```

### 5.3 SaveData 구조

필드:

```text
int totalGold
List<string> clearedStageIds
List<string> unlockedStageIds
List<SerializableUpgradeLevel> upgradeLevels
string lastSelectedStageId
bool hasSeenIntro
```

Unity `JsonUtility`는 Dictionary 직렬화가 약하므로 Dictionary를 직접 저장하지 않는다.

예시:

```csharp
[Serializable]
public class SerializableUpgradeLevel
{
    public string upgradeId;
    public int level;
}
```

### 5.4 SaveManager 책임

필수 메서드:

```text
Load()
Save()
ResetSave()
CreateDefaultSave()
HasSave()
AddGold(int amount)
SpendGold(int amount)
MarkStageCleared(string stageId)
UnlockStage(string stageId)
IsStageUnlocked(string stageId)
GetUpgradeLevel(string upgradeId)
SetUpgradeLevel(string upgradeId, int level)
```

주의:

- 저장 파일이 깨져 있으면 기본 저장으로 복구하고 경고 로그 출력
- null list 방지
- 저장 실패 시 Debug.LogError
- SaveData를 직접 외부에서 난잡하게 수정하지 않도록 래퍼 메서드 제공

## 6. Game Session

### 6.1 생성 파일

```text
Assets/_Project/Scripts/Core/GameSession.cs
```

### 6.2 책임

전투/결과/업그레이드 사이에서 임시 런타임 상태를 유지한다.

필드 예:

```text
string SelectedStageId
StageData SelectedStageData
BattleResult LastBattleResult
int LastEarnedGold
bool LastBattleWasVictory
```

프로토타입 단계에서는 static class 허용.

규칙:

- SaveData는 영구 저장
- GameSession은 현재 실행 중인 세션 전달용
- GameSession은 게임 종료 시 유지될 필요 없음

## 7. Stage Data 확장

### 7.1 생성할 StageData

```text
stage_goblin_forest_01
stage_goblin_forest_02
stage_goblin_forest_03
```

### 7.2 Stage 1

목표:

- 쉬운 테스트용
- 2 waves

구성:

```text
Wave 1:
- Goblin lane 0 x 3
- Goblin lane 1 x 3

Wave 2:
- Goblin lane 0 x 3
- Orc lane 1 x 1
- Goblin lane 2 x 3
```

### 7.3 Stage 2

목표:

- Stage 1보다 빠른 페이싱
- 3 waves

구성:

```text
Wave 1:
- Goblin lane 0 x 4
- Goblin lane 1 x 4

Wave 2:
- Orc lane 0 x 1
- Goblin lane 1 x 5
- Goblin lane 2 x 4

Wave 3:
- Orc lane 1 x 2
- Goblin lane 0 x 5
- Goblin lane 2 x 5
```

### 7.4 Stage 3

목표:

- Orc 압박 증가
- 3 waves

구성:

```text
Wave 1:
- Goblin lane 0 x 5
- Goblin lane 2 x 5

Wave 2:
- Orc lane 0 x 2
- Goblin lane 1 x 6
- Orc lane 2 x 1

Wave 3:
- Orc lane 0 x 2
- Orc lane 1 x 2
- Orc lane 2 x 2
- Goblin lane 1 x 6
```

## 8. Upgrade System

### 8.1 생성 파일

```text
Assets/_Project/Scripts/Data/UpgradeData.cs
Assets/_Project/Scripts/Progression/UpgradeManager.cs
Assets/_Project/Scripts/UI/UpgradeEntryUI.cs
```

만약 `Progression` 폴더가 없으면 생성:

```text
Assets/_Project/Scripts/Progression
Assets/_Project/Data/Upgrades
```

### 8.2 UpgradeData 필드

```text
string upgradeId
string displayName
string description
int baseCost
float costMultiplier
int maxLevel
string effectKey
float valuePerLevel
```

비용 계산:

```text
cost = round(baseCost * pow(costMultiplier, currentLevel))
```

### 8.3 기본 업그레이드 4종

| upgradeId | 이름 | effectKey | 효과 |
|---|---|---|---|
| `crystal_reinforcement` | Crystal Reinforcement | `crystal_max_hp_flat` | 레벨당 Crystal Max HP +20 |
| `hero_training` | Hero Training | `hero_attack_percent` | 레벨당 영웅 공격력 +5% |
| `battle_rhythm` | Battle Rhythm | `hero_attack_speed_percent` | 레벨당 공격속도 +3% |
| `skill_practice` | Skill Practice | `skill_cooldown_percent` | 레벨당 스킬 쿨타임 -3% |

권장 기본값:

```text
baseCost: 50
costMultiplier: 1.35
maxLevel: 10
```

### 8.4 Battle 적용 순서

전투 시작 시 스탯 적용 순서:

```text
1. ScriptableObject base data
2. Permanent upgrade effects from SaveData
3. Temporary rune effects during current battle
```

주의:

- `HeroData`, `MonsterData`, `StageData` 원본 ScriptableObject 값을 런타임에 변경하지 않는다.
- 영웅 컨트롤러는 runtime stat copy를 사용한다.

## 9. Bootstrapper 업데이트

기존 bootstrapper를 확장한다.

### 9.1 메뉴 추가

```text
Tools/RuneGate/Bootstrap Progression Prototype
Tools/RuneGate/Validate Project
```

기존 메뉴는 유지:

```text
Tools/RuneGate/Bootstrap Playable Prototype
```

### 9.2 Bootstrap Progression Prototype 동작

필수:

- 폴더 생성
- Stage 1~3 생성
- UpgradeData 4종 생성
- TitleScene 생성
- StageSelectScene 생성
- BattleScene 갱신
- UpgradeScene 생성
- 샘플 데이터 누락 시 생성
- 기존 데이터가 있으면 가능한 한 덮어쓰지 않음
- Console에 생성/스킵 로그 출력

주의:

- 사용자 커스텀 씬을 무조건 덮어쓰지 말 것
- 덮어쓰기 필요 시 명확한 로그 출력
- meta 파일 정상 생성

## 10. Validation Tool

### 10.1 메뉴

```text
Tools/RuneGate/Validate Project
```

### 10.2 검사 항목

- 필수 폴더 존재
- 필수 씬 존재
- StageData 3개 존재
- UpgradeData 4개 존재
- SaveManager 기본 저장 생성 가능
- BattleScene 존재
- TitleScene 존재
- StageSelectScene 존재
- UpgradeScene 존재
- `Application.persistentDataPath` 저장 경로 로그
- 누락 항목은 `Debug.LogWarning`
- 치명적 누락은 `Debug.LogError`

## 11. README 업데이트

README에 추가:

```text
## v0.3 Progression Loop

Implemented:
- TitleScene
- StageSelectScene
- BattleScene stage selection
- Result flow
- UpgradeScene
- Local JSON save
- Stage unlocks
- Permanent upgrades

How to run:
1. Open Unity project.
2. Run Tools/RuneGate/Bootstrap Progression Prototype.
3. Open Assets/_Project/Scenes/TitleScene.unity.
4. Press Play.
5. Start → Stage Select → Stage 1 → Battle.
```

Save 경로:

```text
Application.persistentDataPath/runegate_save.json
```

MVP 제외사항도 유지:

- no server
- no login
- no gacha
- no ads
- no IAP
- no analytics
- no external paid APIs

## 12. 수동 테스트 시나리오

### 12.1 기본 진행 테스트

```text
1. Unity 실행
2. Tools/RuneGate/Bootstrap Progression Prototype 실행
3. TitleScene 열기
4. Play
5. Start 클릭
6. StageSelectScene 이동 확인
7. Stage 1 unlocked 확인
8. Stage 2/3 locked 확인
9. Stage 1 클릭
10. BattleScene 시작 확인
11. 전투 승리
12. Result 표시 확인
13. earnedGold 표시 확인
14. Stage Select 클릭
15. Stage 2 unlocked 확인
```

### 12.2 저장 테스트

```text
1. Stage 1 클리어
2. Stage 2 해금 확인
3. Play Mode 종료
4. 다시 TitleScene Play
5. Continue 클릭
6. Stage 2 해금 상태 유지 확인
```

### 12.3 업그레이드 테스트

```text
1. 골드 확보
2. UpgradeScene 이동
3. Crystal Reinforcement 구매
4. totalGold 감소 확인
5. upgrade level 증가 확인
6. StageSelect로 복귀
7. Stage 1 다시 시작
8. Crystal HP 증가 확인
```

### 12.4 Reset Save 테스트

```text
1. TitleScene 이동
2. Reset Save 클릭
3. StageSelect로 이동
4. Stage 1만 unlocked 확인
5. totalGold 0 확인
6. upgrade level 0 확인
```

## 13. 완료 기준

v0.3은 아래 조건을 모두 만족해야 완료다.

```text
- TitleScene에서 시작 가능
- StageSelectScene으로 이동 가능
- Stage 1~3 표시
- Stage unlock 저장 가능
- 선택한 StageData로 전투 시작
- 전투 승리/패배 결과 표시
- Victory 시 골드 지급
- Victory 시 다음 스테이지 해금
- UpgradeScene에서 업그레이드 구매 가능
- 업그레이드 효과가 전투에 적용
- Play Mode 재시작 후 저장 유지
- Reset Save 가능
- Unity compile error 없음
```

## 14. Codex 실행 프롬프트

아래 프롬프트를 Codex에 그대로 붙여넣는다.

```text
Read RUNEGATE_MASTER_PLAN.md and docs if they exist.
Then implement Task v0.3 Progression Loop according to docs/11_TASK_V03_PROGRESSION_LOOP.md.

Do not rely on prior chat context.

Current state:
The project has a playable battle prototype with 3 lanes, Crystal HP, Knight/Archer, Goblin/Orc, wave spawning, hero auto attack, manual skill buttons, rune selection, victory and defeat.

Goal:
Implement Title → Stage Select → Battle → Result → Upgrade → Save → Stage Select flow.

Must implement:
- TitleScene
- StageSelectScene
- BattleScene selected StageData loading
- Result flow
- UpgradeScene
- local JSON save system
- stage unlocks
- total gold
- permanent upgrades
- bootstrapper menu Tools/RuneGate/Bootstrap Progression Prototype
- validator menu Tools/RuneGate/Validate Project
- README update

Restrictions:
- no server
- no login
- no gacha
- no ads
- no IAP
- no analytics
- no Firebase
- no Addressables
- no external packages
- no final art integration

After implementation:
- Make the project compile.
- Run Unity validation if possible.
- List created/modified files.
- Explain manual Unity test steps.
```
