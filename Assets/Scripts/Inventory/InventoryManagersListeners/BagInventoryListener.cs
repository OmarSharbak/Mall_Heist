using UnityEngine;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;

namespace JayMountains
{
    public class BagInventoryListener : MMSingleton<BagInventoryListener>, MMEventListener<MMInventoryEvent>
    {
        public InventoryDisplay Bag_InventoryDisplay;

        [SerializeField] private ViewInventoryItem viewInventoryItem;
        [SerializeField] private GameObject itemSelectedDisplay;
        [SerializeField] private GameObject nothingSelectedDisplay;

        private bool initialized = false;

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

        /// <summary>
        /// Ensure this happens AFTER all Inventories are loaded!
        /// </summary>
        private void RedrawInventories()
        {
            Bag_InventoryDisplay.RedrawInventoryDisplay();
        }

        private void ClearSelectionDisplay()
        {
            itemSelectedDisplay.gameObject.SetActive(false);
            nothingSelectedDisplay.gameObject.SetActive(true);
        }

        private void SetSelectionDisplay()
        {
            itemSelectedDisplay.gameObject.SetActive(true);
            nothingSelectedDisplay.gameObject.SetActive(false);
        }

        /// <summary>
        /// Catch events and do stuff.
        /// </summary>
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.InventoryEventType == MMInventoryEventType.Click)
            {
                InventorySlot slot = eventType.Slot;
                if (slot.ParentInventoryDisplay == Bag_InventoryDisplay)
                {
                    Debug.Log($"{slot.gameObject.name} was clicked.");
                    viewInventoryItem.ViewItem(slot, slot.ParentInventoryDisplay.TargetInventory, slot.Index);
                    SetSelectionDisplay();
                }
            }
        }

        /// <summary>
        /// On Enable, we start listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
            ClearSelectionDisplay();

            if (initialized) RedrawInventories();
        }

        /// <summary>
        /// On Disable, we stop listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnDisable() => this.MMEventStopListening<MMInventoryEvent>();
    }
}