using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Mirror;
using System.Linq;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;

public class Inventory : NetworkBehaviour
{
	[Header("Audio")]
	private AudioManager audioManager;

	[Header("Player Settings")]
	[SerializeField] private Transform rightHand;

	[Header("UI Elements")]
	private TMP_Text currentItemText;

	[Header("Inventory Settings")]
	[SerializeField] private int maxItemsPerType = 2;

	[SyncVar(hook = nameof(OnItemPrefabChanged))]
	public string equipedItemPrefabName;

	public GameObject heldItem = null; // The item currently being held.

	private int currentItemIndex = 0; // Tracks the current item in the inventory.
	private Dictionary<string, int> itemCounts = new Dictionary<string, int>(); // Tracks count of each item type.
	private List<InventoryItem> items = new List<InventoryItem>(); // List of items in the inventory.

	private Dictionary<string, TMP_Text> objectiveItemsTextMap = new Dictionary<string, TMP_Text>();

	private string lastItemText = "";

	private Objectives objectives;

	[SerializeField]
	private GameObject[] itemPrefabs=null;
	private void Start()
	{
		audioManager = FindObjectOfType<AudioManager>();
		currentItemText = GameObject.Find("CurrentItem").GetComponent<TMP_Text>();
		objectives = GameObject.Find("Objectives").GetComponent<Objectives>();
		// Initialize the dictionary
		for (int i = 0; i < objectives.objectiveItemNames.Count; i++)
		{
			if (i < objectives.objectiveItemTexts.Count)
			{
				objectiveItemsTextMap[objectives.objectiveItemNames[i]] = objectives.objectiveItemTexts[i];
			}
		}
	}
	private void Update()
	{
		string newText = items.Count > 0 ? items[currentItemIndex].itemName : "No items";
		if (newText != lastItemText)
		{
			currentItemText.text = newText;
			lastItemText = newText;
		}

		UpdateObjectiveItemsUI();
	}


	/// <summary>
	/// Adds an item to the inventory.
	/// </summary>
	public bool ItemExists(InventoryItem item)
	{
		if (itemCounts.ContainsKey(item.itemName) && itemCounts[item.itemName] >= maxItemsPerType)
		{
			return true;
		}
		return false;
	}

	public void AddItem(InventoryItem item)
	{
		Debug.Log("CLIENT - Item Added" + item.itemName);
		// Do not add the item if at max capacity for this type.
		if (itemCounts.ContainsKey(item.itemName) && itemCounts[item.itemName] >= maxItemsPerType)
		{
			Debug.Log("CLIENT - Error Item Not Added");

			return;
		}

		items.Add(item);

		audioManager.PlayAudio("ItemPickup");

		PopupTextManager.Instance.ShowPopupText("+ " + item.itemName);

		if (itemCounts.ContainsKey(item.itemName))
		{
			itemCounts[item.itemName]++;
		}
		else
		{
			itemCounts[item.itemName] = 1;
		}

		currentItemIndex = items.Count - 1;

		UpdateHeldItem(true);
		CmdAddServerItem(item.itemName, 1);

	}

	[Command]
	private void CmdAddServerItem(string itemName, int quantity)
	{
		GameManager.Instance.AddItem(itemName, quantity);

	}

	/// <summary>
	/// Switches the current item in a given direction.
	/// </summary>
	public void SwitchItem(int direction)
	{
		if (items.Count > 0)
		{
			currentItemIndex += direction;

			// Ensure the index loops around the item list.
			if (currentItemIndex >= items.Count) currentItemIndex = 0;
			if (currentItemIndex < 0) currentItemIndex = items.Count - 1;

			InventoryItem currentItem = items[currentItemIndex];

			UpdateHeldItem(true);
		}
		if (items.Count > 1)
		{
			audioManager.PlayAudio("ItemPickup");
		}
	}

	private void UpdateHeldItem(bool destroyExistingItem)
	{
		if (heldItem && destroyExistingItem)
		{

			CmdUpdateHeldItem("destroy");
			
		}
		if (items.Count == 0)
		{
			CmdUpdateHeldItem("null");
			return;
		}


		Invoke(nameof(DelayedUpdateHeldItem), destroyExistingItem? 2f:0);
		



	}

	private void DelayedUpdateHeldItem()
	{

		InventoryItem currentItem = items[currentItemIndex];
		CmdUpdateHeldItem(currentItem.itemName);
	}

	[Command]
	private void CmdUpdateHeldItem(string itemName)
	{
		Debug.Log("SERVER -  update held item prefab");

	
			equipedItemPrefabName = itemName;

	}

	/// <summary>
	/// Decreases the count of the currently held item.
	/// </summary>
	public void DecreaseHeldItem()
	{
		if (!heldItem) return;

		InventoryItem itemComponent = heldItem.GetComponent<InventoryItem>();
		if (!itemComponent) return;

		string itemName = itemComponent.itemName;

		if (itemCounts.ContainsKey(itemName))
		{
			itemCounts[itemName]--;
			if (itemCounts[itemName] <= 0)
			{
				itemCounts.Remove(itemName);
				RemoveItemFromList(itemComponent);

			}
		}
		else
		{
			Debug.LogWarning($"Item {itemName} not found in inventory.");
		}
	}

	private void RemoveItemFromList(InventoryItem itemComponent)
	{
		for (int i = items.Count - 1; i >= 0; i--)
		{
			if (items[i].itemName == itemComponent.itemName)
			{
				items.RemoveAt(i);
				currentItemIndex = Mathf.Max(currentItemIndex - 1, 0);
				UpdateHeldItem(false);
				CmdRemoveServerItem(itemComponent.itemName, 1);
				break;
			}
		}
	}

	private void RemoveItemFromList(string itemName)
	{
		for (int i = items.Count - 1; i >= 0; i--)
		{
			if (items[i].itemName == itemName)
			{
				items.RemoveAt(i);
				currentItemIndex = Mathf.Max(currentItemIndex - 1, 0);
				UpdateHeldItem(true);
				CmdRemoveServerItem(itemName, 1);
				break;
			}
		}
	}



	[Command]
	private void CmdRemoveServerItem(string itemName, int quantity)
	{
		GameManager.Instance.RemoveItem(itemName, quantity);
	}
	/// <summary>
	/// Checks if the held item can be thrown.
	/// </summary>
	public bool Throwable()
	{
		if (!heldItem) return false;
		if (heldItem.GetComponent<ThrowableItem>() == null) return false;
		return heldItem && heldItem.GetComponent<ThrowableItem>().isThrowable;
	}

	public bool Placeable()
	{
		if (!heldItem) return false;
		if (heldItem.GetComponent<TrapItem>() == null) return false;
		return heldItem && heldItem.GetComponent<TrapItem>().isPlaceable;
	}

	public bool IsMelee()
	{
		if (!heldItem) return false;
		if (heldItem.GetComponent<MeleeWeapon>() == null) return false;
		return heldItem && heldItem.GetComponent<MeleeWeapon>().isMelee;
	}

	/// <summary>
	/// Determines if the inventory contains a specific objective item.
	/// </summary>

	[Command]
	public void CmdCheckServerHasObjectiveItem(string objectiveItem)
	{
		// Check if the item exists and has a count greater than 0
		bool hasItem = GameManager.Instance.inventory.Any(item => item.itemName == objectiveItem && item.quantity > 0);

		if (hasItem)

		{
			RpcObjectiveCompleted(objectiveItem);


		}

	}

	public void UpdateObjectiveItemsUI()
	{
		var matchingItems = GetMatchingServerItems();

		foreach (var item in matchingItems)
		{
			string itemName = item.Key;

			CmdCheckServerHasObjectiveItem(itemName);


		}
	}


	[ClientRpc]
	private void RpcObjectiveCompleted(string objectiveItem)
	{

		var matchingItems = GetMatchingServerItems();

		foreach (var item in matchingItems)
		{
			if (item.Key == objectiveItem)
			{

				TMP_Text itemText = item.Value;
				itemText.text = "1/1";

				break;
			}
		}
		StartCoroutine(DelayedRemoveItemFromList(objectiveItem));


	}
	System.Collections.IEnumerator DelayedRemoveItemFromList(string objectiveItem)
	{
		yield return new WaitForSeconds(1);
		RemoveItemFromList(objectiveItem);
	}

	public int CountTotalItems()
	{
		int total = 0;
		foreach (var itemCount in itemCounts.Values)
		{
			total += itemCount;
		}
		return total;
	}

	public Dictionary<string, TMP_Text> GetMatchingServerItems()
	{
		// Get all matching items where the map key matches an inventory item name
		var matchingItems = objectiveItemsTextMap
			.Where(pair => GameManager.Instance.inventory.Any(item => item.itemName == pair.Key))
			.ToDictionary(pair => pair.Key, pair => pair.Value);

		return matchingItems;
	}

	private void OnItemPrefabChanged(string oldItem, string newItem)
	{
		if (newItem == "")
		{
			Debug.Log("CLIENT - on item changed - void");
			return;

		}
		if (newItem == "null")
		{
			Debug.Log("CLIENT - on item changed - null");

			heldItem = null;
			equipedItemPrefabName = "";
			return;

		}
		if (newItem == "destroy")
		{
			Debug.Log("CLIENT - on item changed - destroy");

			Destroy(heldItem);
			equipedItemPrefabName = "";
			return;

		}
		Debug.Log("CLIENT - on item changed");

		GameObject prefab = SearchItemPrefabByName(newItem);
		heldItem = Instantiate(prefab, rightHand.position, rightHand.rotation, rightHand);
		heldItem.name = newItem;
		heldItem.GetComponent<InventoryItem>().playerTransform = transform;
		if (heldItem.GetComponent<InventoryItem>().itemName == "Guitar")
		{
			Debug.Log("CLIENT - on item changed - Guitar");

			float posX = 0.206f;  // Change this value
			float posY = 0.8f;  // Change this value
			float posZ = 0.56f;  // Change this value

			heldItem.transform.localPosition = new Vector3(posX, posY, posZ);

			// Placeholder for rotation values
			float rotX = 42.6f;  // Change this value
			float rotY = 203f;  // Change this value
			float rotZ = -135f;  // Change this value

			heldItem.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
		}

		//avoid collsiions between the current player and the item held
		Collider playerCollider = transform.GetComponent<Collider>();
		Collider itemCollider = heldItem.GetComponent<Collider>();

		Physics.IgnoreCollision(playerCollider, itemCollider);
		Debug.Log("CLIENT - on item changed - Ignored " + playerCollider.transform.name);

	}

	private GameObject SearchItemPrefabByName(string newItem)
	{
		foreach(GameObject i in  itemPrefabs){
			if (i.name == newItem)
				return i;
		}
		Debug.Log("CLIENT - Error! item prefab not found by name");
		return null;
	}
}