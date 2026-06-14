using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "RuneGate/Data/Wave")]
    public sealed class WaveData : ScriptableObject
    {
        [SerializeField] private int waveNo = 1;
        [SerializeField] private List<WaveSpawnData> spawns = new List<WaveSpawnData>();
        [SerializeField] private bool isBossWave;

        public int WaveNo => waveNo;
        public IReadOnlyList<WaveSpawnData> Spawns => spawns;
        public bool IsBossWave => isBossWave;
    }
}
