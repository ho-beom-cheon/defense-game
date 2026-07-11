using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class RuneSelectionUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private RuneManager runeManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private bool showDebugEffectKey;
        [SerializeField] private Rect panelRect = new Rect(380f, 78f, 520f, 360f);

        private readonly List<RuneData> displayedRunes = new List<RuneData>();
        private bool isVisible;
        private bool selectionRequested;
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

            UIResponsiveLayout.ApplyReadableDefaults();
            UIPopupGuiUtility.DrawDimOverlay(0.45f);

            Rect drawRect = CenteredPanelRect();
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUIStyle cardStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiRuneCardBase);
            GUILayout.BeginArea(drawRect, panelStyle);
            GUI.SetNextControlName("PopupLayer_RuneSelectionPopup");
            GUILayout.Label("\uc804\uc220 \uae30\ub85d\uc11c - \ub8ec \uc120\ud0dd");
            GUILayout.Label("\ub2e4\uc74c \uc6e8\uc774\ube0c\ub97c \ubc84\ud2f8 \ub8ec \ud558\ub098\ub97c \uc120\ud0dd\ud558\uc138\uc694.");
            GUILayout.Space(UIResponsiveLayout.SmallGap);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Mathf.Max(180f, drawRect.height - 82f)));
            for (int i = 0; i < displayedRunes.Count; i++)
            {
                RuneData runeData = displayedRunes[i];
                if (runeData == null)
                {
                    continue;
                }

                GUILayout.BeginVertical(cardStyle);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{runeData.DisplayName} ({GameTextMapper.RuneRarityName(runeData.Rarity)})", GUILayout.Width(Mathf.Max(160f, drawRect.width - 170f)));
                GUILayout.FlexibleSpace();
                bool previousEnabled = GUI.enabled;
                GUI.enabled = !selectionRequested;
                if (GUILayout.Button(selectionRequested ? "\uc801\uc6a9 \uc911" : "\uc120\ud0dd", GUILayout.Width(90f), GUILayout.Height(30f)))
                {
                    SelectOption(i);
                }

                GUI.enabled = previousEnabled;
                GUILayout.EndHorizontal();
                GUILayout.Label(runeData.Description);
                GUILayout.Label($"\ud6a8\uacfc \uc218\uce58: {FormatRuneValue(runeData)}");
                if (showDebugEffectKey)
                {
                    GUILayout.Label($"DEBUG {runeData.EffectKey}: {runeData.Value:0.##}");
                }

                GUILayout.EndVertical();
                GUILayout.Space(UIResponsiveLayout.SmallGap);
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

            if (selectionRequested)
            {
                return;
            }

            selectionRequested = true;
            runeManager.SelectRuneAt(index);
        }

        private void ShowOptions(IReadOnlyList<RuneData> options)
        {
            displayedRunes.Clear();
            selectionRequested = false;
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
            selectionRequested = false;
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

        private Rect CenteredPanelRect()
        {
            return GameFrameLayout.PopupFrame(Mathf.Max(panelRect.width, 620f), Mathf.Max(panelRect.height, 520f), 0.92f, 0.78f);
        }

        private static string FormatRuneValue(RuneData runeData)
        {
            if (runeData == null)
            {
                return "-";
            }

            float value = runeData.Value;
            return Mathf.Abs(value) < 1f ? $"{value * 100f:0.#}%" : $"+{value:0.#}";
        }

    }
}
