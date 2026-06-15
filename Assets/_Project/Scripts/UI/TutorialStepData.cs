using System;
using UnityEngine;

namespace RuneGate
{
    [Serializable]
    public sealed class TutorialStepData
    {
        [SerializeField] private string title;
        [SerializeField] private string body;

        public TutorialStepData(string title, string body)
        {
            this.title = title;
            this.body = body;
        }

        public string Title => title;
        public string Body => body;
    }
}
