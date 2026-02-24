using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 9 — Martyr: On death, heals all living allies 30% MaxHP and applies AtkUp.
    /// While alive, heals weakest ally if below 50%, else basic attacks.
    /// </summary>
    public class MartyrAI : EnemyAIBase
    {
        private readonly SkillDefinition _sacrificesBlessing = new SkillDefinition(
            "Sacrifice's Blessing", 0, ElementType.None, 0f, 20, 0f, SkillTargetType.Ally);

        private void OnEnable()
        {
            if (_battleController != null)
                _battleController.OnUnitDied += HandleDeath;
        }

        private void OnDisable()
        {
            if (_battleController != null)
                _battleController.OnUnitDied -= HandleDeath;
        }

        private void HandleDeath(IBattleUnit dead, IBattleUnit killer)
        {
            if (dead != (IBattleUnit)this) return;
            if (_battleController == null) return;

            // On death: heal and buff all living allies
            var enemies = _battleController.EnemyUnits;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsAlive) continue;
                if (enemies[i] == (IBattleUnit)this) continue;

                // Heal 30% MaxHP
                int healAmount = Mathf.RoundToInt(enemies[i].MaxHP * 0.3f);
                enemies[i].CurrentHP = Mathf.Min(enemies[i].CurrentHP + healAmount, enemies[i].MaxHP);

                // Apply AtkUp
                _battleController.ApplyStatusEffect(enemies[i], StatusEffectType.AtkUp, 99, 1.3f);
            }
        }

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            // Heal weakest ally if any below 50%
            IBattleUnit wounded = FindWoundedAlly(enemyParty, 0.5f);
            if (wounded != null)
            {
                return MakeSkillAttack(_sacrificesBlessing, wounded);
            }

            // Basic attack
            IBattleUnit target = PickWeightedTarget(playerParty, Element);
            return MakeBasicAttack(target);
        }
    }
}
