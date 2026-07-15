using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private List<SfxEntry> sfxEntries = new List<SfxEntry>();
        [SerializeField] private bool warnWhenClipMissing;
        [SerializeField] private bool useProceduralFallback = true;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.55f;

        private const string SfxEnabledPreferenceKey = "RuneGate.SfxEnabled";
        private static AudioManager instance;
        private static readonly HashSet<SfxKey> warnedMissingKeys = new HashSet<SfxKey>();

        private readonly Dictionary<SfxKey, AudioClip> clipsByKey = new Dictionary<SfxKey, AudioClip>();
        private AudioSource audioSource;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                instance.AbsorbAssignedClips(sfxEntries);
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            RebuildClipLookup();
            TryBindAudioSource();
            BuildProceduralFallbacks();
            if (HasPlayableClip())
            {
                EnsureAudioListener();
            }

            ApplyAudioSourceSettings();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            if (instance != null)
            {
                return;
            }

            AudioManager existing = FindAnyObjectByType<AudioManager>();
            if (existing != null)
            {
                instance = existing;
                return;
            }

            GameObject runtimeObject = new GameObject("Runtime Audio Manager");
            runtimeObject.AddComponent<AudioManager>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeSceneAudioListener()
        {
            EnsureAudioListener();
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureAudioListener();
        }

        public static void Play(SfxKey key)
        {
            if (instance == null || !SfxEnabled)
            {
                return;
            }

            instance.PlayLocal(key);
        }

        public void PlayLocal(SfxKey key)
        {
            if (!SfxEnabled)
            {
                return;
            }

            if (!clipsByKey.TryGetValue(key, out AudioClip clip) || clip == null)
            {
                if (warnWhenClipMissing && warnedMissingKeys.Add(key))
                {
                    Debug.LogWarning($"AudioManager has no clip assigned for SFX '{key}'.");
                }

                return;
            }

            if (audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip);
        }

        public static bool SfxEnabled => PlayerPrefs.GetInt(SfxEnabledPreferenceKey, 1) != 0;

        public int PlayableClipCount => clipsByKey.Count;

        public bool HasClip(SfxKey key)
        {
            return clipsByKey.TryGetValue(key, out AudioClip clip) && clip != null;
        }

        public static void SetSfxEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(SfxEnabledPreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            instance?.ApplyAudioSourceSettings();
        }

        private void RebuildClipLookup()
        {
            clipsByKey.Clear();
            for (int i = 0; i < sfxEntries.Count; i++)
            {
                SfxEntry entry = sfxEntries[i];
                if (entry != null)
                {
                    clipsByKey[entry.Key] = entry.Clip;
                }
            }
        }

        private void TryBindAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void BuildProceduralFallbacks()
        {
            if (!useProceduralFallback || !ProceduralSfxFactory.IsAvailable)
            {
                return;
            }

            int generatedCount = 0;
            Array keys = Enum.GetValues(typeof(SfxKey));
            for (int i = 0; i < keys.Length; i++)
            {
                SfxKey key = (SfxKey)keys.GetValue(i);
                if (HasClip(key))
                {
                    continue;
                }

                AudioClip clip = ProceduralSfxFactory.Create(key);
                if (clip == null)
                {
                    continue;
                }

                clipsByKey[key] = clip;
                generatedCount++;
            }

            if (generatedCount > 0)
            {
                Debug.Log($"AudioManager generated {generatedCount} procedural SFX fallback clips.");
            }
        }

        private void AbsorbAssignedClips(IReadOnlyList<SfxEntry> entries)
        {
            if (entries == null)
            {
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                SfxEntry entry = entries[i];
                if (entry != null && entry.Clip != null)
                {
                    clipsByKey[entry.Key] = entry.Clip;
                }
            }
        }

        private void ApplyAudioSourceSettings()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.volume = Mathf.Clamp01(sfxVolume);
            audioSource.mute = !SfxEnabled;
        }

        private static void EnsureAudioListener()
        {
            if (instance == null || !instance.HasPlayableClip())
            {
                return;
            }

            if (FindAnyObjectByType<AudioListener>() != null)
            {
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                if (mainCamera.gameObject.GetComponent<AudioListener>() == null)
                {
                    mainCamera.gameObject.AddComponent<AudioListener>();
                }

                return;
            }

            GameObject listenerObject = new GameObject("Runtime Audio Listener");
            listenerObject.hideFlags = HideFlags.DontSave;
            listenerObject.AddComponent<AudioListener>();
        }

        private bool HasPlayableClip()
        {
            foreach (AudioClip clip in clipsByKey.Values)
            {
                if (clip != null)
                {
                    return true;
                }
            }

            return false;
        }

        [Serializable]
        private sealed class SfxEntry
        {
            [SerializeField] private SfxKey key;
            [SerializeField] private AudioClip clip;

            public SfxKey Key => key;
            public AudioClip Clip => clip;
        }
    }
}
