# RuneGate Defense Hero Character Bible

이 문서는 한국 출시 우선 빌드에서 사용하는 문지기 기록 기준 영웅 설정이다. 기존 영어 `displayName`은 내부 호환용으로 유지하되, 신규 도감/기록 UI는 `displayNameKorean`, `subtitleKorean`, `descriptionKorean`, `quoteKorean`을 우선 사용한다.

## Concept Reference Policy

아래 이미지는 전투 런타임 스프라이트가 아니라 문지기 기록 참고 이미지다.

- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_레온의_기록_카드.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/문지기_기록_바람길을_읽는_궁수.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/카엘_잿불의_방랑_마법사.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/미레아_금빛_성서의_사제.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/브롬_룬포지_기술자_카드.png`
- `Assets/_Project/Art/ConceptSheets/Heroes/그림자_균열의_암살자_카드.png`

## Hero Roster

| Asset | 한국어 이름 | 별칭 | 역할 | 대표 대사 |
| --- | --- | --- | --- | --- |
| `Knight.asset` | 레온 | 균열 방패의 기사 | 전열 / 방어 / 빛 | "이번에는, 절대 무너지지 않는다." |
| `Archer.asset` | 세리아 | 바람길을 읽는 궁수 | 후열 / 원거리 / 바람 | "바람은 이미 답을 알고 있어." |
| `Fire Mage.asset` | 카엘 | 잿불의 방랑 마법사 | 후열 / 광역 / 화염 | "내가 나서는 건 이번뿐이다. …살아남아라." |
| `Priest.asset` | 미레아 | 금빛 성서의 사제 | 중열 / 지원 / 빛 | 추후 작성 |
| `Dwarf Engineer.asset` | 브롬 | 룬포지 기술자 | 중열 / 설치 / 기계 | 추후 작성 |
| `Shadow Assassin.asset` | 닉스 | 그림자 균열의 암살자 | 전열 또는 중열 / 암살 / 어둠 | 추후 작성 |

## Direction

- 카드 UI라는 표현보다 `문지기 기록`, `전술 기록서`, `봉문 보고서`를 우선 사용한다.
- 서구 하이판타지 직업명은 내부 개발 편의를 위한 보조 명칭으로만 둔다.
- 캐릭터 실루엣은 모바일 전투에서 작게 읽히는 픽셀 스프라이트로 별도 제작한다.
- 컨셉 이미지의 장식, 복식, 상징은 RuntimePixel 제작 시 축약하여 반영한다.

## Future Codex UI Hooks

- `HeroCodex`는 문지기 기록을 보여주는 도감 UI로 확장한다.
- 리스트 제목은 `영웅`, `카드`보다 `문지기 기록`을 우선한다.
- 상세 화면은 이름, 별칭, 역할, 대표 대사, 봉문 기록 순서로 배치한다.
