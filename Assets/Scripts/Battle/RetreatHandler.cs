using System;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Handles flee/retreat logic. Can be triggered by SelectFlee() or Escape Flare item.
    /// </summary>
    public class RetreatHandler : MonoBehaviour
    {
        [SerializeField] private BattleController _battleController;
        [SerializeField] private BattleStatisticsTracker _statsTracker;

        [SerializeField, Range(0f, 1f)] private float _baseFleeChance = 0.75f;
        [SerializeField] private bool _isBossBattle = false;

        public event Action<RetreatResult> OnRetreatSuccess;
        public event Action OnRetreatFailed;

        public bool CanRetreat => !_isBossBattle;

        /// <summary>
        /// Attempts to retreat from battle.
        /// </summary>
        /// <param name="guaranteedSuccess">True for Escape Flare usage (auto-succeed regardless of boss).</param>
        /// <returns>True if retreat succeeded.</returns>
        public bool TryRetreat(bool guaranteedSuccess = false)
        {
            if (_isBossBattle && !guaranteedSuccess)
            {
                OnRetreatFailed?.Invoke();
                return false;
            }

            bool success = guaranteedSuccess || UnityEngine.Random.value < _baseFleeChance;

            if (success)
            {
                bool hadStreak = BattleStatisticsTracker.WinStreak > 0;
                BattleStatisticsTracker.WinStreak = 0;

                var result = new RetreatResult(0, 0, hadStreak);
                OnRetreatSuccess?.Invoke(result);

                if (_battleController != null)
                {
                    _battleController.AbortBattle();
                    _battleController.NotifyRetreat();
                }

                return true;
            }

            OnRetreatFailed?.Invoke();
            return false;
        }
    }
}
