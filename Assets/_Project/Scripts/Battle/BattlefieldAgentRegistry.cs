using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    [DisallowMultipleComponent]
    public sealed class BattlefieldAgentRegistry : MonoBehaviour
    {
        private readonly List<BattlefieldAgent> agents = new List<BattlefieldAgent>();
        [SerializeField] private BattlefieldSpaceController space;

        public bool IsReady => space != null && space.IsReady;
        public IReadOnlyList<BattlefieldAgent> ActiveAgents => agents;

        public void Configure(BattlefieldSpaceController battlefieldSpace)
        {
            space = battlefieldSpace;
            RemoveInvalidAgents();
        }

        public void Register(BattlefieldAgent agent)
        {
            if (agent == null || !agent.IsConfigured || agents.Contains(agent))
            {
                return;
            }

            agents.Add(agent);
        }

        public void Unregister(BattlefieldAgent agent)
        {
            if (agent != null)
            {
                agents.Remove(agent);
            }
        }

        public void FillNeighbors(BattlefieldAgent requester, float radius, List<BattlefieldAgent> results)
        {
            results?.Clear();
            if (requester == null || results == null)
            {
                return;
            }

            float radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                BattlefieldAgent candidate = agents[i];
                if (candidate == null)
                {
                    agents.RemoveAt(i);
                    continue;
                }

                if (candidate != requester &&
                    candidate.Faction == requester.Faction &&
                    ((Vector2)candidate.transform.position - (Vector2)requester.transform.position).sqrMagnitude <= radiusSquared)
                {
                    results.Add(candidate);
                }
            }
        }

        public void FillAgentsInCircle(Vector2 center, float radius, BattlefieldFaction faction, List<BattlefieldAgent> results)
        {
            results?.Clear();
            if (results == null)
            {
                return;
            }

            float radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                BattlefieldAgent candidate = agents[i];
                if (candidate == null)
                {
                    agents.RemoveAt(i);
                    continue;
                }

                if (candidate.Faction == faction &&
                    ((Vector2)candidate.transform.position - center).sqrMagnitude <= radiusSquared)
                {
                    results.Add(candidate);
                }
            }
        }

        public void FillAgentsInCapsule(
            Vector2 start,
            Vector2 end,
            float radius,
            BattlefieldFaction faction,
            List<BattlefieldAgent> results)
        {
            results?.Clear();
            if (results == null)
            {
                return;
            }

            float radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            Vector2 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                BattlefieldAgent candidate = agents[i];
                if (candidate == null)
                {
                    agents.RemoveAt(i);
                    continue;
                }

                if (candidate.Faction != faction)
                {
                    continue;
                }

                Vector2 point = candidate.transform.position;
                float t = lengthSquared > 0.0001f ? Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared) : 0f;
                Vector2 closest = start + segment * t;
                if ((point - closest).sqrMagnitude <= radiusSquared)
                {
                    results.Add(candidate);
                }
            }
        }

        public void RemapAgents(BattlefieldBounds previous, BattlefieldBounds next)
        {
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                BattlefieldAgent agent = agents[i];
                if (agent == null)
                {
                    agents.RemoveAt(i);
                    continue;
                }

                agent.Remap(previous, next);
            }
        }

        public void Clear()
        {
            agents.Clear();
        }

        private void RemoveInvalidAgents()
        {
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                if (agents[i] == null || !agents[i].IsConfigured)
                {
                    agents.RemoveAt(i);
                }
            }
        }
    }
}
