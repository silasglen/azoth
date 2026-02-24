// Assets/Scripts/Battle/Interfaces/IBattleUnit.cs
using System;

namespace Battle
{
    /// <summary>
    /// Contract for any entity that participates in battle (player or enemy).
    /// Implementations must ensure:
    /// - TakeDamage clamps HP to >= 0
    /// - IsAlive returns false when HP reaches 0
    /// - DodgeBonus and HasShield reflect current gear state
    /// </summary>
    public interface IBattleUnit
    {
        string UnitName { get; }
        UnitType UnitType { get; }
        ElementType Element { get; }
        int CurrentHP { get; set; }
        int MaxHP { get; }
        int AttackPower { get; }
        int Defense { get; }
        bool IsAlive { get; }

        // --- Defensive gear stats ---

        /// <summary>
        /// Additive dodge bonus from gear (0.0 = no bonus, 0.10 = +10%).
        /// Combined with base 75% in BattleController. Must not be negative.
        /// </summary>
        float DodgeBonus { get; }

        /// <summary>
        /// True if this unit has a shield equipped.
        /// Upgrades Block from 99% → 100% and block damage from 1% → 0%.
        /// </summary>
        bool HasShield { get; }

        // --- Skill resource ---

        /// <summary>
        /// Current skill resource: MP for Magus, Charges for Alchemist, 0 for Knight.
        /// Consumed when using skills. Implementation manages the value.
        /// </summary>
        int CurrentResource { get; set; }

        /// <summary>
        /// Maximum skill resource. Set by class/catalyst/level.
        /// 0 means this unit type has no resource (e.g., Knight).
        /// </summary>
        int MaxResource { get; }

        /// <summary>
        /// Display label for the resource: "MP" for Magus, Catalyst name for Alchemist, "" for Knight.
        /// UI uses this to label the resource bar.
        /// </summary>
        string ResourceLabel { get; }

        /// <summary>
        /// Base critical hit chance (0.0 to 1.0). Default ~0.1 (10%).
        /// BattleController rolls against this for bonus damage.
        /// </summary>
        float CritChance { get; }

        /// <summary>
        /// Apply damage to this unit. Implementation MUST:
        /// 1. Subtract amount from CurrentHP
        /// 2. Clamp CurrentHP to >= 0
        /// 3. Set IsAlive = false when CurrentHP reaches 0
        /// </summary>
        void TakeDamage(int amount);

        /// <summary>
        /// Revive a dead unit with the given HP.
        /// Implementation MUST set IsAlive = true and clamp HP to [1, MaxHP].
        /// </summary>
        void Revive(int hp);
    }
}
