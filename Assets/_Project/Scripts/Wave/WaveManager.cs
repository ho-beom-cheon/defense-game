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
        [SerializeField] private BattlefieldVisualController battlefieldVisualController;
        [SerializeField] private BattlefieldSpaceController battlefieldSpace;
        [SerializeField] private CrystalApproachPointProvider crystalApproachPointProvider;
        [SerializeField] private BattlefieldAgentRegistry battlefieldAgentRegistry;
        [SerializeField] private bool addDefaultColliderToGeneratedMonsters = true;

        private readonly List<MonsterController> aliveMonsters = new List<MonsterController>();
        private readonly HashSet<int> usedApproachSlots = new HashSet<int>();
        private WaveData activeWave;
        private int pendingSpawns;
        private int activeSpawnRoutines;
        private int bossReinforcementSpawnCount;
        private int nextMonsterStableId = 1000000;

        public event Action<WaveData> WaveStarted;
        public event Action<WaveData> WaveCompleted;
        public event Action<MonsterController> MonsterSpawned;
        public event Action<MonsterController> MonsterKilled;
        public event Action<MonsterController> BossReinforcementSpawned;

        public IReadOnlyList<MonsterController> AliveMonsters => aliveMonsters;
        public int BossReinforcementSpawnCount => bossReinforcementSpawnCount;
        public int UsedApproachPointCount => usedApproachSlots.Count;

        public void Initialize(StageData data, LaneManager lanes, CrystalController crystal)
        {
            stageData = data;
            laneManager = lanes;
            crystalController = crystal;
            nextMonsterStableId = 1000000;
        }

        public void Initialize(
            StageData data,
            BattlefieldSpaceController space,
            CrystalApproachPointProvider approachProvider,
            BattlefieldAgentRegistry agentRegistry,
            CrystalController crystal)
        {
            stageData = data;
            battlefieldSpace = space;
            crystalApproachPointProvider = approachProvider;
            battlefieldAgentRegistry = agentRegistry;
            crystalController = crystal;
            if (laneManager == null && space != null)
            {
                laneManager = space.GetComponentInParent<LaneManager>();
            }
        }

        public void StartWave(WaveData waveData)
        {
            if (waveData == null)
            {
                Debug.LogWarning("WaveManager cannot start a null wave.");
                return;
            }

            if (battlefieldSpace == null ||
                !battlefieldSpace.IsReady ||
                battlefieldAgentRegistry == null ||
                !battlefieldAgentRegistry.IsReady ||
                crystalApproachPointProvider == null ||
                !crystalApproachPointProvider.IsReady)
            {
                Debug.LogError("WaveManager cannot start because continuous battlefield services are not ready.", this);
                return;
            }

            StopAllCoroutines();
            if (crystalApproachPointProvider != null && crystalApproachPointProvider.ActiveReservationCount != 0)
            {
                Debug.LogError("WaveManager found stale crystal approach reservations before a new wave and cleared them.", crystalApproachPointProvider);
                crystalApproachPointProvider.ResetReservations();
            }

            aliveMonsters.Clear();
            activeWave = waveData;
            pendingSpawns = CountPendingSpawns(waveData);
            activeSpawnRoutines = 0;
            bossReinforcementSpawnCount = 0;
            usedApproachSlots.Clear();

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
                StartCoroutine(SpawnRoutine(spawnData, i, waveData.WaveNo));
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

        public int RequestBossReinforcements(MonsterController boss, int count)
        {
            if (activeWave == null || boss == null || !boss.IsAlive || !boss.IsBoss || battlefieldSpace == null)
            {
                return 0;
            }

            MonsterData reinforcementData = ResolveBossReinforcementData();
            if (reinforcementData == null)
            {
                Debug.LogWarning("Boss reinforcement request was skipped because no non-boss MonsterData exists in the stage.");
                return 0;
            }

            int safeCount = Mathf.Clamp(count, 0, 6);
            if (safeCount <= 0)
            {
                return 0;
            }

            pendingSpawns += safeCount;
            activeSpawnRoutines++;
            StartCoroutine(SpawnBossReinforcementsRoutine(reinforcementData, boss.LaneIndex, safeCount));
            return safeCount;
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
                        battlefieldAgentRegistry?.Unregister(monster.Agent);
                    }
                }
            }

            aliveMonsters.Clear();
            crystalApproachPointProvider?.ResetReservations();
        }

        private IEnumerator SpawnRoutine(WaveSpawnData spawnData, int groupIndex, int waveNumber)
        {
            if (spawnData.StartDelay > 0f)
            {
                yield return new WaitForSeconds(spawnData.StartDelay);
            }

            for (int i = 0; i < spawnData.Count; i++)
            {
                SpawnMonster(spawnData.MonsterData, spawnData.LaneIndex, groupIndex, waveNumber, i);
                pendingSpawns = Mathf.Max(0, pendingSpawns - 1);

                if (i < spawnData.Count - 1 && spawnData.SpawnInterval > 0f)
                {
                    yield return new WaitForSeconds(spawnData.SpawnInterval);
                }
            }

            activeSpawnRoutines = Mathf.Max(0, activeSpawnRoutines - 1);
            CheckWaveComplete();
        }

        private IEnumerator SpawnBossReinforcementsRoutine(MonsterData reinforcementData, int bossLaneIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int laneCount = Mathf.Max(1, laneManager.LaneCount);
                int laneIndex = (Mathf.Clamp(bossLaneIndex, 0, laneCount - 1) + i + 1) % laneCount;
                int groupIndex = activeWave != null && activeWave.Spawns != null ? activeWave.Spawns.Count + 1 : 4;
                int waveNumber = activeWave != null ? activeWave.WaveNo : 1;
                MonsterController reinforcement = SpawnMonster(reinforcementData, laneIndex, groupIndex, waveNumber, i);
                pendingSpawns = Mathf.Max(0, pendingSpawns - 1);
                if (reinforcement != null)
                {
                    bossReinforcementSpawnCount++;
                    BossReinforcementSpawned?.Invoke(reinforcement);
                }

                if (i < count - 1)
                {
                    yield return new WaitForSeconds(0.18f);
                }
            }

            activeSpawnRoutines = Mathf.Max(0, activeSpawnRoutines - 1);
            CheckWaveComplete();
        }

        private MonsterController SpawnMonster(
            MonsterData monsterData,
            int requestedLaneIndex,
            int groupIndex,
            int waveNumber,
            int spawnOrdinal)
        {
            if (monsterData == null)
            {
                Debug.LogWarning("WaveManager skipped a spawn because MonsterData is missing.");
                return null;
            }

            int laneIndex = requestedLaneIndex;
            if (!laneManager.IsValidLaneIndex(laneIndex))
            {
                Debug.LogWarning($"WaveManager clamped invalid lane index {laneIndex}.");
                laneIndex = Mathf.Clamp(laneIndex, 0, laneManager.LaneCount - 1);
            }

            float estimatedHalfWidth = RuntimeSpritePolicy.GetMonsterEstimatedHalfWidth(monsterData);
            float estimatedHalfHeight = RuntimeSpritePolicy.GetMonsterTargetHeight(monsterData) * 0.5f;
            Vector2 estimatedHalfExtents = new Vector2(estimatedHalfWidth, estimatedHalfHeight);
            Vector2 resolvedSpawn = battlefieldSpace.ResolveEnemySpawn(
                laneIndex,
                waveNumber,
                groupIndex,
                spawnOrdinal,
                estimatedHalfExtents);
            Vector3 spawnPosition = new Vector3(resolvedSpawn.x, resolvedSpawn.y, 0f);
            Vector3 targetPosition = battlefieldSpace.CurrentBounds.ToWorld(new Vector2(0.05f, 0.5f));
            MonsterController monster = CreateMonsterInstance(monsterData, spawnPosition);
            MonsterVariantType variantType = ShadowContractService.RollVariant(monsterData, stageData, activeWave);
            monster.Initialize(monsterData, laneIndex, targetPosition, crystalController, this, variantType);
            monster.RefreshBoundsAnchors();
            UnitVisualKind visualKind = monster.IsBoss
                ? UnitVisualKind.Boss
                : monsterData.MonsterType == MonsterType.Flying
                    ? UnitVisualKind.FlyingMonster
                    : UnitVisualKind.Monster;
            battlefieldVisualController?.CreateUnitShadow(monster.transform, monster.VisualSpriteRenderer, visualKind);
            Vector2 actualHalfExtents = monster.VisualSpriteRenderer != null
                ? new Vector2(monster.VisualSpriteRenderer.bounds.extents.x, monster.VisualSpriteRenderer.bounds.extents.y)
                : estimatedHalfExtents;
            Vector2 clampedSpawn = battlefieldSpace.Clamp(monster.transform.position, actualHalfExtents);
            monster.transform.position = new Vector3(clampedSpawn.x, clampedSpawn.y, monster.transform.position.z);
            monster.RefreshBoundsAnchors();

            BattlefieldAgentKind agentKind = monster.IsBoss
                ? BattlefieldAgentKind.Boss
                : monsterData.MonsterType == MonsterType.Flying
                    ? BattlefieldAgentKind.FlyingMonster
                    : BattlefieldAgentKind.GroundMonster;
            BattlefieldAgent agent = monster.GetComponent<BattlefieldAgent>();
            if (agent == null)
            {
                agent = monster.gameObject.AddComponent<BattlefieldAgent>();
            }

            float radius = monster.IsBoss
                ? Mathf.Max(0.8f, actualHalfExtents.x * 0.65f)
                : Mathf.Max(0.2f, actualHalfExtents.x * 0.68f);
            agent.Configure(
                agentKind,
                BattlefieldFaction.Monster,
                nextMonsterStableId++,
                radius,
                actualHalfExtents,
                clampedSpawn,
                battlefieldSpace.CurrentBounds.PlayableRect);
            agent.AttachRegistry(battlefieldAgentRegistry);
            BattlefieldApproachHandle approachHandle = crystalApproachPointProvider.Reserve(agent, laneIndex);
            if (!approachHandle.IsValid)
            {
                Debug.LogError($"WaveManager failed to reserve a crystal approach point for {monster.name}.", monster);
                battlefieldAgentRegistry.Unregister(agent);
                Destroy(monster.gameObject);
                return null;
            }

            usedApproachSlots.Add(approachHandle.PrimarySlotIndex);
            monster.ConfigureSpatialCombat(
                battlefieldSpace,
                battlefieldAgentRegistry,
                crystalApproachPointProvider,
                agent,
                approachHandle,
                clampedSpawn);
            aliveMonsters.Add(monster);
            MonsterSpawned?.Invoke(monster);
            return monster;
        }

        private MonsterData ResolveBossReinforcementData()
        {
            MonsterData fallback = FindNonBossMonster(activeWave, true);
            if (fallback != null && fallback.MonsterType == MonsterType.Normal)
            {
                return fallback;
            }

            if (stageData == null || stageData.Waves == null)
            {
                return fallback;
            }

            for (int i = 0; i < stageData.Waves.Count; i++)
            {
                MonsterData candidate = FindNonBossMonster(stageData.Waves[i], false);
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.MonsterType == MonsterType.Normal)
                {
                    return candidate;
                }

                if (fallback == null)
                {
                    fallback = candidate;
                }
            }

            return fallback;
        }

        private static MonsterData FindNonBossMonster(WaveData wave, bool preferNormal)
        {
            if (wave == null || wave.Spawns == null)
            {
                return null;
            }

            MonsterData fallback = null;
            for (int i = 0; i < wave.Spawns.Count; i++)
            {
                MonsterData candidate = wave.Spawns[i] != null ? wave.Spawns[i].MonsterData : null;
                if (candidate == null || candidate.IsBoss)
                {
                    continue;
                }

                if (!preferNormal || candidate.MonsterType == MonsterType.Normal)
                {
                    return candidate;
                }

                if (fallback == null)
                {
                    fallback = candidate;
                }
            }

            return fallback;
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
