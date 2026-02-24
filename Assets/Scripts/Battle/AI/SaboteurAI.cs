using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 15 — Saboteur: Cycles through resource burn, item destroy, and damage.
    /// </summary>
    public class SaboteurAI : EnemyAIBase
    {
        private int _turnCount;

        private readonly SkillDefinition _manaBurn;
        private readonly SkillDefinition _corrosiveTouch;

        private readonly SkillDefinition _sabotageStrike = new SkillDefinition(
            "Sabotage Strike", 0, ElementType.None, 1.5f, 0, 0f, SkillTargetType.Enemy);

        public SaboteurAI()
        {
            _manaBurn = new SkillDefinition(
                "Mana Burn", 0, ElementType.Umbra, 0.5f, 0, 0f, SkillTargetType.Enemy)
            {
                ResourceBurnAmount = 5
            };

            _corrosiveTouch = new SkillDefinition(
                "Corrosive Touch", 0, ElementType.None, 0.5f, 0, 0f, SkillTargetType.Enemy)
            {
                DestroysItem = true
            };
        }

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            _turnCount++;
            if (_turnCount > 3) _turnCount = 1;

            switch (_turnCount)
            {
                case 1:
                {
                    // Mana Burn on highest-resource player
                    IBattleUnit target = FindHighestResource(playerParty) ?? playerParty[0];
                    return MakeSkillAttack(_manaBurn, target);
                }
                case 2:
                {
                    // Corrosive Touch on random player
                    IBattleUnit target = playerParty[Random.Range(0, playerParty.Count)];
                    return MakeSkillAttack(_corrosiveTouch, target);
                }
                default:
                {
                    // Sabotage Strike on weakest player
                    IBattleUnit target = FindLowestHPPercent(playerParty) ?? playerParty[0];
                    return MakeSkillAttack(_sabotageStrike, target);
                }
            }
        }
    }
}
