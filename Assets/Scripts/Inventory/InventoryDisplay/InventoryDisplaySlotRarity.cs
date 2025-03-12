using UnityEngine;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using JButler;

namespace JayMountains
{
    public class InventoryDisplaySlotRarity : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        public Sprite SlotImageCommon;
        public Sprite SlotImageUncommon;
        public Sprite SlotImageRare;
        public Sprite SlotImageEpic;

        private InventoryDisplay inventoryDisplay;
        private Inventory targetInventory;

        private bool forceSlotRarityUpdate = false;
        private bool initialized = false;

        private void Awake()
        {
            inventoryDisplay = gameObject.GetComponent<InventoryDisplay>();
            targetInventory = inventoryDisplay.TargetInventory;

            forceSlotRarityUpdate = true;
        }

        private void Start()
        {
            initialized = true;
        }

        public Sprite SetSlotRarityImage(InventoryItem item)
        {
            var spr = inventoryDisplay.FilledSlotImage;

            RarityGrades rarity;
            rarity = CheckItemRarityGrade.GetRarity(item);

            switch (rarity)
            {
                case RarityGrades.Common:
                    spr = SlotImageCommon;
                    break;

                case RarityGrades.Uncommon:
                    spr = SlotImageUncommon;
                    break;

                case RarityGrades.Rare:
                    spr = SlotImageRare;
                    break;

                case RarityGrades.Epic:
                    spr = SlotImageEpic;
                    break;

                case RarityGrades.CollectibleCommon:
                    spr = SlotImageCommon;
                    break;

                case RarityGrades.CollectibleRare:
                    spr = SlotImageRare;
                    break;
            }

            return spr;
        }

        private void UpdateDisplay()
        {
            for (int i = 0; i < targetInventory.Content.Length; i++)
            {
                if (targetInventory.Content[i] != null)
                {
                    inventoryDisplay.SlotContainer[i].TargetImage.sprite = SetSlotRarityImage(targetInventory.Content[i]);
                    JLog.Log(true, $"[LateUpdate] > [InventoryDisplaySlotRarity] > [{targetInventory.name}] Changed slot rarity sprite.");
                }
            }
        }

        /// <summary>
        /// Catch events and do stuff.
        /// </summary>
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.InventoryEventType == MMInventoryEventType.ContentChanged || eventType.InventoryEventType == MMInventoryEventType.Redraw)
            {
                if (eventType.TargetInventoryName == targetInventory.name)
                {
                    JLog.Log(true, $"<i>OnMMEvent ►</i> [InventoryDisplaySlotRarity] > [{eventType.TargetInventoryName}] > [{eventType.InventoryEventType}]");
                    forceSlotRarityUpdate = true;
                }
            }
        }

        void LateUpdate()
        {
            if (forceSlotRarityUpdate)
            {
                UpdateDisplay();
                forceSlotRarityUpdate = false;
            }
        }

        /// <summary>
        /// On Enable, we start listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
            if (initialized) UpdateDisplay();
        }

        /// <summary>
        /// On Disable, we stop listening for MMInventoryEvents.
        /// </summary>
        protected virtual void OnDisable() => this.MMEventStopListening<MMInventoryEvent>();
    }
}