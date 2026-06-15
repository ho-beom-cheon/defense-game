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

## Battle Runtime Visual Rules

- `HeroData.battleSprite`는 `Assets/_Project/Art/RuntimePixel/Heroes` 아래의 작은 전투 스프라이트만 허용한다.
- `MonsterData.runtimeSprite`는 `Assets/_Project/Art/RuntimePixel/Monsters` 또는 `Assets/_Project/Art/RuntimePixel/Bosses` 아래의 작은 전투 스프라이트만 허용한다.
- `HeroData.BattleSprite`는 더 이상 `portrait`로 fallback하지 않는다.
- `MonsterData.Sprite`는 런타임 호환용 getter이며 내부적으로 `runtimeSprite`만 반환한다.
- 큰 컨셉 카드나 기록서 이미지는 `BattleScene` 중앙에 표시하지 않는다.

## Current Scale Targets

- Hero: 기본 1.05 world units, 전열 탱커 1.25, 기술자 1.15
- Small monster: 기본 0.9 world units, 빠른/비행 몬스터 0.8
- Orc/tank monster: 1.35 world units
- Boss: 2.25 world units

`RuntimeSpriteFitter`는 전투용 Visual child의 `SpriteRenderer.bounds`를 기준으로 높이를 보정한다. Root object는 라인 이동과 타겟팅 위치 계산에 사용하므로 scale을 직접 변경하지 않는다.

## Do Not Do

- 컨셉 카드 이미지를 BattleScene의 작은 유닛 스프라이트로 강제 연결하지 않는다.
- `Assets/_Project/Art/ConceptSheets` 또는 기존 큰 `Art/Characters` 이미지를 `battleSprite`/`runtimeSprite`로 연결하지 않는다.
- 외부 이미지 다운로드, 유료 에셋, SDK, Addressables를 추가하지 않는다.
- 전투 시스템을 새 아트 때문에 대규모 리팩토링하지 않는다.
