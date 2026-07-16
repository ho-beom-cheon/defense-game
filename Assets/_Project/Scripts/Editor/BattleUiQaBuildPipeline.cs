using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RuneGate.Editor
{
    public static class BattleUiQaBuildPipeline
    {
        private const string BuildPathArgument = "-runegateBuildPath";
        private const string DefaultBuildPath = ".utmp/BattleUiWindows/RuneGateDefense.exe";

        [MenuItem("Tools/RuneGate/Build Battle UI Windows QA Player")]
        public static void BuildWindowsPlayer()
        {
            Build(ResolveBuildPath());
        }

        public static void BuildWindowsPlayerFromCommandLine()
        {
            int exitCode = Build(ResolveBuildPath());
            EditorApplication.Exit(exitCode);
        }

        private static int Build(string outputPath)
        {
            if (!RuneGateProjectValidator.ValidateProject(out List<string> validationErrors))
            {
                foreach (string error in validationErrors)
                {
                    Debug.LogError($"Battle UI Windows QA build validation failed: {error}");
                }

                return 1;
            }

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("Battle UI Windows QA build has no enabled scenes.");
                return 1;
            }

            string fullOutputPath = Path.GetFullPath(outputPath);
            string outputDirectory = Path.GetDirectoryName(fullOutputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                Debug.LogError($"Battle UI Windows QA build path is invalid: {outputPath}");
                return 1;
            }

            Directory.CreateDirectory(outputDirectory);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = fullOutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError($"BATTLE_UI_WINDOWS_QA_BUILD_FAILED: {report.summary.result}");
                    return 1;
                }

                Debug.Log($"BATTLE_UI_WINDOWS_QA_BUILD_PASSED: {fullOutputPath}");
                return 0;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("BATTLE_UI_WINDOWS_QA_BUILD_FAILED: exception");
                return 1;
            }
        }

        private static string ResolveBuildPath()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (string.Equals(arguments[i], BuildPathArgument, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(arguments[i + 1]))
                {
                    return arguments[i + 1];
                }
            }

            return DefaultBuildPath;
        }
    }
}
