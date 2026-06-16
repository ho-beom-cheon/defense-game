using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class UpgradeSceneUI : MonoBehaviour
    {
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private bool drawRuntimeGui = true;
        [SerializeField] private Rect panelRect = new Rect(220f, 70f, 560f, 440f);
        [SerializeField] private string stageSelectSceneName = "StageSelectScene";

        private string feedbackMessage = string.Empty;

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
        }

        private void OnGUI()
        {
            if (!drawRuntimeGui)
            {
                return;
            }

            if (upgradeManager == null)
            {
                upgradeManager = FindAnyObjectByType<UpgradeManager>();
            }

            GUILayout.BeginArea(panelRect, GUI.skin.box);
            GUILayout.Label("Upgrades");
            GUILayout.Label($"Gold: {SaveManager.Current.totalGold}");
            RuntimePixelGuiUtility.DrawIcon(RuntimePixelAssetLoader.UiIconSave, 22f);
            GUILayout.Space(8f);

            if (upgradeManager == null || upgradeManager.AvailableUpgrades.Count == 0)
            {
                GUILayout.Label("No upgrades assigned. Run Tools/RuneGate/Bootstrap Progression Prototype.");
            }
            else
            {
                for (int i = 0; i < upgradeManager.AvailableUpgrades.Count; i++)
                {
                    DrawUpgrade(upgradeManager.AvailableUpgrades[i]);
                    GUILayout.Space(6f);
                }
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("Stage Select", GUILayout.Height(36f)))
            {
                SceneManager.LoadScene(stageSelectSceneName);
            }

            if (!string.IsNullOrWhiteSpace(feedbackMessage))
            {
                GUILayout.Space(8f);
                GUILayout.Label(feedbackMessage);
            }

            GUILayout.EndArea();
        }

        private void DrawUpgrade(UpgradeData upgradeData)
        {
            if (upgradeData == null)
            {
                GUILayout.Label("Missing UpgradeData");
                return;
            }

            int level = upgradeManager.GetLevel(upgradeData);
            int maxLevel = Mathf.Max(0, upgradeData.MaxLevel);
            int cost = upgradeManager.GetCost(upgradeData);
            GUILayout.BeginHorizontal(GUI.skin.box);
            RuntimePixelGuiUtility.DrawIcon(GetUpgradeIconPath(upgradeData), 34f);
            GUILayout.BeginVertical();
            GUILayout.Label($"{upgradeData.DisplayName} Lv {level}/{maxLevel}");
            GUILayout.Label(upgradeData.Description);
            GUILayout.Label($"Effect: {upgradeData.EffectKey} +{upgradeData.ValuePerLevel:0.###} per level");

            bool maxed = level >= maxLevel;
            string buttonLabel = maxed ? "Max Level" : $"Buy ({cost} Gold)";
            using (new GuiEnabledScope(!maxed && SaveManager.Current.totalGold >= cost))
            {
                if (GUILayout.Button(buttonLabel, GUILayout.Height(30f)))
                {
                    bool purchased = upgradeManager.TryPurchase(upgradeData);
                    if (purchased)
                    {
                        AudioManager.Play(SfxKey.UpgradePurchase);
                    }

                    feedbackMessage = purchased ? $"Purchased {upgradeData.DisplayName}." : $"Not enough gold for {upgradeData.DisplayName}.";
                }
            }

            if (!maxed && SaveManager.Current.totalGold < cost)
            {
                GUILayout.Label($"Need {cost - SaveManager.Current.totalGold} more Gold.");
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
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
