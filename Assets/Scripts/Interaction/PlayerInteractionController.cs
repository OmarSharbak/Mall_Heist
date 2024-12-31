using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

// This class handles the interaction of the player with objects that can be added to the inventory.
public class PlayerInteractionController : MonoBehaviour
{

	// Reference to the player's inventory.
	Inventory inventory;

	MMFeedbacks mmFeedbacksPickup;

	// Called once at the beginning of the runtime.
	private void Start()
	{
		// Get the Inventory component attached to the same GameObject as this script.
		inventory = GetComponent<Inventory>();

		mmFeedbacksPickup = GameObject.Find("MMFeedbacks(pickup)").GetComponent<MMFeedbacks>();
	}

	// Public method to allow the player to interact with objects that can be added to the inventory.
	public void InteractWith(InteractableObject addedObject)
	{
		// Attempt to get the InventoryItem component from the interactable object.
		InventoryItem item = addedObject.GetComponent<InventoryItem>();

		if (item != null && !inventory.ItemExists(item))
		{
			if (addedObject.pickedUpTimes > 0)
			{
				if (mmFeedbacksPickup != null)
					mmFeedbacksPickup.PlayFeedbacks();

				inventory.AddItem(item);
				addedObject.CmdSetPickedUpTimes(addedObject.pickedUpTimes - 1);
			}
			// If the InventoryItem component is found, add the item to the player's inventory.

		}
		else
		{
			Debug.Log("Player interaction controller: item null or not in the inventory");
		}
	}
}
