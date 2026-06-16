# RuneGate Defense Art Integration Notes

## Current Integration Decision

이번 작업은 한국 출시 우선 아트 정체성을 정리하는 단계다. 이미 프로젝트에 들어온 컨셉 이미지는 전투용 스프라이트가 아니라 기록서형 참고 이미지로 분류한다.

## Concept Reference Assets

### Heroes

- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_레온의_기록_카드.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_기록_바람길을_읽는_궁수.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/카엘_잿불의_방랑_마법사.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/미레아_금빛_성서의_사제.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/브롬_룬포지_기술자_카드.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/그림자_균열의_암살자_카드.png`

### Enemies and Boss

- `Assets/_Project/Art/ConceptSheets/Enemies/균열_적성_기록_괴물_수집가.png`
- `Assets/_Project/Art/ConceptSheets/Enemies/문파괴자_그룸바르의_위협_기록.png`

## Data Changes

`HeroData`, `MonsterData`, `StageData`에 한국어 표시용 필드를 추가했다.

- `displayNameKorean`
- `subtitleKorean`
- `descriptionKorean`
- `quoteKorean`은 `HeroData` 전용

기존 `displayName`은 파일명, 내부 개발 호환, 기존 로직 보존을 위해 유지한다.

## Runtime Separation

- Concept reference: `Assets/_Project/Art/ConceptSheets`
- Runtime battle sprite: `Assets/_Project/Art/RuntimePixel`
- Current battle fallback: small colored placeholder sprites generated at runtime

`ConceptSheets` 이미지는 `HeroData.conceptImage`, `HeroData.portrait`, `MonsterData.conceptImage` 같은 도감/설정용 필드에만 연결한다. `BattleScene`의 `SpriteRenderer`와 `HeroData.battleSprite`, `MonsterData.runtimeSprite`에는 직접 연결하지 않는다.

`RuntimePixel` 스프라이트가 없으면 전투는 실패하지 않고 `PlaceholderSprite` fallback을 사용한다. fallback 색상은 캐릭터 역할과 몬스터 타입에 맞춰 구분한다.

전투 배경/이펙트/UI sprite는 `Assets/_Project/Resources/RuntimePixelVisualCatalog.asset`에도 참조를 보관한다. 이 catalog는 Editor Play와 빌드 후보에서 같은 RuntimePixel 이미지를 로드하기 위한 최소 연결 지점이다.

## Battle Runtime Visual Rules

- `HeroData.battleSprite`는 `Assets/_Project/Art/RuntimePixel/Heroes` 아래의 작은 전투 스프라이트만 허용한다.
- `MonsterData.runtimeSprite`는 `Assets/_Project/Art/RuntimePixel/Monsters` 또는 `Assets/_Project/Art/RuntimePixel/Bosses` 아래의 작은 전투 스프라이트만 허용한다.
- `HeroData.BattleSprite`는 더 이상 `portrait`로 fallback하지 않는다.
- `MonsterData.Sprite`는 런타임 호환용 getter이며 내부적으로 `runtimeSprite`만 반환한다.
- 큰 컨셉 카드나 기록서 이미지는 `BattleScene` 중앙에 표시하지 않는다.

## Current Scale Targets

- Hero: 기본 1.32~1.4 world units
- Leon 같은 전열 탱커: 1.48 world units
- Brom 같은 큰 영웅/기술자: 1.6 world units
- Small monster: 0.95~1.08 world units
- Orc/tank monster: 1.38 world units
- Boss: 2.55 world units

`RuntimeSpriteFitter`는 전투용 Visual child의 `SpriteRenderer.bounds`를 기준으로 높이를 보정한다. Root object는 라인 이동과 타겟팅 위치 계산에 사용하므로 scale을 직접 변경하지 않는다.

`RuntimeSpriteFitter`는 스프라이트가 바뀌거나 목표 높이가 바뀔 때만 다시 맞춘다. 공격/스킬/사망 피드백은 Visual child의 local position/scale을 짧게 조정하므로, 매 프레임 fit으로 덮지 않는다.

## Combat Visual Polish v0.6

- `LaneManager`가 플레이 중 `Assets/_Project/Art/RuntimePixel/Backgrounds/bg_goblin_forest_lanes.png` 배경, 3개 lane ground strip, crystal ward zone, spawn rift zone, hero slot marker를 자동 생성한다.
- Hero slot 좌표는 lane별 Front/Middle/Back을 분리한다. Front는 적 출현 쪽에 가까운 `x=-0.55`, Middle은 `x=-1.5`, Back은 `x=-2.45` 기준이다.
- Monster path는 `spawnX=5.75`에서 `crystalX=-5.15`로 이동한다.
- 몬스터 HP bar는 RuntimePixel target height를 기준으로 머리 위에 배치한다.
- 공격 시 영웅 Visual child가 짧게 전진 후 복귀한다.
- 원거리 기본 공격은 `fx_rapid_shot.png`를 우선 projectile visual로 사용한다.
- 피격 시 기존 HitFlash와 DamageText를 사용하고, `fx_hit_spark.png`를 표시한다.
- 사망 시 `fx_death_puff.png`를 표시하고 몬스터는 즉시 삭제되지 않고 짧게 축소/페이드 후 삭제된다.
- 스킬 사용 시 `fx_shield_bash.png`, `fx_rapid_shot.png`, `fx_meteor_impact.png`, `fx_holy_heal.png`, `fx_turret_shot.png`, `fx_shadow_slash.png`를 우선 표시한다.
- IMGUI 패널/스킬 버튼/룬 카드에는 `ui_panel_dark.png`, `ui_button_skill.png`, `ui_rune_card_base.png`를 가능한 범위에서 적용한다.

## Do Not Do

- 컨셉 카드 이미지를 BattleScene의 작은 유닛 스프라이트로 강제 연결하지 않는다.
- `Assets/_Project/Art/ConceptSheets` 또는 기존 큰 `Art/Characters` 이미지를 `battleSprite`/`runtimeSprite`로 연결하지 않는다.
- 외부 이미지 다운로드, 유료 에셋, SDK, Addressables를 추가하지 않는다.
- 전투 시스템을 새 아트 때문에 대규모 리팩토링하지 않는다.
