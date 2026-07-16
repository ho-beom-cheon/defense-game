using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RuneGate
{
    public sealed class RuneGateButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private float pressedScale = 0.96f;
        [SerializeField] private float duration = 0.08f;

        private Vector3 restScale = Vector3.one;
        private Coroutine scaleRoutine;

        private void Awake()
        {
            restScale = transform.localScale;
        }

        public void Configure(float animationDuration)
        {
            duration = Mathf.Max(0.01f, animationDuration);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateTo(restScale * pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimateTo(restScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            AnimateTo(restScale);
        }

        private void OnDisable()
        {
            transform.localScale = restScale;
        }

        private void AnimateTo(Vector3 target)
        {
            if (!isActiveAndEnabled)
            {
                transform.localScale = target;
                return;
            }

            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
            }

            scaleRoutine = StartCoroutine(ScaleRoutine(target));
        }

        private IEnumerator ScaleRoutine(Vector3 target)
        {
            Vector3 from = transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localScale = Vector3.LerpUnclamped(from, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            transform.localScale = target;
            scaleRoutine = null;
        }
    }
}
