using UnityEngine;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// Floating damage/heal/status text that animates upward, fades out, then self-destructs.
    /// Instantiated by DamagePopupSpawner. Attach to a prefab with TMP_Text + CanvasGroup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _lifetime = 1f;
        [SerializeField] private float _floatSpeed = 80f;
        [SerializeField] private float _critScaleMultiplier = 1.5f;

        private CanvasGroup _canvasGroup;
        private float _elapsed;
        private Vector3 _startScale;

        public void Init(string text, Color color, bool isCritical)
        {
            if (_text == null)
                _text = GetComponentInChildren<TMP_Text>();

            _text.text = text;
            _text.color = color;

            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 1f;

            _startScale = transform.localScale;
            if (isCritical)
            {
                transform.localScale = _startScale * _critScaleMultiplier;
                _text.fontStyle = FontStyles.Bold;
            }

            _elapsed = 0f;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / _lifetime;

            // Float upward
            transform.localPosition += Vector3.up * (_floatSpeed * Time.deltaTime);

            // Fade out in the second half of lifetime
            if (t > 0.5f)
            {
                _canvasGroup.alpha = 1f - ((t - 0.5f) / 0.5f);
            }

            if (_elapsed >= _lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
