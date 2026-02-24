// Assets/Scripts/Battle/Interfaces/IEnemyAI.cs
using System.Collections.Generic;

namespace Battle
{
    /// <summary>
    /// Contract for enemy decision-making AI.
    /// Implemented per-enemy-type (e.g., MagusGruntAI, MagusBossAI).
    /// BattleController calls DecideAction once per living enemy each turn
    /// during the EnemyIntent phase.
    /// </summary>
    public interface IEnemyAI
    {
        /// <summary>
        /// Called during EnemyIntent phase. The enemy picks a target
        /// from the living player party members and declares whether
        /// its upcoming attack is unblockable.
        /// </summary>
        /// <param name="playerParty">Only living members. Guaranteed non-empty.</param>
        /// <param name="enemyParty">All enemy units (for heal/buff targeting). Guaranteed non-empty.</param>
        /// <returns>An EnemyIntent struct. Target MUST be from one of the provided lists.</returns>
        EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty);
    }

    /// <summary>
    /// Immutable data container for one enemy's declared intent for this turn.
    /// Created during EnemyIntent phase, consumed during EnemyTurn phase.
    /// Skill is null for basic attacks.
    /// </summary>
    public readonly struct EnemyIntent
    {
        public readonly IBattleUnit Target;
        public readonly bool IsUnblockable;
        public readonly int EstimatedDamage;
        public readonly SkillDefinition Skill;

        public EnemyIntent(IBattleUnit target, bool isUnblockable, int estimatedDamage, SkillDefinition skill = null)
        {
            Target = target;
            IsUnblockable = isUnblockable;
            EstimatedDamage = estimatedDamage;
            Skill = skill;
        }
    }
}
