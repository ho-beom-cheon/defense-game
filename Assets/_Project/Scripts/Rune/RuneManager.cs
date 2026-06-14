using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGate
{
    public sealed class RuneManager : MonoBehaviour
    {
        [SerializeField] private List<RuneData> availableRunes = new List<RuneData>();
        [SerializeField] private int optionsPerSelection = 3;

        private readonly List<RuneData> currentOptions = new List<RuneData>();

        public event Action<IReadOnlyList<RuneData>> RuneOptionsGenerated;
        public event Action<RuneData> RuneSelected;

        public IReadOnlyList<RuneData> CurrentOptions => currentOptions;

        public IReadOnlyList<RuneData> GenerateRuneOptions()
        {
            currentOptions.Clear();

            List<RuneData> pool = new List<RuneData>();
            for (int i = 0; i < availableRunes.Count; i++)
            {
                if (availableRunes[i] != null && !pool.Contains(availableRunes[i]))
                {
                    pool.Add(availableRunes[i]);
                }
            }

            int optionCount = Mathf.Min(Mathf.Max(1, optionsPerSelection), pool.Count);
            for (int i = 0; i < optionCount; i++)
            {
                int index = UnityEngine.Random.Range(0, pool.Count);
                currentOptions.Add(pool[index]);
                pool.RemoveAt(index);
            }

            if (currentOptions.Count == 0)
            {
                Debug.LogWarning("RuneManager has no available runes to offer.");
            }

            RuneOptionsGenerated?.Invoke(currentOptions);
            return currentOptions;
        }

        public void SelectRuneAt(int index)
        {
            if (index < 0 || index >= currentOptions.Count)
            {
                Debug.LogWarning($"RuneManager received invalid rune option index {index}.");
                return;
            }

            SelectRune(currentOptions[index]);
        }

        public void SelectRune(RuneData runeData)
        {
            if (runeData == null)
            {
                Debug.LogWarning("RuneManager cannot select a null rune.");
                return;
            }

            if (!currentOptions.Contains(runeData))
            {
                Debug.LogWarning($"RuneManager rejected {runeData.DisplayName} because it is not in the current option set.");
                return;
            }

            RuneSelected?.Invoke(runeData);
            AudioManager.Play(SfxKey.RuneSelect);
            currentOptions.Clear();
        }
    }
}
