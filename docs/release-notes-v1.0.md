# 룬게이트 디펜스 v1.0 릴리스 노트

룬게이트 디펜스 v1.0은 Chapter 1 재문 숲을 끝까지 플레이할 수 있는 첫 Android 공개 테스트 후보입니다.

## 주요 콘텐츠

- 세 라인을 지키는 자동 전투와 직접 사용하는 영웅 스킬
- 레온, 세리아, 카엘, 미레아, 브롬, 닉스 영웅 6인
- 몬스터 6종과 Stage 10 보스 그룸바르
- Stage 1~10, Normal/Hard/Nightmare 순차 진행
- 실제 전투 효과가 연결된 룬 20장
- 3x3 영웅 편성, 영구 강화, 그림자 조각과 7종 그림자 계약
- 그룸바르 3단계 페이즈, 지원군, 전용 공격 패턴과 보스 HUD
- 첫 전투 튜토리얼, 승리/패배 결과, 로컬 JSON 저장과 안전한 초기화
- 장면별 BGM, 전투/UI SFX, 독립 음량과 음소거 저장

## Android 후보 정보

- 패키지: `com.hobeomcheon.runegatedefense`
- 버전: `1.0.0` (`versionCode 10`)
- 방향: Portrait
- 검증 환경: Android 15 / API 35 에뮬레이터, 1080x2400
- APK SHA-256: `C6CB618106A7AD7F2CA9382B3334050FEB535FD9277D03D11FB21E3A2189B310`
- QA 서명 AAB SHA-256: `7936ECFD04D726BA24CA807EFC288D7C8BD01663A31032D3970E3AB5C1D06FAB`

## 검증 결과

- Unity v1.0 Release Track Validator 및 Progression Smoke 통과
- Android 핵심 화면 7종 렌더 캡처 통과
- 튜토리얼, 저장, 편성, 계약, 일시정지, 오디오, 패배 흐름 통과
- Normal Stage 1~10 연속 승리, 강화 10회, 그룸바르 3페이즈와 지원군 검증 통과
- QA 키스토어 서명 AAB의 manifest 해시 일치 및 `jarsigner -verify` 통과

## 포함하지 않는 범위

- 서버, 로그인, 클라우드 저장, 랭킹, 멀티플레이
- 가챠와 확률형 아이템
- 광고 SDK와 결제 SDK
- Firebase, Addressables, 외부 패키지
- 최종 상용 아트와 최종 음원

운영 키스토어 서명, Play Console 업로드, 실기기 컷아웃·장시간 성능은 공개 스토어 제출 전에 별도로 확인해야 합니다.
