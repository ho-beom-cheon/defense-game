# RuneGate Defense — Next Task Package

이 문서 묶음은 Codex가 이전 대화 맥락 없이도 다음 개발을 이어갈 수 있도록 만든 **작업 지시서 패키지**입니다.

현재 기준:

- 게임명: `RuneGate Defense`
- 엔진: Unity 6
- 언어: C#
- 형태: 세로형 2D 로그라이트 판타지 디펜스
- 현재 구현 상태: `v0.2 Playable Battle Prototype`
- 다음 작업: `v0.3 Progression Loop`
- 그 다음 작업: `v0.4 Content Expansion + Placement Foundation + First Art Pipeline Sample`

## 파일 구성

| 파일 | 용도 |
|---|---|
| `11_TASK_V03_PROGRESSION_LOOP.md` | 다음 작업. 타이틀 → 스테이지 선택 → 전투 → 결과 → 업그레이드 → 저장 루프 구현 |
| `12_TASK_V04_CONTENT_EXPANSION_AND_PLACEMENT.md` | 그 다음 작업. 영웅/몬스터/룬/스테이지 확장, 배치 구조, 첫 아트 샘플 준비 |
| `13_CODEX_HANDOFF_PROMPTS.md` | Codex에 바로 붙여넣을 수 있는 실행 프롬프트 모음 |
| `14_ACCEPTANCE_CHECKLIST.md` | v0.3/v0.4 완료 판정 체크리스트 |

## 권장 적용 순서

```powershell
cd C:\workspace\defense-game

git switch main
git pull

git switch -c codex/progression-v03
```

1. `11_TASK_V03_PROGRESSION_LOOP.md` 내용을 Codex에 읽게 한다.
2. v0.3 구현이 끝나면 Unity에서 수동 검증한다.
3. 정상 동작하면 커밋/push/PR을 만든다.
4. main에 머지한 뒤 새 브랜치를 만든다.

```powershell
git switch main
git pull
git switch -c codex/content-v04
```

5. `12_TASK_V04_CONTENT_EXPANSION_AND_PLACEMENT.md` 내용을 Codex에 읽게 한다.
6. v0.4를 구현하고 Unity에서 검증한다.

## 원칙

이 단계에서는 아직 다음을 하지 않는다.

- 서버
- 로그인
- 가챠
- 광고
- 인앱결제
- 분석 SDK
- Firebase
- Addressables
- 멀티플레이
- 랭킹
- 클라우드 세이브
- 최종 일러스트 대량 적용

핵심은 **게임 루프 안정화 → 콘텐츠 구조 확장 → 아트 적용 파이프라인 확보** 순서다.
