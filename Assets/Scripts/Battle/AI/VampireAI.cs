using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 8 — Vampire: Targets highest HP player with lifesteal attacks.
    /// Switches to stronger drain at low HP.
    /// </summary>
    public class VampireAI : EnemyAIBase
    {
        private readonly SkillDefinition _bloodDrain = new SkillDefinition(
            "Blood Drain", 0, ElementType.Umbra, 1.5f, 0, 0.5f, SkillTargetType.Enemy);

        private readonly SkillDefinition _crimsonFeast = new SkillDefinition(
            "Crimson Feast", 0, ElementType.Umbra, 2.0f, 0, 0.75f, SkillTargetType.Enemy);

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            IBattleUnit target = FindHighestHP(playerParty) ?? playerParty[0];

            // Use stronger drain when below 50% HP
            SkillDefinition skill = HPPercent < 0.5f ? _crimsonFeast : _bloodDrain;

            return MakeSkillAttack(skill, target);
        }
    }
}
