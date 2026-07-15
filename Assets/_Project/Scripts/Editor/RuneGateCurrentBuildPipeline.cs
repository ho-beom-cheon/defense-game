using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RuneGate.Editor
{
    public static class RuneGateCurrentBuildPipeline
    {
        private const string BuildPathArgument = "-runegateBuildPath";
        private const string DefaultApkPath = "Builds/Android/RuneGateDefense-current.apk";
        private const string DefaultAabPath = "Builds/Android/RuneGateDefense-current.aab";
        public const string KeystorePathEnvironmentVariable = "RUNEGATE_ANDROID_KEYSTORE_PATH";
        public const string KeystorePasswordEnvironmentVariable = "RUNEGATE_ANDROID_KEYSTORE_PASSWORD";
        public const string KeyAliasEnvironmentVariable = "RUNEGATE_ANDROID_KEY_ALIAS";
        public const string KeyAliasPasswordEnvironmentVariable = "RUNEGATE_ANDROID_KEY_ALIAS_PASSWORD";

        [MenuItem("Tools/RuneGate/Build Current Android APK")]
        public static void BuildCurrentAndroidApk()
        {
            BuildCurrentAndroidArtifact(false, ResolveBuildPath(DefaultApkPath), false);
        }

        [MenuItem("Tools/RuneGate/Build Current Android AAB")]
        public static void BuildCurrentAndroidAab()
        {
            BuildCurrentAndroidArtifact(true, ResolveBuildPath(DefaultAabPath), false);
        }

        [MenuItem("Tools/RuneGate/Build Signed Android APK")]
        public static void BuildSignedAndroidApk()
        {
            BuildCurrentAndroidArtifact(false, ResolveBuildPath(DefaultApkPath), true);
        }

        [MenuItem("Tools/RuneGate/Build Signed Android AAB")]
        public static void BuildSignedAndroidAab()
        {
            BuildCurrentAndroidArtifact(true, ResolveBuildPath(DefaultAabPath), true);
        }

        public static void BuildCurrentAndroidApkFromCommandLine()
        {
            int exitCode = BuildCurrentAndroidArtifact(false, ResolveBuildPath(DefaultApkPath), false);
            EditorApplication.Exit(exitCode);
        }

        public static void BuildCurrentAndroidAabFromCommandLine()
        {
            int exitCode = BuildCurrentAndroidArtifact(true, ResolveBuildPath(DefaultAabPath), false);
            EditorApplication.Exit(exitCode);
        }

        public static void BuildSignedAndroidApkFromCommandLine()
        {
            int exitCode = BuildCurrentAndroidArtifact(false, ResolveBuildPath(DefaultApkPath), true);
            EditorApplication.Exit(exitCode);
        }

        public static void BuildSignedAndroidAabFromCommandLine()
        {
            int exitCode = BuildCurrentAndroidArtifact(true, ResolveBuildPath(DefaultAabPath), true);
            EditorApplication.Exit(exitCode);
        }

        private static int BuildCurrentAndroidArtifact(bool buildAppBundle, string outputPath, bool requireReleaseSigning)
        {
            string artifactName = buildAppBundle ? "AAB" : "APK";
            string failureMarker = ResolveBuildMarker(buildAppBundle, requireReleaseSigning, false);
            string successMarker = ResolveBuildMarker(buildAppBundle, requireReleaseSigning, true);
            if (!RuneGateProjectValidator.ValidateProject(out List<string> validationErrors))
            {
                for (int i = 0; i < validationErrors.Count; i++)
                {
                    Debug.LogError($"RuneGate Android pre-build validation: {validationErrors[i]}");
                }

                Debug.LogError($"{failureMarker}: project validation failed.");
                return 1;
            }

            ReleaseSigningConfiguration signingConfiguration = null;
            if (requireReleaseSigning && !TryResolveReleaseSigningConfiguration(out signingConfiguration, out string signingErrorCode))
            {
                Debug.LogError($"{failureMarker}: release signing configuration is invalid ({signingErrorCode}). See docs/android-release-signing.md.");
                return 1;
            }

            string[] scenes = GetEnabledBuildScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError($"{failureMarker}: no enabled scenes exist in EditorBuildSettings.");
                return 1;
            }

            string fullOutputPath;
            try
            {
                fullOutputPath = Path.GetFullPath(outputPath);
                string expectedExtension = buildAppBundle ? ".aab" : ".apk";
                if (!string.Equals(Path.GetExtension(fullOutputPath), expectedExtension, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError($"{failureMarker}: {artifactName} output must use the {expectedExtension} extension.");
                    return 1;
                }

                string directory = Path.GetDirectoryName(fullOutputPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string staleManifestPath = ArtifactManifestPath(fullOutputPath);
                if (File.Exists(staleManifestPath))
                {
                    File.Delete(staleManifestPath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"{failureMarker}: invalid output path. {exception.Message}");
                return 1;
            }

            bool previousBuildAppBundle = EditorUserBuildSettings.buildAppBundle;
            AndroidSigningSettingsSnapshot signingSnapshot = AndroidSigningSettingsSnapshot.Capture();
            if (signingConfiguration != null && !signingSnapshot.CanRestoreSerializedSettings)
            {
                Debug.LogError($"{failureMarker}: Unity PlayerSettings could not be snapshotted safely (settings_snapshot_unavailable).");
                return 1;
            }

            try
            {
                EditorUserBuildSettings.buildAppBundle = buildAppBundle;
                if (signingConfiguration != null)
                {
                    ApplyReleaseSigningConfiguration(signingConfiguration);
                }

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
                    Debug.LogError($"{failureMarker}: BuildPipeline result={result}.");
                    return 1;
                }

                if (!TryWriteArtifactManifest(fullOutputPath, artifactName, requireReleaseSigning, out AndroidBuildArtifactManifest manifest, out string manifestError))
                {
                    Debug.LogError($"{failureMarker}: artifact manifest could not be created ({manifestError}).");
                    return 1;
                }

                Debug.Log($"{successMarker}: {manifest.artifactFile} ({manifest.sizeBytes} bytes, SHA-256 {manifest.sha256})");
                return 0;
            }
            catch (Exception exception)
            {
                Debug.LogError($"{failureMarker}: {exception}");
                return 1;
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
                signingSnapshot.Restore();
            }
        }

        public static string ValidateReleaseSigningConfiguration(
            string keystorePath,
            string keystorePassword,
            string keyAlias,
            string keyAliasPassword,
            bool keystoreExists)
        {
            if (string.IsNullOrWhiteSpace(keystorePath))
            {
                return "missing_keystore_path";
            }

            if (string.IsNullOrEmpty(keystorePassword))
            {
                return "missing_keystore_password";
            }

            if (string.IsNullOrWhiteSpace(keyAlias))
            {
                return "missing_key_alias";
            }

            if (string.IsNullOrEmpty(keyAliasPassword))
            {
                return "missing_key_alias_password";
            }

            return keystoreExists ? string.Empty : "keystore_file_not_found";
        }

        public static string ArtifactManifestPath(string outputPath)
        {
            return string.IsNullOrWhiteSpace(outputPath) ? string.Empty : outputPath + ".manifest.json";
        }

        private static string ResolveBuildMarker(bool buildAppBundle, bool signed, bool success)
        {
            string result = success ? "PASSED" : "FAILED";
            if (signed)
            {
                return $"RUNEGATE_ANDROID_SIGNED_{(buildAppBundle ? "AAB" : "APK")}_BUILD_{result}";
            }

            return buildAppBundle
                ? $"RUNEGATE_ANDROID_AAB_BUILD_{result}"
                : $"RUNEGATE_ANDROID_BUILD_{result}";
        }

        private static bool TryResolveReleaseSigningConfiguration(out ReleaseSigningConfiguration configuration, out string errorCode)
        {
            configuration = null;
            string keystorePath = Environment.GetEnvironmentVariable(KeystorePathEnvironmentVariable);
            string keystorePassword = Environment.GetEnvironmentVariable(KeystorePasswordEnvironmentVariable);
            string keyAlias = Environment.GetEnvironmentVariable(KeyAliasEnvironmentVariable);
            string keyAliasPassword = Environment.GetEnvironmentVariable(KeyAliasPasswordEnvironmentVariable);

            bool keystoreExists = false;
            string fullKeystorePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(keystorePath))
            {
                try
                {
                    fullKeystorePath = Path.GetFullPath(keystorePath);
                    keystoreExists = File.Exists(fullKeystorePath);
                }
                catch
                {
                    errorCode = "invalid_keystore_path";
                    return false;
                }
            }

            errorCode = ValidateReleaseSigningConfiguration(
                keystorePath,
                keystorePassword,
                keyAlias,
                keyAliasPassword,
                keystoreExists);
            if (!string.IsNullOrEmpty(errorCode))
            {
                return false;
            }

            configuration = new ReleaseSigningConfiguration(fullKeystorePath, keystorePassword, keyAlias.Trim(), keyAliasPassword);
            return true;
        }

        private static void ApplyReleaseSigningConfiguration(ReleaseSigningConfiguration configuration)
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = configuration.KeystorePath;
            PlayerSettings.Android.keystorePass = configuration.KeystorePassword;
            PlayerSettings.Android.keyaliasName = configuration.KeyAlias;
            PlayerSettings.Android.keyaliasPass = configuration.KeyAliasPassword;
        }

        private static bool TryWriteArtifactManifest(
            string outputPath,
            string artifactType,
            bool releaseSigned,
            out AndroidBuildArtifactManifest manifest,
            out string error)
        {
            manifest = null;
            error = string.Empty;
            try
            {
                FileInfo artifact = new FileInfo(outputPath);
                if (!artifact.Exists || artifact.Length <= 0)
                {
                    error = "artifact_missing";
                    return false;
                }

                manifest = new AndroidBuildArtifactManifest
                {
                    artifactFile = artifact.Name,
                    artifactType = artifactType,
                    sizeBytes = artifact.Length,
                    sha256 = ComputeSha256(outputPath),
                    unityVersion = Application.unityVersion,
                    productName = PlayerSettings.productName,
                    applicationIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),
                    bundleVersion = PlayerSettings.bundleVersion,
                    androidBundleVersionCode = PlayerSettings.Android.bundleVersionCode,
                    buildUtc = DateTime.UtcNow.ToString("O"),
                    releaseSigned = releaseSigned
                };

                File.WriteAllText(
                    ArtifactManifestPath(outputPath),
                    JsonUtility.ToJson(manifest, true),
                    new UTF8Encoding(false));
                return true;
            }
            catch (Exception exception)
            {
                error = exception.GetType().Name;
                return false;
            }
        }

        private static string ComputeSha256(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("X2"));
                }

                return builder.ToString();
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

        private static string ResolveBuildPath(string defaultPath)
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

            return defaultPath;
        }

        private sealed class ReleaseSigningConfiguration
        {
            public ReleaseSigningConfiguration(string keystorePath, string keystorePassword, string keyAlias, string keyAliasPassword)
            {
                KeystorePath = keystorePath;
                KeystorePassword = keystorePassword;
                KeyAlias = keyAlias;
                KeyAliasPassword = keyAliasPassword;
            }

            public string KeystorePath { get; }
            public string KeystorePassword { get; }
            public string KeyAlias { get; }
            public string KeyAliasPassword { get; }
        }

        private sealed class AndroidSigningSettingsSnapshot
        {
            private readonly bool useCustomKeystore;
            private readonly string keystoreName;
            private readonly string keystorePass;
            private readonly string keyaliasName;
            private readonly string keyaliasPass;
            private readonly bool hasSerializedSettings;
            private readonly string serializedKeystoreName;
            private readonly string serializedKeyaliasName;
            private readonly bool serializedUseCustomKeystore;

            private AndroidSigningSettingsSnapshot()
            {
                useCustomKeystore = PlayerSettings.Android.useCustomKeystore;
                keystoreName = PlayerSettings.Android.keystoreName;
                keystorePass = PlayerSettings.Android.keystorePass;
                keyaliasName = PlayerSettings.Android.keyaliasName;
                keyaliasPass = PlayerSettings.Android.keyaliasPass;
                hasSerializedSettings = TryCaptureSerializedSettings(
                    out serializedKeystoreName,
                    out serializedKeyaliasName,
                    out serializedUseCustomKeystore);
            }

            public static AndroidSigningSettingsSnapshot Capture()
            {
                return new AndroidSigningSettingsSnapshot();
            }

            public bool CanRestoreSerializedSettings => hasSerializedSettings;

            public void Restore()
            {
                PlayerSettings.Android.keystoreName = keystoreName;
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasName = keyaliasName;
                PlayerSettings.Android.keyaliasPass = keyaliasPass;
                PlayerSettings.Android.useCustomKeystore = useCustomKeystore;
                if (hasSerializedSettings)
                {
                    RestoreSerializedSettings(
                        serializedKeystoreName,
                        serializedKeyaliasName,
                        serializedUseCustomKeystore);
                }
            }

            private static bool TryCaptureSerializedSettings(
                out string rawKeystoreName,
                out string rawKeyaliasName,
                out bool rawUseCustomKeystore)
            {
                rawKeystoreName = string.Empty;
                rawKeyaliasName = string.Empty;
                rawUseCustomKeystore = false;
                SerializedObject settings = GetPlayerSettingsSerializedObject();
                SerializedProperty keystoreProperty = settings?.FindProperty("AndroidKeystoreName");
                SerializedProperty keyaliasProperty = settings?.FindProperty("AndroidKeyaliasName");
                SerializedProperty useCustomProperty = settings?.FindProperty("androidUseCustomKeystore");
                if (keystoreProperty == null || keyaliasProperty == null || useCustomProperty == null)
                {
                    return false;
                }

                rawKeystoreName = keystoreProperty.stringValue;
                rawKeyaliasName = keyaliasProperty.stringValue;
                rawUseCustomKeystore = useCustomProperty.boolValue;
                return true;
            }

            private static void RestoreSerializedSettings(
                string rawKeystoreName,
                string rawKeyaliasName,
                bool rawUseCustomKeystore)
            {
                SerializedObject settings = GetPlayerSettingsSerializedObject();
                SerializedProperty keystoreProperty = settings?.FindProperty("AndroidKeystoreName");
                SerializedProperty keyaliasProperty = settings?.FindProperty("AndroidKeyaliasName");
                SerializedProperty useCustomProperty = settings?.FindProperty("androidUseCustomKeystore");
                if (keystoreProperty == null || keyaliasProperty == null || useCustomProperty == null)
                {
                    Debug.LogWarning("RuneGate Android signing settings could not restore their serialized names exactly.");
                    return;
                }

                keystoreProperty.stringValue = rawKeystoreName;
                keyaliasProperty.stringValue = rawKeyaliasName;
                useCustomProperty.boolValue = rawUseCustomKeystore;
                settings.ApplyModifiedPropertiesWithoutUndo();
            }

            private static SerializedObject GetPlayerSettingsSerializedObject()
            {
                MethodInfo method = typeof(PlayerSettings).GetMethod(
                    "GetSerializedObject",
                    BindingFlags.Static | BindingFlags.NonPublic);
                return method?.Invoke(null, null) as SerializedObject;
            }
        }

        [Serializable]
        private sealed class AndroidBuildArtifactManifest
        {
            public string artifactFile;
            public string artifactType;
            public long sizeBytes;
            public string sha256;
            public string unityVersion;
            public string productName;
            public string applicationIdentifier;
            public string bundleVersion;
            public int androidBundleVersionCode;
            public string buildUtc;
            public bool releaseSigned;
        }
    }
}
