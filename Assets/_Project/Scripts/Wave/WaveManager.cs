using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class WaveManager : MonoBehaviour
    {
        [SerializeField] private StageData stageData;
        [SerializeField] private LaneManager laneManager;
        [SerializeField] private CrystalController crystalController;
        [SerializeField] private MonsterController monsterPrefab;
        [SerializeField] private Transform monsterRoot;
        [SerializeField] private bool addDefaultColliderToGeneratedMonsters = true;

        private readonly List<MonsterController> aliveMonsters = new List<MonsterController>();
        private WaveData activeWave;
        private int pendingSpawns;
        private int activeSpawnRoutines;

        public event Action<WaveData> WaveStarted;
        public event Action<WaveData> WaveCompleted;
        public event Action<MonsterController> MonsterSpawned;
        public event Action<MonsterController> MonsterKilled;

        public IReadOnlyList<MonsterController> AliveMonsters => aliveMonsters;

        public void Initialize(StageData data, LaneManager lanes, CrystalController crystal)
        {
            stageData = data;
            laneManager = lanes;
            crystalController = crystal;
        }

        public void StartWave(WaveData waveData)
        {
            if (waveData == null)
            {
                Debug.LogWarning("WaveManager cannot start a null wave.");
                return;
            }

            if (laneManager == null)
            {
                Debug.LogWarning("WaveManager cannot start because LaneManager is missing.");
                return;
            }

            StopAllCoroutines();
            aliveMonsters.Clear();
            activeWave = waveData;
            pendingSpawns = CountPendingSpawns(waveData);
            activeSpawnRoutines = 0;

            WaveStarted?.Invoke(waveData);
            Debug.Log($"Wave {waveData.WaveNo} started with {pendingSpawns} pending monsters.");

            if (waveData.Spawns.Count == 0 || pendingSpawns == 0)
            {
                CheckWaveComplete();
                return;
            }

            for (int i = 0; i < waveData.Spawns.Count; i++)
            {
                WaveSpawnData spawnData = waveData.Spawns[i];
                if (spawnData == null || spawnData.Count <= 0)
                {
                    continue;
                }

                activeSpawnRoutines++;
                StartCoroutine(SpawnRoutine(spawnData));
            }

            CheckWaveComplete();
        }

        public void NotifyMonsterKilled(MonsterController monster)
        {
            MonsterKilled?.Invoke(monster);
            NotifyMonsterRemoved(monster);
        }

        public void NotifyMonsterRemoved(MonsterController monster)
        {
            aliveMonsters.Remove(monster);
            RemoveNullAliveMonsters();
            CheckWaveComplete();
        }

        public void StopCurrentWave(bool destroyAliveMonsters)
        {
            StopAllCoroutines();
            activeWave = null;
            pendingSpawns = 0;
            activeSpawnRoutines = 0;

            if (destroyAliveMonsters)
            {
                for (int i = 0; i < aliveMonsters.Count; i++)
                {
                    MonsterController monster = aliveMonsters[i];
                    if (monster != null)
                    {
                        Destroy(monster.gameObject);
                    }
                }
            }

            aliveMonsters.Clear();
        }

        private IEnumerator SpawnRoutine(WaveSpawnData spawnData)
        {
            if (spawnData.StartDelay > 0f)
            {
                yield return new WaitForSeconds(spawnData.StartDelay);
            }

            for (int i = 0; i < spawnData.Count; i++)
            {
                SpawnMonster(spawnData);
                pendingSpawns = Mathf.Max(0, pendingSpawns - 1);

                if (i < spawnData.Count - 1 && spawnData.SpawnInterval > 0f)
                {
                    yield return new WaitForSeconds(spawnData.SpawnInterval);
                }
            }

            activeSpawnRoutines = Mathf.Max(0, activeSpawnRoutines - 1);
            CheckWaveComplete();
        }

        private void SpawnMonster(WaveSpawnData spawnData)
        {
            if (spawnData.MonsterData == null)
            {
                Debug.LogWarning("WaveManager skipped a spawn because MonsterData is missing.");
                return;
            }

            int laneIndex = spawnData.LaneIndex;
            if (!laneManager.IsValidLaneIndex(laneIndex))
            {
                Debug.LogWarning($"WaveManager clamped invalid lane index {laneIndex}.");
                laneIndex = Mathf.Clamp(laneIndex, 0, laneManager.LaneCount - 1);
            }

            float estimatedHalfWidth = RuntimeSpritePolicy.GetMonsterEstimatedHalfWidth(spawnData.MonsterData);
            Vector3 spawnPosition = laneManager.GetSafeSpawnPosition(laneIndex, estimatedHalfWidth);
            Vector3 targetPosition = laneManager.GetCrystalTargetPosition(laneIndex);
            MonsterController monster = CreateMonsterInstance(spawnData.MonsterData, spawnPosition);
            MonsterVariantType variantType = ShadowContractService.RollVariant(spawnData.MonsterData, stageData, activeWave);
            monster.Initialize(spawnData.MonsterData, laneIndex, targetPosition, crystalController, this, variantType);
            monster.RefreshBoundsAnchors();
            laneManager.ClampUnitInsideBattlefield(monster.transform, monster.VisualSpriteRenderer);
            monster.RefreshBoundsAnchors();
            aliveMonsters.Add(monster);
            MonsterSpawned?.Invoke(monster);
        }

        private MonsterController CreateMonsterInstance(MonsterData data, Vector3 spawnPosition)
        {
            if (data.Prefab != null)
            {
                GameObject prefabInstance = Instantiate(data.Prefab, spawnPosition, Quaternion.identity, monsterRoot);
                MonsterController controller = prefabInstance.GetComponent<MonsterController>();
                if (controller == null)
                {
                    controller = prefabInstance.AddComponent<MonsterController>();
                }

                EnsureGeneratedMonsterSupport(prefabInstance, data);
                return controller;
            }

            if (monsterPrefab != null)
            {
                return Instantiate(monsterPrefab, spawnPosition, Quaternion.identity, monsterRoot);
            }

            GameObject monsterObject = new GameObject($"Monster_{data.DisplayName}");
            monsterObject.transform.SetParent(monsterRoot);
            monsterObject.transform.position = spawnPosition;
            GameObject visualObject = new GameObject("Visual");
            visualObject.transform.SetParent(monsterObject.transform);
            visualObject.transform.localPosition = Vector3.zero;
            visualObject.AddComponent<SpriteRenderer>();
            PlaceholderSprite placeholder = visualObject.AddComponent<PlaceholderSprite>();
            float targetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(data);
            placeholder.Configure(RuntimeSpritePolicy.GetMonsterColor(data), new Vector2(targetHeight, targetHeight), 5);
            RuntimeSpriteFitter fitter = visualObject.AddComponent<RuntimeSpriteFitter>();
            fitter.TargetHeight = targetHeight;
            monsterObject.AddComponent<CharacterVisualController>();
            monsterObject.AddComponent<HitFlashController>();

            if (addDefaultColliderToGeneratedMonsters)
            {
                CircleCollider2D collider = monsterObject.AddComponent<CircleCollider2D>();
                collider.isTrigger = false;
            }

            return monsterObject.AddComponent<MonsterController>();
        }

        private void EnsureGeneratedMonsterSupport(GameObject monsterObject, MonsterData data)
        {
            SpriteRenderer spriteRenderer = monsterObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                GameObject visualObject = new GameObject("Visual");
                visualObject.transform.SetParent(monsterObject.transform);
                visualObject.transform.localPosition = Vector3.zero;
                spriteRenderer = visualObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = 5;
            if (data == null || data.Sprite == null)
            {
                PlaceholderSprite placeholder = spriteRenderer.gameObject.GetComponent<PlaceholderSprite>();
                if (placeholder == null)
                {
                    placeholder = spriteRenderer.gameObject.AddComponent<PlaceholderSprite>();
                }

                float targetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(data);
                placeholder.Configure(RuntimeSpritePolicy.GetMonsterColor(data), new Vector2(targetHeight, targetHeight), 5);
            }

            RuntimeSpriteFitter fitter = spriteRenderer.gameObject.GetComponent<RuntimeSpriteFitter>();
            if (fitter == null)
            {
                fitter = spriteRenderer.gameObject.AddComponent<RuntimeSpriteFitter>();
            }

            fitter.TargetHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(data);

            if (monsterObject.GetComponentInChildren<CharacterVisualController>() == null)
            {
                monsterObject.AddComponent<CharacterVisualController>();
            }

            if (monsterObject.GetComponentInChildren<HitFlashController>() == null)
            {
                monsterObject.AddComponent<HitFlashController>();
            }

            if (addDefaultColliderToGeneratedMonsters && monsterObject.GetComponentInChildren<Collider2D>() == null)
            {
                CircleCollider2D collider = monsterObject.AddComponent<CircleCollider2D>();
                collider.isTrigger = false;
            }
        }

        private int CountPendingSpawns(WaveData waveData)
        {
            int count = 0;
            for (int i = 0; i < waveData.Spawns.Count; i++)
            {
                WaveSpawnData spawnData = waveData.Spawns[i];
                if (spawnData != null)
                {
                    count += Mathf.Max(0, spawnData.Count);
                }
            }

            return count;
        }

        private void CheckWaveComplete()
        {
            if (activeWave == null)
            {
                return;
            }

            RemoveNullAliveMonsters();
            if (pendingSpawns > 0 || activeSpawnRoutines > 0 || aliveMonsters.Count > 0)
            {
                return;
            }

            WaveData completedWave = activeWave;
            activeWave = null;
            WaveCompleted?.Invoke(completedWave);
            Debug.Log($"Wave {completedWave.WaveNo} completed.");
        }

        private void RemoveNullAliveMonsters()
        {
            aliveMonsters.RemoveAll(monster => monster == null);
        }
    }
}
