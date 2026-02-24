using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 13 — Ritualist: Charges for 3 turns then unleashes AoE.
    /// Stun interrupts and resets the charge.
    /// </summary>
    public class RitualistAI : EnemyAIBase
    {
        private int _chargeCount;

        private readonly SkillDefinition _channeling = new SkillDefinition(
            "Channeling", 0, ElementType.None, 0f, 0, 0f, SkillTargetType.Self);

        private readonly SkillDefinition _cataclysm;

        protected override void Awake()
        {
            base.Awake();
            // Initialize cataclysm with AoE flag (can't set in field initializer)
            // Done via constructor workaround below
        }

        public RitualistAI()
        {
            _cataclysm = new SkillDefinition(
                "Cataclysm", 0, ElementType.Umbra, 3.0f, 0, 0f, SkillTargetType.Enemy,
                StatusEffectType.Burn, 2, 5f, 0.5f)
            {
                IsAoE = true
            };
        }

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            // Stun resets charge
            if (_battleController != null && _battleController.IsStunned(this))
            {
                _chargeCount = 0;
                return new EnemyIntent(playerParty[0], false, 0);
            }

            // Fire when fully charged
            if (_chargeCount >= 3)
            {
                _chargeCount = 0;
                IBattleUnit target = PickWeightedTarget(playerParty, _cataclysm.Element);
                return MakeSkillAttack(_cataclysm, target);
            }

            // Charging
            _chargeCount++;
            return MakeSkillAttack(_channeling, this);
        }
    }
}
