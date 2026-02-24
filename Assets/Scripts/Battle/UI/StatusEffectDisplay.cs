using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// Manages a row of status effect labels for one unit.
    /// Creates/destroys TMP_Text child objects in a container (HorizontalLayoutGroup recommended).
    /// </summary>
    public class StatusEffectDisplay : MonoBehaviour
    {
        [Header("=== Container (HorizontalLayoutGroup parent) ===")]
        [SerializeField] private RectTransform _container;
        [SerializeField] private TMP_Text _labelPrefab;

        private IBattleUnit _unit;
        private BattleController _controller;
        private readonly List<TMP_Text> _activeLabels = new List<TMP_Text>();
        private bool _initialized;

        public void Init(IBattleUnit unit, BattleController controller)
        {
            _unit = unit;
            _controller = controller;
            _initialized = true;
            Refresh();
        }

        public void Refresh()
        {
            if (!_initialized || _unit == null || _controller == null) return;
            if (_container == null || _labelPrefab == null) return;

            var effects = _controller.GetStatusEffects(_unit);

            // Grow or shrink label pool
            while (_activeLabels.Count < effects.Count)
            {
                TMP_Text label = Instantiate(_labelPrefab, _container);
                label.gameObject.SetActive(true);
                _activeLabels.Add(label);
            }

            while (_activeLabels.Count > effects.Count)
            {
                int last = _activeLabels.Count - 1;
                Destroy(_activeLabels[last].gameObject);
                _activeLabels.RemoveAt(last);
            }

            // Update label contents
            for (int i = 0; i < effects.Count; i++)
            {
                StatusEffect eff = effects[i];
                bool isBuff = eff.Type == StatusEffectType.AtkUp || eff.Type == StatusEffectType.DefUp;
                string colorHex = isBuff ? "#44ddff" : "#ff4444";

                _activeLabels[i].text = $"<color={colorHex}>[{eff.Type}:{eff.RemainingTurns}]</color>";
            }
        }

        private void OnDestroy()
        {
            foreach (var label in _activeLabels)
            {
                if (label != null)
                    Destroy(label.gameObject);
            }
            _activeLabels.Clear();
        }
    }
}
