using UnityEngine;

/// <summary>
/// World representation of an item lying on the ground.  When the player
/// interacts, the item is added to their <see cref="Inventory"/> and this
/// GameObject is destroyed.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] ItemData itemData;
    [SerializeField] [Min(1)] int quantity = 1;

    public ItemData ItemData => itemData;
    public int Quantity => quantity;
    public string InteractionPrompt => itemData != null ? $"Pick up {itemData.itemName}" : "Pick up";

    public void Interact(PlayerController player)
    {
        if (itemData == null) return;

        int added = player.Inventory.Add(itemData, quantity);
        if (added <= 0)
        {
            Debug.Log("Inventory full.");
            return;
        }

        if (added < quantity)
        {
            quantity -= added;
            Debug.Log($"Picked up {added}x {itemData.itemName} ({quantity} left on ground).");
        }
        else
        {
            Debug.Log($"Picked up {added}x {itemData.itemName}.");
            Destroy(gameObject);
        }
    }
}
