using System.Collections.Generic;

namespace RuneGate
{
    public sealed class FormationEditorState
    {
        private const int LaneCount = 3;
        private const int MaximumSlotCount = 9;

        private readonly List<FormationSlot> slots = new List<FormationSlot>();

        public IReadOnlyList<FormationSlot> Slots => slots;
        public string SelectedHeroId { get; private set; } = string.Empty;
        public int Count => slots.Count;

        public void Load(IReadOnlyList<FormationSlot> sourceSlots)
        {
            slots.Clear();
            SelectedHeroId = string.Empty;
            if (sourceSlots == null)
            {
                return;
            }

            HashSet<string> heroIds = new HashSet<string>();
            HashSet<string> slotKeys = new HashSet<string>();
            for (int i = 0; i < sourceSlots.Count && slots.Count < MaximumSlotCount; i++)
            {
                FormationSlot slot = sourceSlots[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.HeroId) || !IsValidLane(slot.LaneIndex))
                {
                    continue;
                }

                string slotKey = BuildSlotKey(slot.LaneIndex, slot.PositionType);
                if (!heroIds.Add(slot.HeroId) || !slotKeys.Add(slotKey))
                {
                    continue;
                }

                slots.Add(CopySlot(slot));
            }
        }

        public bool SelectHero(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId))
            {
                return false;
            }

            SelectedHeroId = heroId;
            return true;
        }

        public string HeroIdAt(int laneIndex, HeroPositionType positionType)
        {
            int index = FindSlotIndex(laneIndex, positionType);
            return index >= 0 ? slots[index].HeroId : string.Empty;
        }

        public bool ContainsHero(string heroId)
        {
            return FindHeroIndex(heroId) >= 0;
        }

        public bool TryPlaceSelected(int laneIndex, HeroPositionType positionType, out string message)
        {
            if (string.IsNullOrWhiteSpace(SelectedHeroId))
            {
                message = "먼저 영웅을 선택하세요.";
                return false;
            }

            if (!IsValidLane(laneIndex))
            {
                message = "선택한 라인이 올바르지 않습니다.";
                return false;
            }

            int selectedIndex = FindHeroIndex(SelectedHeroId);
            int targetIndex = FindSlotIndex(laneIndex, positionType);
            if (selectedIndex == targetIndex && selectedIndex >= 0)
            {
                message = "이미 해당 슬롯에 배치되어 있습니다.";
                return true;
            }

            if (targetIndex >= 0 && selectedIndex < 0)
            {
                message = "빈 슬롯을 선택하거나 배치된 영웅을 먼저 이동하세요.";
                return false;
            }

            if (targetIndex >= 0)
            {
                FormationSlot selectedSlot = slots[selectedIndex];
                FormationSlot targetSlot = slots[targetIndex];
                slots[selectedIndex] = new FormationSlot(selectedSlot.LaneIndex, selectedSlot.PositionType, targetSlot.HeroId);
                slots[targetIndex] = new FormationSlot(laneIndex, positionType, SelectedHeroId);
                message = "두 영웅의 위치를 교체하고 저장했습니다.";
                return true;
            }

            if (selectedIndex >= 0)
            {
                slots[selectedIndex] = new FormationSlot(laneIndex, positionType, SelectedHeroId);
                message = "영웅 위치를 이동하고 저장했습니다.";
                return true;
            }

            if (slots.Count >= MaximumSlotCount)
            {
                message = "편성 슬롯이 가득 찼습니다.";
                return false;
            }

            slots.Add(new FormationSlot(laneIndex, positionType, SelectedHeroId));
            message = "영웅을 편성하고 저장했습니다.";
            return true;
        }

        public bool TryRemoveSelected(out string message)
        {
            int selectedIndex = FindHeroIndex(SelectedHeroId);
            if (selectedIndex < 0)
            {
                message = "선택한 영웅은 현재 편성에 없습니다.";
                return false;
            }

            if (slots.Count <= 1)
            {
                message = "전투를 위해 영웅 한 명은 남겨야 합니다.";
                return false;
            }

            slots.RemoveAt(selectedIndex);
            message = "영웅을 편성에서 제외하고 저장했습니다.";
            return true;
        }

        public List<FormationSlot> CreateCopy()
        {
            List<FormationSlot> copy = new List<FormationSlot>(slots.Count);
            for (int i = 0; i < slots.Count; i++)
            {
                copy.Add(CopySlot(slots[i]));
            }

            return copy;
        }

        private int FindHeroIndex(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId))
            {
                return -1;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].HeroId == heroId)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindSlotIndex(int laneIndex, HeroPositionType positionType)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                FormationSlot slot = slots[i];
                if (slot.LaneIndex == laneIndex && slot.PositionType == positionType)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsValidLane(int laneIndex)
        {
            return laneIndex >= 0 && laneIndex < LaneCount;
        }

        private static string BuildSlotKey(int laneIndex, HeroPositionType positionType)
        {
            return $"{laneIndex}:{positionType}";
        }

        private static FormationSlot CopySlot(FormationSlot slot)
        {
            return new FormationSlot(slot.LaneIndex, slot.PositionType, slot.HeroId);
        }
    }
}
