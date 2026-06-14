using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class RuneSelectionUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private RuneManager runeManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(300f, 110f, 460f, 310f);

        private readonly List<RuneData> displayedRunes = new List<RuneData>();
        private bool isVisible;

        public IReadOnlyList<RuneData> DisplayedRunes => displayedRunes;
        public bool IsVisible => isVisible;

        private void OnEnable()
        {
            AutoAssignReferences();

            if (battleManager != null)
            {
                battleManager.RuneOptionsOffered += ShowOptions;
                battleManager.BattleStateChanged += HandleBattleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.RuneOptionsOffered -= ShowOptions;
                battleManager.BattleStateChanged -= HandleBattleStateChanged;
            }
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui || !isVisible)
            {
                return;
            }

            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label("Choose 1 Rune");
            GUILayout.Space(8f);

            for (int i = 0; i < displayedRunes.Count; i++)
            {
                RuneData runeData = displayedRunes[i];
                if (runeData == null)
                {
                    continue;
                }

                string buttonText = $"{runeData.DisplayName} ({runeData.Rarity})\n{runeData.Description}\n{runeData.EffectKey}: {runeData.Value:0.##}";
                if (GUILayout.Button(buttonText, GUILayout.Height(78f)))
                {
                    SelectOption(i);
                }
            }

            GUILayout.EndArea();
        }

        public void SelectOption(int index)
        {
            if (runeManager == null)
            {
                Debug.LogWarning("RuneSelectionUI cannot select a rune because RuneManager is missing.");
                return;
            }

            runeManager.SelectRuneAt(index);
        }

        private void ShowOptions(IReadOnlyList<RuneData> options)
        {
            displayedRunes.Clear();
            for (int i = 0; i < options.Count; i++)
            {
                displayedRunes.Add(options[i]);
            }

            isVisible = displayedRunes.Count > 0;
        }

        private void HandleBattleStateChanged(BattleState battleState)
        {
            if (battleState != BattleState.RuneSelection)
            {
                Hide();
            }
        }

        private void Hide()
        {
            isVisible = false;
            displayedRunes.Clear();
        }

        private void AutoAssignReferences()
        {
            if (battleManager == null)
            {
                battleManager = FindFirstObjectByType<BattleManager>();
            }

            if (runeManager == null)
            {
                runeManager = FindFirstObjectByType<RuneManager>();
            }
        }
    }
}
