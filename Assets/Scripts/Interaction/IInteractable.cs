using UnityEngine;

/// <summary>
/// Contract for any object the player can interact with (NPCs, items, doors, etc.).
/// Attach to a GameObject that also has a Collider2D set as a trigger.
/// </summary>
public interface IInteractable
{
    /// <summary>Short label shown to the player when in range (e.g. "Talk", "Pick up").</summary>
    string InteractionPrompt { get; }

    /// <summary>Called once when the player presses the Interact input while in range.</summary>
    void Interact(PlayerController player);
}
