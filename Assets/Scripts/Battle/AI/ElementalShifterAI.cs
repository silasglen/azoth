using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Pattern 16 — Elemental Shifter: Adapts element to resist last player attack.
    /// Shifts every 2 turns.
    /// </summary>
    public class ElementalShifterAI : EnemyAIBase
    {
        private int _turnsSinceShift;
        private ElementType _currentElement = ElementType.None;

        private readonly SkillDefinition _elementalBlast = new SkillDefinition(
            "Elemental Blast", 0, ElementType.None, 1.8f, 0, 0f, SkillTargetType.Enemy);

        public override ElementType Element => _currentElement;

        protected override void Awake()
        {
            base.Awake();
            _currentElement = ElementType.Ignis; // Starting element
        }

        public override EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty)
        {
            _turnsSinceShift++;

            // Shift element every 2 turns
            if (_turnsSinceShift >= 2 && _battleController != null)
            {
                _turnsSinceShift = 0;
                ElementType lastPlayerElement = _battleController.LastPlayerAttackElement;
                _currentElement = GetResistantElement(lastPlayerElement);
            }

            // Set skill element to match current element
            _elementalBlast.Element = _currentElement;

            // Target weakness of our element or fallback
            IBattleUnit target = FindWeaknessTarget(playerParty, _currentElement);
            if (target == null)
                target = PickWeightedTarget(playerParty, _currentElement);

            return MakeSkillAttack(_elementalBlast, target);
        }

        /// <summary>
        /// Returns the element that resists the given attack element.
        /// Ignis→Aqua, Aqua→Terra, Terra→Ventus, Ventus→Ignis, Lux↔Umbra.
        /// </summary>
        private static ElementType GetResistantElement(ElementType attackElement)
        {
            switch (attackElement)
            {
                case ElementType.Ignis:  return ElementType.Aqua;
                case ElementType.Aqua:   return ElementType.Terra;
                case ElementType.Terra:  return ElementType.Ventus;
                case ElementType.Ventus: return ElementType.Ignis;
                case ElementType.Lux:    return ElementType.Umbra;
                case ElementType.Umbra:  return ElementType.Lux;
                default:                 return ElementType.Ignis;
            }
        }
    }
}
