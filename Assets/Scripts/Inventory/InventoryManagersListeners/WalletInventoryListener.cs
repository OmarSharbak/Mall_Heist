using UnityEngine;
using TMPro;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;

namespace JayMountains
{
    public class WalletInventoryListener : MMSingleton<WalletInventoryListener>, MMEventListener<WalletInventoryEvent>
    {
        public CurrencyItem Gold;

        public TextMeshProUGUI GoldAmount;

        private Inventory walletInventory;


        //----------------------------------------------------------------
        /// <summary>
        /// Initialize.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            walletInventory = GameObject.Find("WalletInventory").GetComponentInChildren<Inventory>();
            UpdateDisplay();
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Setup gui display.
        /// </summary>
        protected void UpdateDisplay(string currencyItemID = "")
        {
            if (currencyItemID == "" || currencyItemID == WalletInventoryManager.Instance.GoldCurrency.ItemID)
                GoldAmount.text = walletInventory.GetQuantity(Gold.ItemID).ToString();
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Catch events and do stuff.
        /// </summary>
        public void OnMMEvent(WalletInventoryEvent eventType)
        {
            if (eventType.EventType == WalletInventoryEventTypes.AddFast)
            {
                UpdateDisplay(eventType.CurrencyItemID);
            }

            if (eventType.EventType == WalletInventoryEventTypes.RemoveFast)
            {
                UpdateDisplay(eventType.CurrencyItemID);
            }
        }


        //----------------------------------------------------------------
        /// <summary>
        /// On Enable, we start listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnEnable() => this.MMEventStartListening<WalletInventoryEvent>();


        //----------------------------------------------------------------
        /// <summary>
        /// On Disable, we stop listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnDisable() => this.MMEventStopListening<WalletInventoryEvent>();
    }
}