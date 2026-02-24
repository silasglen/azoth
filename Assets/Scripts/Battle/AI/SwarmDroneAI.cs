using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 14 — Swarm Drone: Damage scales with number of living allies
    /// whose UnitName contains "Swarm".
    /// </summary>
    public class SwarmDroneAI : EnemyAIBase
    {
        private readonly SkillDefinition _swarmStrike = new SkillDefinition(
            "Swarm Strike", 0, ElementType.None, 1.0f, 0, 0f, SkillTargetType.Enemy);

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            // Count living swarm allies (not self)
            int swarmCount = 0;
            for (int i = 0; i < enemyParty.Count; i++)
            {
                if (!enemyParty[i].IsAlive) continue;
                if (enemyParty[i] == (IBattleUnit)this) continue;
                if (enemyParty[i].UnitName.Contains("Swarm"))
                    swarmCount++;
            }

            // Dynamic damage bonus based on swarm size
            float damageBonus = 1.0f + swarmCount * 0.2f;
            _swarmStrike.DamageMultiplier = damageBonus;

            IBattleUnit target = PickWeightedTarget(playerParty, Element);
            return MakeSkillAttack(_swarmStrike, target);
        }
    }
}
