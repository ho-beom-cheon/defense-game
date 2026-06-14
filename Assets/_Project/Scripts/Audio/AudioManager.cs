using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            TryBindAudioSource();
            RebuildClipLookup();
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
