using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 5 — Healer Priest: Prioritizes healing wounded allies.
    /// If all allies are full HP, uses basic attack.
    /// </summary>
    public class HealerPriestAI : EnemyAIBase
    {
        private readonly SkillDefinition _benediction = new SkillDefinition(
            "Benediction", 0, ElementType.None, 0f, 40, 0f, SkillTargetType.Ally);

        private readonly SkillDefinition _sanctifiedStrike = new SkillDefinition(
            "Sanctified Strike", 0, ElementType.Lux, 1.0f, 0, 0f, SkillTargetType.Enemy);

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            // Heal most-wounded ally if any ally is missing HP
            IBattleUnit wounded = FindWoundedAlly(enemyParty, 1.0f);
            if (wounded != null)
            {
                return MakeSkillAttack(_benediction, wounded);
            }

            // All allies full — attack
            IBattleUnit target = PickWeightedTarget(playerParty, _sanctifiedStrike.Element);
            return MakeSkillAttack(_sanctifiedStrike, target);
        }
    }
}
