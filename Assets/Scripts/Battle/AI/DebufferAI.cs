using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 7 — Debuffer: Cycles through debuff rotation (AtkDown → DefDown → Attack x3).
    /// Prefers attacking debuffed targets.
    /// </summary>
    public class DebufferAI : EnemyAIBase
    {
        private int _turnCount;

        private readonly SkillDefinition _weakeningHex = new SkillDefinition(
            "Weakening Hex", 0, ElementType.Umbra, 0.5f, 0, 0f, SkillTargetType.Enemy,
            StatusEffectType.AtkDown, 3, 0.7f, 1.0f);

        private readonly SkillDefinition _armorRend = new SkillDefinition(
            "Armor Rend", 0, ElementType.None, 0.5f, 0, 0f, SkillTargetType.Enemy,
            StatusEffectType.DefDown, 3, 0.7f, 1.0f);

        private readonly SkillDefinition _exploitOpening = new SkillDefinition(
            "Exploit Opening", 0, ElementType.Umbra, 2.0f, 0, 0f, SkillTargetType.Enemy);

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            _turnCount++;
            if (_turnCount > 5) _turnCount = 1;

            switch (_turnCount)
            {
                case 1:
                {
                    // AtkDown on highest-ATK player
                    IBattleUnit target = FindHighestAttack(playerParty) ?? playerParty[0];
                    return MakeSkillAttack(_weakeningHex, target);
                }
                case 2:
                {
                    // DefDown on highest-DEF player
                    IBattleUnit target = FindHighestDef(playerParty) ?? playerParty[0];
                    return MakeSkillAttack(_armorRend, target);
                }
                default:
                {
                    // Turns 3-5: attack, prefer debuffed targets
                    IBattleUnit target = FindDebuffedTarget(playerParty);
                    if (target == null)
                        target = PickWeightedTarget(playerParty, _exploitOpening.Element);
                    return MakeSkillAttack(_exploitOpening, target);
                }
            }
        }

        private IBattleUnit FindDebuffedTarget(IReadOnlyList<IBattleUnit> players)
        {
            if (_battleController == null) return null;
            for (int i = 0; i < players.Count; i++)
            {
                if (!players[i].IsAlive) continue;
                var effects = _battleController.GetStatusEffects(players[i]);
                for (int j = 0; j < effects.Count; j++)
                {
                    if (effects[j].Type == StatusEffectType.AtkDown || effects[j].Type == StatusEffectType.DefDown)
                        return players[i];
                }
            }
            return null;
        }
    }
}
