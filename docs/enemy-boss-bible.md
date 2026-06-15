# RuneGate Defense Enemy and Boss Bible

이 문서는 한국 출시 우선 빌드에서 사용하는 봉문 위험 기록과 균열 적성 기록 기준 적/보스 설정이다.

## Concept Reference Policy

아래 이미지는 전투 런타임 스프라이트가 아니라 적성 기록 참고 이미지다.

- `Assets/_Project/Art/ConceptSheets/Enemies/균열_적성_기록_괴물_수집가.png`
- `Assets/_Project/Art/ConceptSheets/Enemies/문파괴자_그룸바르의_위협_기록.png`

## Enemy Naming Priority

기존 `Goblin`, `Orc`, `Wolf`, `Bat` 같은 이름은 개발 호환용 보조 명칭으로 둔다. 한국 출시용 문서, 도감, UI, 스토어 텍스트에서는 아래 명칭을 우선한다.

| Asset | 한국어 이름 | 분류 | 설명 |
| --- | --- | --- | --- |
| `Goblin.asset` | 문틈 도깨비 | 균열 하급 적성 | 문이 덜 닫힌 틈에서 가장 먼저 새어 나오는 소형 적성체 |
| `Bat.asset` | 균열 꼬마귀 | 비행 정찰 적성 | 균열 위를 낮게 날며 문지기 진형을 흔드는 작은 날개 적성체 |
| `Wolf.asset` | 부식 늑대 | 봉문 부식형 추적체 | 봉문 결계를 갉아먹는 부식 기운을 두른 빠른 추적체 |
| `Orc.asset` | 봉문 파쇄자 | 중형 파쇄 적성 | 오크풍 체형은 낮은 우선순위이며, 봉문 파쇄 성격을 우선한다 |
| `Slime.asset` | 재문 점액 | 잔류 균열 응집체 | 닫힌 문 주변에 남은 균열 찌꺼기가 뭉쳐 움직이는 적성 |
| `Skeleton.asset` | 균열 잔해병 | 기록 오염 잔재 | 오래된 전장의 기록이 균열에 오염되어 걸어 나온 잔해 병사 |

## Boss

| Asset | 한국어 이름 | 별칭 | 설명 |
| --- | --- | --- | --- |
| `Orc Warlord.asset` | 그룸바르 | 문파괴자 | 봉문 위험 기록 최상위 개체. 재문을 직접 찢고 진입하는 대형 파쇄 적성 |

## Future Codex UI Hooks

- `EnemyCodex`는 `균열 적성 기록`으로 표시한다.
- `BossCodex`는 `봉문 위험 기록`으로 표시한다.
- 보스 상세 화면은 위험 등급, 봉문 피해 기록, 대응 전술, 출현 스테이지를 우선한다.
