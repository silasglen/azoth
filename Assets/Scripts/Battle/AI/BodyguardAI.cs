using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 3 — Bodyguard: Redirects player attacks targeting low-HP allies to self.
    /// Uses self-buff and basic attacks.
    /// </summary>
    public class BodyguardAI : EnemyAIBase
    {
        private readonly SkillDefinition _guardianStrike = new SkillDefinition(
            "Guardian Strike", 0, ElementType.None, 1.2f, 0, 0f, SkillTargetType.Enemy);

        private readonly SkillDefinition _fortify = new SkillDefinition(
            "Fortify", 0, ElementType.None, 0f, 0, 0f, SkillTargetType.Self,
            StatusEffectType.DefUp, 2, 1.5f, 1.0f);

        private void OnEnable()
        {
            if (_battleController != null)
                _battleController.AttackRedirectHandler = RedirectToSelf;
        }

        private void OnDisable()
        {
            if (_battleController != null && _battleController.AttackRedirectHandler == RedirectToSelf)
                _battleController.AttackRedirectHandler = null;
        }

        private IBattleUnit RedirectToSelf(IBattleUnit attacker, IBattleUnit target)
        {
            // Only redirect if target is an ally (enemy unit) with low HP and self is alive
            if (!IsAlive) return target;
            if (target == (IBattleUnit)this) return target;

            // Check if target is an enemy unit (our ally)
            if (_battleController != null)
            {
                bool targetIsAlly = false;
                var enemies = _battleController.EnemyUnits;
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i] == target)
                    {
                        targetIsAlly = true;
                        break;
                    }
                }

                if (targetIsAlly)
                {
                    float targetHPPct = (float)target.CurrentHP / target.MaxHP;
                    if (targetHPPct < 0.4f)
                        return this;
                }
            }

            return target;
        }

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            // 30% chance to Fortify self, else attack
            if (Random.value < 0.3f)
            {
                return MakeSkillAttack(_fortify, this);
            }

            IBattleUnit target = PickWeightedTarget(playerParty, Element);
            return MakeSkillAttack(_guardianStrike, target);
        }
    }
}
