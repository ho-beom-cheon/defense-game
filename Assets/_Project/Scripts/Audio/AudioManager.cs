using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private List<SfxEntry> sfxEntries = new List<SfxEntry>();
        [SerializeField] private bool warnWhenClipMissing;

        private static AudioManager instance;
        private static readonly HashSet<SfxKey> warnedMissingKeys = new HashSet<SfxKey>();

        private readonly Dictionary<SfxKey, AudioClip> clipsByKey = new Dictionary<SfxKey, AudioClip>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

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
            if (!clipsByKey.TryGetValue(key, out AudioClip clip) || clip == null)
            {
                if (warnWhenClipMissing && warnedMissingKeys.Add(key))
                {
                    Debug.LogWarning($"AudioManager has no clip assigned for SFX '{key}'.");
                }

                return;
            }

            audioSource.PlayOneShot(clip);
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
