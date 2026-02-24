// Assets/Scripts/Battle/BattleEnums.cs
namespace Battle
{
    /// <summary>
    /// Top-level state machine states for the battle.
    /// Transitions are managed exclusively by BattleController.
    /// External code can read the current state but must never set it.
    /// </summary>
    public enum BattleState
    {
        None,
        BattleStart,
        PlayerTurn,
        EnemyIntent,
        EnemyTurn,
        TurnEnd,
        BattleWon,
        BattleLost
    }

    /// <summary>
    /// The six player actions available during PlayerTurn phase.
    /// </summary>
    public enum PlayerAction
    {
        None,
        Attack,
        Skill,
        Item,
        Block,
        Dodge,
        Scan,
        Flee
    }

    /// <summary>
    /// Tracks what defensive stance a unit has committed to this turn.
    /// Set during PlayerTurn, active during EnemyTurn, cleared after EnemyTurn.
    /// </summary>
    public enum DefensiveStance
    {
        None,
        Blocking,
        Dodging
    }

    /// <summary>
    /// Unit archetypes. Determines which element pool a unit draws from.
    /// </summary>
    public enum UnitType
    {
        Alchemist,  // Classical elements (Ignis, Ventus, Terra, Aqua)
        Magus,      // Arcane elements (Lux, Umbra)
        Knight      // Melee / element-neutral (mercenaries of the Magus faction)
    }

    /// <summary>
    /// Types of status effects that can be applied to units.
    /// DoT: Poison, Burn (tick damage at TurnEnd)
    /// CC: Stun (skip turn)
    /// Buffs/Debuffs: AtkUp, AtkDown, DefUp, DefDown (modify stats)
    /// </summary>
    public enum StatusEffectType
    {
        None,
        Poison,
        Burn,
        Stun,
        AtkUp,
        AtkDown,
        DefUp,
        DefDown
    }

    /// <summary>
    /// Mutable status effect instance tracked per-unit by BattleController.
    /// </summary>
    public class StatusEffect
    {
        public StatusEffectType Type;
        public int RemainingTurns;
        public float Value; // damage per tick for DoT, multiplier for buffs (e.g. 1.25 = +25%, 0.75 = -25%)

        public StatusEffect(StatusEffectType type, int duration, float value)
        {
            Type = type;
            RemainingTurns = duration;
            Value = value;
        }
    }

    /// <summary>
    /// Skill element types.
    /// Classical cycle: Ignis > Ventus > Terra > Aqua > Ignis
    /// Arcane duality: Lux ↔ Umbra (mutually super-effective)
    /// None: element-neutral melee attacks
    /// </summary>
    public enum ElementType
    {
        None,   // Melee / element-neutral
        // --- Classical (Alchemy) ---
        Ignis,  // Fire  — strong vs Ventus, weak vs Aqua
        Ventus, // Wind  — strong vs Terra, weak vs Ignis
        Terra,  // Earth — strong vs Aqua,  weak vs Ventus
        Aqua,   // Water — strong vs Ignis, weak vs Terra
        // --- Arcane (Magus) ---
        Lux,    // Light — mutual with Umbra
        Umbra   // Dark  — mutual with Lux
    }
}
