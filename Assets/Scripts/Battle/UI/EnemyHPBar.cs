using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// Visual HP bar for one enemy unit. Uses Image (Filled, Horizontal) for the bar
    /// and an optional delayed trail fill for a damage "drain" effect.
    /// HP numbers are hidden by default and shown after scanning.
    /// </summary>
    public class EnemyHPBar : MonoBehaviour
    {
        [Header("=== Bar Elements ===")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _delayedFillImage;
        [SerializeField] private TMP_Text _hpNumberText;

        [Header("=== Colors ===")]
        [SerializeField] private Color _fullColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color _lowColor = new Color(0.8f, 0.2f, 0.2f);

        [Header("=== Trail Settings ===")]
        [SerializeField] private float _trailLerpSpeed = 2f;

        private IBattleUnit _unit;
        private bool _showNumbers;
        private float _delayedFill = 1f;
        private bool _initialized;

        public void Init(IBattleUnit unit, bool showNumbers)
        {
            _unit = unit;
            _showNumbers = showNumbers;
            _initialized = true;
            _delayedFill = 1f;

            RefreshHP();
            // Initialize delayed fill to match current
            if (_delayedFillImage != null)
                _delayedFillImage.fillAmount = _fillImage != null ? _fillImage.fillAmount : 1f;
        }

        public void SetShowNumbers(bool show)
        {
            _showNumbers = show;
            RefreshHP();
        }

        public void RefreshHP()
        {
            if (!_initialized || _unit == null) return;

            float ratio = _unit.MaxHP > 0 ? (float)_unit.CurrentHP / _unit.MaxHP : 0f;

            if (_fillImage != null)
            {
                _fillImage.fillAmount = ratio;
                _fillImage.color = Color.Lerp(_lowColor, _fullColor, ratio);
            }

            if (_hpNumberText != null)
            {
                if (_showNumbers && _unit.IsAlive)
                    _hpNumberText.text = $"{_unit.CurrentHP}/{_unit.MaxHP}";
                else if (!_unit.IsAlive)
                    _hpNumberText.text = "DEAD";
                else
                    _hpNumberText.text = "";
            }
        }

        private void Update()
        {
            if (!_initialized || _delayedFillImage == null || _fillImage == null) return;

            float target = _fillImage.fillAmount;
            if (_delayedFill > target)
            {
                _delayedFill = Mathf.Lerp(_delayedFill, target, _trailLerpSpeed * Time.deltaTime);
                // Snap when close enough
                if (_delayedFill - target < 0.005f)
                    _delayedFill = target;
            }
            else
            {
                _delayedFill = target;
            }

            _delayedFillImage.fillAmount = _delayedFill;
        }
    }
}
