using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 2 — Tactician: Opens with a debuff, then exploits elemental weakness.
    /// </summary>
    public class TacticianAI : EnemyAIBase
    {
        private bool _hasDebuffed;

        private readonly SkillDefinition _suppressingStrike = new SkillDefinition(
            "Suppressing Strike", 0, ElementType.None, 1.0f, 0, 0f, SkillTargetType.Enemy,
            StatusEffectType.AtkDown, 2, 0.75f, 1.0f);

        private readonly SkillDefinition _exploitWeakness = new SkillDefinition(
            "Exploit Weakness", 0, ElementType.None, 1.8f, 0, 0f, SkillTargetType.Enemy);

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            // Turn 1: AtkDown on highest attack player
            if (!_hasDebuffed)
            {
                _hasDebuffed = true;
                IBattleUnit target = FindHighestAttack(playerParty) ?? playerParty[0];
                return MakeSkillAttack(_suppressingStrike, target);
            }

            // After: target elemental weakness or fallback weighted
            _exploitWeakness.Element = Element;
            IBattleUnit weakTarget = FindWeaknessTarget(playerParty, Element);
            if (weakTarget != null)
            {
                return MakeSkillAttack(_exploitWeakness, weakTarget);
            }

            IBattleUnit fallback = PickWeightedTarget(playerParty, Element);
            return MakeSkillAttack(_exploitWeakness, fallback);
        }
    }
}
