# RuneGate Defense 개발 로드맵

## 1. 개발 단계 요약

| 단계 | 목표 | 결과물 |
|---|---|---|
| v0.1 | 최초 전투 프로토타입 | 3라인, 몬스터 이동, HUD, 기본 영웅/몬스터 |
| v0.2 | 전투 루프 완성 | 웨이브 → 룬 선택 → 다음 웨이브 → 승패 → 재시작 |
| v0.3 | 진행 루프 | Title, Stage Select, Battle Result, Upgrade, 로컬 저장 |
| v0.4 | 콘텐츠/배치 준비 | 영웅 6종 준비, 몬스터 6종 준비, 보스 웨이브, 첫 아트/오디오 샘플 |
| v0.5 | 스테이지 구조 확장 | 월드1 스테이지 10개, 스테이지 선택 개선 |
| v0.6 | UX/튜토리얼 | 안내, 버튼, 결과 화면 개선 |
| v0.7 | 아트 파이프라인 | Knight/Goblin 샘플 일러스트 적용 |
| v0.8 | 폴리싱 | 사운드, 이펙트, 밸런스, 성능 |
| v0.9 | Android 테스트 빌드 | 실제 기기 APK 테스트 |
| v1.0 | MVP 출시 후보 | Google Play 내부 테스트 가능 상태 |

## 2. v0.2 상세 계획: Battle Loop 완성

### 2.1 목표

전투 한 판이 처음부터 끝까지 자연스럽게 진행되어야 한다.

```text
전투 시작
→ Wave 1
→ 몬스터 처치
→ 룬 선택
→ Wave 2
→ 최종 클리어
→ Victory
→ Restart
```

패배 흐름도 동작해야 한다.

```text
전투 시작
→ 몬스터가 수정 도달
→ Crystal HP 0
→ Defeat
→ Restart
```

### 2.2 구현 범위

- 웨이브 클리어 감지
- `BattleState.RuneSelection` 전환
- 룬 선택 UI 표시
- 룬 효과 적용
- 다음 웨이브 시작
- Victory/Defeat 처리
- Result UI
- Restart 버튼
- 몬스터 HP Bar
- 피격 Flash
- Crystal 피격 피드백
- 스킬 쿨타임 표시

### 2.3 완료 기준

| 기준 | 설명 |
|---|---|
| Wave Clear | 살아있는 몬스터와 남은 스폰이 없으면 웨이브 종료 |
| Rune Selection | 비최종 웨이브 종료 시 카드 3장 표시 |
| Rune Apply | 선택한 룬이 즉시 효과 적용 |
| Next Wave | 룬 선택 후 다음 웨이브 시작 |
| Victory | 최종 웨이브 클리어 시 승리 화면 |
| Defeat | Crystal HP 0 시 패배 화면 |
| Restart | 버튼 클릭 시 전투 초기화 |

## 3. v0.3 상세 계획: 진행 루프

### 3.1 목표

단독 전투 장면을 실제 게임 셸로 연결한다.

```text
Title
→ Stage Select
→ Battle
→ Result
→ Upgrade
→ Save
→ Stage Select
```

### 3.2 구현 기능

| 기능 | 설명 |
|---|---|
| TitleScene | Start, Continue, Reset Save |
| StageSelectScene | Stage 1~3 표시, 해금/잠금 처리 |
| BattleScene 연동 | GameSession의 선택 StageData 사용, 없으면 Stage 1 fallback |
| Result flow | Victory/Defeat, 획득 골드, Retry/Upgrade/Stage Select |
| UpgradeScene | 골드로 영구 업그레이드 구매 |
| SaveManager | `Application.persistentDataPath` JSON 저장 |

### 3.3 업그레이드

| 업그레이드 | 효과 |
|---|---|
| Crystal Reinforcement | `crystal_max_hp_flat` |
| Hero Training | `hero_attack_percent` |
| Battle Rhythm | `hero_attack_speed_percent` |
| Skill Practice | `skill_cooldown_percent` |

## 4. v0.4 상세 계획: 콘텐츠/배치 준비

### 4.1 목표

진행 루프 위에 콘텐츠 확장과 배치 시스템의 기반을 준비한다.

### 4.2 구현 기능

- Hero placement preparation
- MVP 영웅 6종 준비: Knight, Archer, Mage, Priest, Engineer, Assassin
- MVP 몬스터 6종 준비: Goblin, Orc, Wolf, Bat, Slime, Skeleton
- Orc Warlord 보스 웨이브
- 첫 아트 통합 샘플: Knight 또는 Goblin
- 오디오 placeholder: hit, skill, victory, defeat

## 5. v0.5 상세 계획: 스테이지 구조

### 5.1 목표

월드1 고블린 숲의 스테이지 10개를 만든다.

### 5.2 스테이지 구성

| 스테이지 | 내용 |
|---|---|
| 1 | Goblin 기본 |
| 2 | Goblin 수량 증가 |
| 3 | Orc 등장 |
| 4 | 라인 혼합 스폰 |
| 5 | 중간 난이도 테스트 |
| 6 | 빠른 몬스터 도입 후보 |
| 7 | Tank 몬스터 다수 |
| 8 | 다중 라인 압박 |
| 9 | 보스 전 고난도 |
| 10 | Orc Warlord 보스 |

## 6. v0.6 상세 계획: UX/튜토리얼

### 6.1 목표

처음 플레이하는 사람이 룰을 이해하게 한다.

### 6.2 구현 기능

- 첫 실행 튜토리얼 팝업
- 스킬 버튼 설명
- 룬 카드 설명
- Crystal HP 설명
- Result 화면 보상 설명

## 7. v0.7 상세 계획: 아트 파이프라인

### 7.1 목표

대표 캐릭터와 몬스터에 실제 일러스트를 적용해 파이프라인을 검증한다.

### 7.2 대상

- Knight
- Archer
- Goblin
- Orc
- Sword Rune Icon
- Bow Rune Icon
- Healing Rune Icon

### 7.3 확인 사항

- Sprite 크기
- Pivot 위치
- HP Bar 위치
- 애니메이션 Clip 전환
- 모바일 화면 가독성

## 8. v0.8 상세 계획: 폴리싱

- Hit SFX
- Skill SFX
- Victory/Defeat SFX
- 간단한 Particle Effect
- 화면 흔들림 최소 적용
- 버튼 반응성 개선
- 로딩 시간 확인
- GC Alloc 확인

## 9. v0.9 상세 계획: Android 빌드

- Portrait 고정
- 해상도 대응
- 실제 기기 테스트
- APK 생성
- 입력/터치 반응 확인
- 성능 확인
- 배터리/발열 과도 여부 확인

## 10. v1.0 MVP 출시 후보

### 10.1 포함 기능

- 월드1 스테이지 10개
- 영웅 6종
- 몬스터 6종
- 보스 1종
- 룬 20종 내외
- 영구 성장
- 로컬 저장
- 기본 사운드
- 기본 아트

### 10.2 제외 기능

- 광고
- 결제
- 서버
- 로그인
- 랭킹
- 클라우드 저장

## 11. 우선순위 원칙

1. 먼저 전투 루프 완성
2. 그다음 성장 구조
3. 그다음 콘텐츠 양 증가
4. 그다음 아트 적용
5. 마지막에 수익화 검토

## 12. 작업별 권장 브랜치

| 단계 | 브랜치 |
|---|---|
| v0.2 | `codex/battle-loop-v02` |
| v0.3 | `codex/progression-v03` |
| v0.4 | `codex/content-placement-v04` |
| v0.5 | `codex/stage-select-v05` |
| v0.7 | `codex/art-pipeline-v07` |
| v0.9 | `codex/android-build-v09` |
