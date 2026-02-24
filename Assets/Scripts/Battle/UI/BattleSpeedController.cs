using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// Cycles Time.timeScale between 1x/2x/4x via a button.
    /// Fully standalone — subscribes to battle end events to reset speed.
    /// </summary>
    public class BattleSpeedController : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private BattleController _battleController;
        [SerializeField] private Button _speedButton;
        [SerializeField] private TMP_Text _speedLabel;

        private static readonly float[] SpeedSteps = { 1f, 2f, 4f };
        private int _currentStep;

        private void OnEnable()
        {
            _currentStep = 0;
            UpdateDisplay();

            if (_speedButton != null)
                _speedButton.onClick.AddListener(CycleSpeed);

            if (_battleController != null)
            {
                _battleController.OnBattleWon += ResetSpeed;
                _battleController.OnBattleLost += ResetSpeed;
            }
        }

        private void OnDisable()
        {
            if (_speedButton != null)
                _speedButton.onClick.RemoveListener(CycleSpeed);

            if (_battleController != null)
            {
                _battleController.OnBattleWon -= ResetSpeed;
                _battleController.OnBattleLost -= ResetSpeed;
            }

            // Always reset to normal speed when disabled
            Time.timeScale = 1f;
        }

        private void CycleSpeed()
        {
            _currentStep = (_currentStep + 1) % SpeedSteps.Length;
            Time.timeScale = SpeedSteps[_currentStep];
            UpdateDisplay();
        }

        private void ResetSpeed()
        {
            _currentStep = 0;
            Time.timeScale = 1f;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_speedLabel != null)
                _speedLabel.text = $"{SpeedSteps[_currentStep]:F0}x";
        }
    }
}
