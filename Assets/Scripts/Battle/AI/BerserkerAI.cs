using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 1 — Berserker: Dynamic aggression that increases as HP drops.
    /// At 25% HP or below, enters enrage (permanent AtkUp).
    /// </summary>
    public class BerserkerAI : EnemyAIBase
    {
        private bool _hasEnraged;

        private readonly SkillDefinition _rageStrike = new SkillDefinition(
            "Rage Strike", 0, ElementType.None, 2.0f, 0, 0f, SkillTargetType.Enemy);

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            // Enrage at 25% HP (once)
            if (!_hasEnraged && HPPercent <= 0.25f && _battleController != null)
            {
                _hasEnraged = true;
                _battleController.ApplyStatusEffect(this, StatusEffectType.AtkUp, 99, 1.5f);
            }

            // Dynamic aggression: more aggressive at lower HP
            float aggression = Mathf.Lerp(0.3f, 1.0f, 1f - HPPercent);

            IBattleUnit target = PickWeightedTarget(playerParty, Element);

            // Use Rage Strike based on aggression, else basic attack
            if (Random.value < aggression)
            {
                return MakeSkillAttack(_rageStrike, target);
            }

            return MakeBasicAttack(target);
        }
    }
}
