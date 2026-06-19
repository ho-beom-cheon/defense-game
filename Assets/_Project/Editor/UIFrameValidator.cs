using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RuneGate.Editor
{
    public static class UIFrameValidator
    {
        private const string ReportPath = "docs/ui-frame-validation-report-v088-0.md";

        private static readonly string[] RequiredScenes =
        {
            "Assets/_Project/Scenes/TitleScene.unity",
            "Assets/_Project/Scenes/StageSelectScene.unity",
            "Assets/_Project/Scenes/BattleScene.unity",
            "Assets/_Project/Scenes/UpgradeScene.unity"
        };

        private static readonly string[] RequiredScripts =
        {
            "Assets/_Project/Scripts/UI/Foundation/GameFrameLayout.cs",
            "Assets/_Project/Scripts/UI/Foundation/FrameRootLimiter.cs",
            "Assets/_Project/Scripts/UI/Foundation/ResponsiveLayoutSwitcher.cs",
            "Assets/_Project/Scripts/UI/Foundation/PopupFrameController.cs",
            "Assets/_Project/Scripts/UI/StageSelectUI.cs",
            "Assets/_Project/Scripts/UI/BattleHUD.cs",
            "Assets/_Project/Scripts/UI/FormationSkillPanelUI.cs",
            "Assets/_Project/Scripts/UI/RuneSelectionUI.cs",
            "Assets/_Project/Scripts/UI/StageResultUI.cs",
            "Assets/_Project/Scripts/UI/SafeAreaFitter.cs",
            "Assets/_Project/Scripts/UI/UIResponsiveLayout.cs",
            "Assets/_Project/Scripts/UI/UIPopupGuiUtility.cs",
            "Assets/_Project/Scripts/UI/PanelClampToScreen.cs"
        };

        private static readonly string[] RequiredDocs =
        {
            "docs/game-frame-rebuild-v088-0.md",
            "docs/ui-layout-foundation.md",
            "docs/battlefield-frame.md",
            "docs/popup-layout.md",
            "docs/ui-frame-validation-report-v088-0.md"
        };

        private static readonly SceneBinding[] RequiredSceneBindings =
        {
            new SceneBinding("Assets/_Project/Scenes/TitleScene.unity", "Assembly-CSharp::RuneGate.TitleUI"),
            new SceneBinding("Assets/_Project/Scenes/StageSelectScene.unity", "Assembly-CSharp::RuneGate.StageSelectUI"),
            new SceneBinding("Assets/_Project/Scenes/BattleScene.unity", "Assembly-CSharp::RuneGate.BattleHUD"),
            new SceneBinding("Assets/_Project/Scenes/BattleScene.unity", "Assembly-CSharp::RuneGate.FormationSkillPanelUI"),
            new SceneBinding("Assets/_Project/Scenes/BattleScene.unity", "Assembly-CSharp::RuneGate.RuneSelectionUI"),
            new SceneBinding("Assets/_Project/Scenes/BattleScene.unity", "Assembly-CSharp::RuneGate.StageResultUI"),
            new SceneBinding("Assets/_Project/Scenes/UpgradeScene.unity", "Assembly-CSharp::RuneGate.UpgradeSceneUI")
        };

        private static readonly string[] FrameTerms =
        {
            "StageSelectFrame",
            "BattleFrame",
            "PopupLayer",
            "StageListPanel",
            "StageDetailPanel",
            "RuneSelectionPopup",
            "ResultPopup",
            "UseCompactStageSelect",
            "UseCompactBattle",
            "DrawStageSelectFrame"
        };

        private static readonly string[] SuspiciousUserFacingTokens =
        {
            "Kingdom Crystal defended",
            "Missing StageData",
            "State WaveRunning",
            "State RuneSelection",
            "State Victory",
            "State Defeat"
        };

        [MenuItem("Tools/RuneGate/Validate Game Frame")]
        public static void ValidateGameFrame()
        {
            string root = ProjectRoot;
            List<string> pass = new List<string>();
            List<string> warnings = new List<string>();
            List<string> failures = new List<string>();

            ValidatePaths(root, RequiredScenes, "Required Scene", pass, failures);
            ValidatePaths(root, RequiredScripts, "Required UI Script", pass, failures);
            ValidatePaths(root, RequiredDocs, "Required Doc", pass, warnings);
            ValidateFrameTerms(root, pass, warnings);
            ValidateLayoutMath(pass, failures);
            ValidateStageOrder(pass, failures);
            ValidateSceneBindings(root, pass, failures);
            ValidateStageSelectLegacySerialization(root, pass, failures);
            ValidateBuildSettings(pass, warnings);
            ValidateSceneDuplicates(root, pass, warnings);
            ValidateSuspiciousText(root, warnings);

            WriteReport(root, pass, warnings, failures);

            if (failures.Count > 0)
            {
                Debug.LogError($"RuneGate Game Frame validation failed: {failures.Count} fail, {warnings.Count} warning. Report: {ReportPath}");
            }
            else if (warnings.Count > 0)
            {
                Debug.LogWarning($"RuneGate Game Frame validation passed with warnings: {warnings.Count}. Report: {ReportPath}");
            }
            else
            {
                Debug.Log($"RuneGate Game Frame validation passed. Report: {ReportPath}");
            }

            AssetDatabase.Refresh();
        }

        public static bool ValidateLayoutMathForProjectValidator(out List<string> failures)
        {
            List<string> pass = new List<string>();
            failures = new List<string>();
            ValidateLayoutMath(pass, failures);
            return failures.Count == 0;
        }

        private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        private static void ValidatePaths(string root, IReadOnlyList<string> paths, string label, List<string> pass, List<string> failures)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                string relativePath = paths[i];
                string fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                {
                    pass.Add($"{label} exists: `{relativePath}`");
                }
                else
                {
                    failures.Add($"{label} missing: `{relativePath}`");
                }
            }
        }

        private static void ValidateFrameTerms(string root, List<string> pass, List<string> warnings)
        {
            string combined = string.Empty;
            string[] files =
            {
                "Assets/_Project/Scripts/UI/StageSelectUI.cs",
                "Assets/_Project/Scripts/UI/BattleHUD.cs",
                "Assets/_Project/Scripts/UI/FormationSkillPanelUI.cs",
                "Assets/_Project/Scripts/UI/RuneSelectionUI.cs",
                "Assets/_Project/Scripts/UI/StageResultUI.cs",
                "Assets/_Project/Scripts/UI/Foundation/GameFrameLayout.cs"
            };

            for (int i = 0; i < files.Length; i++)
            {
                string path = Path.Combine(root, files[i].Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(path))
                {
                    combined += File.ReadAllText(path);
                }
            }

            for (int i = 0; i < FrameTerms.Length; i++)
            {
                string term = FrameTerms[i];
                if (combined.Contains(term, StringComparison.Ordinal))
                {
                    pass.Add($"Frame term exists: `{term}`");
                }
                else
                {
                    warnings.Add($"Frame term missing: `{term}`");
                }
            }
        }

        private static void ValidateLayoutMath(List<string> pass, List<string> failures)
        {
            Vector2[] resolutions =
            {
                new Vector2(720f, 1280f),
                new Vector2(1080f, 1920f),
                new Vector2(1440f, 2560f),
                new Vector2(1600f, 900f),
                new Vector2(1536f, 768f)
            };

            for (int i = 0; i < resolutions.Length; i++)
            {
                Vector2 resolution = resolutions[i];
                ValidateTitleLayout(resolution.x, resolution.y, false, pass, failures);
                ValidateTitleLayout(resolution.x, resolution.y, true, pass, failures);
                ValidateStageSelectLayout(resolution.x, resolution.y, pass, failures);
                ValidateUpgradeLayout(resolution.x, resolution.y, pass, failures);
                ValidateBattleLayout(resolution.x, resolution.y, pass, failures);
                ValidatePopupLayout(resolution.x, resolution.y, pass, failures);
            }
        }

        private static void ValidateTitleLayout(float width, float height, bool expanded, List<string> pass, List<string> failures)
        {
            ScreenFrameRects rects = GameFrameLayout.TitleFrameForSize(width, height, expanded);
            string label = $"Title {(expanded ? "expanded" : "normal")} {width:0}x{height:0}";
            Rect[] verticalAreas =
            {
                rects.HeaderArea,
                rects.MainArea,
                rects.FooterArea
            };

            ValidateInside(label, "HeaderArea", rects.HeaderArea, rects.FrameRoot, failures);
            ValidateInside(label, "MainArea", rects.MainArea, rects.FrameRoot, failures);
            ValidateInside(label, "FooterArea", rects.FooterArea, rects.FrameRoot, failures);
            ValidateNoOverlap(label, verticalAreas, failures);

            if (!HasFailureFor(label, failures))
            {
                pass.Add($"{label}: layout rects fit without overlap");
            }
        }

        private static void ValidateStageSelectLayout(float width, float height, List<string> pass, List<string> failures)
        {
            StageSelectFrameRects rects = GameFrameLayout.StageSelectFrameForSize(width, height);
            string label = $"StageSelect {width:0}x{height:0}";
            Rect[] verticalAreas =
            {
                rects.HeaderArea,
                rects.DifficultyArea,
                rects.MainArea,
                rects.PetContractArea,
                rects.FooterArea
            };

            ValidateInside(label, "HeaderArea", rects.HeaderArea, rects.FrameRoot, failures);
            ValidateInside(label, "DifficultyArea", rects.DifficultyArea, rects.FrameRoot, failures);
            ValidateInside(label, "MainArea", rects.MainArea, rects.FrameRoot, failures);
            ValidateInside(label, "PetContractArea", rects.PetContractArea, rects.FrameRoot, failures);
            ValidateInside(label, "FooterArea", rects.FooterArea, rects.FrameRoot, failures);
            ValidateInside(label, "StageListPanel", rects.StageListPanel, rects.MainArea, failures);
            ValidateInside(label, "StageDetailPanel", rects.StageDetailPanel, rects.MainArea, failures);
            ValidateNoOverlap(label, verticalAreas, failures);

            if (rects.DifficultyArea.height > 0.1f)
            {
                failures.Add($"{label}: DifficultyArea must stay collapsed; difficulty selection belongs in the header button.");
            }

            if (rects.PetContractArea.height > 0.1f)
            {
                failures.Add($"{label}: PetContractArea must stay collapsed; pet contract belongs in the header button/popup.");
            }

            if (Intersects(rects.StageListPanel, rects.StageDetailPanel))
            {
                failures.Add($"{label}: StageListPanel overlaps StageDetailPanel");
            }

            if (rects.MainArea.height < 160f)
            {
                failures.Add($"{label}: MainArea is too short for the stage list/detail split. height={rects.MainArea.height:0.#}");
            }

            if (rects.StageListPanel.width < 260f || rects.StageDetailPanel.width < 260f)
            {
                failures.Add($"{label}: Stage panels are too narrow. list={rects.StageListPanel.width:0.#}, detail={rects.StageDetailPanel.width:0.#}");
            }

            if (!HasFailureFor(label, failures))
            {
                pass.Add($"{label}: layout rects fit without overlap");
            }
        }

        private static void ValidateBattleLayout(float width, float height, List<string> pass, List<string> failures)
        {
            BattleFrameRects rects = GameFrameLayout.BattleFrameForSize(width, height);
            string label = $"Battle {width:0}x{height:0}";
            Rect[] verticalAreas =
            {
                rects.HeaderArea,
                rects.BattleFieldFrame,
                rects.SkillPanelArea,
                rects.FooterArea
            };

            ValidateInside(label, "HeaderArea", rects.HeaderArea, rects.FrameRoot, failures);
            ValidateInside(label, "BattleFieldFrame", rects.BattleFieldFrame, rects.FrameRoot, failures);
            ValidateInside(label, "SkillPanelArea", rects.SkillPanelArea, rects.FrameRoot, failures);
            ValidateInside(label, "FooterArea", rects.FooterArea, rects.FrameRoot, failures);
            ValidateNoOverlap(label, verticalAreas, failures);

            if (rects.BattleFieldFrame.height < 140f)
            {
                failures.Add($"{label}: BattleFieldFrame is too short. height={rects.BattleFieldFrame.height:0.#}");
            }

            if (rects.SkillPanelArea.height < 96f)
            {
                failures.Add($"{label}: SkillPanelArea is too short. height={rects.SkillPanelArea.height:0.#}");
            }

            if (!HasFailureFor(label, failures))
            {
                pass.Add($"{label}: layout rects fit without overlap");
            }
        }

        private static void ValidateUpgradeLayout(float width, float height, List<string> pass, List<string> failures)
        {
            ScreenFrameRects rects = GameFrameLayout.UpgradeFrameForSize(width, height);
            string label = $"Upgrade {width:0}x{height:0}";
            Rect[] verticalAreas =
            {
                rects.HeaderArea,
                rects.MainArea,
                rects.FooterArea
            };

            ValidateInside(label, "HeaderArea", rects.HeaderArea, rects.FrameRoot, failures);
            ValidateInside(label, "MainArea", rects.MainArea, rects.FrameRoot, failures);
            ValidateInside(label, "FooterArea", rects.FooterArea, rects.FrameRoot, failures);
            ValidateNoOverlap(label, verticalAreas, failures);

            if (!HasFailureFor(label, failures))
            {
                pass.Add($"{label}: layout rects fit without overlap");
            }
        }

        private static void ValidatePopupLayout(float width, float height, List<string> pass, List<string> failures)
        {
            Rect safe = GameFrameLayout.SafeRectForSize(width, height);
            Rect runePopup = GameFrameLayout.PopupFrameForSize(width, height, 620f, 520f, 0.92f, 0.78f);
            Rect resultPopup = GameFrameLayout.PopupFrameForSize(width, height, 620f, 560f, 0.92f, 0.78f);
            string label = $"Popup {width:0}x{height:0}";

            ValidateInside(label, "RuneSelectionPopup", runePopup, safe, failures);
            ValidateInside(label, "ResultPopup", resultPopup, safe, failures);

            if (runePopup.width < 300f || runePopup.height < 280f)
            {
                failures.Add($"{label}: RuneSelectionPopup is too small. rect={FormatRect(runePopup)}");
            }

            if (resultPopup.width < 300f || resultPopup.height < 300f)
            {
                failures.Add($"{label}: ResultPopup is too small. rect={FormatRect(resultPopup)}");
            }

            if (!HasFailureFor(label, failures))
            {
                pass.Add($"{label}: popup rects fit inside safe area");
            }
        }

        private static void ValidateSceneDuplicates(string root, List<string> pass, List<string> warnings)
        {
            for (int i = 0; i < RequiredScenes.Length; i++)
            {
                string scene = RequiredScenes[i];
                string path = Path.Combine(root, scene.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                {
                    continue;
                }

                string text = File.ReadAllText(path);
                int canvasCount = CountOccurrences(text, "m_Name: Canvas");
                int eventSystemCount = CountOccurrences(text, "m_Name: EventSystem");

                if (canvasCount <= 1)
                {
                    pass.Add($"Canvas duplicate check passed: `{scene}` ({canvasCount})");
                }
                else
                {
                    warnings.Add($"Canvas duplicate risk: `{scene}` ({canvasCount})");
                }

                if (eventSystemCount <= 1)
                {
                    pass.Add($"EventSystem duplicate check passed: `{scene}` ({eventSystemCount})");
                }
                else
                {
                    warnings.Add($"EventSystem duplicate risk: `{scene}` ({eventSystemCount})");
                }
            }
        }

        private static void ValidateSceneBindings(string root, List<string> pass, List<string> failures)
        {
            for (int i = 0; i < RequiredSceneBindings.Length; i++)
            {
                SceneBinding binding = RequiredSceneBindings[i];
                string path = Path.Combine(root, binding.ScenePath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                {
                    failures.Add($"Scene binding check skipped because scene is missing: `{binding.ScenePath}`");
                    continue;
                }

                string text = File.ReadAllText(path);
                if (text.Contains(binding.EditorClassIdentifier, StringComparison.Ordinal))
                {
                    pass.Add($"Scene binding exists: `{binding.ScenePath}` -> `{binding.EditorClassIdentifier}`");
                }
                else
                {
                    failures.Add($"Scene binding missing: `{binding.ScenePath}` -> `{binding.EditorClassIdentifier}`");
                }
            }
        }

        private static void ValidateStageOrder(List<string> pass, List<string> failures)
        {
            List<StageData> stages = PrototypeAssetLoader.LoadStages();
            if (stages.Count < 10)
            {
                failures.Add($"Stage order check failed: expected 10 stages, found {stages.Count}");
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                StageData stage = stages[i];
                int expectedStageNumber = i + 1;
                int actualStageNumber = PrototypeAssetLoader.GetStageNumber(stage);
                if (actualStageNumber != expectedStageNumber)
                {
                    string stageId = stage != null ? stage.StageId : "null";
                    failures.Add($"Stage order check failed at index {i}: expected {expectedStageNumber}, got `{stageId}`");
                    return;
                }
            }

            pass.Add("Stage order check passed: Stage 1 through Stage 10 are loaded in numeric order");
        }

        private static void ValidateStageSelectLegacySerialization(string root, List<string> pass, List<string> failures)
        {
            const string scenePath = "Assets/_Project/Scenes/StageSelectScene.unity";
            string path = Path.Combine(root, scenePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                failures.Add($"StageSelect legacy serialization check skipped because scene is missing: `{scenePath}`");
                return;
            }

            string text = File.ReadAllText(path);
            if (text.Contains("panelRect:", StringComparison.Ordinal))
            {
                failures.Add("StageSelectScene still contains legacy `panelRect` serialized data; remove it so the new frame layout is authoritative.");
                return;
            }

            pass.Add("StageSelect legacy serialized `panelRect` data is absent");
        }

        private static void ValidateBuildSettings(List<string> pass, List<string> warnings)
        {
            HashSet<string> enabledScenes = new HashSet<string>(StringComparer.Ordinal);
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < buildScenes.Length; i++)
            {
                if (buildScenes[i] != null && buildScenes[i].enabled)
                {
                    enabledScenes.Add(buildScenes[i].path);
                }
            }

            for (int i = 0; i < RequiredScenes.Length; i++)
            {
                string scene = RequiredScenes[i];
                if (enabledScenes.Contains(scene))
                {
                    pass.Add($"Build Settings scene enabled: `{scene}`");
                }
                else
                {
                    warnings.Add($"Build Settings scene not enabled or not listed: `{scene}`");
                }
            }
        }

        private static void ValidateSuspiciousText(string root, List<string> warnings)
        {
            string uiScriptPath = Path.Combine(root, "Assets", "_Project", "Scripts", "UI");
            if (!Directory.Exists(uiScriptPath))
            {
                warnings.Add("User-facing text policy skipped because UI script folder is missing.");
                return;
            }

            foreach (string file in Directory.EnumerateFiles(uiScriptPath, "*.cs", SearchOption.AllDirectories))
            {
                string relative = ToProjectRelativePath(root, file);
                string text = File.ReadAllText(file);
                for (int i = 0; i < SuspiciousUserFacingTokens.Length; i++)
                {
                    string token = SuspiciousUserFacingTokens[i];
                    if (text.Contains(token, StringComparison.Ordinal))
                    {
                        warnings.Add($"User-facing text review needed: `{relative}` contains `{token}`");
                        break;
                    }
                }
            }
        }

        private static void ValidateInside(string label, string rectName, Rect rect, Rect bounds, List<string> failures)
        {
            const float tolerance = 0.1f;
            bool inside = rect.xMin + tolerance >= bounds.xMin &&
                          rect.yMin + tolerance >= bounds.yMin &&
                          rect.xMax - tolerance <= bounds.xMax &&
                          rect.yMax - tolerance <= bounds.yMax;
            if (!inside)
            {
                failures.Add($"{label}: {rectName} is outside bounds. rect={FormatRect(rect)} bounds={FormatRect(bounds)}");
            }
        }

        private static void ValidateNoOverlap(string label, IReadOnlyList<Rect> rects, List<string> failures)
        {
            for (int i = 0; i < rects.Count; i++)
            {
                for (int j = i + 1; j < rects.Count; j++)
                {
                    if (Intersects(rects[i], rects[j]))
                    {
                        failures.Add($"{label}: layout areas overlap at index {i} and {j}. a={FormatRect(rects[i])} b={FormatRect(rects[j])}");
                    }
                }
            }
        }

        private static bool Intersects(Rect a, Rect b)
        {
            const float tolerance = 0.1f;
            return a.xMin < b.xMax - tolerance &&
                   a.xMax > b.xMin + tolerance &&
                   a.yMin < b.yMax - tolerance &&
                   a.yMax > b.yMin + tolerance;
        }

        private static bool HasFailureFor(string label, IReadOnlyList<string> failures)
        {
            for (int i = 0; i < failures.Count; i++)
            {
                if (failures[i].StartsWith(label, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string FormatRect(Rect rect)
        {
            return $"({rect.x:0.#},{rect.y:0.#},{rect.width:0.#},{rect.height:0.#})";
        }

        private static int CountOccurrences(string text, string value)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static string ToProjectRelativePath(string root, string fullPath)
        {
            string relative = Path.GetRelativePath(root, fullPath);
            return relative.Replace(Path.DirectorySeparatorChar, '/');
        }

        private static void WriteReport(string root, IReadOnlyList<string> pass, IReadOnlyList<string> warnings, IReadOnlyList<string> failures)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# UI Frame Validation Report v0.88.0");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine();
            builder.AppendLine($"Summary: PASS {pass.Count}, WARNING {warnings.Count}, FAIL {failures.Count}");
            builder.AppendLine();
            AppendSection(builder, "Fail", failures);
            AppendSection(builder, "Warning", warnings);
            AppendSection(builder, "Pass", pass);

            string fullPath = Path.Combine(root, ReportPath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, builder.ToString(), new UTF8Encoding(false));
        }

        private static void AppendSection(StringBuilder builder, string title, IReadOnlyList<string> items)
        {
            builder.AppendLine($"## {title}");
            if (items.Count == 0)
            {
                builder.AppendLine("- None");
                builder.AppendLine();
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                builder.AppendLine($"- {items[i]}");
            }

            builder.AppendLine();
        }

        private readonly struct SceneBinding
        {
            public SceneBinding(string scenePath, string editorClassIdentifier)
            {
                ScenePath = scenePath;
                EditorClassIdentifier = editorClassIdentifier;
            }

            public string ScenePath { get; }
            public string EditorClassIdentifier { get; }
        }
    }
}
