using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 17 — Commander: Applies passive aura buffs to all allies at battle start.
    /// Buffs are removed when Commander dies. Alternates between rally, focus fire, and attack.
    /// </summary>
    public class CommanderAI : EnemyAIBase
    {
        private int _turnCount;

        private readonly SkillDefinition _rallyCommand = new SkillDefinition(
            "Rally Command", 0, ElementType.None, 0f, 0, 0f, SkillTargetType.Ally,
            StatusEffectType.AtkUp, 3, 1.25f, 1.0f);

        private readonly SkillDefinition _focusFire = new SkillDefinition(
            "Focus Fire", 0, ElementType.None, 0.5f, 0, 0f, SkillTargetType.Enemy,
            StatusEffectType.DefDown, 2, 0.7f, 1.0f);

        private readonly SkillDefinition _commandersStrike = new SkillDefinition(
            "Commander's Strike", 0, ElementType.None, 1.5f, 0, 0f, SkillTargetType.Enemy);

        private void OnEnable()
        {
            if (_battleController != null)
            {
                _battleController.OnBattleStart += ApplyAuraBuff;
                _battleController.OnUnitDied += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_battleController != null)
            {
                _battleController.OnBattleStart -= ApplyAuraBuff;
                _battleController.OnUnitDied -= HandleDeath;
            }
        }

        private void ApplyAuraBuff()
        {
            if (_battleController == null) return;

            var enemies = _battleController.EnemyUnits;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsAlive) continue;
                _battleController.ApplyStatusEffect(enemies[i], StatusEffectType.AtkUp, 99, 1.15f);
                _battleController.ApplyStatusEffect(enemies[i], StatusEffectType.DefUp, 99, 1.15f);
            }
        }

        private void HandleDeath(IBattleUnit dead, IBattleUnit killer)
        {
            if (dead != (IBattleUnit)this) return;
            if (_battleController == null) return;

            // Remove aura buffs from all allies on Commander death
            var enemies = _battleController.EnemyUnits;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsAlive) continue;
                _battleController.CureStatusEffect(enemies[i], StatusEffectType.AtkUp);
                _battleController.CureStatusEffect(enemies[i], StatusEffectType.DefUp);
            }
        }

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            _turnCount++;
            if (_turnCount > 3) _turnCount = 1;

            switch (_turnCount)
            {
                case 1:
                {
                    // Rally Command: AtkUp on a living ally
                    IBattleUnit ally = GetRandomLivingAlly(enemyParty);
                    if (ally != null)
                        return MakeSkillAttack(_rallyCommand, ally);
                    goto default;
                }
                case 2:
                {
                    // Focus Fire: DefDown on a player
                    IBattleUnit target = FindHighestAttack(playerParty) ?? playerParty[0];
                    return MakeSkillAttack(_focusFire, target);
                }
                default:
                {
                    // Commander's Strike
                    IBattleUnit target = PickWeightedTarget(playerParty, Element);
                    return MakeSkillAttack(_commandersStrike, target);
                }
            }
        }
    }
}
