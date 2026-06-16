using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class RuneSelectionUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private RuneManager runeManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(430f, 96f, 420f, 280f);

        private readonly List<RuneData> displayedRunes = new List<RuneData>();
        private bool isVisible;
        private Vector2 scrollPosition;

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

            KoreanFontManager.ApplyToGuiSkin();
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUIStyle cardStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiRuneCardBase);
            GUILayout.BeginArea(panelRect, panelStyle);
            GUILayout.Label("룬 선택");
            GUILayout.Space(8f);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < displayedRunes.Count; i++)
            {
                RuneData runeData = displayedRunes[i];
                if (runeData == null)
                {
                    continue;
                }

                GUILayout.BeginVertical(cardStyle);
                GUILayout.Label($"{runeData.DisplayName} ({runeData.Rarity})");
                GUILayout.Label(runeData.Description);
                GUILayout.Label($"{runeData.EffectKey}: {runeData.Value:0.##}");
                if (GUILayout.Button("선택", GUILayout.Height(28f)))
                {
                    SelectOption(i);
                }

                GUILayout.EndVertical();
                GUILayout.Space(6f);
            }

            GUILayout.EndScrollView();
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
                battleManager = FindAnyObjectByType<BattleManager>();
            }

            if (runeManager == null)
            {
                runeManager = FindAnyObjectByType<RuneManager>();
            }
        }
    }
}
