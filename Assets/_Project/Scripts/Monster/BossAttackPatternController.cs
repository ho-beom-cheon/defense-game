using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    public sealed class BossAttackPatternController : MonoBehaviour
    {
        [SerializeField] private float initialDelay = 2.5f;
        [SerializeField] private float phaseOneInterval = 7f;
        [SerializeField] private float phaseTwoInterval = 5.5f;
        [SerializeField] private float phaseThreeInterval = 4.25f;
        [SerializeField] private float telegraphDuration = 0.6f;

        private MonsterController boss;
        private BossPhaseController phaseController;
        private CrystalController crystalController;
        private BattlefieldSpaceController battlefieldSpace;
        private Coroutine patternRoutine;
        private float cooldownRemaining;
        private int totalPatternsResolved;
        private int totalHeroesHit;
        private int totalCrystalDamage;
        private int lastPatternPhase;

        public event Action<int, int, int> PatternResolved;

        public int TotalPatternsResolved => totalPatternsResolved;
        public int TotalHeroesHit => totalHeroesHit;
        public int TotalCrystalDamage => totalCrystalDamage;
        public int LastPatternPhase => lastPatternPhase;
        public bool IsPatternRunning => patternRoutine != null;

        public void Configure(MonsterController bossController, BossPhaseController bossPhaseController, CrystalController crystal)
        {
            if (phaseController != null)
            {
                phaseController.PhaseChanged -= HandlePhaseChanged;
            }

            boss = bossController;
            phaseController = bossPhaseController;
            crystalController = crystal;
            battlefieldSpace = FindAnyObjectByType<BattlefieldSpaceController>();
            cooldownRemaining = Mathf.Max(0.1f, initialDelay);
            totalPatternsResolved = 0;
            totalHeroesHit = 0;
            totalCrystalDamage = 0;
            lastPatternPhase = 0;
            if (phaseController != null)
            {
                phaseController.PhaseChanged += HandlePhaseChanged;
            }
        }

        public static int BaseDamageForPhase(int phase)
        {
            switch (Mathf.Clamp(phase, 1, 3))
            {
                case 1:
                    return 4;
                case 2:
                    return 6;
                default:
                    return 8;
            }
        }

        public static int BaseCrystalDamageForPhase(int phase)
        {
            return phase >= 3 ? 4 : 0;
        }

        private void Update()
        {
            if (boss == null || phaseController == null || !boss.IsAlive || patternRoutine != null)
            {
                return;
            }

            cooldownRemaining -= Time.deltaTime;
            if (cooldownRemaining <= 0f)
            {
                patternRoutine = StartCoroutine(ExecutePatternRoutine(Mathf.Clamp(phaseController.CurrentPhase, 1, 3)));
            }
        }

        private void OnDisable()
        {
            if (phaseController != null)
            {
                phaseController.PhaseChanged -= HandlePhaseChanged;
            }

            if (patternRoutine != null)
            {
                StopCoroutine(patternRoutine);
                patternRoutine = null;
            }
        }

        private IEnumerator ExecutePatternRoutine(int phase)
        {
            List<HeroController> targets = SelectTargets(phase);
            for (int i = 0; i < targets.Count; i++)
            {
                HeroController hero = targets[i];
                if (hero != null && hero.IsAlive)
                {
                    CombatVisualEffectFactory.SpawnBossTelegraph(hero.transform.position, phase, telegraphDuration);
                }
            }

            if (phase >= 3 && crystalController != null && crystalController.CurrentHp > 0)
            {
                CombatVisualEffectFactory.SpawnBossTelegraph(crystalController.transform.position, phase, telegraphDuration);
            }

            yield return new WaitForSeconds(Mathf.Max(0.1f, telegraphDuration));
            if (boss == null || !boss.IsAlive)
            {
                patternRoutine = null;
                yield break;
            }

            if (phaseController != null && phaseController.CurrentPhase > phase)
            {
                cooldownRemaining = 0.15f;
                patternRoutine = null;
                yield break;
            }

            int heroDamage = DifficultyRules.ApplyMonsterCrystalDamage(BaseDamageForPhase(phase));
            int heroesHit = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                HeroController hero = targets[i];
                if (hero == null ||
                    !hero.IsAlive ||
                    phase <= 1 && !IsInSelectedBand(hero.transform.position))
                {
                    continue;
                }

                hero.TakeDamage(heroDamage);
                CombatVisualEffectFactory.SpawnBossPatternImpact(hero.transform.position, phase);
                heroesHit++;
            }

            int crystalDamage = 0;
            if (phase >= 3 && crystalController != null && crystalController.CurrentHp > 0)
            {
                crystalDamage = DifficultyRules.ApplyMonsterCrystalDamage(BaseCrystalDamageForPhase(phase));
                crystalController.TakeDamage(crystalDamage);
                CombatVisualEffectFactory.SpawnBossPatternImpact(crystalController.transform.position, phase);
                CombatFeedbackEvents.RaiseCrystalDamaged(boss.transform.position);
            }

            AudioManager.Play(SfxKey.CrystalHit);
            lastPatternPhase = phase;
            totalPatternsResolved++;
            totalHeroesHit += heroesHit;
            totalCrystalDamage += crystalDamage;
            PatternResolved?.Invoke(phase, heroesHit, crystalDamage);
            Debug.Log($"{boss.Data.DisplayNameKorean} resolved boss pattern phase {phase}. HeroesHit={heroesHit}, CrystalDamage={crystalDamage}.");

            cooldownRemaining = phaseController != null && phaseController.CurrentPhase > phase
                ? 0.15f
                : ResolveInterval(phase);
            patternRoutine = null;
        }

        private void HandlePhaseChanged(int phase)
        {
            cooldownRemaining = Mathf.Min(cooldownRemaining, 0.15f);
        }

        private List<HeroController> SelectTargets(int phase)
        {
            IReadOnlyList<HeroController> heroes = HeroController.ActiveHeroes;
            List<HeroController> targets = new List<HeroController>();
            if (phase <= 1)
            {
                for (int i = 0; i < heroes.Count; i++)
                {
                    HeroController hero = heroes[i];
                    if (hero != null && hero.IsAlive && IsInSelectedBand(hero.transform.position))
                    {
                        targets.Add(hero);
                    }
                }

                return targets;
            }

            if (phase == 2)
            {
                Dictionary<int, HeroController> frontByBand = new Dictionary<int, HeroController>();
                for (int i = 0; i < heroes.Count; i++)
                {
                    HeroController hero = heroes[i];
                    if (hero == null || !hero.IsAlive)
                    {
                        continue;
                    }

                    int bandIndex = ResolveBandIndex(hero.transform.position);
                    if (!frontByBand.TryGetValue(bandIndex, out HeroController selected) ||
                        hero.transform.position.x > selected.transform.position.x)
                    {
                        frontByBand[bandIndex] = hero;
                    }
                }

                foreach (HeroController hero in frontByBand.Values)
                {
                    targets.Add(hero);
                }

                return targets;
            }

            for (int i = 0; i < heroes.Count; i++)
            {
                HeroController hero = heroes[i];
                if (hero != null && hero.IsAlive)
                {
                    targets.Add(hero);
                }
            }

            return targets;
        }

        private bool IsInSelectedBand(Vector2 position)
        {
            if (battlefieldSpace == null || !battlefieldSpace.IsReady || boss == null)
            {
                return Vector2.Distance(position, boss != null ? boss.transform.position : Vector3.zero) <= 2.2f;
            }

            Rect playable = battlefieldSpace.CurrentBounds.PlayableRect;
            float centerV = battlefieldSpace.Config.GetFormationRowV(Mathf.Clamp(boss.LaneIndex, 0, 2));
            float centerY = Mathf.Lerp(playable.yMin, playable.yMax, centerV);
            float halfHeight = playable.height * 0.18f;
            return position.y >= centerY - halfHeight && position.y <= centerY + halfHeight;
        }

        private int ResolveBandIndex(Vector2 position)
        {
            if (battlefieldSpace == null || !battlefieldSpace.IsReady)
            {
                return position.y < -0.5f ? 0 : position.y > 0.5f ? 2 : 1;
            }

            float v = battlefieldSpace.CurrentBounds.ToNormalized(position).y;
            return v < 0.34f ? 0 : v > 0.66f ? 2 : 1;
        }

        private float ResolveInterval(int phase)
        {
            switch (Mathf.Clamp(phase, 1, 3))
            {
                case 1:
                    return Mathf.Max(1f, phaseOneInterval);
                case 2:
                    return Mathf.Max(1f, phaseTwoInterval);
                default:
                    return Mathf.Max(1f, phaseThreeInterval);
            }
        }
    }
}
