using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MoreMountains.InventoryEngine;

namespace JayMountains
{
    public class ShopBagSellItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI confirmSellItemName;
        [SerializeField] private TextMeshProUGUI itemAmountToSell;
        [SerializeField] private TextMeshProUGUI goldGetFromSelling;

        private InventorySlot _inventorySlot;
        private Inventory _inventory;
        private int _itemIndex;
        private BagItem _item;

        private int _itemAmount = 1;
        private int _itemAmountMax;


        //----------------------------------------------------------------
        public void SetItemData(InventorySlot inventorySlot, Inventory inventory, int itemIndex)
        {
            _inventorySlot = inventorySlot;
            _inventory = inventory;
            _itemIndex = itemIndex;

            _item = _inventory.Content[_itemIndex] as BagItem;
            _itemAmountMax = _item.Quantity;
            _itemAmount = 1;

            confirmSellItemName.text = $"Confirm Sell {_item.ItemName}?";
            itemAmountToSell.text = $"Amount {_itemAmount}/{_itemAmountMax}";
            goldGetFromSelling.text = $"You will receive {_item.goldSell * _itemAmount} gold.";
        }


        //----------------------------------------------------------------
        public void IncreaseAmount()
        {
            _itemAmount++;
            _itemAmount = Mathf.Clamp(_itemAmount, 1, _itemAmountMax);
            itemAmountToSell.text = $"Amount {_itemAmount}/{_itemAmountMax}";
            goldGetFromSelling.text = $"You will receive {_item.goldSell * _itemAmount} gold.";
        }


        //----------------------------------------------------------------
        public void DecreaseAmount()
        {
            _itemAmount--;
            _itemAmount = Mathf.Clamp(_itemAmount, 1, _itemAmountMax);
            itemAmountToSell.text = $"Amount {_itemAmount}/{_itemAmountMax}";
            goldGetFromSelling.text = $"You will receive {_item.goldSell * _itemAmount} gold.";
        }


        //----------------------------------------------------------------
        public void SellItem()
        {
            WalletInventoryManager.Instance.AddCurrency(WalletInventoryManager.Instance.GoldCurrency, _item.goldSell * _itemAmount, WalletInventoryEventTypes.AddFast, true);
            BagInventoryManager.Instance.RemoveItemWithPop(_item, _itemAmount, true);
            ShopBagSellInventoryListener.Instance.ToggleSellConfirmationDisplay(false);
        }
    }
}