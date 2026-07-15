# Android Runtime Sprite Alpha QA v0.90

## 목적

Android Portrait 전투에서 일부 몬스터와 그룸바르 주변에 표시되던 큰 검은 직사각형을 제거하고, 같은 문제가 RuntimePixel 원본에 다시 들어오지 않도록 검증한다.

## 원인

- 일부 몬스터와 보스 PNG의 투명 영역 경계에 알파가 남은 순수 검정 픽셀이 있었다.
- `RuntimeSpriteFitter`가 전투 표시 높이에 맞춰 스프라이트를 확대하면서 해당 픽셀 영역도 큰 검은 판처럼 보였다.
- `alphaIsTransparency` 임포트 설정만으로는 원본 PNG의 불투명 픽셀을 제거할 수 없다.

## 수정 방식

- `Tools/RuneGate/Repair RuntimePixel Actor Alpha` 메뉴를 추가했다.
- 유틸리티는 PNG 바깥 경계에서 시작해 연결된 `RGB 0~2`, `Alpha 250~255` 픽셀만 투명화한다.
- 캐릭터 내부의 검은 윤곽선과 경계에 연결되지 않은 어두운 디테일은 유지한다.
- 현재 몬스터 6종과 그룸바르 원본을 검사하고, 실제 변경이 있는 파일만 다시 저장한다.
- `Tools/RuneGate/Validate Project`는 같은 경계 픽셀을 오류로 보고해 재발을 막는다.

## 적용 자산

- `Assets/_Project/Art/RuntimePixel/Monsters/GateImp/gate_imp_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/OrcBrute/orc_brute_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/DireWolf/dire_wolf_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/CaveBat/cave_bat_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/CoreSlime/core_slime_idle.png`
- `Assets/_Project/Art/RuntimePixel/Monsters/BoneSoldier/bone_soldier_idle.png`
- `Assets/_Project/Art/RuntimePixel/Bosses/Grumbar/grumbar_idle.png`

## 실제 검증

- 날짜: 2026-07-16
- 브랜치: `codex/issue-81-runtime-sprite-alpha-cleanup`
- Unity: `6000.4.11f1`
- Project Validator: 통과
- Progression Smoke Test: 통과
- Android APK 빌드: 통과
- 기기: Android 15 / API 35 에뮬레이터, 1080x2400 Portrait
- Stage 3: 부식 늑대 포함 전투 화면에서 검은 배경 없음
- Stage 7: 망각의 뼈병 포함 전투 화면에서 검은 배경 없음
- Stage 10: 그룸바르 본체와 무기 주변에 검은 배경 없음
- 전체 회귀: Normal Stage 1~10 Victory, 업그레이드 10회, Hard/Nightmare 해금 통과

APK:

- 경로: `C:\workspace\defense-game-issue81-artifacts\RuneGateDefense-runtime-alpha-cleanup.apk`
- 크기: `72,097,413 bytes`
- SHA-256: `0C24E184DFF4B479D2A83620E66F2456DDB2C5C71910817828115341A63F5F75`

## 남은 범위

- 이번 수정은 원본 RuntimePixel 자산의 검은 경계 제거만 다룬다.
- 최종 Idle / Walk / Attack / Hit / Death 시트 제작과 아트 스타일 통일은 별도 아트 제작 범위다.
- 실제 Android 기기의 GPU 압축, 디스플레이 컷아웃, 장시간 전투는 추가 확인이 필요하다.
