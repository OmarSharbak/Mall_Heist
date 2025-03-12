using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using MoreMountains.InventoryEngine;

namespace JayMountains
{
    public class ViewInventoryItem : MonoBehaviour
    {
        [SerializeField] private Image slotImage;
        [SerializeField] private Image slotItemImage;
        [SerializeField] private Sprite slotImageCommon;
        [SerializeField] private Sprite slotImageUncommon;
        [SerializeField] private Sprite slotImageRare;
        [SerializeField] private Sprite slotImageEpic;

        [SerializeField] private TextMeshProUGUI itemRarity;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemDesc;

        [SerializeField] private GameObject itemUseButton;

        private InventorySlot _inventorySlot;
        private Inventory _inventory;
        private int _itemIndex;


        //----------------------------------------------------------------
        public void ViewItem(InventorySlot inventorySlot, Inventory inventory, int itemIndex, bool isUnequip = false)
        {
            _inventorySlot = inventorySlot;
            _inventory = inventory;
            _itemIndex = itemIndex;

            if (_inventory.Content[_itemIndex] == null) return;

            inventorySlot.GetComponent<PlayButtonSFX>().PlayButtonSound();

            this.gameObject.SetActive(true);

            BagItem item = _inventory.Content[_itemIndex] as BagItem;
            SetRarityUI(item);
            slotItemImage.sprite = item.Icon;
            itemName.text = item.ItemName;
            itemDesc.text = item.ShortDescription;

            itemUseButton.SetActive(false);
            // Show 'USE' button for usable/interactable items in the game room.
            if (SceneManager.GetActiveScene().name == ScenesManager.Instance.GameRoomScene.SceneName)
            {
                if (item.Usable) itemUseButton.SetActive(true);
                else itemUseButton.SetActive(false);
            }
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Button click.
        /// </summary>
        public void UseItem()
        {
            if (!_inventorySlot.Usable() && !itemUseButton.activeInHierarchy)
                return;

            _inventorySlot.Use();
            BagInventoryManager.Instance.SaveInventory();
        }


        //----------------------------------------------------------------
        private void SetRarityUI(InventoryItem item)
        {
            switch (CheckItemRarityGrade.GetRarity(item))
            {
                case RarityGrades.Common:
                    itemRarity.text = "Common";
                    slotImage.sprite = slotImageCommon;
                    break;

                case RarityGrades.Uncommon:
                    itemRarity.text = "Uncommon";
                    slotImage.sprite = slotImageUncommon;
                    break;

                case RarityGrades.Rare:
                    itemRarity.text = "Rare";
                    slotImage.sprite = slotImageRare;
                    break;

                case RarityGrades.Epic:
                    itemRarity.text = "Epic";
                    slotImage.sprite = slotImageEpic;
                    break;
            }
        }
    }
}