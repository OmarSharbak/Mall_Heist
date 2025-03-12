using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using JButler;

namespace JayMountains
{
    public class BagInventoryManager : MMSingleton<BagInventoryManager>
    {
        public Inventory Bag_Inventory;


        //----------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="saveData">If you're adding a lot of items don't set this to true. Use the BagInventoryManager.SaveInventory once all items have been added.</param>
        [Button(ButtonStyle.CompactBox, Expanded = true), BoxGroup("add", ShowLabel = false),
         InfoBox("Item gets added according to the TargetInventoryName on the Scriptable Object.")]
        public void AddItem(InventoryItem item, int quantity = 1, bool saveData = false)
        {
            item.TargetInventory("Player1").AddItem(item, quantity);

            if (item is MineralItem)
            {
                MineralsCollectionPerksManager.Instance.AddMineral(item, quantity, true);
            }

            JLog.Log(true, $"<b>Inventory (Bag) ►</b> Added: [Lv.{((BagItem)item).Level}] {item.ItemID}");
            if (saveData) SaveData();
        }


        //----------------------------------------------------------------
        public void RemoveItemWithPop(InventoryItem item, int quantity, bool saveDataLocal = false)
        {
            int quantityLeftToRemove = quantity;

            while (quantityLeftToRemove > 0)
            {
                List<int> list = item.TargetInventory("Player1").InventoryContains(item.ItemID);

                if (list.Count == 0)
                {
                    // This should be handled from the main function that is calling this.
                    Debug.LogWarning($"Nothing to remove but pending: {quantityLeftToRemove}");
                    break;
                }

                int quantityAtIndex = item.TargetInventory("Player1").Content[list[0]].Quantity;

                //LevelDownItem(item.TargetInventory("Player1"), list[0]);
                item.TargetInventory("Player1").RemoveItem(list[0], quantityLeftToRemove);
                JLog.Log(true, $"<b>Inventory (Bag) ►</b> Removed: {item.ItemID}");

                if (item.TargetInventory("Player1").Content[list[0]] == null)
                    Pop(item.TargetInventory("Player1"), list[0]);

                quantityLeftToRemove -= quantityAtIndex;
            }

            // When making such changes to the inventory you need to redraw.
            MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, item.TargetInventory("Player1").name, null, 0, 0, "Player1");

            if (saveDataLocal) SaveData();
        }


        //----------------------------------------------------------------
        /// <summary>
        /// NOTE: Taken from InventoryDisplayFilter.cs, make sure there are no conflicts.
        /// When an item is removed from the array of an inventory, shift the remaining items up by 1 index.
        /// </summary>
        private void Pop(Inventory inventory, int removedAtIndex)
        {
            int slotsToShiftUp = inventory.NumberOfFilledSlots - removedAtIndex;
            //Debug.Log($"Slots to shift up [{slotsToShiftUp}] from index [{removedAtIndex}].");
            for (int i = removedAtIndex + 1; i <= removedAtIndex + 1 + inventory.NumberOfFilledSlots - removedAtIndex; i++)
            {
                if (i != inventory.Content.Length)
                {
                    inventory.Content[i - 1] = inventory.Content[i];
                }
                --slotsToShiftUp;
                if (slotsToShiftUp == 0)
                {
                    inventory.Content[i] = null;
                }
            }
        }


        //----------------------------------------------------------------
        public void LevelUpItem(Inventory inventory, string itemID)
        {
            List<int> n = inventory.InventoryContains(itemID);

            if (inventory.Content[n[0]] is BagItem)
            {
                BagItem item = inventory.Content[n[0]] as BagItem;

                int goldNeeded = (int)item.UpgradeCost[item.Level - 1];

                if (goldNeeded <= WalletInventoryManager.Instance.GetCurrency(WalletInventoryManager.Instance.GoldCurrency))
                {
                    item.LevelUp();

                    WalletInventoryManager.Instance.RemoveCurrency(WalletInventoryManager.Instance.GoldCurrency, goldNeeded, true);
                    SaveData();
                    JLog.Log(true, $"<b>Inventory (Bag) ►</b> Item leveled up for {goldNeeded} Gold.");

                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChangedSlot, null, inventory.name, null, 0, n[0], "Player1");
                }
                else
                {
                    JLog.Log(true, $"<b>Inventory (Bag) ►</b> You don't have enough Gold to level up the item.");
                    //AlertPopupsManager.Instance.ToastMessage("Not enough Gold.");
                }
            }
        }


        #region Local Saving & Loading + First Launch
        //----------------------------------------------------------------
        public void SaveInventory() => SaveData();


        //----------------------------------------------------------------
        public void LoadInventory() => LoadData();


        //----------------------------------------------------------------
        private void LoadFirstLaunchInventory()
        {
            transform.GetComponent<Lootbox>().GiveLootboxItems();
            JLog.Log(true, $"<b>Inventory (Bag) ►</b> Loaded first launch items.");

            SaveData();
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Ensure this happens BEFORE the InventoryDisplays are loaded! See 'BagInventoryListener' script to understand why.
        /// </summary>
        private void LoadData()
        {
            if (!ES3.FileExists() && GameDataManager.Instance.FirstLaunch != 1)
            {
                JLog.Log(true, $"<b>Inventory (Bag) ►</b> Error - Local save does not exist.");
                return;
            }

            ES3Settings ES3Settings = GameDataManager.Instance.ES3Settings;

            if (!ES3.KeyExists(Bag_Inventory.gameObject.name, ES3Settings))
            {
                JLog.Log(true, $"<bInventory (Bag) ►</b> Save/Load not ran before. This is a new content/feature.");
                LoadFirstLaunchInventory();
                return;
            }

            SerializedInventoryWithItemLevels serializedInventory;

            serializedInventory = ES3.KeyExists(Bag_Inventory.gameObject.name, ES3Settings) == true ? ES3.Load<SerializedInventoryWithItemLevels>(Bag_Inventory.gameObject.name, ES3Settings) : null;
            ExtractSerializedInventoryWithItemLevels(serializedInventory, Bag_Inventory);

            JLog.Log(true, $"<b>Inventory (Bag) ►</b> Loaded from local save.");
        }


        //----------------------------------------------------------------
        private void SaveData()
        {
            if (!GameDataManager.Instance.EnableLocalSave) return;

            SerializedInventoryWithItemLevels serializedInventory = new SerializedInventoryWithItemLevels();

            FillSerializedInventoryWithItemLevels(serializedInventory, Bag_Inventory);
            ES3.Save<SerializedInventoryWithItemLevels>(Bag_Inventory.gameObject.name, serializedInventory);

            JLog.Log(true, $"<b>Inventory (Bag) ►</b> Saved to local save.");
        }
        #endregion


        #region Extended Serialized Inventory
        //----------------------------------------------------------------
        /// <summary>
        /// Extended from MoreMountains.InventoryEngine > Inventory
        /// Fills the serialized inventory for storage
        /// This is for inventories that need to save item levels.
        /// </summary>
        private void FillSerializedInventoryWithItemLevels(SerializedInventoryWithItemLevels serializedInventory, Inventory inventory)
        {
            //serializedInventory.InventoryType = inventory.InventoryType;
            //serializedInventory.DrawContentInInspector = inventory.DrawContentInInspector;
            serializedInventory.ContentType = new string[inventory.Content.Length];
            serializedInventory.ContentQuantity = new int[inventory.Content.Length];
            serializedInventory.ContentLevel = new int[inventory.Content.Length];
            for (int i = 0; i < inventory.Content.Length; i++)
            {
                if (!InventoryItem.IsNull(inventory.Content[i]))
                {
                    serializedInventory.ContentType[i] = inventory.Content[i].ItemID;
                    serializedInventory.ContentQuantity[i] = inventory.Content[i].Quantity;
                    BagItem item = inventory.Content[i] as BagItem;
                    serializedInventory.ContentLevel[i] = item.Level;
                }
                else
                {
                    serializedInventory.ContentType[i] = null;
                    serializedInventory.ContentQuantity[i] = 0;
                }
            }
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Extended from MoreMountains.InventoryEngine > SerializedInventory
        /// Extracts the serialized inventory from a file content
        /// This is for inventories that need to save item levels.
        /// </summary>
        private void ExtractSerializedInventoryWithItemLevels(SerializedInventoryWithItemLevels serializedInventory, Inventory inventory, int newCollectionSize = -1)
        {
            if (serializedInventory == null)
            {
                return;
            }

            //inventory.InventoryType = serializedInventory.InventoryType;
            //inventory.DrawContentInInspector = serializedInventory.DrawContentInInspector;
            inventory.Content = newCollectionSize == -1 ? new InventoryItem[serializedInventory.ContentType.Length] : new InventoryItem[newCollectionSize];
            for (int i = 0; i < serializedInventory.ContentType.Length; i++)
            {
                if ((serializedInventory.ContentType[i] != null) && (serializedInventory.ContentType[i] != ""))
                {
                    InventoryItem _loadedInventoryItem = Resources.Load<InventoryItem>(Inventory._resourceItemPath + serializedInventory.ContentType[i]);

                    if (_loadedInventoryItem == null)
                    {
                        _loadedInventoryItem = Resources.Load<InventoryItem>(Inventory._resourceItemPath + "Minerals/" + serializedInventory.ContentType[i]);
                    }

                    if (_loadedInventoryItem == null)
                    {
                        Debug.LogError("InventoryEngine : Couldn't find any inventory item to load at " + Inventory._resourceItemPath
                            + " named " + serializedInventory.ContentType[i] + ". Make sure all your items definitions names (the name of the InventoryItem scriptable " +
                            "objects) are exactly the same as their ItemID string in their inspector. " +
                            "Once that's done, also make sure you reset all saved inventories as the mismatched names and IDs may have " +
                            "corrupted them.");
                    }
                    else
                    {
                        BagItem item = (BagItem)((InventoryItem)_loadedInventoryItem.Copy());
                        item.Quantity = serializedInventory.ContentQuantity[i];
                        item.Level = serializedInventory.ContentLevel[i];
                        inventory.Content[i] = (InventoryItem)((BagItem)item);
                    }
                }
                else
                {
                    inventory.Content[i] = null;
                }
            }
        }
        #endregion
    }


    #region Extended Serialized Inventory
    //----------------------------------------------------------------
    //----------------------------------------------------------------
    [Serializable]
    /// <summary>
    /// Extended from MoreMountains.InventoryEngine > SerializedInventory
    /// Serialized class to help store / load inventories from files.
    /// This is for inventories that need to save item levels.
    /// </summary>
    public class SerializedInventoryWithItemLevels
    {
        //public int NumberOfRows;
        //public int NumberOfColumns;
        //public string InventoryName = "Inventory";
        //public MoreMountains.InventoryEngine.Inventory.InventoryTypes InventoryType;
        //public bool DrawContentInInspector = false;
        public string[] ContentType;
        public int[] ContentQuantity;
        public int[] ContentLevel;
    }
    #endregion
}