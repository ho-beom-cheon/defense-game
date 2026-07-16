using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public readonly struct BattlefieldApproachHandle
    {
        public BattlefieldApproachHandle(int reservationId, int generation, int primarySlotIndex, int ownerStableId)
        {
            ReservationId = reservationId;
            Generation = generation;
            PrimarySlotIndex = primarySlotIndex;
            OwnerStableId = ownerStableId;
        }

        public int ReservationId { get; }
        public int Generation { get; }
        public int PrimarySlotIndex { get; }
        public int OwnerStableId { get; }
        public bool IsValid => ReservationId > 0 && Generation > 0 && PrimarySlotIndex >= 0 && OwnerStableId != 0;
    }

    [DisallowMultipleComponent]
    public sealed class CrystalApproachPointProvider : MonoBehaviour
    {
        private sealed class Reservation
        {
            public int ReservationId;
            public int Generation;
            public int OwnerStableId;
            public int PrimarySlotIndex;
            public int[] SlotIndices;
        }

        [SerializeField] private BattlefieldSpaceController battlefieldSpace;
        [SerializeField] private BattlefieldSpaceConfig config;

        private readonly Dictionary<int, Reservation> reservations = new Dictionary<int, Reservation>();
        private Vector2[] positions = new Vector2[0];
        private int[] occupancy = new int[0];
        private int nextReservationId = 1;
        private int generation = 1;

        public bool IsReady => battlefieldSpace != null && battlefieldSpace.IsReady && config != null && config.ApproachPointCount == 7;
        public int ActiveReservationCount => reservations.Count;
        public int ApproachPointCount => positions.Length;

        public void Configure(BattlefieldSpaceController space, BattlefieldSpaceConfig spaceConfig)
        {
            battlefieldSpace = space;
            config = spaceConfig;
            EnsureStorage();
            RefreshLayout();
        }

        public BattlefieldApproachHandle Reserve(BattlefieldAgent agent, int spawnBandIndex)
        {
            if (!IsReady || agent == null || !agent.IsConfigured || agent.StableId == 0)
            {
                return default;
            }

            ReleaseByOwner(agent.StableId);
            int primary = FindBestSlot(agent.transform.position, spawnBandIndex);
            int[] slots = agent.Kind == BattlefieldAgentKind.Boss
                ? ResolveBossSlots(primary)
                : new[] { primary };

            Reservation reservation = new Reservation
            {
                ReservationId = nextReservationId++,
                Generation = generation,
                OwnerStableId = agent.StableId,
                PrimarySlotIndex = primary,
                SlotIndices = slots
            };
            reservations.Add(reservation.ReservationId, reservation);
            for (int i = 0; i < slots.Length; i++)
            {
                occupancy[slots[i]]++;
            }

            return new BattlefieldApproachHandle(
                reservation.ReservationId,
                reservation.Generation,
                reservation.PrimarySlotIndex,
                reservation.OwnerStableId);
        }

        public bool TryGetPosition(BattlefieldApproachHandle handle, out Vector2 position)
        {
            position = default;
            if (!TryGetReservation(handle, out Reservation reservation) ||
                reservation.PrimarySlotIndex < 0 ||
                reservation.PrimarySlotIndex >= positions.Length)
            {
                return false;
            }

            position = positions[reservation.PrimarySlotIndex];
            return true;
        }

        public void Release(BattlefieldApproachHandle handle)
        {
            if (!TryGetReservation(handle, out Reservation reservation))
            {
                return;
            }

            ReleaseReservation(reservation);
        }

        public void ReleaseByOwner(int ownerStableId)
        {
            if (ownerStableId == 0 || reservations.Count == 0)
            {
                return;
            }

            Reservation match = null;
            foreach (KeyValuePair<int, Reservation> pair in reservations)
            {
                if (pair.Value.OwnerStableId == ownerStableId)
                {
                    match = pair.Value;
                    break;
                }
            }

            if (match != null)
            {
                ReleaseReservation(match);
            }
        }

        public void ResetReservations()
        {
            reservations.Clear();
            generation = generation == int.MaxValue ? 1 : generation + 1;
            nextReservationId = 1;
            EnsureStorage();
            for (int i = 0; i < occupancy.Length; i++)
            {
                occupancy[i] = 0;
            }
        }

        public void RefreshLayout()
        {
            EnsureStorage();
            if (!IsReady)
            {
                return;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = battlefieldSpace.CurrentBounds.ToWorld(new Vector2(
                    config.GetApproachPointU(i),
                    config.GetApproachPointV(i)));
            }
        }

        public bool TryGetPoint(int index, out Vector2 position)
        {
            position = default;
            if (index < 0 || index >= positions.Length)
            {
                return false;
            }

            position = positions[index];
            return true;
        }

        private void OnDisable()
        {
            ResetReservations();
        }

        private int FindBestSlot(Vector2 origin, int spawnBandIndex)
        {
            Vector2 band = config.GetSpawnBandV(spawnBandIndex);
            float bandCenter = (band.x + band.y) * 0.5f;
            float bestCost = float.MaxValue;
            int bestIndex = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                float distance = Vector2.Distance(origin, positions[i]);
                float congestion = occupancy[i] * config.ApproachCongestionCost;
                float bandBias = Mathf.Abs(config.GetApproachPointV(i) - bandCenter) *
                    battlefieldSpace.CurrentBounds.PlayableRect.height *
                    config.ApproachBandBias;
                float cost = distance + congestion + bandBias;
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private int[] ResolveBossSlots(int primary)
        {
            if (positions.Length < 3)
            {
                return new[] { primary };
            }

            int center = Mathf.Clamp(primary, 1, positions.Length - 2);
            return new[] { center - 1, center, center + 1 };
        }

        private bool TryGetReservation(BattlefieldApproachHandle handle, out Reservation reservation)
        {
            reservation = null;
            return handle.IsValid &&
                handle.Generation == generation &&
                reservations.TryGetValue(handle.ReservationId, out reservation) &&
                reservation.Generation == handle.Generation &&
                reservation.OwnerStableId == handle.OwnerStableId &&
                reservation.PrimarySlotIndex == handle.PrimarySlotIndex;
        }

        private void ReleaseReservation(Reservation reservation)
        {
            if (reservation == null || !reservations.Remove(reservation.ReservationId))
            {
                return;
            }

            for (int i = 0; i < reservation.SlotIndices.Length; i++)
            {
                int slot = reservation.SlotIndices[i];
                if (slot >= 0 && slot < occupancy.Length)
                {
                    occupancy[slot] = Mathf.Max(0, occupancy[slot] - 1);
                }
            }
        }

        private void EnsureStorage()
        {
            int count = config != null ? config.ApproachPointCount : 0;
            if (count <= 0)
            {
                positions = new Vector2[0];
                occupancy = new int[0];
                return;
            }

            if (positions.Length != count)
            {
                positions = new Vector2[count];
            }

            if (occupancy.Length != count)
            {
                occupancy = new int[count];
            }
        }
    }
}
