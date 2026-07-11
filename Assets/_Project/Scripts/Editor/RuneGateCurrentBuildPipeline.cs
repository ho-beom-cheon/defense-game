using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RuneGate.Editor
{
    public static class RuneGateCurrentBuildPipeline
    {
        private const string BuildPathArgument = "-runegateBuildPath";
        private const string DefaultApkPath = "Builds/Android/RuneGateDefense-current.apk";

        [MenuItem("Tools/RuneGate/Build Current Android APK")]
        public static void BuildCurrentAndroidApk()
        {
            BuildCurrentAndroidApkInternal(ResolveBuildPath());
        }

        public static void BuildCurrentAndroidApkFromCommandLine()
        {
            int exitCode = BuildCurrentAndroidApkInternal(ResolveBuildPath());
            EditorApplication.Exit(exitCode);
        }

        private static int BuildCurrentAndroidApkInternal(string outputPath)
        {
            if (!RuneGateProjectValidator.ValidateProject(out List<string> validationErrors))
            {
                for (int i = 0; i < validationErrors.Count; i++)
                {
                    Debug.LogError($"RuneGate Android pre-build validation: {validationErrors[i]}");
                }

                Debug.LogError("RUNEGATE_ANDROID_BUILD_FAILED: project validation failed.");
                return 1;
            }

            string[] scenes = GetEnabledBuildScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("RUNEGATE_ANDROID_BUILD_FAILED: no enabled scenes exist in EditorBuildSettings.");
                return 1;
            }

            string fullOutputPath;
            try
            {
                fullOutputPath = Path.GetFullPath(outputPath);
                string directory = Path.GetDirectoryName(fullOutputPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"RUNEGATE_ANDROID_BUILD_FAILED: invalid output path. {exception.Message}");
                return 1;
            }

            bool previousBuildAppBundle = EditorUserBuildSettings.buildAppBundle;
            try
            {
                EditorUserBuildSettings.buildAppBundle = false;
                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = fullOutputPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                if (report == null || report.summary.result != BuildResult.Succeeded)
                {
                    string result = report != null ? report.summary.result.ToString() : "NoReport";
                    Debug.LogError($"RUNEGATE_ANDROID_BUILD_FAILED: BuildPipeline result={result}.");
                    return 1;
                }

                Debug.Log($"RUNEGATE_ANDROID_BUILD_PASSED: {fullOutputPath} ({report.summary.totalSize} bytes)");
                return 0;
            }
            catch (Exception exception)
            {
                Debug.LogError($"RUNEGATE_ANDROID_BUILD_FAILED: {exception}");
                return 1;
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            }
        }

        private static string[] GetEnabledBuildScenes()
        {
            EditorBuildSettingsScene[] configuredScenes = EditorBuildSettings.scenes;
            List<string> enabledScenes = new List<string>();
            for (int i = 0; i < configuredScenes.Length; i++)
            {
                EditorBuildSettingsScene scene = configuredScenes[i];
                if (scene != null && scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
                {
                    enabledScenes.Add(scene.path);
                }
            }

            return enabledScenes.ToArray();
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

            return DefaultApkPath;
        }
    }
}
