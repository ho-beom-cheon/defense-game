# RuneGate Defense 백로그

## 1. 우선순위 기준

| 우선순위 | 의미 |
|---|---|
| P0 | 현재 단계 필수. 없으면 다음 단계 진행 불가 |
| P1 | MVP 필수 |
| P2 | MVP 이후 품질 개선 |
| P3 | 출시 이후 확장 |

## 2. P0: v0.2 전투 루프 완성

- [ ] Wave clear detection 안정화
- [ ] RuneSelection 상태 전환
- [ ] RuneSelectionUI 카드 3개 표시
- [ ] Rune 선택 이벤트 연결
- [ ] Sword Rune 효과 적용
- [ ] Bow Rune 효과 적용
- [ ] Healing Rune 효과 적용
- [ ] 다음 Wave 시작
- [ ] Final wave clear 시 Victory
- [ ] Crystal HP 0 시 Defeat
- [ ] ResultPanel 표시
- [ ] Restart 버튼
- [ ] Monster HP Bar
- [ ] Hit Flash
- [ ] Crystal Hit Feedback
- [ ] Skill cooldown 표시

## 3. P1: v0.3 콘텐츠 확장

- [ ] Mage HeroData
- [ ] Priest HeroData
- [ ] Engineer HeroData
- [ ] Assassin HeroData
- [ ] Mage skill: Fireball
- [ ] Priest skill: Heal
- [ ] Engineer skill: Temporary Turret
- [ ] Assassin skill: Shadow Strike
- [ ] Wolf MonsterData
- [ ] Bat MonsterData
- [ ] Slime MonsterData
- [ ] Skeleton MonsterData
- [ ] Orc Warlord BossData
- [ ] 추가 룬 10종

## 4. P1: v0.4 영구 성장

- [ ] SaveManager
- [ ] Local save data model
- [ ] Gold reward calculation
- [ ] UpgradeData
- [ ] UpgradeScene placeholder
- [ ] Crystal HP upgrade
- [ ] Hero Attack upgrade
- [ ] Hero HP upgrade
- [ ] Skill Cooldown upgrade
- [ ] Reward Gold upgrade
- [ ] Reset save debug button

## 5. P1: v0.5 스테이지 구조

- [ ] StageSelectScene
- [ ] World 1 data structure
- [ ] Stage 1~10 StageData
- [ ] Stage clear tracking
- [ ] Next stage unlock
- [ ] Stage retry
- [ ] Simple stage card UI

## 6. P2: UI/UX 개선

- [ ] TitleScene
- [ ] Better HUD layout
- [ ] Skill button icons
- [ ] Rune card visual layout
- [ ] Result reward summary
- [ ] Pause button
- [ ] Settings button placeholder
- [ ] Tutorial overlay

## 7. P2: 아트 파이프라인

- [ ] Knight sprite 적용
- [ ] Archer sprite 적용
- [ ] Goblin sprite 적용
- [ ] Orc sprite 적용
- [ ] Rune icon 적용
- [ ] Basic background 적용
- [ ] Sprite import settings 통일
- [ ] AnimatorController 샘플 적용

## 8. P2: 사운드/이펙트

- [ ] Basic hit SFX
- [ ] Monster death SFX
- [ ] Skill SFX
- [ ] Rune select SFX
- [ ] Victory/Defeat SFX
- [ ] Basic hit particle
- [ ] Skill particle placeholder

## 9. P2: 성능/품질

- [ ] Projectile pooling 검토
- [ ] Monster pooling 검토
- [ ] GC Alloc 확인
- [ ] Android 기기 FPS 확인
- [ ] Portrait 해상도 대응
- [ ] Canvas scaling 확인

## 10. P3: 출시 이후 확장

- [ ] 광고 제거 IAP
- [ ] 보상형 광고
- [ ] 월드팩 판매
- [ ] 영웅팩 판매
- [ ] 스킨팩
- [ ] Firebase Analytics 검토
- [ ] Cloud Save 검토
- [ ] Addressables 검토

## 11. 하지 않을 작업 목록

초기에는 다음 작업을 하지 않는다.

- [ ] 실시간 PvP
- [ ] 멀티플레이
- [ ] 서버 랭킹
- [ ] 가챠
- [ ] 확률형 장비
- [ ] 유저 계정
- [ ] 채팅
- [ ] 복잡한 스토리 컷신
- [ ] 대량 일러스트 선적용

## 12. 다음 Codex 작업 추천

현재 가장 적절한 다음 작업:

```text
Implement Battle Prototype v0.2.
```

관련 문서:

- `docs/00_PROJECT_CONTEXT.md`
- `docs/01_GAME_DESIGN_DOCUMENT.md`
- `docs/02_DEVELOPMENT_ROADMAP.md`
- `docs/03_CODEX_IMPLEMENTATION_GUIDE.md`
- `docs/04_SYSTEM_ARCHITECTURE.md`
- `docs/08_TEST_AND_QA_PLAN.md`
