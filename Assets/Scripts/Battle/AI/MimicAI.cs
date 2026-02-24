using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 10 — Mimic: Copies the last player skill and uses it next turn.
    /// </summary>
    public class MimicAI : EnemyAIBase
    {
        private SkillDefinition _copiedSkill;
        private bool _hasCopiedThisTurn;

        private readonly SkillDefinition _mirrorStrike = new SkillDefinition(
            "Mirror Strike", 0, ElementType.None, 1.0f, 0, 0f, SkillTargetType.Enemy);

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            // If we have a copied skill from last turn, use it
            if (_copiedSkill != null)
            {
                SkillDefinition skillToUse = _copiedSkill;
                _copiedSkill = null;

                // Adapt targeting: Ally → enemy ally, Self → self, Enemy → player
                IBattleUnit target;
                switch (skillToUse.TargetType)
                {
                    case SkillTargetType.Ally:
                        target = GetRandomLivingAlly(enemyParty) ?? (IBattleUnit)this;
                        break;
                    case SkillTargetType.Self:
                        target = this;
                        break;
                    default:
                        target = PickWeightedTarget(playerParty, skillToUse.Element);
                        break;
                }

                return MakeSkillAttack(skillToUse, target);
            }

            // Try to copy last player skill
            if (_battleController != null && _battleController.LastPlayerSkill != null)
            {
                SkillDefinition source = _battleController.LastPlayerSkill;
                // Copy the skill with 0 resource cost
                _copiedSkill = new SkillDefinition(
                    source.Name, 0, source.Element, source.DamageMultiplier,
                    source.HealAmount, source.LifestealRatio, source.TargetType,
                    source.AppliesEffect, source.EffectDuration, source.EffectValue, source.EffectChance);

                // Basic attack this turn (will use copied skill next turn)
                IBattleUnit target = PickWeightedTarget(playerParty, Element);
                return MakeBasicAttack(target);
            }

            // Fallback: Mirror Strike
            IBattleUnit fallbackTarget = PickWeightedTarget(playerParty, Element);
            return MakeSkillAttack(_mirrorStrike, fallbackTarget);
        }
    }
}
