using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// UI for the flee button and retreat feedback.
    /// </summary>
    public class RetreatUI : MonoBehaviour
    {
        [Header("=== References ===")]
        [SerializeField] private BattleController _battleController;
        [SerializeField] private RetreatHandler _retreatHandler;

        [Header("=== UI Elements ===")]
        [SerializeField] private Button _fleeButton;
        [SerializeField] private TMP_Text _fleeChanceText;
        [SerializeField] private GameObject _retreatResultPanel;
        [SerializeField] private TMP_Text _retreatResultText;

        private void Awake()
        {
            if (_retreatResultPanel != null)
                _retreatResultPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_fleeButton != null)
                _fleeButton.onClick.AddListener(OnFleePressed);

            if (_battleController != null)
                _battleController.OnPlayerTurnStart += HandlePlayerTurnStart;

            if (_retreatHandler != null)
            {
                _retreatHandler.OnRetreatSuccess += HandleRetreatSuccess;
                _retreatHandler.OnRetreatFailed += HandleRetreatFailed;
            }
        }

        private void OnDisable()
        {
            if (_fleeButton != null)
                _fleeButton.onClick.RemoveListener(OnFleePressed);

            if (_battleController != null)
                _battleController.OnPlayerTurnStart -= HandlePlayerTurnStart;

            if (_retreatHandler != null)
            {
                _retreatHandler.OnRetreatSuccess -= HandleRetreatSuccess;
                _retreatHandler.OnRetreatFailed -= HandleRetreatFailed;
            }
        }

        private void HandlePlayerTurnStart(int ap)
        {
            RefreshFleeButton();
        }

        private void RefreshFleeButton()
        {
            if (_fleeButton == null) return;

            if (_retreatHandler == null || !_retreatHandler.CanRetreat)
            {
                _fleeButton.interactable = false;
                if (_fleeChanceText != null)
                    _fleeChanceText.text = "Cannot Flee";
            }
            else
            {
                _fleeButton.interactable = _battleController != null && _battleController.CanPerformAction(PlayerAction.Flee);
                if (_fleeChanceText != null)
                    _fleeChanceText.text = "Flee (75%)";
            }
        }

        private void OnFleePressed()
        {
            if (_battleController != null)
                _battleController.SelectFlee();
        }

        private void HandleRetreatSuccess(RetreatResult result)
        {
            if (_retreatResultPanel != null && _retreatResultText != null)
            {
                string streakMsg = result.StreakLost ? "\nWin streak lost!" : "";
                _retreatResultText.text = $"<color=yellow>Escaped!</color>{streakMsg}";
                _retreatResultPanel.SetActive(true);
            }
        }

        private void HandleRetreatFailed()
        {
            if (_retreatResultPanel != null && _retreatResultText != null)
            {
                _retreatResultText.text = "<color=red>Failed to flee!</color>\nAll AP consumed.";
                _retreatResultPanel.SetActive(true);
            }

            // Hide after a short delay
            if (_retreatResultPanel != null)
                StartCoroutine(HideResultAfterDelay(2f));
        }

        private System.Collections.IEnumerator HideResultAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_retreatResultPanel != null)
                _retreatResultPanel.SetActive(false);
        }
    }
}
