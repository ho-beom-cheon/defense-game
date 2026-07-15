# RuneGate Defense Enemy and Boss Bible

이 문서는 한국 출시 우선 빌드에서 사용하는 봉문 위험 기록과 균열 적성 기록 기준 적/보스 설정을 정리한다.

## Concept Reference Policy

아래 이미지는 BattleScene 런타임 스프라이트가 아니라 도감/설정 참고 이미지다.

- `Assets/_Project/Art/ConceptSheets/Enemies/균열_적성_기록_괴물_수집가.png`
- `Assets/_Project/Art/ConceptSheets/Enemies/문파괴자_그룸바르의_위협_기록.png`

BattleScene에서는 `Assets/_Project/Art/RuntimePixel/Monsters`와 `Assets/_Project/Art/RuntimePixel/Bosses`의 작은 전투용 스프라이트를 우선 사용하고, 없으면 placeholder fallback을 사용한다.

## Enemy Naming Priority

기존 `Goblin`, `Orc`, `Wolf`, `Bat`, `Slime`, `Skeleton` 이름은 개발 호환용 보조 명칭이다. 한국 출시용 문서, UI, 도감, 스토리 텍스트에서는 아래 명칭을 우선한다.

| Asset | 한국어 이름 | 분류 | 설명 |
| --- | --- | --- | --- |
| `Goblin.asset` | 문틈 도깨비 | 기본형 / 빠른 다수 몬스터 | 문이 덜 닫힌 틈에서 가장 먼저 새어 나오는 소형 적성체. |
| `Orc.asset` | 재갑 돌격병 | 탱커형 / 높은 HP | 재문 갑각을 두르고 봉문을 정면으로 밀어붙이는 중형 돌격 적성. |
| `Wolf.asset` | 부식 늑대 | 속도형 / 빠른 라인 돌파 | 봉문 결계를 갉아먹는 부식 기운을 두른 빠른 추적체. |
| `Bat.asset` | 균열 까마귀 | 비행/침투형 / 빠른 이동 | 균열 위를 낮게 날며 문지기 진형을 흔드는 작은 날개 적성체. |
| `Slime.asset` | 룬핵 점액 | 분열형 / 사망 hook | 닫힌 문 주변에 남은 룬핵 찌꺼기가 뭉쳐 움직이는 점액형 적성. |
| `Skeleton.asset` | 망각의 뼈병 | 언데드형 / 부활 hook | 오래된 전장의 기록이 균열에 오염되어 걸어 나온 뼈 병사. |

## v0.7 Combat Data

| 적 | HP | 이동 속도 | 크리스탈 피해 | 골드 | 의도 |
| --- | ---: | ---: | ---: | ---: | --- |
| 문틈 도깨비 | 58 | 1.45 | 1 | 5 | 빠른 다수 기본 적 |
| 재갑 돌격병 | 255 | 0.78 | 2 | 14 | 전열 압박 |
| 부식 늑대 | 72 | 2.05 | 1 | 7 | 세리아 가치 체감 |
| 균열 까마귀 | 64 | 2.15 | 1 | 8 | 빠른 타겟 대응 |
| 룬핵 점액 | 155 | 0.82 | 1 | 11 | 광역 피해 필요성 예고 |
| 망각의 뼈병 | 135 | 1.05 | 1 | 12 | 지속전과 정화 hook |

## Boss

| Asset | 한국어 이름 | 별칭 | HP | 이동 속도 | 크리스탈 피해 | 골드 |
| --- | --- | --- | ---: | ---: | ---: | ---: |
| `Orc Warlord.asset` | 그룸바르 | 문파괴자 | 1750 | 0.48 | 6 | 360 |

그룸바르는 Stage 10에서 등장하는 첫 보스다. 체력 65%와 30%에서 페이즈가 상승하며 이동 속도, 공격 빈도, 공격 피해가 강화된다. 각 페이즈 전환에는 문틈 도깨비 지원군이 여러 라인에 등장한다. 보스 전용 광역 공격과 추가 소환 패턴은 이후 확장 대상으로 남긴다.

## Future Codex UI Hooks

- `EnemyCodex`는 `균열 적성 기록`으로 표시한다.
- `BossCodex`는 `봉문 위험 기록`으로 표시한다.
- 보스 상세 화면은 위험 등급, 봉문 피해 기록, 대응 전술, 출현 스테이지를 우선한다.
