using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Enum mapping to the 18 enemy AI MonoBehaviour types.
    /// Used in Inspector-serializable encounter configurations.
    /// </summary>
    public enum EnemyAIPattern
    {
        Basic,
        Berserker,
        Tactician,
        Bodyguard,
        Sniper,
        HealerPriest,
        GlassCannon,
        Debuffer,
        Vampire,
        Martyr,
        Mimic,
        Coward,
        Avenger,
        Ritualist,
        SwarmDrone,
        Saboteur,
        ElementalShifter,
        Commander
    }

    /// <summary>
    /// Inspector-serializable definition for a single enemy in an encounter.
    /// </summary>
    [Serializable]
    public class EnemySpawnData
    {
        public string Name = "Enemy";
        public EnemyAIPattern AIPattern = EnemyAIPattern.Basic;
        public ElementType Element = ElementType.None;
        public int HP = 100;
        public int ATK = 20;
        public int DEF = 5;
    }

    /// <summary>
    /// Defines a complete battle encounter: a named list of enemies to spawn.
    /// Used to pass data from overworld NPCs to the battle scene via the static Pending field.
    /// </summary>
    [Serializable]
    public class EncounterData
    {
        public string EncounterName = "Battle";
        public List<EnemySpawnData> Enemies = new List<EnemySpawnData>();

        /// <summary>
        /// Set this before loading the battle scene additively.
        /// BattleSceneBootstrap reads and consumes it on Start().
        /// </summary>
        public static EncounterData Pending { get; set; }

        /// <summary>
        /// Fired when an encounter battle ends. Args: (playerWon).
        /// The overworld NPC subscribes to this to handle post-battle cleanup.
        /// </summary>
        public static event Action<bool> OnEncounterComplete;

        public static void NotifyComplete(bool playerWon)
        {
            OnEncounterComplete?.Invoke(playerWon);
        }

        /// <summary>
        /// Maps an EnemyAIPattern enum value to the corresponding MonoBehaviour Type.
        /// </summary>
        public static Type GetAIType(EnemyAIPattern pattern)
        {
            switch (pattern)
            {
                case EnemyAIPattern.Berserker:        return typeof(BerserkerAI);
                case EnemyAIPattern.Tactician:        return typeof(TacticianAI);
                case EnemyAIPattern.Bodyguard:        return typeof(BodyguardAI);
                case EnemyAIPattern.Sniper:           return typeof(SniperAI);
                case EnemyAIPattern.HealerPriest:     return typeof(HealerPriestAI);
                case EnemyAIPattern.GlassCannon:      return typeof(GlassCannonAI);
                case EnemyAIPattern.Debuffer:         return typeof(DebufferAI);
                case EnemyAIPattern.Vampire:          return typeof(VampireAI);
                case EnemyAIPattern.Martyr:           return typeof(MartyrAI);
                case EnemyAIPattern.Mimic:            return typeof(MimicAI);
                case EnemyAIPattern.Coward:           return typeof(CowardAI);
                case EnemyAIPattern.Avenger:          return typeof(AvengerAI);
                case EnemyAIPattern.Ritualist:        return typeof(RitualistAI);
                case EnemyAIPattern.SwarmDrone:       return typeof(SwarmDroneAI);
                case EnemyAIPattern.Saboteur:         return typeof(SaboteurAI);
                case EnemyAIPattern.ElementalShifter: return typeof(ElementalShifterAI);
                case EnemyAIPattern.Commander:        return typeof(CommanderAI);
                default:                              return typeof(TestBattleUnit);
            }
        }
    }
}
