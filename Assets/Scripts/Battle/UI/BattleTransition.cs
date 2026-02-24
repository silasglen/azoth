using System.Collections;
using UnityEngine;

namespace Battle.UI
{
    /// <summary>
    /// CanvasGroup-based fade transition for entering/exiting battles.
    /// Supports Fade (standard) and Flash (boss) transition styles.
    /// </summary>
    public class BattleTransition : MonoBehaviour
    {
        public enum TransitionStyle
        {
            Fade,
            Flash
        }

        [Header("=== References ===")]
        [SerializeField] private CanvasGroup _fadeCanvasGroup;

        [Header("=== Settings ===")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.5f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private TransitionStyle _style = TransitionStyle.Fade;

        [Header("=== Flash Settings (boss encounters) ===")]
        [SerializeField] private float _flashDuration = 0.15f;
        [SerializeField] private int _flashCount = 3;

        private void Awake()
        {
            if (_fadeCanvasGroup != null)
            {
                _fadeCanvasGroup.alpha = 0f;
                _fadeCanvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Fade in (screen goes black / overlay appears). alpha 0 -> 1.
        /// </summary>
        public Coroutine FadeIn()
        {
            return StartCoroutine(_style == TransitionStyle.Flash ? FlashRoutine() : FadeRoutine(0f, 1f, _fadeInDuration));
        }

        /// <summary>
        /// Fade out (overlay disappears, scene becomes visible). alpha 1 -> 0.
        /// </summary>
        public Coroutine FadeOut()
        {
            return StartCoroutine(FadeRoutine(1f, 0f, _fadeOutDuration));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration)
        {
            if (_fadeCanvasGroup == null) yield break;

            _fadeCanvasGroup.blocksRaycasts = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveT = _fadeCurve.Evaluate(t);
                _fadeCanvasGroup.alpha = Mathf.Lerp(from, to, curveT);
                yield return null;
            }

            _fadeCanvasGroup.alpha = to;
            _fadeCanvasGroup.blocksRaycasts = to > 0.5f;
        }

        private IEnumerator FlashRoutine()
        {
            if (_fadeCanvasGroup == null) yield break;

            _fadeCanvasGroup.blocksRaycasts = true;

            for (int i = 0; i < _flashCount; i++)
            {
                _fadeCanvasGroup.alpha = 1f;
                yield return new WaitForSecondsRealtime(_flashDuration);
                _fadeCanvasGroup.alpha = 0f;
                yield return new WaitForSecondsRealtime(_flashDuration);
            }

            // End with full fade in
            yield return FadeRoutine(0f, 1f, _fadeInDuration);
        }
    }
}
