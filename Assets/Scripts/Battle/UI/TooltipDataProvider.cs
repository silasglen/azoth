namespace Battle.UI
{
    /// <summary>
    /// Static helper that generates tooltip title/body strings for various battle data types.
    /// </summary>
    public static class TooltipDataProvider
    {
        public static (string title, string body) GetSkillTooltip(SkillDefinition skill, IBattleUnit caster)
        {
            string title = skill.Name;
            string body = "";

            if (skill.ResourceCost > 0)
            {
                string label = caster != null ? caster.ResourceLabel : "Resource";
                body += $"Cost: {skill.ResourceCost} {label}\n";
            }
            else
            {
                body += "Cost: Free\n";
            }

            if (skill.Element != ElementType.None)
                body += $"Element: {skill.Element}\n";

            if (skill.DamageMultiplier > 0f)
                body += $"Damage: {skill.DamageMultiplier:F1}x ATK\n";

            if (skill.HealAmount > 0)
                body += $"Heal: {skill.HealAmount} HP\n";

            if (skill.LifestealRatio > 0f)
                body += $"Lifesteal: {(skill.LifestealRatio * 100f):F0}%\n";

            if (skill.AppliesEffect != StatusEffectType.None)
                body += $"Effect: {skill.AppliesEffect} ({(skill.EffectChance * 100f):F0}% chance, {skill.EffectDuration}t)\n";

            body += $"Target: {skill.TargetType}";
            return (title, body);
        }

        public static (string title, string body) GetItemTooltip(ItemDefinition item)
        {
            string title = item.Name;
            string body = "";

            if (item.IsRevive)
                body += "Revives a fallen ally\n";

            if (item.HealHP > 0)
                body += $"Heal: {item.HealHP} HP\n";

            if (item.RestoreResource > 0)
                body += $"Restore: {item.RestoreResource} Resource\n";

            if (item.Damage > 0)
            {
                body += $"Damage: {item.Damage}";
                if (item.DamageElement != ElementType.None)
                    body += $" ({item.DamageElement})";
                body += "\n";
            }

            if (item.CuresEffect != StatusEffectType.None)
                body += $"Cures: {item.CuresEffect}\n";

            body += $"Target: {item.TargetType}";
            return (title, body);
        }

        public static (string title, string body) GetStatusEffectTooltip(StatusEffectType type, StatusEffect effect)
        {
            string title = type.ToString();
            string body = GetStatusDescription(type);

            if (effect != null)
                body += $"\nRemaining: {effect.RemainingTurns} turn(s)";

            return (title, body);
        }

        public static (string title, string body) GetElementTooltip(ElementType element)
        {
            string title = element.ToString();
            string body = GetElementDescription(element);
            return (title, body);
        }

        public static (string title, string body) GetEnemyTooltip(IBattleUnit enemy)
        {
            string title = enemy.UnitName;

            if (BestiaryData.IsScanned(enemy.UnitName))
            {
                var record = BestiaryData.GetRecord(enemy.UnitName);
                string body = $"HP: {enemy.CurrentHP}/{record.MaxHP}\n" +
                              $"ATK: {record.AttackPower}\n" +
                              $"DEF: {record.Defense}\n" +
                              $"Element: {record.Element}\n" +
                              $"Crit: {(record.CritChance * 100f):F0}%";
                return (title, body);
            }
            else
            {
                return (title, "??? (Not scanned)");
            }
        }

        private static string GetStatusDescription(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Poison: return "Takes damage each turn end.";
                case StatusEffectType.Burn:   return "Takes fire damage each turn end.";
                case StatusEffectType.Stun:   return "Cannot act on their next turn.";
                case StatusEffectType.AtkUp:  return "Attack power increased.";
                case StatusEffectType.AtkDown: return "Attack power decreased.";
                case StatusEffectType.DefUp:  return "Defense increased.";
                case StatusEffectType.DefDown: return "Defense decreased.";
                default: return "";
            }
        }

        private static string GetElementDescription(ElementType element)
        {
            switch (element)
            {
                case ElementType.Ignis:
                    return "Fire - Strong vs Ventus, Weak vs Aqua";
                case ElementType.Ventus:
                    return "Wind - Strong vs Terra, Weak vs Ignis";
                case ElementType.Terra:
                    return "Earth - Strong vs Aqua, Weak vs Ventus";
                case ElementType.Aqua:
                    return "Water - Strong vs Ignis, Weak vs Terra";
                case ElementType.Lux:
                    return "Light - Mutual with Umbra (both super-effective)";
                case ElementType.Umbra:
                    return "Dark - Mutual with Lux (both super-effective)";
                default:
                    return "Neutral - No elemental interactions";
            }
        }
    }
}
