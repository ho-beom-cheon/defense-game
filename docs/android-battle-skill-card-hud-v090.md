# Android Battle Skill Card HUD v0.9.0

## 작업 범위

- 관련 이슈: `#73`
- 작업 브랜치: `codex/issue-73-battle-skill-card-hud`
- 기존 텍스트 중심 영웅 스킬 버튼을 모바일 Portrait용 3열 x 2행 카드 HUD로 교체했다.
- 전투 스킬 로직과 `HeroController.RequestManualSkill()` 호출 흐름은 유지했다.

## 카드 구성

각 카드에는 다음 정보를 표시한다.

- RuntimePixel 영웅 전투 스프라이트
- 영웅 한국어 표시명
- 한국어 스킬 표시명
- 현재 HP / 최대 HP
- 준비 완료, 재사용 대기, 전투 종료 상태
- 쿨다운 암전 오버레이, 남은 초, 진행 바

전투 중 사용 가능한 카드는 초록색, 재사용 대기 카드는 파란색, 사용할 수 없는 카드는 회색 계열로 구분한다. 카드 전체를 터치 영역으로 사용한다.

## 반응형 배치

- 사용 가능 폭이 520 이상이면 3열을 사용한다.
- 폭이 330 이상이면 2열, 그보다 작으면 1열을 사용한다.
- 카드 행 수와 높이는 영웅 수와 스킬 패널 높이에 따라 계산한다.
- Progression Smoke Test가 6개 카드의 패널 내부 배치, 최소 크기, 중첩 여부와 Portrait 3열 구성을 검사한다.

## Android 검증

- 검증일: 2026-07-16
- 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080 x 2400 Portrait
- APK: `RuneGateDefense-skill-card-hud.apk`
- 파일 크기: `72,068,337 bytes`
- SHA-256: `F143BCA159EA025DAA061B883C25E3605829C5C4DDFF7A333077D5848D035780`

확인 결과:

- Unity Progression Smoke Test 통과
- Android APK 빌드 및 재설치 통과
- BattleScene에서 영웅 6인 카드가 3열 x 2행으로 표시됨
- RuntimePixel 초상, 한국어 스킬명, HP, 준비 상태 표시 확인
- 레온 카드 터치 후 준비 수가 `6/6`에서 `5/6`으로 변경되고 쿨다운 상태로 전환됨
- 룬 선택 중 카드가 `0/6 준비`와 전투 불가 상태로 표시됨
- 룬 선택 후 다음 웨이브 진행 확인
- Normal Stage 1~10 전체 승리, 강화 10회, Hard/Nightmare 해금 회귀 테스트 통과
- Stage 10 그룸바르 페이즈 1~3 및 공격 패턴 회귀 통과

## 남은 한계

- 카드 HUD는 기존 IMGUI 구조를 유지한다.
- 영웅 초상은 전용 스킬 아이콘이 아니라 RuntimePixel idle 스프라이트를 사용한다.
- 실제 Android 기기의 시스템 글꼴 배율, 화면 컷아웃, 장시간 연속 터치는 추가 확인이 필요하다.
- 최종 릴리스에서는 스킬별 전용 아이콘과 눌림/쿨다운 애니메이션을 별도 제작할 수 있다.
