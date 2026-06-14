# Codex 구현 가이드

> 이 문서는 Codex가 각 작업을 수행할 때 따라야 할 운영 절차, 코딩 규칙, 검증 방법을 정의한다.

## 1. Codex가 항상 먼저 읽어야 하는 문서

작업 시작 전 다음 문서를 순서대로 읽는다.

1. `docs/00_PROJECT_CONTEXT.md`
2. `docs/01_GAME_DESIGN_DOCUMENT.md`
3. `docs/02_DEVELOPMENT_ROADMAP.md`
4. 작업과 관련된 세부 문서
   - 시스템 구조 작업: `docs/04_SYSTEM_ARCHITECTURE.md`
   - 밸런스/데이터 작업: `docs/05_CONTENT_AND_BALANCE_SPEC.md`
   - UI 작업: `docs/06_UI_UX_SPEC.md`
   - 아트 작업: `docs/07_ART_AUDIO_PIPELINE.md`
   - 테스트 작업: `docs/08_TEST_AND_QA_PLAN.md`

## 2. 기본 작업 절차

Codex는 다음 절차로 작업한다.

```text
1. git status 확인
2. 현재 브랜치 확인
3. 관련 docs 읽기
4. 구현 계획 요약
5. 파일 수정
6. 컴파일 가능성 점검
7. Unity validation 가능하면 실행
8. 변경 파일 목록 보고
9. 수동 테스트 방법 보고
10. commit/push는 사용자가 명시했을 때만 수행하거나 작업 지시에 따름
```

## 3. 금지 사항

Codex는 다음을 하지 않는다.

- 외부 패키지 설치
- 유료 에셋 추가
- 광고 SDK 추가
- Google Play Billing 추가
- Firebase 추가
- 서버 코드 추가
- 로그인 추가
- Addressables 추가
- 복잡한 DI 컨테이너 추가
- ECS로 재구현
- 기존 구조를 통째로 갈아엎기
- 대량의 아트 바이너리 생성
- `Library/`, `Temp/`, `Build/` 등을 Git에 추가

## 4. 구현 우선순위

항상 다음 순서를 따른다.

1. 컴파일 가능한 구조
2. 실제 실행 가능한 플레이 루프
3. 유지보수 가능한 코드
4. 테스트 가능한 설계
5. UI/UX 개선
6. 아트/연출

## 5. 네임스페이스

모든 게임 스크립트는 기본적으로 다음 네임스페이스를 사용한다.

```csharp
namespace RuneGate
{
}
```

Editor 전용 스크립트도 가능하면 `RuneGate.EditorTools` 또는 `RuneGate` 내부로 정리한다.

## 6. C# 코딩 규칙

### 6.1 필드 규칙

권장:

```csharp
[SerializeField] private int maxHp;
[SerializeField] private HeroData heroData;
```

비권장:

```csharp
public int maxHp;
public HeroData heroData;
```

### 6.2 Null Guard

외부 참조가 필요한 경우 반드시 null guard를 둔다.

```csharp
if (heroData == null)
{
    Debug.LogWarning("HeroData is missing.", this);
    return;
}
```

### 6.3 이벤트 기반 UI 갱신

전투 로직이 직접 UI Text를 수정하지 않도록 한다.

권장:

```csharp
public event Action<int, int> OnCrystalHpChanged;
```

비권장:

```csharp
hpText.text = currentHp.ToString();
```

단, MVP에서는 과도한 추상화를 피한다. UI placeholder에서 최소 참조는 허용한다.

## 7. ScriptableObject 규칙

밸런스 데이터는 `ScriptableObject`로 관리한다.

대상:

- HeroData
- MonsterData
- SkillData
- RuneData
- StageData
- WaveData
- WaveSpawnData

게임 중 변하는 런타임 값은 ScriptableObject를 직접 수정하지 않는다.

비권장:

```csharp
heroData.attack += 10;
```

권장:

```csharp
runtimeStats.Attack += 10;
```

## 8. BattleState 규칙

전투 상태는 명확히 유지한다.

| 상태 | 의미 |
|---|---|
| None | 초기 미설정 |
| Preparing | 전투 준비 |
| WaveRunning | 웨이브 진행 중 |
| RuneSelection | 룬 선택 중 |
| Victory | 승리 |
| Defeat | 패배 |

상태 전환은 가능하면 `BattleManager`가 담당한다.

## 9. v0.2 구현 지시 템플릿

Codex에 v0.2를 구현시킬 때 사용할 프롬프트:

```text
Read docs/00_PROJECT_CONTEXT.md, docs/01_GAME_DESIGN_DOCUMENT.md, docs/02_DEVELOPMENT_ROADMAP.md, docs/03_CODEX_IMPLEMENTATION_GUIDE.md, docs/04_SYSTEM_ARCHITECTURE.md, and docs/08_TEST_AND_QA_PLAN.md.

Implement Battle Prototype v0.2.

Scope:
- Complete wave clear detection.
- Show rune selection after non-final wave.
- Apply functional rune effects: hero_attack_percent, hero_attack_speed_percent, crystal_heal_flat.
- Start next wave after rune selection.
- Show Victory on final wave clear.
- Show Defeat when Crystal HP reaches 0.
- Add Restart button.
- Add monster HP bar and hit flash.
- Improve skill cooldown button state.

Restrictions:
- No ads.
- No IAP.
- No server.
- No login.
- No analytics.
- No external packages.
- No Addressables.
- No art import.
- Preserve namespace RuneGate.
- Preserve ScriptableObject data-driven design.

Before implementing, summarize the plan.
After implementing, list changed files and manual Unity test steps.
```

## 10. 컴파일 검증

가능하면 Unity batchmode를 사용한다.

예시 경로는 로컬 설치 상태에 따라 다르다.

```powershell
"C:\Program Files\Unity\Hub\Editor\<UnityVersion>\Editor\Unity.exe" -batchmode -quit -projectPath "C:\workspace\defense-game" -logFile "C:\workspace\defense-game\unity-validate.log"
```

테스트 어셈블리가 있으면:

```powershell
Unity.exe -batchmode -projectPath "C:\workspace\defense-game" -runTests -testPlatform EditMode -logFile unity-tests.log -quit
```

Codex는 실제로 실행하지 못했다면 실행했다고 말하지 않는다.

## 11. 수동 테스트 보고 형식

작업 후 다음 형식으로 보고한다.

```text
Manual Test Steps:
1. Open Unity Hub.
2. Open C:\workspace\defense-game.
3. Open Assets/_Project/Scenes/BattleScene.unity.
4. Press Play.
5. Confirm Wave 1 starts.
6. Confirm monsters spawn and move.
7. Confirm heroes attack.
8. Confirm rune selection appears after wave clear.
9. Select a rune.
10. Confirm next wave starts.
11. Confirm Victory/Defeat result panel.
```

## 12. 커밋 기준

커밋 전 체크:

- Unity 컴파일 에러 없음
- `git status` 확인
- `Library/`, `Temp/`, `Logs/` 미포함
- 변경 파일 설명 가능
- 수동 테스트 절차 명확

커밋 메시지는 기능 단위로 작성한다.

```text
Implement battle loop v0.2
Add rune selection flow
Add monster hp feedback
```
