using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuneGate
{
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private List<SfxEntry> sfxEntries = new List<SfxEntry>();
        [SerializeField] private bool warnWhenClipMissing;

        private static AudioManager instance;
        private static readonly HashSet<SfxKey> warnedMissingKeys = new HashSet<SfxKey>();

        private readonly Dictionary<SfxKey, UnityEngine.Object> clipsByKey = new Dictionary<SfxKey, UnityEngine.Object>();
        private Component audioSource;
        private Type audioClipType;
        private MethodInfo playOneShotMethod;
        private static Type audioListenerType;
        private static MethodInfo findAnyObjectByTypeMethod;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            RebuildClipLookup();
            if (HasPlayableClip())
            {
                TryBindAudioSource();
                EnsureAudioListener();
            }
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
            if (instance == null)
            {
                return;
            }

            instance.PlayLocal(key);
        }

        public void PlayLocal(SfxKey key)
        {
            if (!clipsByKey.TryGetValue(key, out UnityEngine.Object clip) || clip == null)
            {
                if (warnWhenClipMissing && warnedMissingKeys.Add(key))
                {
                    Debug.LogWarning($"AudioManager has no clip assigned for SFX '{key}'.");
                }

                return;
            }

            if (audioSource == null || playOneShotMethod == null || audioClipType == null || !audioClipType.IsInstanceOfType(clip))
            {
                return;
            }

            playOneShotMethod.Invoke(audioSource, new object[] { clip });
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
            Type audioSourceType = Type.GetType("UnityEngine.AudioSource, UnityEngine.AudioModule");
            if (audioSourceType == null)
            {
                return;
            }

            audioSource = GetComponent(audioSourceType);
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent(audioSourceType);
            }

            audioClipType = Type.GetType("UnityEngine.AudioClip, UnityEngine.AudioModule");
            if (audioClipType == null)
            {
                return;
            }

            playOneShotMethod = audioSourceType.GetMethod("PlayOneShot", new[] { audioClipType });
        }

        private static void EnsureAudioListener()
        {
            if (instance == null || !instance.HasPlayableClip())
            {
                return;
            }

            Type listenerType = GetAudioListenerType();
            if (!IsEngineComponentType(listenerType) || FindExistingAudioListener(listenerType) != null)
            {
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                if (mainCamera.gameObject.GetComponent(listenerType) == null)
                {
                    mainCamera.gameObject.AddComponent(listenerType);
                }

                return;
            }

            GameObject listenerObject = new GameObject("Runtime Audio Listener");
            listenerObject.hideFlags = HideFlags.DontSave;
            listenerObject.AddComponent(listenerType);
        }

        private static bool IsEngineComponentType(Type componentType)
        {
            return componentType != null && typeof(Component).IsAssignableFrom(componentType);
        }

        private bool HasPlayableClip()
        {
            foreach (UnityEngine.Object clip in clipsByKey.Values)
            {
                if (clip != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static Type GetAudioListenerType()
        {
            if (audioListenerType == null)
            {
                audioListenerType = Type.GetType("UnityEngine.AudioListener, UnityEngine.AudioModule");
            }

            return audioListenerType;
        }

        private static UnityEngine.Object FindExistingAudioListener(Type listenerType)
        {
            MethodInfo method = GetFindAnyObjectByTypeMethod();
            if (method == null)
            {
                return null;
            }

            return method.Invoke(null, new object[] { listenerType }) as UnityEngine.Object;
        }

        private static MethodInfo GetFindAnyObjectByTypeMethod()
        {
            if (findAnyObjectByTypeMethod == null)
            {
                findAnyObjectByTypeMethod = typeof(UnityEngine.Object).GetMethod("FindAnyObjectByType", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Type) }, null);
            }

            return findAnyObjectByTypeMethod;
        }

        [Serializable]
        private sealed class SfxEntry
        {
            [SerializeField] private SfxKey key;
            [SerializeField] private UnityEngine.Object clip;

            public SfxKey Key => key;
            public UnityEngine.Object Clip => clip;
        }
    }
}
