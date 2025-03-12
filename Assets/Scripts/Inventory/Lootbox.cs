using System;
using UnityEngine;
using Random = UnityEngine.Random;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;

namespace JayMountains
{
    public class Lootbox : MonoBehaviour
    {
        /// <summary>
        /// An array of inventory items contained in the lootbox.
        /// </summary>
        public LootboxItem[] LootboxItems;

        /// <summary>
        /// False: Gives everything in the lootbox. <br/>
        /// True:  Gives random item(s) from the lootbox.
        /// </summary>
        public bool RandomSelect = false;

        /// <summary>
        /// False: Normal random. <br/>
        /// True:  Weighted random.
        /// </summary>
        [ShowIf("RandomSelect")] public bool WeightedRandom = false;

        /// <summary>
        /// Number of items to randomly give from the lootbox.
        /// </summary>
        [ShowIf("RandomSelect")] public int RandomAmount = 1;

        public Inventory InventoryToAddTo;

        public void GiveLootboxItems(bool debug = false)
        {
            if (RandomSelect && WeightedRandom)
            {
                for (int i = 0; i < RandomAmount; i++)
                {
                    int selectedItemIndex = 0;

                    // Get a random weight value.
                    int weightedSum = 0;
                    for (int j = 0; j < LootboxItems.Length; j++)
                    {
                        weightedSum += LootboxItems[j].Weight;
                    }
                    int randomWeightValue = Random.Range(1, weightedSum + 1);

                    // Check where the random weight value falls.
                    int processedWeight = 0;
                    for (int j = 0; j < LootboxItems.Length; j++)
                    {
                        processedWeight += LootboxItems[j].Weight;
                        if (randomWeightValue <= processedWeight)
                        {
                            selectedItemIndex = j;
                            break;
                        }
                    }

                    if (debug)
                    {
                        Debug.LogWarning($"Item: {LootboxItems[selectedItemIndex].InventoryItem.ItemID} (item not added to inventory as this was set to debug testing)");
                    }
                    else
                    {
                        LootboxItems[selectedItemIndex].AddToInventory(InventoryToAddTo);
                    }
                }
            }

            if (RandomSelect && !WeightedRandom)
            {
                for (int i = 0; i < RandomAmount; i++)
                {
                    int random = UnityEngine.Random.Range(0, LootboxItems.Length);

                    if (debug)
                    {
                        Debug.LogWarning($"Item: {LootboxItems[random].InventoryItem.ItemID} (item not added to inventory as this was set to debug testing)");
                    }
                    else
                    {
                        LootboxItems[random].AddToInventory(InventoryToAddTo);
                    }
                }
            }

            if (!RandomSelect)
            {
                for (int i = 0; i < LootboxItems.Length; i++)
                {
                    if (debug)
                    {
                        Debug.LogWarning($"Item: {LootboxItems[i].InventoryItem.ItemID} (item not added to inventory as this was set to debug testing)");
                    }
                    else
                    {
                        LootboxItems[i].AddToInventory(InventoryToAddTo);
                    }
                }
            }
        }

        [Button]
        private void DisplayPercentageChance()
        {
            if (RandomSelect && WeightedRandom)
            {
                int weightedSum = 0;
                for (int i = 0; i < LootboxItems.Length; i++)
                    weightedSum += LootboxItems[i].Weight;

                for (int i = 0; i < LootboxItems.Length; i++)
                    LootboxItems[i].PercentageChance = (float)Math.Round((float)LootboxItems[i].Weight / (float)weightedSum * 100f, 2);
            }
        }
    }

    [Serializable]
    public class LootboxItem
    {
        public InventoryItem InventoryItem;
        public int Amount;
        public int Weight;
        [ReadOnly] public float PercentageChance;

        public void AddToInventory(Inventory inventory = null, WalletInventoryEventTypes updateType = WalletInventoryEventTypes.AddFast)
        {
            if (inventory != null)
            {
                inventory.AddItem(InventoryItem, 1);
                return;
            }

            if (InventoryItem.TargetInventoryName == "Bag_Inventory")
            {
                BagInventoryManager.Instance.AddItem(InventoryItem);
            }

            if (InventoryItem.TargetInventoryName == "WalletInventory")
            {
                WalletInventoryManager.Instance.AddCurrency(InventoryItem as CurrencyItem, Amount, updateType);
            }
        }
    }
}