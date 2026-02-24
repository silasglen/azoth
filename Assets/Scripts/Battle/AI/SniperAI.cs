using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 4 — Sniper: Always targets the player with lowest defense.
    /// 70% chance to use Piercing Shot, else basic attack.
    /// </summary>
    public class SniperAI : EnemyAIBase
    {
        private readonly SkillDefinition _piercingShot = new SkillDefinition(
            "Piercing Shot", 0, ElementType.Ventus, 2.5f, 0, 0f, SkillTargetType.Enemy);

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            IBattleUnit target = FindLowestDef(playerParty) ?? playerParty[0];

            if (Random.value < 0.7f)
            {
                return MakeSkillAttack(_piercingShot, target);
            }

            return MakeBasicAttack(target);
        }
    }
}
