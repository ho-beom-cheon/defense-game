# 아트 및 오디오 파이프라인

## 1. 기본 전략

일러스트를 전부 마지막에 한 번에 넣지 않는다. 다음 순서로 진행한다.

```text
1. Placeholder로 전투 로직 완성
2. 아트 규격/폴더/파일명 확정
3. Knight + Goblin 샘플 적용
4. 파이프라인 검증
5. 영웅/몬스터 확장 아트 적용
6. UI/이펙트/사운드 폴리싱
```

## 2. 현재 단계

현재는 placeholder 네모 오브젝트로 충분하다. v0.2에서는 아트를 추가하지 않는다.

v0.7부터 대표 아트 적용을 시작한다.

## 3. 폴더 구조

```text
Assets/_Project/Art/
  Characters/
    Heroes/
      Knight/
      Archer/
      Mage/
      Priest/
      Engineer/
      Assassin/
    Monsters/
      Goblin/
      Orc/
      Wolf/
      Bat/
      Slime/
      Skeleton/
    Bosses/
      OrcWarlord/
  Effects/
    Skills/
    Hit/
    Death/
  UI/
    Icons/
    Buttons/
    Panels/
    Runes/
  Backgrounds/
Audio/
  BGM/
  SFX/
```

## 4. 파일명 규칙

### 4.1 영웅

```text
hero_knight_idle.png
hero_knight_attack.png
hero_knight_hit.png
hero_knight_skill.png
hero_knight_death.png

hero_archer_idle.png
hero_archer_attack.png
hero_archer_hit.png
hero_archer_skill.png
hero_archer_death.png
```

### 4.2 몬스터

```text
monster_goblin_walk.png
monster_goblin_hit.png
monster_goblin_death.png

monster_orc_walk.png
monster_orc_hit.png
monster_orc_death.png
```

### 4.3 아이콘

```text
icon_skill_shield_bash.png
icon_skill_rapid_shot.png
icon_rune_sword.png
icon_rune_bow.png
icon_rune_healing.png
```

### 4.4 배경/UI

```text
bg_goblin_forest_01.png
ui_panel_basic.png
ui_button_basic.png
ui_rune_card_basic.png
```

## 5. 권장 스프라이트 크기

| 자산 | 권장 크기 |
|---|---:|
| 영웅 단일 프레임 | 256x256 |
| 몬스터 단일 프레임 | 192x192 |
| 보스 단일 프레임 | 384x384 |
| 스킬 아이콘 | 128x128 |
| 룬 아이콘 | 128x128 |
| 룬 카드 배경 | 512x768 |
| 배경 | 1080x1920 또는 1080x1600 |
| UI 버튼 | 512x160 |

## 6. 애니메이션 방식

MVP 이후 첫 아트 적용은 Sprite Sheet 방식으로 한다.

### 6.1 영웅 애니메이션

| 애니메이션 | 프레임 수 |
|---|---:|
| Idle | 4 |
| Attack | 6 |
| Hit | 2 |
| Skill | 6~8 |
| Death | 4~6 |

### 6.2 몬스터 애니메이션

| 애니메이션 | 프레임 수 |
|---|---:|
| Walk | 4~6 |
| Hit | 2 |
| Death | 4 |

## 7. Unity Import Settings 권장

| 항목 | 값 |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Multiple 또는 Single |
| Pixels Per Unit | 100 기준, 프로젝트에서 통일 |
| Filter Mode | Bilinear 또는 Point, 아트 스타일에 따라 통일 |
| Compression | Normal Quality 또는 None, 모바일 테스트 후 조정 |
| Max Size | 1024 또는 2048 |

## 8. Pivot 규칙

- 영웅/몬스터는 발 위치 기준 하단 중앙 pivot 권장
- 체력바 위치는 sprite bounds 기준 상단에 배치
- Projectile spawn point는 캐릭터 중심 또는 무기 위치 기준으로 후속 조정

## 9. 첫 샘플 적용 대상

v0.7에서 먼저 적용할 대상:

- Knight idle/attack/hit/death
- Archer idle/attack/hit/death
- Goblin walk/hit/death
- Orc walk/hit/death
- Sword Rune icon
- Bow Rune icon
- Healing Rune icon
- 기본 배경 1장

## 10. 사운드 계획

MVP에서는 사운드 없어도 된다. v0.8부터 적용한다.

### 10.1 SFX 목록

| 효과음 | 용도 |
|---|---|
| hit_basic | 일반 피격 |
| skill_shield_bash | Knight 스킬 |
| skill_rapid_shot | Archer 스킬 |
| monster_death | 몬스터 사망 |
| rune_select | 룬 선택 |
| crystal_hit | 수정 피격 |
| victory | 승리 |
| defeat | 패배 |

### 10.2 BGM

- Battle BGM 1곡
- Result 짧은 jingle

### 10.3 v0.89 런타임 폴백 정책

- 최종 SFX 파일이 없는 동안 `ProceduralSfxFactory`가 9개 `SfxKey`의 임시 클립을 생성한다.
- 실제 WAV 클립이 연결되면 같은 키의 절차형 폴백보다 우선한다.
- 절차형 클립은 최종 에셋이 아니며 릴리스 전 음량 정규화와 실기기 청취 검증이 필요하다.
- 현재 BGM은 구현되지 않았고 TitleScene에는 `준비 중`으로 표시한다.

## 11. Git LFS 규칙

다음 파일은 Git LFS로 관리한다.

```text
*.psd
*.psb
*.png
*.jpg
*.jpeg
*.wav
*.mp3
*.ogg
*.fbx
```

## 12. 아트 적용 시 주의사항

- 대량 아트 추가 전 반드시 1개 영웅/1개 몬스터로 테스트
- 플레이 화면에서 크기 확인
- HP Bar 위치 확인
- 애니메이션 속도 확인
- 모바일 화면에서 너무 작거나 흐릿하지 않은지 확인
- 빌드 용량 증가 확인

## 13. 하지 말 것

- v0.2 단계에서 아트 대량 적용 금지
- AI 생성 이미지를 무분별하게 저장소에 추가 금지
- 출처 불명 유료/상업 제한 에셋 추가 금지
- 애니메이션 시스템을 복잡하게 만들지 말 것
- Live2D, Spine 같은 외부 런타임은 MVP에서 제외
