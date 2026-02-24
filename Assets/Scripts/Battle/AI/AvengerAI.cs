using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 12 — Avenger: Attack power stacks when allies die.
    /// Uses stronger skills at higher stacks.
    /// </summary>
    public class AvengerAI : EnemyAIBase
    {
        private int _deathStacks;

        private readonly SkillDefinition _vengeanceStrike = new SkillDefinition(
            "Vengeance Strike", 0, ElementType.None, 1.5f, 0, 0f, SkillTargetType.Enemy);

        private readonly SkillDefinition _wrathUnleashed = new SkillDefinition(
            "Wrath Unleashed", 0, ElementType.None, 2.5f, 0, 0f, SkillTargetType.Enemy);

        public override int AttackPower => _attackPower + (_deathStacks * 5);

        private void OnEnable()
        {
            if (_battleController != null)
                _battleController.OnUnitDied += HandleAllyDeath;
        }

        private void OnDisable()
        {
            if (_battleController != null)
                _battleController.OnUnitDied -= HandleAllyDeath;
        }

        private void HandleAllyDeath(IBattleUnit dead, IBattleUnit killer)
        {
            if (dead == (IBattleUnit)this) return;

            // Check if the dead unit is in our enemy party (our ally)
            if (_battleController != null)
            {
                var enemies = _battleController.EnemyUnits;
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i] == dead)
                    {
                        _deathStacks++;
                        return;
                    }
                }
            }
        }

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            IBattleUnit target = PickWeightedTarget(playerParty, Element);

            // Use Wrath Unleashed at 2+ stacks, Vengeance Strike at 1+ stack, else basic attack
            if (_deathStacks >= 2)
                return MakeSkillAttack(_wrathUnleashed, target);

            if (_deathStacks >= 1)
                return MakeSkillAttack(_vengeanceStrike, target);

            return MakeBasicAttack(target);
        }
    }
}
