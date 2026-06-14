using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "StageData", menuName = "RuneGate/Data/Stage")]
    public sealed class StageData : ScriptableObject
    {
        [SerializeField] private string stageId = "stage_id";
        [SerializeField] private string displayName = "Stage";
        [SerializeField] private int crystalHp = 100;
        [SerializeField] private List<WaveData> waves = new List<WaveData>();
        [SerializeField] private MonsterData bossMonster;

        public string StageId => stageId;
        public string DisplayName => displayName;
        public int CrystalHp => crystalHp;
        public IReadOnlyList<WaveData> Waves => waves;
        public MonsterData BossMonster => bossMonster;
    }
}
