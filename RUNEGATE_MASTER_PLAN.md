# RuneGate Defense Master Plan

이 파일은 `docs/` 폴더의 핵심 내용을 요약한 단일 진입점이다. Codex가 프로젝트 맥락을 빠르게 파악해야 할 때 이 파일을 먼저 읽고, 세부 구현 전에는 반드시 `docs/`의 관련 문서를 함께 읽는다.

## 1. 게임 정의

- 게임명: RuneGate Defense
- 장르: 세로형 2D 로그라이트 판타지 디펜스
- 엔진: Unity 6
- 언어: C#
- 플랫폼: Android 우선
- 서버: MVP 없음
- 수익화: MVP 없음. 이후 광고 제거, 영웅팩, 월드팩, 스킨팩 검토

## 2. 현재 상태

현재 구현 기준은 Battle Prototype v0.2 이후, v0.3 Progression Loop 단계다.

- BattleScene 실행 가능
- 3라인 전투
- Crystal HP / Wave / State / Gold 표시
- Knight/Archer 자동 공격과 수동 스킬
- Goblin/Orc 웨이브 스폰
- 웨이브 사이 룬 3택1
- Victory/Defeat 결과 흐름
- Title / Stage Select / Upgrade / Save 루프 준비

## 3. 다음 목표

다음 목표는 v0.4 콘텐츠/배치 준비다.

핵심 목표:

```text
Hero placement preparation
→ 6 MVP heroes
→ 6 MVP monsters
→ Boss wave
→ First art sample
→ Audio placeholders
```

## 4. MVP 제한사항

MVP에는 다음을 넣지 않는다.

- 서버
- 로그인
- 가챠
- 광고
- 인앱결제
- Analytics
- Firebase
- Addressables
- 외부 유료 API
- 멀티플레이
- 랭킹
- 클라우드 저장
- 대량 일러스트 적용

## 5. 코드 구조 원칙

- namespace: `RuneGate`
- ScriptableObject 기반 데이터 관리
- MonoBehaviour는 작고 명확하게 유지
- 전투 로직과 UI 직접 결합 최소화
- Null guard 필수
- 하드코딩 최소화
- 외부 패키지 추가 금지

## 6. 핵심 문서 목록

- `docs/00_PROJECT_CONTEXT.md`: 전체 컨텍스트
- `docs/01_GAME_DESIGN_DOCUMENT.md`: 게임 디자인
- `docs/02_DEVELOPMENT_ROADMAP.md`: 단계별 로드맵
- `docs/03_CODEX_IMPLEMENTATION_GUIDE.md`: Codex 작업 규칙
- `docs/04_SYSTEM_ARCHITECTURE.md`: 시스템 구조
- `docs/05_CONTENT_AND_BALANCE_SPEC.md`: 콘텐츠/밸런스
- `docs/06_UI_UX_SPEC.md`: UI/UX
- `docs/07_ART_AUDIO_PIPELINE.md`: 아트/오디오 파이프라인
- `docs/08_TEST_AND_QA_PLAN.md`: 테스트 계획
- `docs/09_BACKLOG.md`: 백로그
- `docs/10_NEXT_CODEX_PROMPTS.md`: 다음 Codex 프롬프트

## 7. Codex 기본 지시

Codex는 구현 전 반드시 관련 md 문서를 읽고 작업한다. 대화 맥락에 의존하지 않는다.

다음 작업은 `docs/10_NEXT_CODEX_PROMPTS.md`의 `v0.4 콘텐츠/배치 준비 프롬프트`를 사용한다.
