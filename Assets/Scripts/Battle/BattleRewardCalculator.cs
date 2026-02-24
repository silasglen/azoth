using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Subscribes to OnBattleWon and produces a BattleRewards data packet.
    /// Single source of truth for all reward data.
    /// </summary>
    public class BattleRewardCalculator : MonoBehaviour
    {
        [SerializeField] private BattleController _battleController;
        [SerializeField] private BattleStatisticsTracker _statsTracker;
        [SerializeField] private BonusConditionEvaluator _bonusEvaluator;

        [SerializeField] private float _maxTotalMultiplier = 5f;

        public event Action<BattleRewards> OnRewardsCalculated;
        public BattleRewards LastRewards { get; private set; }

        private void OnEnable()
        {
            if (_battleController != null)
                _battleController.OnBattleWon += HandleBattleWon;
        }

        private void OnDisable()
        {
            if (_battleController != null)
                _battleController.OnBattleWon -= HandleBattleWon;
        }

        private void HandleBattleWon()
        {
            if (_statsTracker == null) return;

            // Ensure default loot tables are registered
            LootTableRegistry.RegisterDefaultTables();

            // 1. Grade
            BattleGrade grade = _statsTracker.CalculateGradeEnum();

            // 2. Multipliers
            float gradeMult = PerformanceMetrics.GetGradeXPMultiplier(grade);
            float streakMult = PerformanceMetrics.GetStreakMultiplier(BattleStatisticsTracker.WinStreak);
            float totalMult = Mathf.Min(gradeMult * streakMult, _maxTotalMultiplier);

            // 3. Bonus conditions
            List<BonusConditionResult> bonusResults = _bonusEvaluator != null
                ? _bonusEvaluator.EvaluateAll()
                : new List<BonusConditionResult>();

            int bonusXP = 0;
            foreach (var bonus in bonusResults)
            {
                if (bonus.Completed)
                    bonusXP += bonus.BonusXP;
            }

            // 4. Roll loot + sum base XP/Gold from each killed enemy
            int baseXP = 0;
            int baseGold = 0;
            var allLoot = new List<LootDrop>();

            for (int i = 0; i < _battleController.EnemyUnits.Count; i++)
            {
                IBattleUnit enemy = _battleController.EnemyUnits[i];
                if (!enemy.IsAlive) // dead = killed
                {
                    EnemyLootTable table = LootTableRegistry.GetTable(enemy.UnitName);
                    if (table != null)
                    {
                        baseXP += table.BaseXP;
                        baseGold += table.BaseGold;
                        var drops = LootTableRegistry.RollLoot(table, grade, BattleStatisticsTracker.WinStreak);
                        allLoot.AddRange(drops);
                    }
                }
            }

            // 5. Calculate finals
            int finalXP = (int)(baseXP * totalMult) + bonusXP;
            int goldEarned = (int)(baseGold * totalMult);

            // 6. Build rewards
            LastRewards = new BattleRewards
            {
                Grade = grade,
                BaseXP = baseXP,
                BonusXP = bonusXP,
                GradeMultiplier = gradeMult,
                StreakMultiplier = streakMult,
                TotalMultiplier = totalMult,
                FinalXP = finalXP,
                GoldEarned = goldEarned,
                Loot = allLoot,
                BonusConditions = bonusResults,
                WinStreak = BattleStatisticsTracker.WinStreak,
            };

            OnRewardsCalculated?.Invoke(LastRewards);
        }
    }
}
