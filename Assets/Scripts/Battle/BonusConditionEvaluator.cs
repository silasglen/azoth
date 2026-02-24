using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Tracks bonus condition state during battle and evaluates results on demand.
    /// Subscribes to BattleController events independently.
    /// </summary>
    public class BonusConditionEvaluator : MonoBehaviour
    {
        [SerializeField] private BattleController _battleController;
        [SerializeField] private BattleStatisticsTracker _statsTracker;

        [Header("=== Condition Thresholds ===")]
        [SerializeField] private int _speedClearTurnLimit = 5;
        [SerializeField] private int _elementVarietyMin = 3;
        [SerializeField] private int _weaknessExploitMin = 3;
        [SerializeField] private int _perfectBlockMin = 2;
        [SerializeField] private int _critStreakMin = 3;

        [Header("=== Bonus XP per Condition ===")]
        [SerializeField] private int _noDeathXP = 50;
        [SerializeField] private int _speedClearXP = 75;
        [SerializeField] private int _elementVarietyXP = 40;
        [SerializeField] private int _weaknessExploitXP = 60;
        [SerializeField] private int _noDamageTakenXP = 100;
        [SerializeField] private int _noItemsUsedXP = 30;
        [SerializeField] private int _allEnemiesScannedXP = 50;
        [SerializeField] private int _perfectBlockXP = 40;
        [SerializeField] private int _critStreakXP = 50;
        [SerializeField] private int _fullHPXP = 60;

        // Tracked state
        private HashSet<ElementType> _elementsUsed = new HashSet<ElementType>();
        private int _enemyCount;
        private HashSet<string> _scannedEnemyNames = new HashSet<string>();

        // Active conditions (configurable — defaults set on reset)
        private List<BonusConditionType> _activeConditions = new List<BonusConditionType>();

        private void OnEnable()
        {
            if (_battleController == null) return;

            _battleController.OnBattleStart += HandleBattleStart;
            _battleController.OnPlayerActionSelected += HandlePlayerAction;
            _battleController.OnScanCompleted += HandleScanCompleted;
            _battleController.OnAttackExecuted += HandleAttackExecuted;
        }

        private void OnDisable()
        {
            if (_battleController == null) return;

            _battleController.OnBattleStart -= HandleBattleStart;
            _battleController.OnPlayerActionSelected -= HandlePlayerAction;
            _battleController.OnScanCompleted -= HandleScanCompleted;
            _battleController.OnAttackExecuted -= HandleAttackExecuted;
        }

        /// <summary>
        /// Configure which bonus conditions are active. Defaults: NoDeath, SpeedClear, ElementVariety.
        /// </summary>
        public void SetConditions(List<BonusConditionType> conditions)
        {
            _activeConditions = conditions ?? new List<BonusConditionType>();
        }

        private void HandleBattleStart()
        {
            _elementsUsed.Clear();
            _scannedEnemyNames.Clear();
            _enemyCount = 0;

            for (int i = 0; i < _battleController.EnemyUnits.Count; i++)
            {
                if (_battleController.EnemyUnits[i].IsAlive)
                    _enemyCount++;
            }

            // Set default conditions if none configured
            if (_activeConditions.Count == 0)
            {
                _activeConditions = new List<BonusConditionType>
                {
                    BonusConditionType.NoDeath,
                    BonusConditionType.SpeedClear,
                    BonusConditionType.ElementVariety,
                    BonusConditionType.WeaknessExploit,
                    BonusConditionType.NoDamageTaken,
                    BonusConditionType.NoItemsUsed,
                    BonusConditionType.AllEnemiesScanned,
                    BonusConditionType.PerfectBlock,
                    BonusConditionType.CriticalStreak,
                    BonusConditionType.FullHP,
                };
            }
        }

        private void HandlePlayerAction(PlayerAction action, IBattleUnit target)
        {
            if (action == PlayerAction.Skill)
            {
                IBattleUnit active = _battleController.ActiveUnit;
                if (active != null)
                {
                    var skills = SkillAndItemData.GetSkills(active.UnitType);
                    // Track element from the active unit's element
                    if (active.Element != ElementType.None)
                        _elementsUsed.Add(active.Element);
                }
            }
        }

        private void HandleScanCompleted(IBattleUnit scanner, IBattleUnit target)
        {
            if (target != null)
                _scannedEnemyNames.Add(target.UnitName);
        }

        private void HandleAttackExecuted(IBattleUnit attacker, IBattleUnit target, int damage)
        {
            // Track attacker element for element variety
            if (attacker != null && attacker.Element != ElementType.None)
            {
                // Only track player attacks
                for (int i = 0; i < _battleController.PlayerParty.Count; i++)
                {
                    if (_battleController.PlayerParty[i] == attacker)
                    {
                        _elementsUsed.Add(attacker.Element);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates all active conditions against current tracked state and stats.
        /// </summary>
        public List<BonusConditionResult> EvaluateAll()
        {
            var results = new List<BonusConditionResult>();
            if (_statsTracker == null) return results;

            foreach (var condition in _activeConditions)
            {
                results.Add(Evaluate(condition));
            }

            return results;
        }

        private BonusConditionResult Evaluate(BonusConditionType type)
        {
            switch (type)
            {
                case BonusConditionType.NoDeath:
                {
                    bool met = _statsTracker.PlayerDeaths == 0;
                    return new BonusConditionResult(type, "No party members died", met, met ? _noDeathXP : 0);
                }
                case BonusConditionType.SpeedClear:
                {
                    bool met = _statsTracker.TurnsElapsed <= _speedClearTurnLimit;
                    return new BonusConditionResult(type, $"Clear in {_speedClearTurnLimit} turns or less", met, met ? _speedClearXP : 0);
                }
                case BonusConditionType.ElementVariety:
                {
                    bool met = _elementsUsed.Count >= _elementVarietyMin;
                    return new BonusConditionResult(type, $"Use {_elementVarietyMin}+ different elements", met, met ? _elementVarietyXP : 0);
                }
                case BonusConditionType.WeaknessExploit:
                {
                    bool met = _statsTracker.WeaknessExploits >= _weaknessExploitMin;
                    return new BonusConditionResult(type, $"Exploit {_weaknessExploitMin}+ weaknesses", met, met ? _weaknessExploitXP : 0);
                }
                case BonusConditionType.NoDamageTaken:
                {
                    bool met = _statsTracker.TotalDamageTaken == 0;
                    return new BonusConditionResult(type, "Take no damage", met, met ? _noDamageTakenXP : 0);
                }
                case BonusConditionType.NoItemsUsed:
                {
                    bool met = _statsTracker.ItemsUsed == 0;
                    return new BonusConditionResult(type, "Use no items", met, met ? _noItemsUsedXP : 0);
                }
                case BonusConditionType.AllEnemiesScanned:
                {
                    bool met = _scannedEnemyNames.Count >= _enemyCount && _enemyCount > 0;
                    return new BonusConditionResult(type, "Scan all enemies", met, met ? _allEnemiesScannedXP : 0);
                }
                case BonusConditionType.PerfectBlock:
                {
                    bool met = _statsTracker.BlocksSucceeded >= _perfectBlockMin;
                    return new BonusConditionResult(type, $"Block {_perfectBlockMin}+ attacks", met, met ? _perfectBlockXP : 0);
                }
                case BonusConditionType.CriticalStreak:
                {
                    bool met = _statsTracker.CriticalHitsLanded >= _critStreakMin;
                    return new BonusConditionResult(type, $"Land {_critStreakMin}+ critical hits", met, met ? _critStreakXP : 0);
                }
                case BonusConditionType.FullHP:
                {
                    bool met = true;
                    for (int i = 0; i < _battleController.PlayerParty.Count; i++)
                    {
                        var unit = _battleController.PlayerParty[i];
                        if (unit.IsAlive && unit.CurrentHP < unit.MaxHP)
                        {
                            met = false;
                            break;
                        }
                    }
                    return new BonusConditionResult(type, "All allies at full HP", met, met ? _fullHPXP : 0);
                }
                default:
                    return new BonusConditionResult(type, "Unknown", false, 0);
            }
        }
    }
}
