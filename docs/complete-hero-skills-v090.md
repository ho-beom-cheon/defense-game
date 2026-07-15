# Hero Skill Runtime Completion v0.9.0

## 목표

영웅 6인의 스킬을 공용 직접 피해 폴백에서 분리하고, 각 역할을 전투에서 확인할 수 있는 실제 런타임 효과로 완성한다. `SkillData`는 설정값만 보유하며 전투 중 원본 값을 변경하지 않는다.

## 스킬 동작

| 영웅 | 스킬 | effectKey | 런타임 효과 |
| --- | --- | --- | --- |
| 레온 | 방패 강타 | `shield_bash` | 단일 피해, 전선 방향 밀치기, 짧은 행동 제어 |
| 세리아 | 연속 사격 | `rapid_shot` | 0.12초 간격의 3연타 |
| 카엘 | 운석 낙하 | `meteor_area` | 충돌 지점 반경 내 모든 생존 몬스터에게 범위 피해 |
| 미레아 | 성스러운 회복 | `holy_heal` | HP 비율이 가장 낮은 영웅과 크리스탈 동시 회복 |
| 브롬 | 임시 포탑 | `temporary_turret` | 같은 라인의 선두 적을 일정 시간 자동 공격하는 런타임 포탑 배치 |
| 닉스 | 그림자 급습 | `shadow_strike` | 보스 30%, HP 35% 이하 대상 35% 추가 피해 |

`SkillController`는 알 수 없는 effectKey를 직접 피해로 대체하지 않는다. 지원하지 않는 키는 경고와 함께 발동을 거부하므로 데이터 누락이 조용히 숨겨지지 않는다.

## 보스 접촉 규칙

그룸바르는 크리스탈에 닿아도 일반 몬스터처럼 즉시 제거되지 않는다. 크리스탈 앞에 남아 페이즈 공격력과 공격 주기를 적용한 반복 공격을 수행하며, Victory를 위해 실제로 처치해야 한다.

## 자동 검증

- Project Validator: 스킬 6종의 고유 effectKey, placeholder 제거, `TemporaryTurretController` 존재 확인
- Progression Smoke: 스킬 6종 연결, 닉스 보스/처형 배율, 미레아 회복 배분 확인
- Android System Flow: 밀치기/제어, 3연타, 범위 피해, 영웅·크리스탈 회복, 포탑 발사, 보스 대상 그림자 급습 확인
- Android Full Chapter: 플레이어 행동을 모사해 사용 가능한 영웅 스킬을 주기적으로 발동하고 Stage 1~10 Victory, 강화 10회, 그룸바르 3페이즈와 지원군 5기를 확인

## 2026-07-16 검증 결과

- Unity Project Validator: 통과
- Unity Progression Smoke: 통과
- Android APK 빌드: 통과
- Android 15 API 35 시스템 흐름: 통과
- Android 15 API 35 전체 챕터: 통과
- APK: `C:\workspace\defense-game-issue52-artifacts\RuneGateDefense-issue52-complete-skills.apk`
- APK SHA-256: `4375FB6290A0DDC5E5D8B09F9E6CECD053E53543400CCB87B97329DE5987C579`

## 남은 범위

- 스킬 애니메이션은 현재 RuntimePixel 이펙트 또는 색상 폴백을 사용한다.
- 포탑 전용 최종 스프라이트와 발사 애니메이션은 후속 아트 범위다.
- 스킬 수치와 보스 접촉 압박은 장시간 실기기 플레이로 추가 조정해야 한다.
