using System;
using UnityEngine;

// Represents an item in the player's inventory.
public abstract class InventoryItem : MonoBehaviour
{
    // Basic properties of the inventory item
    public string itemName;                    // Name of the inventory item
    public GameObject prefab;                  // Prefab instance of the item
    public Transform playerTransform;          // Reference to the player's position for AI interactions

    void Update()
    {
        // Base class Update logic can be put here
    }
}
