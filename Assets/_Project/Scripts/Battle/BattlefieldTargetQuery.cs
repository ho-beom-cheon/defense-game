using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class BattlefieldTargetQuery
    {
        private readonly BattlefieldAgentRegistry registry;
        private readonly List<BattlefieldAgent> candidates = new List<BattlefieldAgent>();

        public BattlefieldTargetQuery(BattlefieldAgentRegistry agentRegistry)
        {
            registry = agentRegistry;
        }

        public MonsterController FindMonster(HeroController requester, float range, TargetingType targetingType)
        {
            if (requester == null || registry == null)
            {
                return null;
            }

            registry.FillAgentsInCircle(requester.transform.position, range, BattlefieldFaction.Monster, candidates);
            MonsterController selected = null;
            for (int i = 0; i < candidates.Count; i++)
            {
                BattlefieldAgent candidateAgent = candidates[i];
                MonsterController candidate = candidateAgent != null ? candidateAgent.GetComponent<MonsterController>() : null;
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                if (targetingType == TargetingType.Boss && !candidate.IsBoss)
                {
                    continue;
                }

                selected = PickBetterMonster(requester.transform.position, selected, candidate, targetingType);
            }

            return selected;
        }

        public HeroController FindInterceptorAhead(
            MonsterController requester,
            Vector2 destination,
            float lookAheadDistance)
        {
            if (requester == null || requester.Agent == null || registry == null)
            {
                return null;
            }

            Vector2 start = requester.transform.position;
            Vector2 direction = destination - start;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return null;
            }

            Vector2 end = start + direction.normalized * Mathf.Min(direction.magnitude, Mathf.Max(0.1f, lookAheadDistance));
            float capsuleRadius = Mathf.Max(0.45f, requester.Agent.Radius + 0.32f);
            registry.FillAgentsInCapsule(start, end, capsuleRadius, BattlefieldFaction.Hero, candidates);
            HeroController selected = null;
            float bestPathDistance = float.MaxValue;
            float bestCenterDistance = float.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                BattlefieldAgent candidateAgent = candidates[i];
                HeroController candidate = candidateAgent != null ? candidateAgent.GetComponent<HeroController>() : null;
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                Vector2 candidatePosition = candidate.transform.position;
                float forward = Vector2.Dot(candidatePosition - start, direction.normalized);
                if (forward < -0.05f)
                {
                    continue;
                }

                float pathDistance = CombatGeometry.DistanceToSegment(candidatePosition, start, end);
                float centerDistance = Vector2.Distance(start, candidatePosition);
                if (pathDistance < bestPathDistance - 0.001f ||
                    Mathf.Abs(pathDistance - bestPathDistance) <= 0.001f && centerDistance < bestCenterDistance)
                {
                    selected = candidate;
                    bestPathDistance = pathDistance;
                    bestCenterDistance = centerDistance;
                }
            }

            return selected;
        }

        public HeroController FindAttackableBlocker(MonsterController requester, float attackRange)
        {
            if (requester == null || registry == null)
            {
                return null;
            }

            registry.FillAgentsInCircle(requester.transform.position, attackRange, BattlefieldFaction.Hero, candidates);
            HeroController selected = null;
            float bestDistance = float.MaxValue;
            Vector2 start = requester.transform.position;
            Vector2 destination = requester.ObjectivePosition;
            Vector2 route = destination - start;
            Vector2 routeDirection = route.sqrMagnitude > 0.0001f ? route.normalized : Vector2.left;
            for (int i = 0; i < candidates.Count; i++)
            {
                BattlefieldAgent candidateAgent = candidates[i];
                HeroController candidate = candidateAgent != null ? candidateAgent.GetComponent<HeroController>() : null;
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                Vector2 candidatePosition = candidate.transform.position;
                float forward = Vector2.Dot(candidatePosition - start, routeDirection);
                float pathDistance = CombatGeometry.DistanceToSegment(candidatePosition, start, destination);
                float allowedPathDistance = (requester.Agent != null ? requester.Agent.Radius : 0.3f) +
                    candidateAgent.Radius +
                    0.18f;
                if (forward < -0.05f || pathDistance > allowedPathDistance)
                {
                    continue;
                }

                float distance = Vector2.Distance(start, candidatePosition);
                if (distance < bestDistance)
                {
                    selected = candidate;
                    bestDistance = distance;
                }
            }

            return selected;
        }

        public void FillMonstersInRadius(Vector2 center, float radius, List<MonsterController> results)
        {
            results?.Clear();
            if (results == null || registry == null)
            {
                return;
            }

            registry.FillAgentsInCircle(center, radius, BattlefieldFaction.Monster, candidates);
            for (int i = 0; i < candidates.Count; i++)
            {
                MonsterController monster = candidates[i] != null ? candidates[i].GetComponent<MonsterController>() : null;
                if (monster != null && monster.IsAlive)
                {
                    results.Add(monster);
                }
            }
        }

        private static MonsterController PickBetterMonster(
            Vector2 requesterPosition,
            MonsterController current,
            MonsterController candidate,
            TargetingType targetingType)
        {
            if (current == null)
            {
                return candidate;
            }

            switch (targetingType)
            {
                case TargetingType.Nearest:
                    return Vector2.Distance(requesterPosition, candidate.transform.position) <
                        Vector2.Distance(requesterPosition, current.transform.position)
                            ? candidate
                            : current;
                case TargetingType.HighestHp:
                    return candidate.CurrentHp > current.CurrentHp ? candidate : current;
                case TargetingType.LowestHp:
                    return candidate.CurrentHp < current.CurrentHp ? candidate : current;
                case TargetingType.First:
                    float currentProgress = current.Agent != null ? current.Agent.ObjectiveProgress : 0f;
                    float candidateProgress = candidate.Agent != null ? candidate.Agent.ObjectiveProgress : 0f;
                    return candidateProgress > currentProgress ? candidate : current;
                case TargetingType.Boss:
                default:
                    return Vector2.Distance(requesterPosition, candidate.transform.position) <
                        Vector2.Distance(requesterPosition, current.transform.position)
                            ? candidate
                            : current;
            }
        }
    }
}
