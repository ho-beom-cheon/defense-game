# RuneGate Defense — Acceptance Checklist for v0.3 and v0.4

이 문서는 v0.3과 v0.4가 “완료”인지 판단하기 위한 체크리스트다.

---

# v0.3 Progression Loop Checklist

## Git

- [ ] `codex/progression-v03` 브랜치에서 작업했다.
- [ ] Unity generated folder가 커밋되지 않았다.
- [ ] `.meta` 파일이 관련 asset과 함께 커밋됐다.
- [ ] 커밋 메시지는 `feat(progression): ...` 형식이다.

## Scenes

- [ ] `TitleScene.unity` 존재
- [ ] `StageSelectScene.unity` 존재
- [ ] `BattleScene.unity` 존재
- [ ] `UpgradeScene.unity` 존재

## TitleScene

- [ ] 게임 제목 표시
- [ ] Start 버튼 존재
- [ ] Continue 버튼 존재
- [ ] Reset Save 버튼 존재
- [ ] Start 클릭 시 StageSelectScene으로 이동
- [ ] Continue 클릭 시 저장 데이터 로드 후 StageSelectScene 이동
- [ ] Reset Save 클릭 시 진행상태 초기화

## StageSelectScene

- [ ] Stage 1 표시
- [ ] Stage 2 표시
- [ ] Stage 3 표시
- [ ] Stage 1은 기본 unlocked
- [ ] Stage 2는 Stage 1 클리어 전 locked
- [ ] Stage 3은 Stage 2 클리어 전 locked
- [ ] locked stage 클릭 시 전투로 진입하지 않음
- [ ] unlocked stage 클릭 시 BattleScene으로 이동

## BattleScene

- [ ] 선택한 StageData로 전투 시작
- [ ] 선택된 StageData가 없을 때 fallback stage 사용
- [ ] 전투 중 기존 v0.2 기능 유지
- [ ] Victory 시 Result 표시
- [ ] Defeat 시 Result 표시
- [ ] Retry 가능
- [ ] Stage Select 복귀 가능
- [ ] UpgradeScene 이동 가능

## Save System

- [ ] `Application.persistentDataPath`에 JSON 저장
- [ ] 저장 파일이 없으면 기본 저장 생성
- [ ] totalGold 저장
- [ ] clearedStageIds 저장
- [ ] unlockedStageIds 저장
- [ ] upgradeLevels 저장
- [ ] lastSelectedStageId 저장
- [ ] Play Mode 재시작 후 진행상태 유지
- [ ] Reset Save 후 초기화

## Stage Unlock

- [ ] Stage 1 클리어 시 Stage 2 해금
- [ ] Stage 2 클리어 시 Stage 3 해금
- [ ] 해금 정보가 저장됨
- [ ] 재실행 후 해금 정보 유지

## Upgrade

- [ ] UpgradeScene에서 totalGold 표시
- [ ] Crystal Reinforcement 표시
- [ ] Hero Training 표시
- [ ] Battle Rhythm 표시
- [ ] Skill Practice 표시
- [ ] 골드 충분 시 구매 가능
- [ ] 골드 부족 시 구매 불가
- [ ] 구매 후 레벨 증가
- [ ] 구매 후 골드 감소
- [ ] 구매 정보 저장
- [ ] 구매한 업그레이드가 BattleScene에 적용

## Validation

- [ ] Unity compile error 없음
- [ ] Tools/RuneGate/Bootstrap Progression Prototype 실행 가능
- [ ] Tools/RuneGate/Validate Project 실행 가능
- [ ] README 업데이트 완료

---

# v0.4 Content Expansion Checklist

## Git

- [ ] v0.3이 main에 머지된 뒤 시작했다.
- [ ] `codex/content-v04` 브랜치에서 작업했다.
- [ ] Unity generated folder가 커밋되지 않았다.
- [ ] `.meta` 파일이 관련 asset과 함께 커밋됐다.

## HeroData

- [ ] Knight 존재
- [ ] Archer 존재
- [ ] Fire Mage 존재
- [ ] Cleric 존재
- [ ] Dwarf Engineer 존재
- [ ] Shadow Assassin 존재

## MonsterData

- [ ] Goblin 존재
- [ ] Wolf 존재
- [ ] Orc 존재
- [ ] Bat 존재
- [ ] Slime 존재
- [ ] Skeleton 존재
- [ ] Orc Warlord boss 존재

## SkillData

- [ ] Shield Bash 존재
- [ ] Rapid Shot 존재
- [ ] Flame Burst 존재
- [ ] Holy Heal 존재
- [ ] Deploy Turret 존재
- [ ] Shadow Strike 존재

## RuneData

- [ ] 총 20개 룬 존재
- [ ] Sword Rune 작동
- [ ] Bow Rune 작동
- [ ] Healing Rune 작동
- [ ] skill_cooldown_percent 작동
- [ ] monster_slow_percent 작동
- [ ] boss_damage_percent 작동
- [ ] healing_percent 작동
- [ ] 미구현 룬 선택 시 에러 없음

## StageData

- [ ] Stage 1 존재
- [ ] Stage 2 존재
- [ ] Stage 3 존재
- [ ] Stage 4 존재
- [ ] Stage 5 존재
- [ ] Stage 6 존재
- [ ] Stage 7 존재
- [ ] Stage 8 존재
- [ ] Stage 9 존재
- [ ] Stage 10 존재
- [ ] Stage 10에 boss wave 존재

## Stage Select

- [ ] Stage 1~10 표시
- [ ] Stage unlock이 1→10으로 동작
- [ ] locked/unlocked/cleared 상태가 구분됨

## Battle

- [ ] Stage 1 실행 가능
- [ ] Stage 4에서 Bat 데이터 스폰 확인
- [ ] Stage 6에서 Slime 데이터 스폰 확인
- [ ] Stage 7에서 Skeleton 데이터 스폰 확인
- [ ] Stage 10에서 Orc Warlord 등장 확인
- [ ] 보스 처치 시 Victory
- [ ] 기존 v0.3 저장/업그레이드 흐름이 깨지지 않음

## Placement Foundation

- [ ] HeroPlacementSlot 존재
- [ ] HeroPlacementManager 존재
- [ ] 3 lanes x 3 slots 구조 존재
- [ ] 각 slot에 laneIndex 존재
- [ ] 각 slot에 HeroPositionType 존재
- [ ] 자동 배치가 동작하거나 최소한 future hook이 존재
- [ ] 드래그앤드롭은 구현하지 않음

## Art Pipeline

- [ ] Art folder structure 생성
- [ ] `docs/ASSET_LIST_V04.md` 존재
- [ ] hero sprite naming convention 문서화
- [ ] monster sprite naming convention 문서화
- [ ] rune icon naming convention 문서화
- [ ] final art 대량 적용은 하지 않음

## Bootstrapper / Validator

- [ ] Tools/RuneGate/Bootstrap Content v0.4 메뉴 존재
- [ ] Tools/RuneGate/Validate Project가 v0.4 항목 검사
- [ ] 누락 asset에 대해 명확한 warning/error 출력

## Documentation

- [ ] README에 v0.4 요약 추가
- [ ] docs/02_DEVELOPMENT_ROADMAP.md 업데이트
- [ ] docs/09_BACKLOG.md 업데이트
- [ ] docs/10_NEXT_CODEX_PROMPTS.md 업데이트

## MVP Restrictions

- [ ] 서버 없음
- [ ] 로그인 없음
- [ ] 가챠 없음
- [ ] 광고 없음
- [ ] 인앱결제 없음
- [ ] 분석 SDK 없음
- [ ] Firebase 없음
- [ ] Addressables 없음
- [ ] 최종 일러스트 대량 적용 없음
