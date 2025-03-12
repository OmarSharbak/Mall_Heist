using UnityEngine;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using Sirenix.OdinInspector;

namespace JayMountains
{
    public class ShopBagUpgradeInventoryListener : MMSingleton<ShopBagUpgradeInventoryListener>, MMEventListener<MMInventoryEvent>
    {
        public InventoryDisplay ShopBagUpgrade_InventoryDisplay;

        //[SerializeField] private ShopBagSellItem shopBagUpgradeItem;
        //[SerializeField] private GameObject sellConfirmationDisplay;

        [ShowInInspector, ReadOnly] private Inventory bag = null;
        [ShowInInspector, ReadOnly] private Inventory bagUpgrade = null;

        private bool initialized = false;

        protected override void Awake()
        {
            base.Awake();
            foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
            {
                if ((inventory.name == "Bag_Inventory"))
                {
                    bag = inventory;
                }

                if ((inventory.name == "ShopBagUpgrade_Inventory"))
                {
                    bagUpgrade = inventory;
                }
            }
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Start()
        {
            // We run this on Start and not on Awake. Because on Awake the Inventories are loaded so naturally InventoryDisplays are loaded afterwards.
            // Not using 'MMGameEvent.Trigger("Load")' which automatically handles the loading of Inventories first before moving on to loading the InventoryDisplays.
            // So have set this up myself since the saving and loading system I'm using is from ES3.
            RedrawInventories();

            initialized = true;
        }

        private void GetUpgradableItems()
        {
            bagUpgrade.EmptyInventory();

            for (int i = 0; i < bag.Content.Length; i++)
            {
                if (bag.Content[i] != null)
                {
                    if (((BagItem)bag.Content[i]).CanUpgrade == true)
                    {
                        bagUpgrade.AddItem(bag.Content[i], bag.Content[i].Quantity);
                    }
                }
            }
        }

        /// <summary>
        /// Ensure this happens AFTER all Inventories are loaded!
        /// </summary>
        public void RedrawInventories()
        {
            GetUpgradableItems();
            ShopBagUpgrade_InventoryDisplay.RedrawInventoryDisplay();
        }

        //public void ToggleSellConfirmationDisplay(bool toggle)
        //{
        //    sellConfirmationDisplay.gameObject.SetActive(toggle);

        //    if (toggle == false)
        //    {
        //        RedrawInventories();
        //    }
        //}

        /// <summary>
        /// Catch events and do stuff.
        /// </summary>
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.InventoryEventType == MMInventoryEventType.Click)
            {
                InventorySlot slot = eventType.Slot;
                if (slot.ParentInventoryDisplay == ShopBagUpgrade_InventoryDisplay)
                {
                    Debug.Log($"{slot.gameObject.name} was clicked.");
                    //shopBagUpgradeItem.SetItemData(slot, slot.ParentInventoryDisplay.TargetInventory, slot.Index);
                    //ToggleSellConfirmationDisplay(true);
                }
            }
        }

        /// <summary>
        /// On Enable, we start listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();

            if (initialized)
            {
                RedrawInventories();
            }
        }

        /// <summary>
        /// On Disable, we stop listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnDisable() => this.MMEventStopListening<MMInventoryEvent>();
    }
}