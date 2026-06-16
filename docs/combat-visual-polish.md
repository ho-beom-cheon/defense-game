# Combat Visual Polish v0.6

이 문서는 `codex/v06-combat-visual-polish` 브랜치에서 정리한 BattleScene 시각 기준을 기록한다.

## Scope

- 새 전투 기능 추가가 아니라 기존 BattleScene 흐름의 가독성 개선이 목표다.
- ConceptSheets 이미지는 전투에 직접 쓰지 않는다.
- BattleScene은 `RuntimePixel` 스프라이트를 우선 사용하고, 없으면 `PlaceholderSprite` fallback을 사용한다.
- `Assets/_Project/Resources/RuntimePixelVisualCatalog.asset`가 전투 배경/이펙트/UI sprite를 참조해 빌드에서도 로드될 수 있게 한다.
- 외부 이미지, 외부 패키지, 서버, 로그인, 광고, 결제 SDK는 추가하지 않는다.

## Unit Scale

- 일반 영웅: 1.32~1.4 world units
- 전열 탱커 레온: 1.48 world units
- 큰 영웅/기술자 브롬: 1.6 world units
- 작은 몬스터: 0.95~1.08 world units
- 오크/중형 몬스터: 1.38 world units
- 보스 그룸바르: 2.55 world units

스케일은 Visual child의 `SpriteRenderer` 기준으로만 맞춘다. Root object는 위치 계산과 타겟팅용으로 유지한다.

## Lane and Slot Layout

- Lane y 좌표: `-2.15`, `0`, `2.15`
- Monster spawn x: `5.75`
- Crystal target x: `-5.15`
- Hero Front slot x: `-0.55`
- Hero Middle slot x: `-1.5`
- Hero Back slot x: `-2.45`

Front는 적과 가장 가까운 슬롯이고, Back은 크리스탈 쪽에 가까운 슬롯이다.

## Battlefield Visuals

`LaneManager`가 플레이 중 다음 placeholder visuals를 자동 생성한다.

- `Assets/_Project/Art/RuntimePixel/Backgrounds/bg_goblin_forest_lanes.png` 배경
- 3개 lane ground strip
- lane center line
- crystal ward zone
- spawn rift zone
- hero slot marker

배경 sprite가 로드되지 않으면 낮은 대비 dark placeholder backdrop으로 fallback한다.

## Combat Feedback

- 기본 공격: 공격자 Visual child가 짧게 전진 후 복귀한다.
- 원거리 기본 공격: `fx_rapid_shot.png`를 작은 projectile visual로 우선 사용하고, 없으면 노란 placeholder projectile을 사용한다.
- 피격: 기존 `HitFlashController`와 `DamageText`를 유지하고 `fx_hit_spark.png`를 표시한다.
- 사망: `fx_death_puff.png`를 표시하고, 몬스터는 즉시 삭제되지 않고 짧게 scale down/fade out 후 삭제된다.
- HP bar: 몬스터 종류별 크기와 RuntimePixel target height를 기준으로 머리 위에 배치한다.

## Skill Feedback

현재 스킬별 placeholder effect:

- Leon Shield Bash: `fx_shield_bash.png`
- Seria Rapid Shot: `fx_rapid_shot.png`
- Kael Meteor: `fx_meteor_impact.png` + 낙하 placeholder line
- Mirea Holy Heal: `fx_holy_heal.png`
- Brom Build Turret: `fx_turret_shot.png`
- Nyx Shadow Strike: `fx_shadow_slash.png`

각 sprite가 로드되지 않으면 이전 colored placeholder effect를 사용한다.

## UI Skin

IMGUI 프로토타입 UI에 다음 이미지를 가능한 범위에서 연결했다.

- HUD/skill/rune panel: `ui_panel_dark.png`
- Skill button: `ui_button_skill.png`
- Rune card: `ui_rune_card_base.png`

현재는 Unity IMGUI 기반이므로 9-slice, safe area, 터치용 최종 레이아웃은 다음 단계 TODO다.

## Remaining Asset Needs

- 실제 픽셀 이펙트 스프라이트
- 공격/피격/사망 animation strip
- 보스 전용 HP bar UI
- 모바일 safe area를 고려한 비-IMGUI HUD
