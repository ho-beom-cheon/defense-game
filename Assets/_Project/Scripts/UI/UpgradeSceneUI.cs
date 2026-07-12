using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class UpgradeSceneUI : MonoBehaviour
    {
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(190f, 54f, 660f, 500f);
        [SerializeField] private string stageSelectSceneName = "StageSelectScene";

        private string feedbackMessage = string.Empty;
        private Vector2 scrollPosition;
        private bool sceneTransitionRequested;

        private void OnEnable()
        {
            if (upgradeManager == null)
            {
                upgradeManager = FindAnyObjectByType<UpgradeManager>();
            }

            SaveManager.LoadOrCreate();
            if (upgradeManager != null)
            {
                SaveManager.ClampUpgradeLevels(upgradeManager.AvailableUpgrades);
            }

            sceneTransitionRequested = false;
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            UIResponsiveLayout.ApplyReadableDefaults();
            if (upgradeManager == null)
            {
                upgradeManager = FindAnyObjectByType<UpgradeManager>();
            }

            ScreenFrameRects frame = GameFrameLayout.UpgradeFrame();
            GUIStyle panelStyle = RuntimePixelGuiUtility.CreateBoxStyle(GUI.skin.box, RuntimePixelAssetLoader.UiPanelDark);
            GUI.Box(frame.FrameRoot, GUIContent.none, panelStyle);

            GUILayout.BeginArea(frame.HeaderArea, GUI.skin.box);
            GUILayout.Label("\ubd09\ubb38 \uc815\ube44\uc18c");
            GUILayout.Label($"\ubcf4\uc720 \uace8\ub4dc: {SaveManager.Current.totalGold}");
            GUILayout.Label("\uc804\ud22c \uc804\uc5d0 \ubc29\uc5b4\uc120\uc744 \uac15\ud654\ud558\uc138\uc694.");
            GUILayout.EndArea();

            GUILayout.BeginArea(frame.MainArea, GUI.skin.box);
            if (upgradeManager == null || upgradeManager.AvailableUpgrades.Count == 0)
            {
                GUILayout.Label("\uc5c5\uadf8\ub808\uc774\ub4dc \ub370\uc774\ud130\uac00 \uc5c6\uc2b5\ub2c8\ub2e4. Bootstrap Progression Prototype\uc744 \uc2e4\ud589\ud558\uc138\uc694.");
            }
            else
            {
                float scrollHeight = Mathf.Max(120f, frame.MainArea.height - 8f);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollHeight));
                for (int i = 0; i < upgradeManager.AvailableUpgrades.Count; i++)
                {
                    DrawUpgrade(upgradeManager.AvailableUpgrades[i]);
                    GUILayout.Space(UIResponsiveLayout.SmallGap);
                }

                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(frame.FooterArea, GUI.skin.box);
            if (!string.IsNullOrWhiteSpace(feedbackMessage))
            {
                GUILayout.Label(feedbackMessage);
            }

            using (new GuiEnabledScope(!sceneTransitionRequested))
            {
                if (GUILayout.Button("\uc2a4\ud14c\uc774\uc9c0 \uc120\ud0dd\uc73c\ub85c", GUILayout.Height(UIResponsiveLayout.TouchHeight(38f))))
                {
                    LoadSceneOnce(stageSelectSceneName);
                }
            }

            GUILayout.EndArea();
        }

        private void DrawUpgrade(UpgradeData upgradeData)
        {
            if (upgradeData == null)
            {
                GUILayout.Label("\uc5c5\uadf8\ub808\uc774\ub4dc \ub370\uc774\ud130 \uc5c6\uc74c");
                return;
            }

            int level = upgradeManager.GetLevel(upgradeData);
            int maxLevel = Mathf.Max(0, upgradeData.MaxLevel);
            int cost = upgradeManager.GetCost(upgradeData);
            bool maxed = level >= maxLevel;
            bool canAfford = SaveManager.Current.totalGold >= cost;

            GUILayout.BeginHorizontal(GUI.skin.box);
            RuntimePixelGuiUtility.DrawIcon(GetUpgradeIconPath(upgradeData), 42f);
            GUILayout.BeginVertical(GUILayout.MinWidth(180f));
            string displayName = GameTextMapper.UpgradeName(upgradeData);
            GUILayout.Label($"{displayName}  \ub808\ubca8 {level}/{maxLevel}");
            GUILayout.Label(GameTextMapper.UpgradeDescription(upgradeData));
            GUILayout.Label($"\ud604\uc7ac \ud6a8\uacfc: {FormatTotalEffect(upgradeData, level)}");
            GUILayout.Label(maxed ? "\ub2e4\uc74c \ud6a8\uacfc: \ucd5c\ub300 \ub808\ubca8" : $"\ub2e4\uc74c \ud6a8\uacfc: {FormatTotalEffect(upgradeData, level + 1)}");
            GUILayout.Label(maxed ? "\ube44\uc6a9: -" : $"\ube44\uc6a9: {cost} \uace8\ub4dc");

            using (new GuiEnabledScope(!maxed && canAfford))
            {
                string buttonLabel = maxed ? "\ucd5c\ub300 \ub808\ubca8" : canAfford ? "\uad6c\ub9e4" : "\uace8\ub4dc \ubd80\uc871";
                if (GUILayout.Button(buttonLabel, GUILayout.Height(UIResponsiveLayout.TouchHeight(30f))))
                {
                    bool purchased = upgradeManager.TryPurchase(upgradeData);
                    if (purchased)
                    {
                        AudioManager.Play(SfxKey.UpgradePurchase);
                    }

                    feedbackMessage = purchased ? $"{displayName} \uac15\ud654 \uc644\ub8cc." : $"{displayName} \uad6c\ub9e4\uc5d0 \ud544\uc694\ud55c \uace8\ub4dc\uac00 \ubd80\uc871\ud569\ub2c8\ub2e4.";
                    GUIUtility.ExitGUI();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private static string FormatTotalEffect(UpgradeData upgradeData, int level)
        {
            if (upgradeData == null)
            {
                return "-";
            }

            float value = upgradeData.ValuePerLevel * Mathf.Max(0, level);
            string valueText;
            if (upgradeData.EffectKey == UpgradeManager.SkillCooldownPercent)
            {
                valueText = $"-{Mathf.Abs(value) * 100f:0.#}%";
            }
            else
            {
                valueText = Mathf.Abs(value) < 1f ? $"{value * 100f:0.#}%" : $"+{value:0.#}";
            }
            return $"{GameTextMapper.UpgradeEffectName(upgradeData.EffectKey)} {valueText}";
        }

        private static string GetUpgradeIconPath(UpgradeData upgradeData)
        {
            if (upgradeData == null)
            {
                return RuntimePixelAssetLoader.UiUpgradeHeroAttack;
            }

            switch (upgradeData.EffectKey)
            {
                case UpgradeManager.CrystalMaxHpFlat:
                    return RuntimePixelAssetLoader.UiUpgradeCrystalHp;
                case UpgradeManager.HeroAttackSpeedPercent:
                    return RuntimePixelAssetLoader.UiUpgradeAttackSpeed;
                case UpgradeManager.SkillCooldownPercent:
                    return RuntimePixelAssetLoader.UiUpgradeSkillCooldown;
                default:
                    return RuntimePixelAssetLoader.UiUpgradeHeroAttack;
            }
        }

        private void LoadSceneOnce(string sceneName)
        {
            if (sceneTransitionRequested)
            {
                return;
            }

            sceneTransitionRequested = true;
            SceneManager.LoadScene(sceneName);
        }

        private readonly struct GuiEnabledScope : System.IDisposable
        {
            private readonly bool previousValue;

            public GuiEnabledScope(bool enabled)
            {
                previousValue = GUI.enabled;
                GUI.enabled = enabled;
            }

            public void Dispose()
            {
                GUI.enabled = previousValue;
            }
        }
    }
}
