using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 11 — Coward: Flees when HP drops to 30% or below.
    /// Moderate aggression otherwise.
    /// </summary>
    public class CowardAI : EnemyAIBase
    {
        /// <summary>Whether this unit fled instead of dying. Reward system can check this.</summary>
        public bool HasFled { get; private set; }

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            // Flee at 30% HP or below
            if (HPPercent <= 0.3f)
            {
                HasFled = true;
                _isAlive = false;
                // Return a dummy intent targeting first player (won't execute since unit is dead)
                return new EnemyIntent(playerParty[0], false, 0);
            }

            // Basic attack with moderate aggression
            IBattleUnit target = PickWeightedTarget(playerParty, Element);
            return MakeBasicAttack(target);
        }
    }
}
