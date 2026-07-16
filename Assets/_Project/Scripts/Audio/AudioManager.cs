using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private List<SfxEntry> sfxEntries = new List<SfxEntry>();
        [SerializeField] private List<BgmEntry> bgmEntries = new List<BgmEntry>();
        [SerializeField] private bool warnWhenClipMissing;
        [SerializeField] private bool useProceduralFallback = true;
        [SerializeField] private bool useProceduralBgmFallback = true;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.55f;
        [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.4f;
        [SerializeField] [Range(0.05f, 1f)] private float bgmCrossFadeDuration = 0.35f;

        private const string SfxEnabledPreferenceKey = "RuneGate.SfxEnabled";
        private const string SfxVolumePreferenceKey = "RuneGate.SfxVolume";
        private const string BgmEnabledPreferenceKey = "RuneGate.BgmEnabled";
        private const string BgmVolumePreferenceKey = "RuneGate.BgmVolume";
        private static AudioManager instance;
        private static readonly HashSet<SfxKey> warnedMissingKeys = new HashSet<SfxKey>();

        private readonly Dictionary<SfxKey, AudioClip> clipsByKey = new Dictionary<SfxKey, AudioClip>();
        private readonly Dictionary<BgmTheme, AudioClip> musicClipsByTheme = new Dictionary<BgmTheme, AudioClip>();
        private AudioSource audioSource;
        private AudioSource musicSourceA;
        private AudioSource musicSourceB;
        private AudioSource activeMusicSource;
        private Coroutine musicTransition;
        private BgmTheme currentMusicTheme;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                instance.AbsorbAssignedClips(sfxEntries);
                instance.AbsorbAssignedMusic(bgmEntries);
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            RebuildClipLookup();
            RebuildMusicLookup();
            TryBindAudioSources();
            BuildProceduralFallbacks();
            BuildProceduralMusicFallbacks();
            if (HasPlayableClip() || HasPlayableMusic())
            {
                EnsureAudioListener();
            }

            ApplyAudioSourceSettings();
            PlayThemeForScene(SceneManager.GetActiveScene(), true);
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
            instance?.PlayThemeForScene(scene, false);
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
        public static bool BgmEnabled => PlayerPrefs.GetInt(BgmEnabledPreferenceKey, 1) != 0;
        public static float SfxVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumePreferenceKey, 0.55f));
        public static float BgmVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumePreferenceKey, 0.4f));

        public int PlayableClipCount => clipsByKey.Count;
        public int PlayableMusicClipCount => musicClipsByTheme.Count;
        public BgmTheme CurrentMusicTheme => currentMusicTheme;
        public bool IsMusicPlaying => activeMusicSource != null && activeMusicSource.isPlaying && BgmEnabled;

        public bool HasClip(SfxKey key)
        {
            return clipsByKey.TryGetValue(key, out AudioClip clip) && clip != null;
        }

        public bool HasMusicClip(BgmTheme theme)
        {
            return musicClipsByTheme.TryGetValue(theme, out AudioClip clip) && clip != null;
        }

        public static void SetSfxEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(SfxEnabledPreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            instance?.ApplyAudioSourceSettings();
        }

        public static void SetBgmEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(BgmEnabledPreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            if (instance == null)
            {
                return;
            }

            if (enabled)
            {
                instance.PlayThemeForScene(SceneManager.GetActiveScene(), false);
            }
            else
            {
                instance.StopMusic();
            }

            instance.ApplyAudioSourceSettings();
        }

        public static void SetSfxVolume(float volume)
        {
            PlayerPrefs.SetFloat(SfxVolumePreferenceKey, Mathf.Clamp01(volume));
            PlayerPrefs.Save();
            instance?.ApplyAudioSourceSettings();
        }

        public static void SetBgmVolume(float volume)
        {
            PlayerPrefs.SetFloat(BgmVolumePreferenceKey, Mathf.Clamp01(volume));
            PlayerPrefs.Save();
            instance?.ApplyAudioSourceSettings();
        }

        public static float NextVolumeStep(float current)
        {
            if (current < 0.35f)
            {
                return 0.5f;
            }

            if (current < 0.6f)
            {
                return 0.75f;
            }

            if (current < 0.85f)
            {
                return 1f;
            }

            return 0.25f;
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

        private void RebuildMusicLookup()
        {
            musicClipsByTheme.Clear();
            for (int i = 0; i < bgmEntries.Count; i++)
            {
                BgmEntry entry = bgmEntries[i];
                if (entry != null && entry.Theme != BgmTheme.None)
                {
                    musicClipsByTheme[entry.Theme] = entry.Clip;
                }
            }
        }

        private void TryBindAudioSources()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            musicSourceA = gameObject.AddComponent<AudioSource>();
            musicSourceB = gameObject.AddComponent<AudioSource>();
            ConfigureMusicSource(musicSourceA);
            ConfigureMusicSource(musicSourceB);
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

        private void BuildProceduralMusicFallbacks()
        {
            if (!useProceduralBgmFallback || !ProceduralBgmFactory.IsAvailable)
            {
                return;
            }

            BgmTheme[] themes = { BgmTheme.Menu, BgmTheme.Battle };
            int generatedCount = 0;
            for (int i = 0; i < themes.Length; i++)
            {
                BgmTheme theme = themes[i];
                if (HasMusicClip(theme))
                {
                    continue;
                }

                AudioClip clip = ProceduralBgmFactory.Create(theme);
                if (clip == null)
                {
                    continue;
                }

                musicClipsByTheme[theme] = clip;
                generatedCount++;
            }

            if (generatedCount > 0)
            {
                Debug.Log($"AudioManager generated {generatedCount} procedural BGM fallback clips.");
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

        private void AbsorbAssignedMusic(IReadOnlyList<BgmEntry> entries)
        {
            if (entries == null)
            {
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                BgmEntry entry = entries[i];
                if (entry != null && entry.Theme != BgmTheme.None && entry.Clip != null)
                {
                    musicClipsByTheme[entry.Theme] = entry.Clip;
                }
            }
        }

        private void ApplyAudioSourceSettings()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.volume = PlayerPrefs.HasKey(SfxVolumePreferenceKey) ? SfxVolume : Mathf.Clamp01(sfxVolume);
            audioSource.mute = !SfxEnabled;

            float resolvedBgmVolume = PlayerPrefs.HasKey(BgmVolumePreferenceKey) ? BgmVolume : Mathf.Clamp01(bgmVolume);
            if (musicTransition == null && activeMusicSource != null)
            {
                activeMusicSource.volume = resolvedBgmVolume;
            }

            if (musicSourceA != null)
            {
                musicSourceA.mute = !BgmEnabled;
            }

            if (musicSourceB != null)
            {
                musicSourceB.mute = !BgmEnabled;
            }
        }

        private void PlayThemeForScene(Scene scene, bool immediate)
        {
            BgmTheme theme = ResolveTheme(scene.name);
            currentMusicTheme = theme;
            if (!BgmEnabled || theme == BgmTheme.None || !musicClipsByTheme.TryGetValue(theme, out AudioClip clip) || clip == null)
            {
                StopMusic();
                return;
            }

            if (activeMusicSource != null && activeMusicSource.clip == clip && activeMusicSource.isPlaying)
            {
                activeMusicSource.mute = false;
                activeMusicSource.volume = ResolveBgmVolume();
                return;
            }

            if (musicTransition != null)
            {
                StopCoroutine(musicTransition);
                musicTransition = null;
            }

            AudioSource nextSource = activeMusicSource == musicSourceA ? musicSourceB : musicSourceA;
            AudioSource previousSource = activeMusicSource;
            nextSource.clip = clip;
            nextSource.loop = true;
            nextSource.mute = false;
            nextSource.volume = immediate ? ResolveBgmVolume() : 0f;
            nextSource.Play();

            if (immediate || previousSource == null || !previousSource.isPlaying)
            {
                previousSource?.Stop();
                activeMusicSource = nextSource;
                nextSource.volume = ResolveBgmVolume();
                return;
            }

            musicTransition = StartCoroutine(CrossFadeMusic(previousSource, nextSource));
        }

        private IEnumerator CrossFadeMusic(AudioSource previousSource, AudioSource nextSource)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, bgmCrossFadeDuration);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float volume = ResolveBgmVolume();
                if (previousSource != null)
                {
                    previousSource.volume = volume * (1f - progress);
                }

                if (nextSource != null)
                {
                    nextSource.volume = volume * progress;
                }

                yield return null;
            }

            previousSource?.Stop();
            if (nextSource != null)
            {
                nextSource.volume = ResolveBgmVolume();
            }

            activeMusicSource = nextSource;
            musicTransition = null;
        }

        private void StopMusic()
        {
            if (musicTransition != null)
            {
                StopCoroutine(musicTransition);
                musicTransition = null;
            }

            musicSourceA?.Stop();
            musicSourceB?.Stop();
            activeMusicSource = null;
        }

        private float ResolveBgmVolume()
        {
            return PlayerPrefs.HasKey(BgmVolumePreferenceKey) ? BgmVolume : Mathf.Clamp01(bgmVolume);
        }

        private static BgmTheme ResolveTheme(string sceneName)
        {
            return string.Equals(sceneName, "BattleScene", StringComparison.Ordinal)
                ? BgmTheme.Battle
                : BgmTheme.Menu;
        }

        private static void ConfigureMusicSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
        }

        private static void EnsureAudioListener()
        {
            if (instance == null || (!instance.HasPlayableClip() && !instance.HasPlayableMusic()))
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

        private bool HasPlayableMusic()
        {
            foreach (AudioClip clip in musicClipsByTheme.Values)
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

        [Serializable]
        private sealed class BgmEntry
        {
            [SerializeField] private BgmTheme theme;
            [SerializeField] private AudioClip clip;

            public BgmTheme Theme => theme;
            public AudioClip Clip => clip;
        }
    }
}
