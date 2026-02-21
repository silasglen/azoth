using UnityEngine;

public enum ItemCategory
{
    Catalyst,       // Boost alchemy skills
    Gear,           // Equippable items with a GearSlot
    Potion,         // Consumable potions
    Trinket,        // Miscellaneous collectibles
    Key,            // Quest / progression keys
    Ingredient,     // Crafting ingredients (subcategories TBD)
    MagnumOpus      // Ultimate alchemical creations
}

public enum GearSlot
{
    None,
    Head,
    Body,
    Shield,
    Weapon,
    Ring,
    Amulet
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

/// <summary>
/// Immutable definition of an item type.  Create instances via
/// Assets > Create > Azoth > Item Data.  Referenced by <see cref="ItemPickup"/>
/// (world representation) and <see cref="Inventory"/> (owned items).
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Azoth/Item Data")]
public class ItemData : ScriptableObject
{
    [Tooltip("Display name shown in UI and interaction prompts.")]
    public string itemName;

    [TextArea(2, 4)]
    [Tooltip("Flavour / description text.")]
    public string description;

    [Tooltip("Icon used in inventory UI.")]
    public Sprite icon;

    [Tooltip("Whether multiple units can share one inventory slot.")]
    public bool stackable = true;

    [Tooltip("Maximum units per stack (only relevant when stackable).")]
    [Min(1)] public int maxStack = 99;

    [Header("Category & Rarity")]
    public ItemCategory category = ItemCategory.Trinket;
    public ItemRarity rarity = ItemRarity.Common;

    [Header("Gear (only relevant when Category = Gear)")]
    public GearSlot gearSlot = GearSlot.None;

    [Header("Catalyst (only relevant when Category = Catalyst)")]
    public Element element = Element.None;
    [Min(0)] public int potency;

    public bool IsCatalyst => category == ItemCategory.Catalyst;
}
