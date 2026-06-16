# Privacy Checklist

v0.9 Release Candidate 기준의 개인정보/정책 점검 메모다. 최종 법적 판단이 아니라 스토어 제출 전 확인해야 할 작업 목록이다.

## Current Technical State

- 서버 없음
- 로그인 없음
- 계정 시스템 없음
- 클라우드 저장 없음
- 랭킹 없음
- 멀티플레이 없음
- Firebase 없음
- 광고 SDK 없음
- 결제 SDK 없음
- 분석/트래킹 SDK 없음
- 외부 유료 API 없음
- 로컬 JSON 저장 사용

## Local Save Data

로컬 저장 파일에는 다음 게임 진행 정보가 포함될 수 있다.

- 총 골드
- 클리어한 스테이지
- 해금된 스테이지
- 업그레이드 레벨
- 편성 슬롯
- 마지막 선택 스테이지
- 마지막 선택 난이도
- 튜토리얼 완료 여부
- 세이브 버전

이 정보는 현재 기기 로컬 저장소에만 기록되며 서버로 전송하지 않는다.

## Google Play Data Safety Draft

현재 구현만 기준으로 보면 계정 개인정보 수집, 위치 수집, 연락처 수집, 광고 식별자 수집, 결제 정보 수집은 없다. 단, 실제 제출 전 Unity/Android 생성 Manifest, Play Console 요구사항, 개인정보 처리방침 URL 필요 여부를 반드시 다시 확인한다.

## Manual Review Before Submission

- [ ] 최종 Android Manifest 권한 확인
- [ ] Google Play Data Safety 항목 확인
- [ ] 개인정보 처리방침 URL 필요 여부 확인
- [ ] 앱 내 저장 데이터 설명 문구 확인
- [ ] 광고/결제 SDK가 추가되었는지 재확인
- [ ] 타겟 SDK 정책 확인

## Future Monetization Warning

보상형 광고 또는 인앱결제를 추가하는 순간 이 문서는 반드시 갱신해야 한다. SDK 추가, 광고 식별자, 결제 데이터, 외부 서비스 정책, 개인정보 처리방침 문구를 별도 작업으로 검토한다.
