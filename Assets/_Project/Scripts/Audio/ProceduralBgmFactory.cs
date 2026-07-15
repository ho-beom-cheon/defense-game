using UnityEngine;

namespace RuneGate
{
    public static class ProceduralBgmFactory
    {
        private const int SampleRate = 22050;

        public static bool IsAvailable => true;

        public static AudioClip Create(BgmTheme theme)
        {
            if (theme == BgmTheme.None)
            {
                return null;
            }

            float beatDuration = theme == BgmTheme.Battle ? 0.375f : 0.5f;
            int beatCount = theme == BgmTheme.Battle ? 24 : 16;
            float duration = beatDuration * beatCount;
            int sampleCount = Mathf.CeilToInt(duration * SampleRate);
            AudioClip clip = AudioClip.Create($"RuneGate Procedural {theme}", sampleCount, 1, SampleRate, false);
            if (clip == null)
            {
                return null;
            }

            float[] samples = BuildSamples(theme, sampleCount, beatDuration, beatCount);
            if (!clip.SetData(samples, 0))
            {
                Object.Destroy(clip);
                return null;
            }

            return clip;
        }

        private static float[] BuildSamples(BgmTheme theme, int sampleCount, float beatDuration, int beatCount)
        {
            float[] notes = theme == BgmTheme.Battle
                ? new[] { 146f, 174f, 196f, 220f, 196f, 174f, 164f, 174f }
                : new[] { 220f, 262f, 330f, 294f, 247f, 294f, 330f, 262f };
            float[] samples = new float[sampleCount];
            System.Random random = new System.Random(theme == BgmTheme.Battle ? 9137 : 4211);

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)SampleRate;
                int beatIndex = Mathf.Min(beatCount - 1, Mathf.FloorToInt(time / beatDuration));
                float beatProgress = Mathf.Repeat(time, beatDuration) / beatDuration;
                float note = notes[beatIndex % notes.Length];
                float noteEnvelope = Mathf.Pow(Mathf.Sin(beatProgress * Mathf.PI), 1.35f);
                float melody = Sine(time, note) * 0.58f + Sine(time, note * 2f) * 0.12f;
                float bass = Sine(time, note * 0.5f) * 0.3f;
                float percussion = 0f;

                if (theme == BgmTheme.Battle)
                {
                    float decay = Mathf.Exp(-beatProgress * 18f);
                    float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                    percussion = (Sine(time, 72f - beatProgress * 24f) * 0.65f + noise * 0.35f) * decay;
                }

                float sample = (melody + bass) * noteEnvelope + percussion * 0.42f;
                samples[i] = Mathf.Clamp(sample * (theme == BgmTheme.Battle ? 0.22f : 0.18f), -0.65f, 0.65f);
            }

            return samples;
        }

        private static float Sine(float time, float frequency)
        {
            return Mathf.Sin(time * frequency * Mathf.PI * 2f);
        }
    }
}
