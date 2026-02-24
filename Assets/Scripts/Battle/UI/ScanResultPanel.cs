using System.Collections;
using UnityEngine;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// Displays scan results for a scanned enemy. Auto-hides after a configurable duration.
    /// </summary>
    public class ScanResultPanel : MonoBehaviour
    {
        [Header("=== Panel ===")]
        [SerializeField] private GameObject _panelRoot;

        [Header("=== Text Fields ===")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _atkText;
        [SerializeField] private TMP_Text _defText;
        [SerializeField] private TMP_Text _elementText;
        [SerializeField] private TMP_Text _critText;

        [Header("=== Timing ===")]
        [SerializeField] private float _displayDuration = 3f;

        private Coroutine _hideCoroutine;

        public void ShowScanResult(IBattleUnit unit)
        {
            if (_panelRoot == null) return;

            if (_nameText != null) _nameText.text = unit.UnitName;
            if (_hpText != null) _hpText.text = $"HP: {unit.CurrentHP}/{unit.MaxHP}";
            if (_atkText != null) _atkText.text = $"ATK: {unit.AttackPower}";
            if (_defText != null) _defText.text = $"DEF: {unit.Defense}";
            if (_elementText != null) _elementText.text = $"Element: {unit.Element}";
            if (_critText != null) _critText.text = $"Crit: {(unit.CritChance * 100f):F0}%";

            _panelRoot.SetActive(true);

            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);
            _hideCoroutine = StartCoroutine(AutoHide());
        }

        public void Hide()
        {
            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }

            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        private IEnumerator AutoHide()
        {
            yield return new WaitForSeconds(_displayDuration);
            Hide();
        }

        private void Awake()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }
    }
}
