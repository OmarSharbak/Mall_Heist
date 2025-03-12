using UnityEngine;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using Sirenix.OdinInspector;

namespace JayMountains
{
    public class ShopSuppliesInventoryListener : MMSingleton<ShopSuppliesInventoryListener>, MMEventListener<MMInventoryEvent>
    {
        public InventoryDisplay ShopSupplies_InventoryDisplay;

        [SerializeField] private ShopBuyItem shopBuyItem;
        [SerializeField] private GameObject buyConfirmationDisplay;

        public void ToggleSellConfirmationDisplay(bool toggle)
        {
            buyConfirmationDisplay.gameObject.SetActive(toggle);
        }

        /// <summary>
        /// Catch events and do stuff.
        /// </summary>
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.InventoryEventType == MMInventoryEventType.Click)
            {
                InventorySlot slot = eventType.Slot;
                if (slot.ParentInventoryDisplay == ShopSupplies_InventoryDisplay)
                {
                    Debug.Log($"{slot.gameObject.name} was clicked.");
                    shopBuyItem.SetItemData(slot, slot.ParentInventoryDisplay.TargetInventory, slot.Index);
                    ToggleSellConfirmationDisplay(true);
                }
            }
        }

        /// <summary>
        /// On Enable, we start listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
        }

        /// <summary>
        /// On Disable, we stop listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnDisable() => this.MMEventStopListening<MMInventoryEvent>();
    }
}