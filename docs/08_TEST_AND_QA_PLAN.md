# 테스트 및 QA 계획

## 1. 테스트 목표

MVP 단계의 테스트 목표는 다음이다.

- Unity 컴파일 에러 없음
- BattleScene Play 가능
- 전투 루프 동작
- Wave/Rune/Victory/Defeat 흐름 검증
- Restart 가능
- 주요 null reference 방지

## 2. 테스트 범위

### 2.1 v0.2 필수 테스트

| 테스트 | 기대 결과 |
|---|---|
| BattleScene Play | 에러 없이 실행 |
| Wave 1 Start | 몬스터 스폰 |
| Hero Attack | Knight/Archer가 공격 |
| Monster Damage | 몬스터 HP 감소 |
| Monster Death | 몬스터 제거 및 Gold 증가 |
| Crystal Damage | 몬스터 도달 시 HP 감소 |
| Rune Selection | Wave 1 클리어 후 룬 3개 표시 |
| Rune Apply | 선택 효과 반영 |
| Next Wave | Wave 2 시작 |
| Victory | 최종 웨이브 클리어 시 표시 |
| Defeat | Crystal HP 0 시 표시 |
| Restart | 초기 상태로 재시작 |

## 3. 수동 테스트 체크리스트

### 3.1 기본 실행

```text
[ ] Unity 프로젝트 열기
[ ] Assets/_Project/Scenes/BattleScene.unity 열기
[ ] Console Clear
[ ] Play 클릭
[ ] Console Error 없음
```

### 3.2 전투 시작

```text
[ ] HUD에 RuneGate Defense 표시
[ ] Crystal HP 표시
[ ] Wave 1/2 표시
[ ] State WaveRunning 표시
[ ] Knight/Archer 스킬 버튼 표시
[ ] 몬스터가 라인에 스폰됨
```

### 3.3 공격/피격

```text
[ ] Knight가 사거리 내 적 공격
[ ] Archer가 사거리 내 적 공격
[ ] 몬스터 HP Bar 감소
[ ] 몬스터 피격 feedback 표시
[ ] 몬스터 사망 시 제거
[ ] Gold 증가
```

### 3.4 Crystal 피해

```text
[ ] 몬스터가 끝까지 도달하면 Crystal HP 감소
[ ] Crystal 피격 feedback 표시
[ ] Crystal HP 0 이하일 때 Defeat 표시
```

### 3.5 룬 선택

```text
[ ] Wave 1의 모든 몬스터 처치 후 RuneSelection 상태 진입
[ ] RuneSelectionPanel 표시
[ ] 룬 카드 3개 표시
[ ] Sword Rune 선택 시 영웅 ATK 증가
[ ] Bow Rune 선택 시 영웅 SPD 증가
[ ] Healing Rune 선택 시 Crystal HP 회복
[ ] 선택 후 Wave 2 시작
```

### 3.6 승리/패배

```text
[ ] Wave 2 클리어 시 Victory 표시
[ ] Restart 버튼 클릭 시 전투 초기화
[ ] Crystal HP를 낮게 설정해 Defeat 재현 가능
[ ] Defeat 후 Restart 가능
```

## 4. 자동 테스트 후보

Unity Test Framework가 준비되어 있다면 다음을 고려한다.

### 4.1 EditMode 테스트 후보

- RuneEffectApplier가 `hero_attack_percent`를 올바르게 적용하는지
- RuneEffectApplier가 `hero_attack_speed_percent`를 올바르게 적용하는지
- CrystalController.Heal이 maxHp를 초과하지 않는지
- CrystalController.TakeDamage가 HP 0 이하에서 destroyed 이벤트를 내는지

### 4.2 PlayMode 테스트 후보

- WaveManager가 모든 스폰 완료 후 wave complete를 발생시키는지
- MonsterController가 crystal 도달 시 damage를 주는지
- BattleManager가 final wave clear 시 Victory로 전환하는지

단, v0.2에서는 자동 테스트보다 수동 실행 검증이 우선이다.

## 5. Unity Batchmode 검증

가능하면 다음 식으로 실행한다.

```powershell
"C:\Program Files\Unity\Hub\Editor\<UnityVersion>\Editor\Unity.exe" -batchmode -quit -projectPath "C:\workspace\defense-game" -logFile "C:\workspace\defense-game\unity-validate.log"
```

테스트 어셈블리가 있다면:

```powershell
"C:\Program Files\Unity\Hub\Editor\<UnityVersion>\Editor\Unity.exe" -batchmode -projectPath "C:\workspace\defense-game" -runTests -testPlatform EditMode -logFile "C:\workspace\defense-game\unity-tests.log" -quit
```

## 6. Console Error 대응 원칙

1. 첫 에러부터 본다.
2. NullReference면 참조 누락인지 bootstrapper 생성 누락인지 확인한다.
3. ScriptableObject 값 누락이면 Bootstrapper에서 샘플 데이터 생성을 보완한다.
4. UI 참조 누락이면 Inspector 연결 또는 코드 자동 생성 경로를 확인한다.
5. 임시 해결로 `try-catch`를 남발하지 않는다.

## 7. Definition of Done

v0.2 완료 기준:

```text
[ ] Unity Console Error 0개
[ ] BattleScene Play 가능
[ ] Wave 1 진행
[ ] Wave 1 클리어 후 룬 선택
[ ] 룬 효과 적용
[ ] Wave 2 진행
[ ] Victory 표시
[ ] Crystal HP 0 시 Defeat 표시
[ ] Restart 가능
[ ] README 또는 docs에 테스트 방법 갱신
```

## 8. 버그 기록 양식

```markdown
## Bug: 제목

- 발견 버전:
- 재현 경로:
- 기대 결과:
- 실제 결과:
- Console 로그:
- 원인 추정:
- 수정 파일:
- 검증 결과:
```
