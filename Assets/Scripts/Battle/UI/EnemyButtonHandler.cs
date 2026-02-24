using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI
{
    /// <summary>
    /// Attach to each enemy's UI button/panel. Handles click events
    /// and visual states for targeting and unblockable glow.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EnemyButtonHandler : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _targetableColor = Color.yellow;
        [SerializeField] private Color _glowColor = Color.red;

        public event Action OnClicked;

        private Button _button;
        private bool _isGlowing;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        /// <summary>
        /// Highlight this enemy as a valid attack target.
        /// </summary>
        public void SetTargetable(bool targetable)
        {
            if (_isGlowing) return; // Don't override glow

            if (_backgroundImage != null)
                _backgroundImage.color = targetable ? _targetableColor : _normalColor;

            _button.interactable = targetable;
        }

        /// <summary>
        /// Apply red glow for unblockable attack intent.
        /// Overrides targetable highlight.
        /// </summary>
        public void SetGlow(bool glow)
        {
            _isGlowing = glow;
            if (_backgroundImage != null)
                _backgroundImage.color = glow ? _glowColor : _normalColor;
        }
    }
}
