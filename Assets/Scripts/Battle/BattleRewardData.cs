using System.Collections.Generic;

namespace Battle
{
    public enum BattleGrade
    {
        S,
        A,
        B,
        C,
        D
    }

    public class BattleRewards
    {
        public BattleGrade Grade;
        public int BaseXP;
        public int BonusXP;
        public float GradeMultiplier;
        public float StreakMultiplier;
        public float TotalMultiplier;
        public int FinalXP;
        public int GoldEarned;
        public List<LootDrop> Loot;
        public List<BonusConditionResult> BonusConditions;
        public int WinStreak;
    }

    public class LootDrop
    {
        public ItemData ItemDataRef;
        public int Quantity;
        public ItemRarity Rarity;

        public LootDrop(ItemData itemData, int quantity, ItemRarity rarity)
        {
            ItemDataRef = itemData;
            Quantity = quantity;
            Rarity = rarity;
        }
    }

    public class BonusConditionResult
    {
        public BonusConditionType Type;
        public string Description;
        public bool Completed;
        public int BonusXP;

        public BonusConditionResult(BonusConditionType type, string description, bool completed, int bonusXP)
        {
            Type = type;
            Description = description;
            Completed = completed;
            BonusXP = bonusXP;
        }
    }

    public enum BonusConditionType
    {
        NoDeath,
        SpeedClear,
        ElementVariety,
        WeaknessExploit,
        NoDamageTaken,
        NoItemsUsed,
        AllEnemiesScanned,
        PerfectBlock,
        CriticalStreak,
        FullHP
    }

    public class LootTableEntry
    {
        public ItemData ItemDataRef;
        public float BaseDropRate;
        public ItemRarity Rarity;
        public int MinQuantity;
        public int MaxQuantity;

        public LootTableEntry(ItemData itemData, float baseDropRate, ItemRarity rarity, int minQty, int maxQty)
        {
            ItemDataRef = itemData;
            BaseDropRate = baseDropRate;
            Rarity = rarity;
            MinQuantity = minQty;
            MaxQuantity = maxQty;
        }
    }

    public class EnemyLootTable
    {
        public string EnemyName;
        public int BaseXP;
        public int BaseGold;
        public List<LootTableEntry> Entries;

        public EnemyLootTable(string enemyName, int baseXP, int baseGold, List<LootTableEntry> entries)
        {
            EnemyName = enemyName;
            BaseXP = baseXP;
            BaseGold = baseGold;
            Entries = entries ?? new List<LootTableEntry>();
        }
    }

    public class RetreatResult
    {
        public int GoldLost;
        public int PartialXP;
        public bool StreakLost;

        public RetreatResult(int goldLost, int partialXP, bool streakLost)
        {
            GoldLost = goldLost;
            PartialXP = partialXP;
            StreakLost = streakLost;
        }
    }
}
