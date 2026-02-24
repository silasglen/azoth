using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Abstract base class for all enemy AI units.
    /// Implements IBattleUnit (stats/HP) and IEnemyAI (decision-making).
    /// Subclasses only need to implement DecideAction and define their own skills.
    /// </summary>
    public abstract class EnemyAIBase : MonoBehaviour, IBattleUnit, IEnemyAI
    {
        [Header("=== Identity ===")]
        [SerializeField] private string _unitName = "Enemy";
        [SerializeField] private UnitType _unitType = UnitType.Knight;
        [SerializeField] private ElementType _element = ElementType.None;

        [Header("=== Stats ===")]
        [SerializeField] protected int _maxHP = 100;
        [SerializeField] protected int _attackPower = 20;
        [SerializeField] protected int _defense = 5;
        [SerializeField, Range(0f, 1f)] protected float _critChance = 0.1f;

        [Header("=== Skill Resource ===")]
        [SerializeField] private int _maxResource = 0;
        [SerializeField] private string _catalystName = "";

        [Header("=== Gear ===")]
        [SerializeField] private float _dodgeBonus = 0f;
        [SerializeField] private bool _hasShield = false;

        [Header("=== Enemy AI Settings ===")]
        [SerializeField, Range(0f, 1f)]
        protected float _unblockableChance = 0.2f;

        [Header("=== Battle Controller Reference ===")]
        [SerializeField] protected BattleController _battleController;

        protected int _currentHP;
        protected int _currentResource;
        protected bool _isAlive = true;

        // --- IBattleUnit ---
        public string UnitName => _unitName;
        public UnitType UnitType => _unitType;
        public virtual ElementType Element => _element;
        public int CurrentHP
        {
            get => _currentHP;
            set => _currentHP = value;
        }
        public int MaxHP => _maxHP;
        public virtual int AttackPower => _attackPower;
        public virtual int Defense => _defense;
        public bool IsAlive => _isAlive;
        public float DodgeBonus => _dodgeBonus;
        public bool HasShield => _hasShield;
        public float CritChance => _critChance;
        public int CurrentResource
        {
            get => _currentResource;
            set => _currentResource = value;
        }
        public int MaxResource => _maxResource;
        public string ResourceLabel => _unitType switch
        {
            UnitType.Magus => "MP",
            UnitType.Alchemist => string.IsNullOrEmpty(_catalystName) ? "Charges" : _catalystName,
            _ => ""
        };

        protected virtual void Awake()
        {
            _currentHP = _maxHP;
            _currentResource = _maxResource;
        }

        /// <summary>
        /// Runtime configuration for dynamically spawned enemies.
        /// Call this immediately after AddComponent to set stats before battle starts.
        /// </summary>
        public void Configure(string unitName, UnitType type, ElementType element,
            int maxHP, int atk, int def, float crit = 0.1f,
            int maxResource = 0, BattleController battleController = null)
        {
            _unitName = unitName;
            _unitType = type;
            _element = element;
            _maxHP = maxHP;
            _attackPower = atk;
            _defense = def;
            _critChance = crit;
            _maxResource = maxResource;
            _currentHP = maxHP;
            _currentResource = maxResource;
            _battleController = battleController;
        }

        public void TakeDamage(int amount)
        {
            _currentHP = Mathf.Max(0, _currentHP - amount);
            if (_currentHP <= 0)
            {
                _isAlive = false;
            }
        }

        public void Revive(int hp)
        {
            _isAlive = true;
            _currentHP = Mathf.Clamp(hp, 1, _maxHP);
        }

        // --- IEnemyAI ---
        public abstract EnemyIntent DecideAction(IReadOnlyList<IBattleUnit> playerParty, IReadOnlyList<IBattleUnit> enemyParty);

        // ============================================================
        // Protected utility methods for subclasses
        // ============================================================

        protected float HPPercent => _maxHP > 0 ? (float)_currentHP / _maxHP : 0f;

        protected EnemyIntent MakeBasicAttack(IBattleUnit target)
        {
            bool unblockable = Random.value < _unblockableChance;
            int estimatedDamage = Mathf.Max(1, _attackPower - target.Defense);
            return new EnemyIntent(target, unblockable, estimatedDamage);
        }

        protected EnemyIntent MakeSkillAttack(SkillDefinition skill, IBattleUnit target)
        {
            int estDmg = 0;
            if (skill.DamageMultiplier > 0f && target != null)
                estDmg = Mathf.Max(1, Mathf.RoundToInt((_attackPower - target.Defense) * skill.DamageMultiplier));
            else if (skill.HealAmount > 0)
                estDmg = skill.HealAmount;
            return new EnemyIntent(target, false, estDmg, skill);
        }

        protected IBattleUnit PickWeightedTarget(IReadOnlyList<IBattleUnit> playerParty, ElementType attackElement)
        {
            float totalWeight = 0f;
            float[] weights = new float[playerParty.Count];

            for (int i = 0; i < playerParty.Count; i++)
            {
                IBattleUnit p = playerParty[i];
                if (!p.IsAlive) { weights[i] = 0f; continue; }
                float w = 10f;
                float hpPercent = (float)p.CurrentHP / p.MaxHP;
                w += (1f - hpPercent) * 50f;
                if (IsElementStrong(attackElement, p.Element))
                    w += 30f;
                w += Random.Range(0f, 20f);
                weights[i] = w;
                totalWeight += w;
            }

            if (totalWeight <= 0f) return playerParty[0];

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                    return playerParty[i];
            }
            return playerParty[playerParty.Count - 1];
        }

        protected static bool IsElementStrong(ElementType attacker, ElementType target)
        {
            if (attacker == ElementType.None || target == ElementType.None) return false;
            return (attacker == ElementType.Ignis  && target == ElementType.Ventus) ||
                   (attacker == ElementType.Ventus && target == ElementType.Terra)  ||
                   (attacker == ElementType.Terra  && target == ElementType.Aqua)   ||
                   (attacker == ElementType.Aqua   && target == ElementType.Ignis)  ||
                   (attacker == ElementType.Lux    && target == ElementType.Umbra)  ||
                   (attacker == ElementType.Umbra  && target == ElementType.Lux);
        }

        protected static IBattleUnit FindLowestDef(IReadOnlyList<IBattleUnit> units)
        {
            IBattleUnit best = null;
            int lowestDef = int.MaxValue;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsAlive) continue;
                if (units[i].Defense < lowestDef)
                {
                    lowestDef = units[i].Defense;
                    best = units[i];
                }
            }
            return best;
        }

        protected static IBattleUnit FindHighestHP(IReadOnlyList<IBattleUnit> units)
        {
            IBattleUnit best = null;
            int highestHP = -1;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsAlive) continue;
                if (units[i].CurrentHP > highestHP)
                {
                    highestHP = units[i].CurrentHP;
                    best = units[i];
                }
            }
            return best;
        }

        protected static IBattleUnit FindHighestAttack(IReadOnlyList<IBattleUnit> units)
        {
            IBattleUnit best = null;
            int highestAtk = -1;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsAlive) continue;
                if (units[i].AttackPower > highestAtk)
                {
                    highestAtk = units[i].AttackPower;
                    best = units[i];
                }
            }
            return best;
        }

        protected static IBattleUnit FindLowestHPPercent(IReadOnlyList<IBattleUnit> units)
        {
            IBattleUnit best = null;
            float lowestPct = float.MaxValue;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsAlive) continue;
                float pct = (float)units[i].CurrentHP / units[i].MaxHP;
                if (pct < lowestPct)
                {
                    lowestPct = pct;
                    best = units[i];
                }
            }
            return best;
        }

        protected static IBattleUnit FindWeaknessTarget(IReadOnlyList<IBattleUnit> units, ElementType element)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsAlive) continue;
                if (IsElementStrong(element, units[i].Element))
                    return units[i];
            }
            return null;
        }

        protected static IBattleUnit FindWoundedAlly(IReadOnlyList<IBattleUnit> allies, float threshold)
        {
            IBattleUnit worst = null;
            float worstPct = 1f;
            for (int i = 0; i < allies.Count; i++)
            {
                if (!allies[i].IsAlive) continue;
                float pct = (float)allies[i].CurrentHP / allies[i].MaxHP;
                if (pct < threshold && pct < worstPct)
                {
                    worst = allies[i];
                    worstPct = pct;
                }
            }
            return worst;
        }

        protected static IBattleUnit GetRandomLivingAlly(IReadOnlyList<IBattleUnit> allies)
        {
            List<IBattleUnit> living = new List<IBattleUnit>();
            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i].IsAlive) living.Add(allies[i]);
            }
            if (living.Count == 0) return null;
            return living[Random.Range(0, living.Count)];
        }

        protected static int CountLivingUnits(IReadOnlyList<IBattleUnit> units)
        {
            int count = 0;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].IsAlive) count++;
            }
            return count;
        }

        protected static bool AllAlliesFullHP(IReadOnlyList<IBattleUnit> allies)
        {
            for (int i = 0; i < allies.Count; i++)
            {
                if (!allies[i].IsAlive) continue;
                if (allies[i].CurrentHP < allies[i].MaxHP) return false;
            }
            return true;
        }

        protected static IBattleUnit FindHighestDef(IReadOnlyList<IBattleUnit> units)
        {
            IBattleUnit best = null;
            int highestDef = -1;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsAlive) continue;
                if (units[i].Defense > highestDef)
                {
                    highestDef = units[i].Defense;
                    best = units[i];
                }
            }
            return best;
        }

        protected static IBattleUnit FindHighestResource(IReadOnlyList<IBattleUnit> units)
        {
            IBattleUnit best = null;
            int highestRes = -1;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsAlive) continue;
                if (units[i].CurrentResource > highestRes)
                {
                    highestRes = units[i].CurrentResource;
                    best = units[i];
                }
            }
            return best;
        }
    }
}
