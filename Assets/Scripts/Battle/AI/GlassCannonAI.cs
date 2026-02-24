using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 6 — Glass Cannon: Always uses highest damage skill.
    /// </summary>
    public class GlassCannonAI : EnemyAIBase
    {
        private readonly SkillDefinition _obliterate = new SkillDefinition(
            "Obliterate", 0, ElementType.None, 3.0f, 0, 0f, SkillTargetType.Enemy);

        private readonly SkillDefinition _devastationBeam = new SkillDefinition(
            "Devastation Beam", 0, ElementType.Lux, 2.5f, 0, 0f, SkillTargetType.Enemy,
            StatusEffectType.Burn, 2, 5f, 0.3f);

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            IBattleUnit target = PickWeightedTarget(playerParty, Element);

            // Always pick highest damage multiplier skill
            SkillDefinition chosen = _obliterate.DamageMultiplier >= _devastationBeam.DamageMultiplier
                ? _obliterate
                : _devastationBeam;

            return MakeSkillAttack(chosen, target);
        }
    }
}
