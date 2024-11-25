using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Inventory : MonoBehaviour
{
    [Header("Audio")]
    private AudioManager audioManager;

    [Header("Player Settings")]
    [SerializeField] private Transform rightHand;

    [Header("UI Elements")]
    private TMP_Text currentItemText;

    [Header("Inventory Settings")]
    [SerializeField] private int maxItemsPerType = 2;
    public GameObject heldItem = null; // The item currently being held.
    private int currentItemIndex = 0; // Tracks the current item in the inventory.
    private Dictionary<string, int> itemCounts = new Dictionary<string, int>(); // Tracks count of each item type.
    private List<InventoryItem> items = new List<InventoryItem>(); // List of items in the inventory.

    private Dictionary<string, TMP_Text> objectiveItemsTextMap = new Dictionary<string, TMP_Text>();

    private string lastItemText = "";

    private Objectives objectives;
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
        // Do not add the item if at max capacity for this type.
        if (itemCounts.ContainsKey(item.itemName) && itemCounts[item.itemName] >= maxItemsPerType)
        {
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
        UpdateHeldItem();
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

            UpdateHeldItem();
        }
        if (items.Count > 1)
        {
            audioManager.PlayAudio("ItemPickup");
        }
    }

    /// <summary>
    /// Updates the held item and optionally destroys the existing one.
    /// </summary>
    private void UpdateHeldItem(bool destroyExistingItem = true)
    {
        if (heldItem && destroyExistingItem)
        {
            Destroy(heldItem);
        }

        if (items.Count == 0)
        {
            heldItem = null;
            return;
        }

        InventoryItem currentItem = items[currentItemIndex];
        heldItem = Instantiate(currentItem.prefab, rightHand.position, rightHand.rotation, rightHand);
        heldItem.GetComponent<InventoryItem>().playerTransform = transform;
        if (heldItem.GetComponent<InventoryItem>().itemName == "Guitar")
        {
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
                break;
            }
        }
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

    public bool HasObjectiveItem(string objectiveItem)
    {
        return itemCounts.ContainsKey(objectiveItem) && itemCounts[objectiveItem] > 0;
    }

    public void UpdateObjectiveItemsUI()
    {
        foreach (var item in objectiveItemsTextMap)
        {
            string itemName = item.Key;
            TMP_Text itemText = item.Value;

            if (HasObjectiveItem(itemName))
            {
                itemText.text = "1/1";
                RemoveItemFromList(itemName);
                //UpdateHeldItem(false);
            }

        }
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

}