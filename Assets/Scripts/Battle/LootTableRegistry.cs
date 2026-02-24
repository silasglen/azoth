using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Static registry for per-enemy loot tables. Follows BestiaryData/SkillAndItemData pattern.
    /// </summary>
    public static class LootTableRegistry
    {
        private static readonly Dictionary<string, EnemyLootTable> _tables =
            new Dictionary<string, EnemyLootTable>();

        private static bool _defaultsRegistered;

        public static void RegisterTable(EnemyLootTable table)
        {
            if (table == null || string.IsNullOrEmpty(table.EnemyName)) return;
            _tables[table.EnemyName] = table;
        }

        public static EnemyLootTable GetTable(string enemyName)
        {
            return _tables.TryGetValue(enemyName, out var table) ? table : null;
        }

        public static bool HasTable(string enemyName)
        {
            return _tables.ContainsKey(enemyName);
        }

        /// <summary>
        /// Rolls loot for a given enemy loot table, applying grade and streak modifiers.
        /// </summary>
        public static List<LootDrop> RollLoot(EnemyLootTable table, BattleGrade grade, int streak)
        {
            var drops = new List<LootDrop>();
            if (table == null || table.Entries == null) return drops;

            float gradeDropMod = PerformanceMetrics.GetGradeDropRateMultiplier(grade);
            float streakDropMod = PerformanceMetrics.GetStreakDropRateMultiplier(streak);

            foreach (var entry in table.Entries)
            {
                float adjustedRate = entry.BaseDropRate * gradeDropMod * streakDropMod;
                float roll = Random.value;

                if (roll < adjustedRate)
                {
                    int quantity = Random.Range(entry.MinQuantity, entry.MaxQuantity + 1);
                    drops.Add(new LootDrop(entry.ItemDataRef, quantity, entry.Rarity));
                }
            }

            return drops;
        }

        /// <summary>
        /// Registers placeholder test entries for TestBattleUnit enemies.
        /// Uses null ItemData refs for testing purposes.
        /// </summary>
        public static void RegisterDefaultTables()
        {
            if (_defaultsRegistered) return;
            _defaultsRegistered = true;

            RegisterTable(new EnemyLootTable("Magus Grunt", 30, 15, new List<LootTableEntry>
            {
                new LootTableEntry(null, 0.5f, ItemRarity.Common, 1, 2),
                new LootTableEntry(null, 0.2f, ItemRarity.Uncommon, 1, 1),
            }));

            RegisterTable(new EnemyLootTable("Shadow Knight", 45, 25, new List<LootTableEntry>
            {
                new LootTableEntry(null, 0.6f, ItemRarity.Common, 1, 3),
                new LootTableEntry(null, 0.3f, ItemRarity.Uncommon, 1, 1),
                new LootTableEntry(null, 0.1f, ItemRarity.Rare, 1, 1),
            }));

            RegisterTable(new EnemyLootTable("Dark Alchemist", 40, 20, new List<LootTableEntry>
            {
                new LootTableEntry(null, 0.5f, ItemRarity.Common, 1, 2),
                new LootTableEntry(null, 0.25f, ItemRarity.Uncommon, 1, 1),
                new LootTableEntry(null, 0.05f, ItemRarity.Rare, 1, 1),
            }));
        }

        public static void ClearAll()
        {
            _tables.Clear();
            _defaultsRegistered = false;
        }
    }
}
