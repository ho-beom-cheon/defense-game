# Android Battle Status HUD v0.9.0

## 작업 범위

- 관련 이슈: `#75`
- 작업 브랜치: `codex/issue-75-battle-status-hud`
- Android Portrait 전투 상단의 텍스트 나열형 HUD를 스테이지, 크리스탈, 전투 진행, 일시정지 영역으로 재구성했다.
- 기존 BattleManager, CrystalController, BattlePauseController 이벤트와 전투 수치는 변경하지 않았다.

## HUD 구성

### 스테이지 카드

- 한국어 스테이지 표시명
- 현재 난이도
- 봉문 전선 표시

### 크리스탈 카드

- 현재 HP / 최대 HP
- HP 진행 바
- HP 60% 이하 경고색, 30% 이하 위험색
- 보호막 활성 시 파란 보호막 바와 수치
- 크리스탈 피격 시 피해 피드백

### 전투 진행 카드

- 한국어 전투 상태
- 현재 Gold
- 현재 Wave / 전체 Wave
- Wave 진행 바

### 일시정지

- 상단 오른쪽 고정 터치 영역
- 기존 계속하기, 전투 재시작, 스테이지 선택 팝업 유지

## 정적 검증

`RuneGateProgressionSmokeTest`는 720x1280, 1080x1920, 1440x2560, 1600x900, 2048x1152에서 다음을 검사한다.

- 네 개 HUD 하위 영역이 HeaderArea 안에 위치하는지
- 각 영역의 최소 폭과 높이
- 하위 영역 간 중첩 여부
- 크리스탈 정상, 경고, 위험 색상이 서로 구분되는지

## Android 검증

- 검증일: 2026-07-16
- 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080 x 2400 Portrait
- APK: `RuneGateDefense-battle-status-hud.apk`
- 파일 크기: `72,071,149 bytes`
- SHA-256: `C04B7648DF5ED29B02C746C42417DFA8003F9006A469E3BEFA443105F4894601`

확인 결과:

- Unity Project Validator 통과
- Unity Progression Smoke Test 통과
- Android APK 빌드 및 재설치 통과
- 스테이지명, 난이도, Crystal HP, 전투 상태, Gold, Wave 표시 확인
- Crystal HP와 Wave 진행 바 표시 확인
- 일시정지 버튼 터치와 계속하기 흐름 확인
- 룬 선택 중 상태가 파란 상태 텍스트로 표시됨
- 수호 룬 선택 후 `보호막 +35`와 파란 보호막 바 표시 확인
- 하단 3x2 영웅 스킬 카드와 중첩되지 않음
- Normal Stage 1~10 전체 승리와 강화 10회 통과
- Stage 10 그룸바르 페이즈 1~3 및 공격 패턴 통과

## 남은 한계

- HUD는 기존 IMGUI 구조를 유지한다.
- 상태 카드에는 전용 아이콘 대신 색상과 텍스트를 사용한다.
- 실제 Android 기기의 화면 컷아웃, 시스템 글꼴 배율, 장시간 상태 갱신은 추가 확인이 필요하다.
- 최종 릴리스용 전용 Crystal, Gold, Wave 아이콘은 별도 아트 작업으로 교체할 수 있다.
