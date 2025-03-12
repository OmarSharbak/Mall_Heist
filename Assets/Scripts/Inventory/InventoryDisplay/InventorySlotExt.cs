using UnityEngine;
using MoreMountains.InventoryEngine;
using TMPro;

namespace JayMountains
{
    public class InventorySlotExt : InventorySlot
    {
        public TextMeshProUGUI QuantityTextTMPro;
        public TextMeshProUGUI BuyGoldAmountTextTMPro;
        public TextMeshProUGUI UpgradeTextTMPro;
        public TextMeshProUGUI LevelTextTMPro;

        private InventoryItem inventoryItem;

        public override void SetQuantity(int quantity)
        {
            base.SetQuantity(quantity);
            if (quantity > 0)
            {
                if (QuantityTextTMPro)
                {
                    QuantityTextTMPro.text = QuantityText.text;
                    QuantityTextTMPro.gameObject.SetActive(true);
                }
            }

        }

        public override void DrawIcon(InventoryItem item, int index)
        {
            base.DrawIcon(item, index);
            inventoryItem = item;

            if (inventoryItem is BagItem)
            {
                if (BuyGoldAmountTextTMPro)
                {
                    if (((BagItem)inventoryItem).CanUpgrade)
                    {
                        LevelTextTMPro.text = $"Lv. {((BagItem)inventoryItem).Level}";
                        if (((BagItem)inventoryItem).Level != ((BagItem)inventoryItem).UpgradeCost.Length + 1)
                        {
                            BuyGoldAmountTextTMPro.text = ((BagItem)inventoryItem).UpgradeCost[((BagItem)inventoryItem).Level - 1].ToString();
                            UpgradeTextTMPro.text = $"Upgrade {((BagItem)inventoryItem).Level - 1}/{((BagItem)inventoryItem).UpgradeCost.Length}";
                        }
                        else
                        {
                            BuyGoldAmountTextTMPro.text = "maxed";
                            UpgradeTextTMPro.text = $"Upgrade {((BagItem)inventoryItem).Level - 1}/{((BagItem)inventoryItem).UpgradeCost.Length}";
                        }
                    }
                    else
                    {
                        BuyGoldAmountTextTMPro.text = ((BagItem)inventoryItem).goldBuy.ToString();
                    }
                }
            }
        }

        //----------------------------------------------------------------
        public void UpgradeItem()
        {
            if (((BagItem)inventoryItem).Level == ((BagItem)inventoryItem).UpgradeCost.Length + 1)
            {
                return;
            }
            BagInventoryManager.Instance.LevelUpItem(inventoryItem.TargetInventory("Player1"), inventoryItem.ItemID);
            ShopBagUpgradeInventoryListener.Instance.RedrawInventories();
        }
    }
}