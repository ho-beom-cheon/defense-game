# 시스템 아키텍처 문서

## 1. 전체 구조

RuneGate Defense는 Unity 6 기반 2D 게임이며, 데이터와 런타임 로직을 분리한다.

```text
ScriptableObject Data
→ Runtime Controllers
→ Managers
→ UI Observers
```

## 2. 폴더 구조

```text
Assets/_Project/
  Scripts/
    Core/
    Battle/
    Hero/
    Monster/
    Skill/
    Rune/
    Wave/
    Data/
    Save/
    UI/
    Editor/
  Data/
    Heroes/
    Monsters/
    Skills/
    Runes/
    Stages/
  Prefabs/
    Heroes/
    Monsters/
    UI/
  Scenes/
  Art/
  Audio/
  Resources/
```

## 3. 주요 계층

### 3.1 Data Layer

전투 밸런스와 콘텐츠를 정의한다.

- HeroData
- MonsterData
- SkillData
- RuneData
- StageData
- WaveData
- WaveSpawnData

### 3.2 Runtime Layer

씬에서 실제로 움직이는 오브젝트다.

- HeroController
- MonsterController
- ProjectileController
- CrystalController
- SkillController

### 3.3 Manager Layer

게임 흐름을 제어한다.

- BattleManager
- WaveManager
- RuneManager
- RuneEffectApplier
- LaneManager

### 3.4 UI Layer

상태를 보여주고 입력을 받는다.

- BattleHUD
- RuneSelectionUI
- HeroSkillButton
- StageResultUI

## 4. 클래스 책임

### 4.1 BattleManager

역할:

- 현재 BattleState 관리
- StageData 초기화
- Wave 시작 지시
- Wave 완료 이벤트 수신
- RuneSelection 상태 진입
- Victory/Defeat 처리
- Restart 처리

하지 말 것:

- 몬스터 직접 이동 처리
- 영웅 공격 세부 처리
- UI Text 직접 조작 과다

주요 이벤트 후보:

```csharp
public event Action<BattleState> OnBattleStateChanged;
public event Action<int, int> OnWaveChanged;
public event Action<BattleResult> OnBattleEnded;
```

### 4.2 WaveManager

역할:

- WaveData를 기반으로 몬스터 스폰
- 살아있는 몬스터 수 추적
- 남은 스폰 수 추적
- 모든 스폰 완료 + 살아있는 몬스터 0이면 WaveComplete 이벤트 발생

주의:

- Coroutine 기반 스폰 가능
- Wave 중 Defeat 발생 시 스폰 중단 필요

### 4.3 LaneManager

역할:

- 3개 라인의 spawn position 제공
- 3개 라인의 crystal target position 제공
- hero slot position 제공
- laneIndex validation

확장:

- 후속 버전에서 3라인 외 변형 스테이지 가능

### 4.4 CrystalController

역할:

- maxHp/currentHp 보유
- TakeDamage 처리
- Heal 처리
- Shield placeholder 가능
- OnHpChanged 이벤트 발생
- HP 0 시 OnDestroyed 이벤트 발생

주의:

- HP는 0~maxHp 범위로 clamp

### 4.5 HeroController

역할:

- HeroData 기반 초기화
- RuntimeHeroStats 보유
- 타겟 탐색
- 자동 공격
- SkillController 호출
- 룬 효과로 runtime stats 갱신

타겟 규칙 MVP:

- 같은 라인에 있는 몬스터 우선
- 사거리 안의 가장 가까운 몬스터

확장 후보:

- TargetingType.First
- TargetingType.Nearest
- TargetingType.HighestHp
- TargetingType.Boss

### 4.6 MonsterController

역할:

- MonsterData 기반 초기화
- 라인 따라 수정 방향 이동
- TakeDamage 처리
- 사망 처리
- Crystal 도달 시 피해 적용 후 제거
- HP Bar 갱신 이벤트 제공

주의:

- 죽은 몬스터가 중복으로 골드를 지급하지 않게 `isDead` guard 필요

### 4.7 ProjectileController

역할:

- 타겟으로 이동
- 타겟에 도달하면 피해 적용
- 타겟이 사라지면 안전하게 제거

MVP에서는 궁수용 projectile만 있어도 충분하다.

### 4.8 RuneManager

역할:

- 사용 가능한 RuneData 목록 보유
- 랜덤 3개 선택
- 중복 방지
- 선택 이벤트 발생

### 4.9 RuneEffectApplier

역할:

- RuneData.effectKey를 해석해 전투 런타임에 적용

MVP effectKey:

| effectKey | 대상 | 효과 |
|---|---|---|
| hero_attack_percent | 모든 영웅 | 공격력 증가 |
| hero_attack_speed_percent | 모든 영웅 | 공격속도 증가 |
| crystal_heal_flat | Crystal | HP 회복 |

후속 effectKey:

- skill_cooldown_percent
- monster_slow_percent
- crystal_shield_flat
- boss_damage_percent

### 4.10 SkillController

역할:

- SkillData 보유
- 쿨타임 관리
- CanUseSkill 제공
- UseSkill 처리

MVP 스킬:

- Shield Bash: 근접 적에게 피해
- Rapid Shot: 타겟에게 연속 피해 또는 높은 단일 피해

## 5. 데이터 클래스 설계

### 5.1 HeroData

```csharp
public class HeroData : ScriptableObject
{
    public string heroId;
    public string displayName;
    public HeroRole role;
    public HeroPositionType positionType;
    public ElementType element;
    public int maxHp;
    public int attack;
    public float attackSpeed;
    public float attackRange;
    public SkillData skillData;
    public Sprite portrait;
    public RuntimeAnimatorController animatorController;
}
```

### 5.2 RuntimeHeroStats 권장

ScriptableObject를 직접 수정하지 않기 위해 런타임 스탯 클래스를 둔다.

```csharp
public class RuntimeHeroStats
{
    public int MaxHp;
    public int CurrentHp;
    public int Attack;
    public float AttackSpeed;
    public float AttackRange;
}
```

### 5.3 MonsterData

```csharp
public class MonsterData : ScriptableObject
{
    public string monsterId;
    public string displayName;
    public MonsterType monsterType;
    public ElementType element;
    public int maxHp;
    public float moveSpeed;
    public int damageToCrystal;
    public int rewardGold;
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;
}
```

## 6. 이벤트 흐름

### 6.1 몬스터 사망

```text
HeroController/ProjectileController
→ MonsterController.TakeDamage()
→ MonsterController.Die()
→ WaveManager.OnMonsterDied(monster)
→ BattleManager.AddGold(reward)
→ HUD 갱신
```

### 6.2 몬스터 수정 도달

```text
MonsterController reaches crystal target
→ CrystalController.TakeDamage(monster.damageToCrystal)
→ MonsterController removed
→ WaveManager.OnMonsterRemoved(monster)
→ Crystal HP 0이면 BattleManager.Defeat()
```

### 6.3 웨이브 클리어

```text
WaveManager detects no remaining spawns and no alive monsters
→ OnWaveCompleted
→ BattleManager receives
→ if final wave: Victory
→ else: RuneSelection
```

### 6.4 룬 선택

```text
BattleManager enters RuneSelection
→ RuneManager selects 3 runes
→ RuneSelectionUI displays
→ user selects rune
→ RuneEffectApplier.Apply(rune)
→ BattleManager.StartNextWave()
```

## 7. 씬 구성

`BattleScene`에는 다음 오브젝트가 있어야 한다.

```text
BattleScene
  BattleManager
  LaneManager
  CrystalController
  WaveManager
  RuneManager
  RuneEffectApplier
  Canvas
    BattleHUD
    RuneSelectionUI
    StageResultUI
    SkillButtonPanel
  Lanes
    Lane0
    Lane1
    Lane2
  Heroes
  Monsters
```

## 8. 성능 고려

MVP에서는 object pooling을 필수로 하지 않는다. 단, 후속 단계에서는 projectile과 monster에 pooling을 도입할 수 있다.

성능 주의점:

- 매 프레임 `FindObjectsOfType` 남발 금지
- 타겟 탐색 범위를 제한
- UI 갱신은 이벤트 기반 우선
- Coroutine 스폰 중단 처리 명확히

## 9. 후속 확장 포인트

- HeroPlacementManager
- MetaProgressionManager
- StageSelectManager
- SaveManager
- AudioManager
- VfxManager
- ObjectPool

단, v0.2에서는 만들지 않아도 된다.
