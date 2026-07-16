using System;
using UnityEngine;

namespace RuneGate
{
    public static class ProceduralSfxFactory
    {
        private const int SampleRate = 22050;

        public static bool IsAvailable => true;

        public static AudioClip Create(SfxKey key)
        {
            float duration = ResolveDuration(key);
            int sampleCount = Mathf.Max(128, Mathf.CeilToInt(duration * SampleRate));
            AudioClip clip = AudioClip.Create($"RuneGate Procedural {key}", sampleCount, 1, SampleRate, false);
            if (clip == null)
            {
                return null;
            }

            float[] samples = BuildSamples(key, sampleCount, duration);
            if (!clip.SetData(samples, 0))
            {
                UnityEngine.Object.Destroy(clip);
                return null;
            }

            return clip;
        }

        private static float[] BuildSamples(SfxKey key, int sampleCount, float duration)
        {
            float[] samples = new float[sampleCount];
            System.Random random = new System.Random(7127 + (int)key * 7919);
            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)SampleRate;
                float progress = Mathf.Clamp01(time / duration);
                float attack = Mathf.Clamp01(progress / 0.06f);
                float release = Mathf.Pow(Mathf.Clamp01(1f - progress), 1.45f);
                float envelope = attack * release;
                float sample = ResolveWave(key, time, progress, random);
                samples[i] = Mathf.Clamp(sample * envelope * 0.24f, -0.72f, 0.72f);
            }

            return samples;
        }

        private static float ResolveWave(SfxKey key, float time, float progress, System.Random random)
        {
            float noise = (float)(random.NextDouble() * 2.0 - 1.0);
            switch (key)
            {
                case SfxKey.ButtonClick:
                    return Sine(time, 720f - progress * 180f);
                case SfxKey.HeroAttack:
                    return Sine(time, 360f - progress * 150f) * 0.7f + noise * 0.3f;
                case SfxKey.MonsterHit:
                    return Square(time, 190f - progress * 70f) * 0.45f + noise * 0.55f;
                case SfxKey.MonsterDeath:
                    return Sine(time, 175f - progress * 115f) * 0.55f + noise * (0.45f * (1f - progress));
                case SfxKey.CrystalHit:
                    return Sine(time, 115f - progress * 35f) * 0.7f + Sine(time, 230f) * 0.3f;
                case SfxKey.RuneSelect:
                    return Sine(time, 460f + progress * 520f) * 0.65f + Sine(time, 690f + progress * 260f) * 0.35f;
                case SfxKey.Victory:
                    return Arpeggio(time, progress, new[] { 523.25f, 659.25f, 783.99f, 1046.5f });
                case SfxKey.Defeat:
                    return Sine(time, 260f - progress * 165f) * 0.72f + Sine(time, 130f - progress * 45f) * 0.28f;
                case SfxKey.UpgradePurchase:
                    return Arpeggio(time, progress, new[] { 440f, 554.37f, 659.25f });
                default:
                    return Sine(time, 440f);
            }
        }

        private static float Arpeggio(float time, float progress, float[] notes)
        {
            int index = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(progress * notes.Length));
            return Sine(time, notes[index]) * 0.78f + Sine(time, notes[index] * 2f) * 0.22f;
        }

        private static float Sine(float time, float frequency)
        {
            return Mathf.Sin(time * frequency * Mathf.PI * 2f);
        }

        private static float Square(float time, float frequency)
        {
            return Sine(time, frequency) >= 0f ? 1f : -1f;
        }

        private static float ResolveDuration(SfxKey key)
        {
            switch (key)
            {
                case SfxKey.ButtonClick:
                    return 0.07f;
                case SfxKey.HeroAttack:
                case SfxKey.MonsterHit:
                    return 0.10f;
                case SfxKey.MonsterDeath:
                case SfxKey.CrystalHit:
                    return 0.20f;
                case SfxKey.RuneSelect:
                case SfxKey.UpgradePurchase:
                    return 0.24f;
                case SfxKey.Victory:
                case SfxKey.Defeat:
                    return 0.55f;
                default:
                    return 0.12f;
            }
        }
    }
}
