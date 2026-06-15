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

## Current State

현재 BattleScene은 영웅 6인의 RuntimePixel idle 스프라이트를 사용한다. RuntimePixel 스프라이트가 없는 몬스터/보스는 `PlaceholderSprite` fallback을 사용한다. 새 컨셉 이미지는 도감/기록서 방향을 잡는 참고 이미지로만 사용한다.

## Required RuntimePixel Backlog

Heroes:

- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Leon/leon_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Seria/seria_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Kael/kael_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Mirea/mirea_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Brom/brom_idle.png`
- Done: `Assets/_Project/Art/RuntimePixel/Heroes/Nyx/nyx_idle.png`

Monsters:

- `Assets/_Project/Art/RuntimePixel/Monsters/gate_imp_idle.png` 또는 `goblin_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/orc_brute_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/dire_wolf_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/cave_bat_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/core_slime_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/bone_soldier_idle.png`

Bosses:

- `Assets/_Project/Art/RuntimePixel/Bosses/grumbar_idle.png`
