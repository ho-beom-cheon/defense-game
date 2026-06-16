# RuneGate Defense Hero Character Bible

이 문서는 한국 출시 우선 빌드에서 사용하는 문지기 기록 기준 영웅 설정과 v0.7 전투 데이터를 정리한다. 기존 영어 `displayName`은 개발 호환용으로 유지하고, UI/도감/설정 문서에서는 `displayNameKorean`, `subtitleKorean`, `descriptionKorean`, `quoteKorean`을 우선 사용한다.

## Concept Reference Policy

아래 이미지는 BattleScene 런타임 스프라이트가 아니라 문지기 기록/도감/설정 참고 이미지다.

- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_레온의_기록_카드.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_기록_바람길을_읽는_궁수.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/카엘_잿불의_방랑_마법사.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/미레아_금빛_성서의_사제.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/브롬_룬포지_기술자_카드.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/그림자_균열의_암살자_카드.png`

BattleScene에서는 `Assets/_Project/Art/RuntimePixel/Heroes/...` 아래의 작은 전투용 스프라이트를 우선 사용하고, 없으면 placeholder fallback을 사용한다.

## Hero Roster

| Asset | 한국어 이름 | 별칭 | 역할 | 대표 대사 |
| --- | --- | --- | --- | --- |
| `Knight.asset` | 레온 | 균열 방패의 기사 | 전열 / 방어 / 빛 | "이번에는, 절대 무너지지 않는다." |
| `Archer.asset` | 세리아 | 바람길을 읽는 궁수 | 후열 / 원거리 / 바람 | "바람은 이미 답을 알고 있어." |
| `Fire Mage.asset` | 카엘 | 잿불의 방랑 마법사 | 후열 / 광역 / 화염 | "내가 나서는 건 이번뿐이다. …살아남아라." |
| `Priest.asset` | 미레아 | 금빛 성서의 사제 | 중열 / 지원 / 빛 | 추후 작성 |
| `Dwarf Engineer.asset` | 브롬 | 룬포지 기술자 | 중열 / 설치 / 기계 | 추후 작성 |
| `Shadow Assassin.asset` | 닉스 | 그림자 균열의 암살자 | 전열 또는 중열 / 암살 / 어둠 | 추후 작성 |

## v0.7 Combat Data

| 영웅 | 스킬 | HP | 공격 | 공격 속도 | 사거리 | 전투 의도 |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| 레온 | Shield Bash | 520 | 28 | 0.95 | 1.25 | 라인 고정, 생존, 크리스탈 방어 |
| 세리아 | Rapid Shot | 190 | 32 | 1.75 | 4.8 | 빠른 적 우선 공격, 연속 사격 |
| 카엘 | Meteor | 165 | 60 | 0.65 | 3.8 | 범위 피해, 다수 몬스터 대응 |
| 미레아 | Holy Heal | 220 | 14 | 1.0 | 3.5 | 아군/크리스탈 보조 회복 |
| 브롬 | Temporary Turret | 260 | 24 | 1.0 | 3.0 | 임시 포탑 hook, 라인 보강 |
| 닉스 | Shadow Strike | 230 | 86 | 0.75 | 1.75 | HP 높은 적, 네임드, 보스 우선 폭딜 |

## UI Direction

- "카드 UI"보다 `문지기 기록`, `전술 기록서`, `봉문 보고서` 표현을 우선 사용한다.
- `HeroCodex`는 문지기 기록을 보여주는 도감 UI로 확장한다.
- 전투 유닛은 RuntimePixel 스프라이트 기준으로 별도 제작한다.
