using System.Collections.Generic;

namespace Battle
{
    /// <summary>
    /// Static bestiary that persists scanned enemy data across battles.
    /// Save/load to disk deferred to a future persistence system.
    /// </summary>
    public static class BestiaryData
    {
        public class EnemyScanRecord
        {
            public string UnitName;
            public int MaxHP;
            public int AttackPower;
            public int Defense;
            public ElementType Element;
            public float CritChance;
        }

        private static readonly Dictionary<string, EnemyScanRecord> _scannedEnemies =
            new Dictionary<string, EnemyScanRecord>();

        public static bool IsScanned(string unitName)
        {
            return _scannedEnemies.ContainsKey(unitName);
        }

        public static void RecordScan(IBattleUnit unit)
        {
            _scannedEnemies[unit.UnitName] = new EnemyScanRecord
            {
                UnitName = unit.UnitName,
                MaxHP = unit.MaxHP,
                AttackPower = unit.AttackPower,
                Defense = unit.Defense,
                Element = unit.Element,
                CritChance = unit.CritChance
            };
        }

        public static EnemyScanRecord GetRecord(string unitName)
        {
            return _scannedEnemies.TryGetValue(unitName, out var record) ? record : null;
        }

        public static void ClearAll()
        {
            _scannedEnemies.Clear();
        }
    }
}
