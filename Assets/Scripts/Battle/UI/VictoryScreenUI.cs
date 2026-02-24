using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// Shows a post-battle results panel on victory.
    /// Subscribes to OnBattleWon, populates stats from BattleStatisticsTracker.
    /// </summary>
    public class VictoryScreenUI : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private BattleController _battleController;
        [SerializeField] private BattleStatisticsTracker _statsTracker;

        [Header("=== Panel ===")]
        [SerializeField] private GameObject _victoryPanel;

        [Header("=== Text Fields ===")]
        [SerializeField] private TMP_Text _gradeText;
        [SerializeField] private TMP_Text _turnsText;
        [SerializeField] private TMP_Text _damageDealtText;
        [SerializeField] private TMP_Text _damageTakenText;
        [SerializeField] private TMP_Text _healingText;
        [SerializeField] private TMP_Text _critsText;
        [SerializeField] private TMP_Text _durationText;
        [SerializeField] private TMP_Text _streakText;

        [Header("=== Continue Button ===")]
        [SerializeField] private Button _continueButton;

        private void Awake()
        {
            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_battleController != null)
                _battleController.OnBattleWon += ShowVictoryScreen;

            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinue);
        }

        private void OnDisable()
        {
            if (_battleController != null)
                _battleController.OnBattleWon -= ShowVictoryScreen;

            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(OnContinue);
        }

        private void ShowVictoryScreen()
        {
            if (_victoryPanel == null || _statsTracker == null) return;

            if (_gradeText != null)
            {
                BattleGrade grade = _statsTracker.CalculateGradeEnum();
                string gradeColor = GetGradeColor(grade);
                _gradeText.text = $"<color={gradeColor}><size=150%>{grade}</size></color>";
            }

            if (_turnsText != null)
                _turnsText.text = $"Turns: {_statsTracker.TurnsElapsed}";
            if (_damageDealtText != null)
                _damageDealtText.text = $"Damage Dealt: {_statsTracker.TotalDamageDealt}";
            if (_damageTakenText != null)
                _damageTakenText.text = $"Damage Taken: {_statsTracker.TotalDamageTaken}";
            if (_healingText != null)
                _healingText.text = $"Healing Done: {_statsTracker.TotalHealingDone}";
            if (_critsText != null)
                _critsText.text = $"Critical Hits: {_statsTracker.CriticalHitsLanded}";
            if (_durationText != null)
                _durationText.text = $"Duration: {_statsTracker.BattleDuration:F1}s";
            if (_streakText != null)
                _streakText.text = $"Win Streak: {BattleStatisticsTracker.WinStreak}";

            _victoryPanel.SetActive(true);
        }

        private void OnContinue()
        {
            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);
        }

        private static string GetGradeColor(BattleGrade grade)
        {
            switch (grade)
            {
                case BattleGrade.S: return "#ffdd44";
                case BattleGrade.A: return "#44ff44";
                case BattleGrade.B: return "#4488ff";
                case BattleGrade.C: return "#aaaaaa";
                case BattleGrade.D: return "#cc4444";
                default: return "#cccccc";
            }
        }
    }
}
