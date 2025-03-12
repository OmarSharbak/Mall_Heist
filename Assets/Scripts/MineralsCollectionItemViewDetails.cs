using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

namespace JayMountains
{
    public class MineralsCollectionItemViewDetails : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelName;
        [SerializeField] private TextMeshProUGUI labelLocation;
        [SerializeField] private Image icon;

        [SerializeField] private GameObject starsGroup;
        [SerializeField] private Image[] stars;
        [SerializeField] private Sprite starOn;
        [SerializeField] private Sprite starOff;

        [SerializeField] private GameObject itemSelectedDisplay;
        [SerializeField] private GameObject nothingSelectedDisplay;

        [ShowInInspector, ReadOnly] private MineralData data;


        //----------------------------------------------------------------
        public void ViewItem(MineralData _data, bool _discovered)
        {
            data = _data;
            bool discovered = false;

            if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1] != null)
            {
                if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity > 0)
                {
                    discovered = true;
                }
            }

            if (discovered)
            {
                labelName.text = $"{data.MineralItem.ItemName}";
                //labelLocation.text = $"{data.MineralItem.Region}";
                string info = "";
                info += $"Location: {data.MineralItem.Region}\n";
                List<int> mItem;
                mItem = BagInventoryManager.Instance.Bag_Inventory.InventoryContains(data.MineralItem.ItemID);
                Debug.Log(data.MineralItem.ItemID);
                if (mItem.Count > 1) Debug.LogError("Shouldn't happen..");
                info += $"In Bag: {(mItem.Count == 0 ? "0" : BagInventoryManager.Instance.Bag_Inventory.Content[mItem[0]].Quantity)}\n";
                info += $"Total Collected: {MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity}\n";
                info += $"[Total Collected Perks]\n";

                if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity >= data.MineralItem.UpgradeCost[0])
                    info += $"<color=green>{data.MineralItem.UpgradeCost[0]} = {Regex.Replace(data.MineralItem.CollectionPerkUpgrades[0].UpgradeType.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")}</color>\n";
                else
                    info += $"<color=black>{data.MineralItem.UpgradeCost[0]} = {Regex.Replace(data.MineralItem.CollectionPerkUpgrades[0].UpgradeType.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")}</color>\n";

                if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity >= data.MineralItem.UpgradeCost[1])
                    info += $"<color=green>{data.MineralItem.UpgradeCost[1]} = {Regex.Replace(data.MineralItem.CollectionPerkUpgrades[1].UpgradeType.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")}</color>\n";
                else
                    info += $"<color=black>{data.MineralItem.UpgradeCost[1]} = {Regex.Replace(data.MineralItem.CollectionPerkUpgrades[1].UpgradeType.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")}</color>\n";

                if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity >= data.MineralItem.UpgradeCost[2])
                    info += $"<color=green>{data.MineralItem.UpgradeCost[2]} = {Regex.Replace(data.MineralItem.CollectionPerkUpgrades[2].UpgradeType.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")}</color>\n";
                else
                    info += $"<color=black>{data.MineralItem.UpgradeCost[2]} = {Regex.Replace(data.MineralItem.CollectionPerkUpgrades[2].UpgradeType.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")}</color>\n";

                if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity >= data.MineralItem.UpgradeCost[3])
                    info += $"<color=green>{data.MineralItem.UpgradeCost[3]} = {Regex.Replace(data.MineralItem.CollectionPerkUpgrades[3].UpgradeType.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")}</color>";
                else
                    info += $"<color=black>{data.MineralItem.UpgradeCost[3]} = {Regex.Replace(data.MineralItem.CollectionPerkUpgrades[3].UpgradeType.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0")}</color>";

                labelLocation.text = info;

                // Set 1 star.
                starsGroup.SetActive(true);
                for (int i = 0; i < stars.Length; i++)
                {
                    stars[i].sprite = starOff;
                    if (i == 0)
                    {
                        stars[i].sprite = starOn;
                    }
                }

                // Now set actual stars.
                if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity >= data.MineralItem.UpgradeCost[0]) stars[1].sprite = starOn;
                if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity >= data.MineralItem.UpgradeCost[1]) stars[2].sprite = starOn;
                if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity >= data.MineralItem.UpgradeCost[2]) stars[3].sprite = starOn;
                if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity >= data.MineralItem.UpgradeCost[3]) stars[4].sprite = starOn;

                if (data.MineralItem.Icon)
                {
                    icon.gameObject.SetActive(true);
                    icon.sprite = data.MineralItem.Icon;
                    icon.color = new Color(255, 255, 255);
                }
                else
                {
                    icon.gameObject.SetActive(false);
                }
            }
            else
            {
                labelName.text = $"Undiscovered...";
                labelLocation.text = $"Unknown.";

                starsGroup.SetActive(false);

                if (data.MineralItem.Icon)
                {
                    icon.gameObject.SetActive(true);
                    icon.sprite = data.MineralItem.Icon;
                    //icon.color = new Color(44, 55, 33);
                    icon.color = new Color(0, 0, 0);
                }
                else
                {
                    icon.gameObject.SetActive(false);
                }
            }

            if (data.ChrisCheck == MineralData.ExcelState.Undecided)
            {
                labelName.text = $"Check Excel.";
                labelLocation.text = $"Check Excel.";
            }

            if (data.ChrisCheck == MineralData.ExcelState.NotInGame)
            {
                labelName.text = $"Check Excel.";
                labelLocation.text = $"Check Excel.";
            }

            SetSelectionDisplay();
        }


        //----------------------------------------------------------------
        private void ClearSelectionDisplay()
        {
            itemSelectedDisplay.gameObject.SetActive(false);
            nothingSelectedDisplay.gameObject.SetActive(true);
        }


        //----------------------------------------------------------------
        private void SetSelectionDisplay()
        {
            itemSelectedDisplay.gameObject.SetActive(true);
            nothingSelectedDisplay.gameObject.SetActive(false);
        }


        //----------------------------------------------------------------
        private void OnEnable()
        {
            ClearSelectionDisplay();
        }
    }
}