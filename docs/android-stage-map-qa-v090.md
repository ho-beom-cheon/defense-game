# Android Stage Map QA v0.9.0

## 작업 범위

- 이슈: `#69`
- 브랜치: `codex/issue-69-stage-map-presentation`
- 기준 기기: Android 15 / API 35 에뮬레이터
- 해상도: 1080x2400 Portrait
- 검증일: 2026-07-16

## StageSelect 표현

- Stage 1~10 목록을 좌우 교차형 챕터 노드 지도로 표시한다.
- 클리어, 출전 가능, 잠김 상태에 기존 RuntimePixel 스테이지 노드 이미지를 사용한다.
- 선택한 노드는 강조 배경과 번호로 구분하고, 연결 경로는 다음 스테이지 해금 상태를 반영한다.
- 상세 패널은 스테이지명, 상태, 설명, 크리스탈 HP, 웨이브 수, 예상 전투 골드, 난이도 보상 배율, 실제 웨이브 출현 적을 표시한다.
- 잠긴 스테이지의 출전 버튼은 비활성화되고, 기존 편성/업그레이드/타이틀 이동 흐름은 유지한다.

## 데이터 정합성 수정

- Stage 1~9의 `bossMonster`에 잘못 연결되어 있던 그룸바르 참조를 제거했다.
- Stage 10만 그룸바르를 보스 데이터로 유지한다.
- Bootstrapper, Project Validator, Progression Smoke Test에 같은 규칙을 반영했다.
- 상세 패널의 적 목록은 `bossMonster` 필드를 추정 표시하지 않고 실제 WaveData 스폰만 수집한다.

## 검증 결과

- Unity Android 빌드 파이프라인의 Project Validator: 통과
- Android 1080x2400 StageSelect 화면 확인: 통과
- 노드 번호, 잠금/해금/클리어 상태, 연결 경로: 통과
- Stage 1 상세 패널에서 실제 출현 적만 표시: 통과
- Normal Stage 1~10 전체 전투: 통과
- 업그레이드 구매: 10회 통과
- Stage 10 그룸바르 3페이즈와 공격 패턴: 통과
- 최종 결과: `RUNEGATE_FULL_CHAPTER_E2E_PASSED: upgrades=10, gold=1616, difficulties=normal|hard|nightmare`

## Android 빌드

- APK: `RuneGateDefense-stage-map.apk`
- 크기: `72,063,313 bytes`
- SHA-256: `330569E721E1088B5BFC6952546F5E335797D5071AE64646787D3186304AD8BB`
- 빌드 결과: `RUNEGATE_ANDROID_BUILD_PASSED`

빌드 산출물과 로컬 로그는 저장소 밖 QA 산출물 폴더에서 관리하며 Git에는 포함하지 않는다.

## 남은 확인

- StageSelect는 현재 IMGUI 기반이라 최종 Canvas/RectTransform 전환 전까지 화면비별 수동 확인이 필요하다.
- 해금 노드 원본 이미지에 숫자 `1`이 포함되어 있어 런타임 중앙 번호 오버레이로 실제 스테이지 번호를 표시한다. 최종 아트에서는 숫자 없는 공용 노드 이미지가 필요하다.
- 실제 Android 기기의 노치, 시스템 글꼴 배율, 장시간 스크롤 감각은 아직 검증하지 않았다.
- 현재 지도는 Chapter 1의 10개 스테이지만 표현한다.
