# RuneGate Defense Pixel Art Pipeline

이 문서는 전투 런타임용 픽셀 스프라이트 제작과 임포트 기준을 정의한다.

## Folder Contract

```text
Assets/_Project/Art/RuntimePixel/
  Heroes/
    Leon/
    Seria/
    Kael/
    Mirea/
    Brom/
    Nyx/
  Monsters/
  Bosses/
  UI/
```

`ConceptSheets` 이미지는 참고용 기록 이미지이고, `RuntimePixel`은 BattleScene에서 사용하는 작은 전투 스프라이트 전용이다.

## Runtime Sprite Requirements

- 모바일 세로 화면에서 3라인 전투를 읽을 수 있어야 한다.
- 캐릭터 실루엣은 32x32, 48x48, 64x64 중 하나로 시작한다.
- Pivot은 캐릭터/몬스터 모두 bottom-center를 기본으로 한다.
- Texture Type은 Sprite (2D and UI)를 사용한다.
- Sprite Mode는 단일 이미지면 Single, 시트면 Multiple을 사용한다.
- 컨셉 카드 이미지를 억지로 잘라 BattleScene 유닛으로 쓰지 않는다.
- RuntimePixel 스프라이트가 아직 없으면 작은 colored square fallback을 사용한다.
- BattleScene용 스프라이트는 `HeroData.battleSprite` 또는 `MonsterData.runtimeSprite`에만 연결한다.

## Battle Display Scale

현재 v0.6 전투 화면 기준 target height는 다음과 같다.

- 일반 영웅: 1.32~1.4 world units
- 레온 같은 전열 탱커: 1.48 world units
- 브롬 같은 큰 영웅/기술자: 1.6 world units
- 작은 몬스터: 0.95~1.08 world units
- 오크/중형 몬스터: 1.38 world units
- 그룸바르 보스: 2.55 world units

스케일 보정은 Visual child의 `SpriteRenderer`에만 적용한다. Root object는 lane 이동, 사거리 계산, 타겟팅 기준점이므로 직접 scale하지 않는다.

## Lane and Slot Layout

- Lane y 좌표는 3라인 기준 `-2.15`, `0`, `2.15`로 읽히게 한다.
- Monster path는 `spawnX=5.75`에서 `crystalX=-5.15`로 이동한다.
- Hero front slot은 `x=-0.55`, middle slot은 `x=-1.5`, back slot은 `x=-2.45` 기준이다.
- Front는 적과 가장 가까운 슬롯, Back은 크리스탈에 가까운 슬롯이다.
- UI/HUD는 왼쪽 고정 IMGUI 패널을 유지하되, 전투 유닛은 중앙 lane 영역에서 읽히도록 배치한다.

## Suggested Runtime Files

### Heroes

- `leon_idle.png`
- `seria_idle.png`
- `kael_idle.png`
- `mirea_idle.png`
- `brom_idle.png`
- `nyx_idle.png`

### Monsters

- `gate_imp_idle.png` 또는 `goblin_idle.png`
- `orc_brute_idle.png`
- `dire_wolf_idle.png`
- `cave_bat_idle.png`
- `core_slime_idle.png`
- `bone_soldier_idle.png`

### Bosses

- `grumbar_idle.png`

### UI

- `ui_record_panel_9slice.png`
- `ui_seal_warning_badge.png`
- `ui_rune_record_icon.png`
- `ui_gatekeeper_record_tab.png`
- Done: `Assets/_Project/Art/RuntimePixel/UI/ui_panel_dark.png`
- Done: `Assets/_Project/Art/RuntimePixel/UI/ui_button_skill.png`
- Done: `Assets/_Project/Art/RuntimePixel/UI/ui_rune_card_base.png`

### Backgrounds

- Done: `Assets/_Project/Art/RuntimePixel/Backgrounds/bg_goblin_forest_lanes.png`

### Effects

- Done: `Assets/_Project/Art/RuntimePixel/Effects/fx_shield_bash.png`
- Done: `Assets/_Project/Art/RuntimePixel/Effects/fx_rapid_shot.png`
- Done: `Assets/_Project/Art/RuntimePixel/Effects/fx_meteor_impact.png`
- Done: `Assets/_Project/Art/RuntimePixel/Effects/fx_holy_heal.png`
- Done: `Assets/_Project/Art/RuntimePixel/Effects/fx_turret_shot.png`
- Done: `Assets/_Project/Art/RuntimePixel/Effects/fx_shadow_slash.png`
- Done: `Assets/_Project/Art/RuntimePixel/Effects/fx_hit_spark.png`
- Done: `Assets/_Project/Art/RuntimePixel/Effects/fx_death_puff.png`

## Current State

현재 BattleScene은 영웅 6인, 몬스터 6종, 보스 1종의 RuntimePixel idle 스프라이트를 사용한다. RuntimePixel 스프라이트가 없는 새 유닛은 `PlaceholderSprite` fallback을 사용한다. 새 컨셉 이미지는 도감/기록서 방향을 잡는 참고 이미지로만 사용한다.

## Required RuntimePixel Backlog

Heroes:

- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Leon/leon_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Seria/seria_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Kael/kael_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Mirea/mirea_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Brom/brom_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Nyx/nyx_idle.png`

Monsters:

- Done: `Assets/_Project/Art/RuntimePixel/Monsters/GateImp/gate_imp_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Monsters/OrcBrute/orc_brute_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Monsters/DireWolf/dire_wolf_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Monsters/CaveBat/cave_bat_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Monsters/CoreSlime/core_slime_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Monsters/BoneSoldier/bone_soldier_idle.png`

Bosses:

- Done: `Assets/_Project/Art/RuntimePixel/Bosses/Grumbar/grumbar_idle.png`

## Next Effect and Animation Assets

- idle/attack/hit/death animation strips for all six heroes
- walk/hit/death animation strips for all six monsters and Grumbar
- final 9-slice versions of panel/button/rune card UI
- dedicated boss HP bar frame and fill sprites
