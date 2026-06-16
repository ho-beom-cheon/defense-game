# Store Listing Draft

한국 출시 우선 기준의 Google Play 스토어 등록 초안이다. 실제 제출 전 정책, 등급, 개인정보 처리방침 URL, 스크린샷, 타겟 SDK 요구사항을 다시 확인한다.

## 게임명 후보

- 룬게이트 디펜스
- 룬게이트 디펜스: 문지기 전선

## Short Description

영웅을 배치하고 룬을 선택해 균열의 몬스터를 막는 2D 픽셀 디펜스 게임.

## Long Description

룬게이트 디펜스는 한국 출시를 우선으로 준비 중인 2D 픽셀 판타지 디펜스 게임입니다.

플레이어는 문지기 영웅들을 3개의 라인에 배치하고, 균열에서 몰려오는 적성체로부터 크리스탈을 지켜야 합니다. 영웅은 자동으로 공격하며, 전투 중 스킬을 사용하고 웨이브 사이에는 룬을 선택해 조합을 강화합니다. 스테이지를 클리어하면 골드를 얻고, 영구 업그레이드를 통해 다음 전투를 준비할 수 있습니다.

현재 v0.9 Release Candidate는 Stage 1~10, 영웅 6종, 몬스터 6종, 보스 1종, 룬 20장을 포함한 첫 공개 테스트 후보입니다.

## 주요 특징

- 3라인 x 3슬롯 기반 방어 전투
- 문지기 영웅 6종
- 균열 몬스터 6종과 보스 그룸바르
- 전투 중 선택하는 룬 20장
- Stage 1~10 진행
- 로컬 JSON 저장
- 오프라인 우선 구조
- RuntimePixel 전투 스프라이트와 기록서풍 ConceptSheets 분리

## 조작 방법

- 영웅은 자동으로 공격합니다.
- 스킬 버튼을 눌러 수동 스킬을 사용합니다.
- 웨이브가 끝나면 룬 1개를 선택합니다.
- 승리 후 골드로 업그레이드를 구매합니다.

## 광고 / 결제 정책

v0.9 RC에는 실제 광고 SDK와 결제 SDK가 없습니다.

- 강제 광고 없음
- 보상형 광고 SDK 없음
- 인앱결제 SDK 없음
- 가챠 없음
- 확률형 아이템 없음

v1.0 이후 선택형 보상 광고, 광고 제거, 스타터팩, 후원자팩은 별도 작업으로 검토할 수 있다. 실제 SDK 연동은 별도 브랜치에서 정책 검토 후 진행한다.

## 개인정보 처리 메모

현재 후보는 서버, 로그인, 클라우드 저장, 랭킹, 광고 SDK, 결제 SDK를 사용하지 않는다. 저장 데이터는 로컬 JSON 파일에만 기록된다. 다만 스토어 제출 전 Google Play Data Safety와 개인정보 처리방침 필요 여부는 최종 확인한다.

## 스토어 이미지 후보

- App icon: `Assets/_Project/Art/RuntimePixel/App/app_icon_1024.png`
- Adaptive icon foreground: `Assets/_Project/Art/RuntimePixel/App/app_icon_adaptive_foreground.png`
- Splash background candidate: `Assets/_Project/Art/RuntimePixel/App/splash_background_1080x1920.png`
- Feature graphic: `Assets/_Project/Art/Store/store_feature_graphic_1024x500.png`
