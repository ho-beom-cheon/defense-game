using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    [CreateAssetMenu(fileName = "FormationData", menuName = "RuneGate/Data/Formation")]
    public sealed class FormationData : ScriptableObject
    {
        [SerializeField] private string formationId = "formation_default";
        [SerializeField] private string displayName = "Default Formation";
        [SerializeField] private List<FormationSlot> slots = new List<FormationSlot>();

        public string FormationId => formationId;
        public string DisplayName => displayName;
        public IReadOnlyList<FormationSlot> Slots => slots;
    }
}
