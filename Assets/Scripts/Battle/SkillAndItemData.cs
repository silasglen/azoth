using System.Collections.Generic;

namespace Battle
{
    public enum SkillTargetType
    {
        Enemy,
        Self,
        Ally
    }

    public class SkillDefinition
    {
        public string Name;
        public int ResourceCost;
        public ElementType Element;
        public float DamageMultiplier;
        public int HealAmount;
        public float LifestealRatio;
        public SkillTargetType TargetType;

        // Status effect fields
        public StatusEffectType AppliesEffect;
        public int EffectDuration;
        public float EffectValue;
        public float EffectChance; // 0.0-1.0

        // B1 AI pattern extensions
        public int ResourceBurnAmount;  // Saboteur: burns this much MP/charges from target (default 0)
        public bool DestroysItem;       // Saboteur: destroys a random party item (default false)
        public bool IsAoE;              // Ritualist: hits all opposing units (default false)

        public SkillDefinition(string name, int cost, ElementType element,
            float dmgMult, int heal, float lifesteal, SkillTargetType target,
            StatusEffectType appliesEffect = StatusEffectType.None,
            int effectDuration = 0, float effectValue = 0f, float effectChance = 0f)
        {
            Name = name;
            ResourceCost = cost;
            Element = element;
            DamageMultiplier = dmgMult;
            HealAmount = heal;
            LifestealRatio = lifesteal;
            TargetType = target;
            AppliesEffect = appliesEffect;
            EffectDuration = effectDuration;
            EffectValue = effectValue;
            EffectChance = effectChance;
        }
    }

    public class ItemDefinition
    {
        public string Name;
        public int HealHP;
        public int RestoreResource;
        public int Damage;
        public ElementType DamageElement;
        public SkillTargetType TargetType;
        public bool IsRevive;
        public StatusEffectType CuresEffect;
        public bool IsEscapeItem;

        public ItemDefinition(string name, int heal, int restoreResource,
            int damage, ElementType dmgElement, SkillTargetType target,
            bool isRevive = false, StatusEffectType curesEffect = StatusEffectType.None,
            bool isEscapeItem = false)
        {
            Name = name;
            HealHP = heal;
            RestoreResource = restoreResource;
            Damage = damage;
            DamageElement = dmgElement;
            TargetType = target;
            IsRevive = isRevive;
            CuresEffect = curesEffect;
            IsEscapeItem = isEscapeItem;
        }
    }

    public class ItemSlot
    {
        public ItemDefinition Item;
        public int Quantity;

        public ItemSlot(ItemDefinition item, int qty)
        {
            Item = item;
            Quantity = qty;
        }
    }

    public static class SkillAndItemData
    {
        private static readonly Dictionary<UnitType, List<SkillDefinition>> _skills =
            new Dictionary<UnitType, List<SkillDefinition>>
            {
                {
                    UnitType.Alchemist, new List<SkillDefinition>
                    {
                        new SkillDefinition("Ignis Toss",    1, ElementType.Ignis, 1.5f, 0,  0f,   SkillTargetType.Enemy,
                            StatusEffectType.Burn, 2, 5f, 0.3f),
                        new SkillDefinition("Mending Salve", 1, ElementType.None,  0f,   30, 0f,   SkillTargetType.Self),
                        new SkillDefinition("Aqua Splash",   2, ElementType.Aqua,  2.0f, 0,  0f,   SkillTargetType.Enemy),
                    }
                },
                {
                    UnitType.Magus, new List<SkillDefinition>
                    {
                        new SkillDefinition("Lux Bolt",      8,  ElementType.Lux,   1.5f, 0,  0f,   SkillTargetType.Enemy),
                        new SkillDefinition("Umbra Drain",   10, ElementType.Umbra, 1.5f, 0,  0.5f, SkillTargetType.Enemy,
                            StatusEffectType.AtkDown, 2, 0.75f, 0.5f),
                        new SkillDefinition("Arcane Shield", 5,  ElementType.None,  0f,   20, 0f,   SkillTargetType.Self),
                    }
                },
                {
                    UnitType.Knight, new List<SkillDefinition>
                    {
                        new SkillDefinition("Power Strike", 0, ElementType.None, 2.0f, 0, 0f, SkillTargetType.Enemy),
                        new SkillDefinition("Shield Bash",  0, ElementType.None, 1.0f, 0, 0f, SkillTargetType.Enemy,
                            StatusEffectType.Stun, 1, 0f, 0.25f),
                    }
                }
            };

        // Enemy skills keyed by UnitType — cost 0 so enemies never run out of resource
        private static readonly Dictionary<UnitType, List<SkillDefinition>> _enemySkills =
            new Dictionary<UnitType, List<SkillDefinition>>
            {
                {
                    UnitType.Magus, new List<SkillDefinition>
                    {
                        new SkillDefinition("Dark Pulse",   0, ElementType.Umbra, 1.8f, 0,  0f, SkillTargetType.Enemy,
                            StatusEffectType.AtkDown, 2, 0.75f, 0.4f),
                        new SkillDefinition("Shadow Heal",  0, ElementType.None,  0f,   25, 0f, SkillTargetType.Ally),
                        new SkillDefinition("Lux Blast",    0, ElementType.Lux,   2.0f, 0,  0f, SkillTargetType.Enemy,
                            StatusEffectType.Burn, 2, 5f, 0.3f),
                    }
                },
                {
                    UnitType.Knight, new List<SkillDefinition>
                    {
                        new SkillDefinition("Heavy Swing",  0, ElementType.None, 2.0f, 0, 0f, SkillTargetType.Enemy,
                            StatusEffectType.Stun, 1, 0f, 0.2f),
                        new SkillDefinition("War Cry",      0, ElementType.None, 0f,   0, 0f, SkillTargetType.Ally,
                            StatusEffectType.AtkUp, 2, 1.25f, 1.0f),
                    }
                },
                {
                    UnitType.Alchemist, new List<SkillDefinition>
                    {
                        new SkillDefinition("Venom Splash", 0, ElementType.Aqua, 1.0f, 0, 0f, SkillTargetType.Enemy,
                            StatusEffectType.Poison, 3, 4f, 0.5f),
                    }
                }
            };

        public static List<SkillDefinition> GetSkills(UnitType type)
        {
            return _skills.TryGetValue(type, out var list) ? list : new List<SkillDefinition>();
        }

        public static List<SkillDefinition> GetEnemySkills(UnitType type)
        {
            return _enemySkills.TryGetValue(type, out var list) ? list : new List<SkillDefinition>();
        }

        public static List<ItemSlot> GetStartingInventory()
        {
            return new List<ItemSlot>
            {
                new ItemSlot(new ItemDefinition("Health Potion", 40, 0, 0,  ElementType.None,  SkillTargetType.Ally),  3),
                new ItemSlot(new ItemDefinition("Ether",         0,  5, 0,  ElementType.None,  SkillTargetType.Ally),  2),
                new ItemSlot(new ItemDefinition("Fire Bomb",     0,  0, 25, ElementType.Ignis, SkillTargetType.Enemy), 2),
                new ItemSlot(new ItemDefinition("Antidote",      10, 0, 0,  ElementType.None,  SkillTargetType.Ally,
                    curesEffect: StatusEffectType.Poison), 1),
                new ItemSlot(new ItemDefinition("Phoenix Down",  50, 0, 0,  ElementType.None,  SkillTargetType.Ally,
                    isRevive: true), 1),
                new ItemSlot(new ItemDefinition("Escape Flare",  0,  0, 0,  ElementType.None,  SkillTargetType.Self,
                    isEscapeItem: true), 1),
            };
        }
    }
}
