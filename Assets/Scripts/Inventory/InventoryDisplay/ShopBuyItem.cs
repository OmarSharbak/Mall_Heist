using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MoreMountains.InventoryEngine;

namespace JayMountains
{
    public class ShopBuyItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI confirmBuyItemName;
        [SerializeField] private TextMeshProUGUI itemAmountToBuy;
        [SerializeField] private TextMeshProUGUI goldNeededForBuying;

        private InventorySlot _inventorySlot;
        private Inventory _inventory;
        private int _itemIndex;
        private BagItem _item;

        private int _itemAmount = 1;


        //----------------------------------------------------------------
        public void SetItemData(InventorySlot inventorySlot, Inventory inventory, int itemIndex)
        {
            _inventorySlot = inventorySlot;
            _inventory = inventory;
            _itemIndex = itemIndex;

            _item = _inventory.Content[_itemIndex] as BagItem;
            _itemAmount = 1;

            confirmBuyItemName.text = $"Confirm Buy {_item.ItemName}?";
            itemAmountToBuy.text = $"Amount {_itemAmount}";
            goldNeededForBuying.text = $"You will spend {_item.goldBuy * _itemAmount} gold.";
        }


        //----------------------------------------------------------------
        public void IncreaseAmount()
        {
            _itemAmount++;
            itemAmountToBuy.text = $"Amount {_itemAmount}";
            goldNeededForBuying.text = $"You will spend {_item.goldBuy * _itemAmount} gold.";
        }


        //----------------------------------------------------------------
        public void DecreaseAmount()
        {
            _itemAmount--;
            _itemAmount = Mathf.Max(_itemAmount, 1);
            itemAmountToBuy.text = $"Amount {_itemAmount}";
            goldNeededForBuying.text = $"You will spend {_item.goldBuy * _itemAmount} gold.";
        }


        //----------------------------------------------------------------
        public void BuyItem()
        {
            WalletInventoryManager.Instance.RemoveCurrency(WalletInventoryManager.Instance.GoldCurrency, _item.goldBuy * _itemAmount, true);
            BagInventoryManager.Instance.AddItem(_item, _itemAmount, true);
            ShopSuppliesInventoryListener.Instance.ToggleSellConfirmationDisplay(false);
        }
    }
}