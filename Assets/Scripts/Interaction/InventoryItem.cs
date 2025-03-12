using Mirror;
using UnityEngine;

// Represents an item in the player's inventory.
public abstract class InventoryItem : NetworkBehaviour
{
    // Basic properties of the inventory item
    public string itemName;                    // Name of the inventory item
    public GameObject prefab;                  // Prefab instance of the item
    public Transform playerTransform;          // Reference to the player's position for AI interactions

}
