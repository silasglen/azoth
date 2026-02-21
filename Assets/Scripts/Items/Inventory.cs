using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple slot-based inventory attached to the player.  Manages adding,
/// removing, and querying items.  Fires <see cref="OnChanged"/> whenever
/// the contents change so UI can react.
/// </summary>
public class Inventory : MonoBehaviour
{
    [Serializable]
    public class Slot
    {
        public ItemData item;
        public int count;
    }

    [SerializeField] int maxSlots = 20;

    readonly List<Slot> _slots = new();
    public IReadOnlyList<Slot> Slots => _slots;

    /// <summary>Fired after any add / remove operation.</summary>
    public event Action OnChanged;

    /// <summary>
    /// Try to add <paramref name="amount"/> units of <paramref name="item"/>.
    /// Returns the number of units actually added (may be less if full).
    /// </summary>
    public int Add(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return 0;

        int added = 0;

        // Fill existing stacks first.
        if (item.stackable)
        {
            foreach (var slot in _slots)
            {
                if (slot.item != item) continue;
                int space = item.maxStack - slot.count;
                if (space <= 0) continue;
                int toAdd = Mathf.Min(space, amount - added);
                slot.count += toAdd;
                added += toAdd;
                if (added >= amount) break;
            }
        }

        // Create new slots for the remainder.
        while (added < amount && _slots.Count < maxSlots)
        {
            int perSlot = item.stackable ? Mathf.Min(item.maxStack, amount - added) : 1;
            _slots.Add(new Slot { item = item, count = perSlot });
            added += perSlot;
        }

        if (added > 0) OnChanged?.Invoke();
        return added;
    }

    /// <summary>
    /// Remove up to <paramref name="amount"/> units of <paramref name="item"/>.
    /// Returns the number actually removed.
    /// </summary>
    public int Remove(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return 0;

        int removed = 0;
        for (int i = _slots.Count - 1; i >= 0 && removed < amount; i--)
        {
            if (_slots[i].item != item) continue;
            int toRemove = Mathf.Min(_slots[i].count, amount - removed);
            _slots[i].count -= toRemove;
            removed += toRemove;
            if (_slots[i].count <= 0) _slots.RemoveAt(i);
        }

        if (removed > 0) OnChanged?.Invoke();
        return removed;
    }

    /// <summary>Total count of <paramref name="item"/> across all slots.</summary>
    public int Count(ItemData item)
    {
        int total = 0;
        foreach (var slot in _slots)
            if (slot.item == item) total += slot.count;
        return total;
    }

    /// <summary>Whether the inventory contains at least one unit of <paramref name="item"/>.</summary>
    public bool Has(ItemData item) => Count(item) > 0;

    /// <summary>
    /// Sum of potency * count for all catalyst items matching <paramref name="element"/>.
    /// </summary>
    public int GetCatalystBonus(Element element)
    {
        if (element == Element.None) return 0;
        int total = 0;
        foreach (var slot in _slots)
        {
            if (slot.item != null && slot.item.IsCatalyst && slot.item.element == element)
                total += slot.item.potency * slot.count;
        }
        return total;
    }
}
