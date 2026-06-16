# Tutorial Design

v0.8 튜토리얼은 첫 BattleScene 진입 시 표시되는 간단한 overlay다. 사용자가 완료하면 SaveData에 저장되어 다음 실행부터 자동으로 다시 나오지 않는다.

## 단계

1. 크리스탈을 지켜야 한다.
2. 몬스터는 3개의 라인을 따라 이동한다.
3. 영웅은 자동으로 공격한다.
4. 스킬 버튼을 눌러 사용할 수 있다.
5. 웨이브가 끝나면 3개의 룬 중 하나를 선택한다.
6. 승리하면 Gold와 Stage 해금을 얻는다.
7. Gold로 업그레이드를 구매하고 다음 Stage로 이동한다.

## 재시청

TitleScene Settings에서 `Replay Tutorial Next Battle`을 누르면 `hasSeenTutorial`이 false로 돌아가고, 다음 BattleScene에서 다시 표시된다.

## 현재 한계

- 튜토리얼은 상황별 클릭 강제형이 아니라 읽고 넘기는 overlay다.
- 화살표와 탭 인디케이터 이미지는 안내용 장식으로 표시되며, 특정 UI 좌표를 따라 이동하지 않는다.
