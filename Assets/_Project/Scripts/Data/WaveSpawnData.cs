using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "WaveSpawnData", menuName = "RuneGate/Data/Wave Spawn")]
    public sealed class WaveSpawnData : ScriptableObject
    {
        [SerializeField] private MonsterData monsterData;
        [SerializeField] private int laneIndex;
        [SerializeField] private int count = 1;
        [SerializeField] private float startDelay;
        [SerializeField] private float spawnInterval = 1f;

        public MonsterData MonsterData => monsterData;
        public int LaneIndex => laneIndex;
        public int Count => count;
        public float StartDelay => startDelay;
        public float SpawnInterval => spawnInterval;
    }
}
