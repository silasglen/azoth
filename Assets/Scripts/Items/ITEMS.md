# Azoth -- Item System Reference

---

## Item Categories

| Category | Description |
|----------|-------------|
| `Catalyst` | Boosts alchemy skills of a matching element during battle. Each potency point adds +1% to skill base power (stacks additively). |
| `Gear` | Equippable items assigned to a GearSlot (Head, Body, Shield, Weapon, Ring, Amulet). |
| `Potion` | Consumable potions (healing, buffs, etc.). |
| `Trinket` | Miscellaneous collectibles. Default category for new items. |
| `Key` | Quest and progression keys. |
| `Ingredient` | Crafting ingredients (subcategories TBD). |
| `MagnumOpus` | Ultimate alchemical creations. |

---

## Item Rarity

| Rarity | Tier |
|--------|------|
| `Common` | 1 (default) |
| `Uncommon` | 2 |
| `Rare` | 3 |
| `Epic` | 4 |
| `Legendary` | 5 |

---

## Gear Slots

| Slot | Description |
|------|-------------|
| `None` | Default for non-gear items. |
| `Head` | Head armour |
| `Body` | Body armour |
| `Shield` | Off-hand shield |
| `Weapon` | Weapon |
| `Ring` | Finger ring |
| `Amulet` | Neck amulet |

Only relevant when Category = `Gear`.

---

## ItemData Fields

| Field | Type | Default | Notes |
|-------|------|---------|-------|
| Item Name | string | -- | Display name shown in UI and interaction prompts. |
| Description | string | -- | Flavour text (multi-line). |
| Icon | Sprite | -- | Inventory UI icon. |
| Stackable | bool | true | Whether multiple units share one inventory slot. |
| Max Stack | int | 99 | Maximum units per stack. |
| Category | ItemCategory | Trinket | See categories above. |
| Rarity | ItemRarity | Common | See rarities above. |
| Gear Slot | GearSlot | None | Equipment slot (only when Category = Gear). |
| Element | Element | None | Catalyst element (only when Category = Catalyst). |
| Potency | int | 0 | Catalyst strength (only when Category = Catalyst). |

**Property:** `bool IsCatalyst` -- shorthand for `category == ItemCategory.Catalyst`.

---

## Elements

| Element | Meaning |
|---------|---------|
| `None` | Unaligned / neutral |
| `Ignis` | Fire |
| `Aqua` | Water |
| `Terra` | Earth |
| `Ventus` | Air |
| `Lux` | Light |
| `Umbra` | Dark |

**Weakness cycle:** Ignis > Ventus > Terra > Aqua > Ignis. Lux and Umbra are mutually weak to each other. None is neutral.

---

## Catalyst Bonus Formula

```
catalystBonus = sum of (potency * count) for all Catalyst items in inventory
                matching the skill's element
effectivePower = skill.basePower * (1 + catalystBonus * 0.01)
```

---

## Creating Items in Unity

Assets > Create > Azoth > Item Data

1. Set **Item Name** and **Description**.
2. Pick a **Category** and **Rarity**.
3. For **Gear**: choose a **Gear Slot**.
4. For **Catalyst**: set **Element** and **Potency**.
5. Assign an **Icon** sprite.
6. Place in the world with an `ItemPickup` component, or add directly to `Inventory` via code.
