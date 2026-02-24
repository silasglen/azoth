using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Barebones test combatant. Extends EnemyAIBase (IBattleUnit + IEnemyAI)
    /// so it can be used as either a player unit or enemy in the Inspector.
    /// Uses aggression-based AI for generic enemy behavior.
    /// </summary>
    public class TestBattleUnit : EnemyAIBase
    {
        [Header("=== Test AI Settings ===")]
        [SerializeField, Range(0f, 1f)]
        private float _aiAggression = 0.5f;

        // --- IEnemyAI ---
        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            var enemySkills = SkillAndItemData.GetEnemySkills(UnitType);

            // --- Heal logic: if low aggression and an ally is below 40% HP, try heal ---
            if (_aiAggression < 0.7f)
            {
                SkillDefinition healSkill = FindHealSkill(enemySkills);
                if (healSkill != null)
                {
                    IBattleUnit woundedAlly = FindWoundedAlly(enemyParty, 0.4f);
                    if (woundedAlly != null)
                    {
                        int estHeal = healSkill.HealAmount;
                        return new EnemyIntent(woundedAlly, false, estHeal, healSkill);
                    }
                }
            }

            // --- Buff logic: if has buff skill and random chance ---
            if (Random.value > _aiAggression)
            {
                SkillDefinition buffSkill = FindBuffSkill(enemySkills);
                if (buffSkill != null)
                {
                    IBattleUnit buffTarget = GetRandomLivingAlly(enemyParty);
                    if (buffTarget != null)
                    {
                        return new EnemyIntent(buffTarget, false, 0, buffSkill);
                    }
                }
            }

            // --- Offensive skill: chance based on aggression ---
            if (enemySkills.Count > 0 && Random.value < _aiAggression)
            {
                SkillDefinition offSkill = FindOffensiveSkill(enemySkills);
                if (offSkill != null)
                {
                    IBattleUnit target = PickWeightedTarget(playerParty, offSkill.Element);
                    int estDmg = Mathf.Max(1, Mathf.RoundToInt((AttackPower - target.Defense) * offSkill.DamageMultiplier));
                    return new EnemyIntent(target, false, estDmg, offSkill);
                }
            }

            // --- Basic attack fallback ---
            {
                IBattleUnit target = PickWeightedTarget(playerParty, Element);
                return MakeBasicAttack(target);
            }
        }

        // --- AI Helpers ---

        private static SkillDefinition FindHealSkill(List<SkillDefinition> skills)
        {
            foreach (var s in skills)
            {
                if (s.HealAmount > 0 && s.TargetType == SkillTargetType.Ally)
                    return s;
            }
            return null;
        }

        private static SkillDefinition FindBuffSkill(List<SkillDefinition> skills)
        {
            foreach (var s in skills)
            {
                if (s.TargetType == SkillTargetType.Ally && s.HealAmount == 0 &&
                    s.AppliesEffect != StatusEffectType.None &&
                    (s.AppliesEffect == StatusEffectType.AtkUp || s.AppliesEffect == StatusEffectType.DefUp))
                    return s;
            }
            return null;
        }

        private static SkillDefinition FindOffensiveSkill(List<SkillDefinition> skills)
        {
            List<SkillDefinition> offensive = new List<SkillDefinition>();
            foreach (var s in skills)
            {
                if (s.DamageMultiplier > 0f && s.TargetType == SkillTargetType.Enemy)
                    offensive.Add(s);
            }
            if (offensive.Count == 0) return null;
            return offensive[Random.Range(0, offensive.Count)];
        }
    }
}
