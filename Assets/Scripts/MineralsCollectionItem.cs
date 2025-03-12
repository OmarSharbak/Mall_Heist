using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JayMountains;
using Sirenix.OdinInspector;

public class MineralsCollectionItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelIndex;
    [SerializeField] private TextMeshProUGUI labelName;
    [SerializeField] private Image icon;

    [SerializeField] private GameObject undecided;
    [SerializeField] private GameObject notInGame;

    [SerializeField, ReadOnly] private MineralData data;
    private bool discovered;


    //----------------------------------------------------------------
    public void SetData(MineralData _data)
    {
        data = _data;
        discovered = false;

        if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1] != null)
        {
            if (MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory.Content[data.Index - 1].Quantity > 0)
            {
                discovered = true;
            }
        }

        labelIndex.text = $"{data.Index.ToString("00")}";

        if (discovered)
        {
            labelName.text = $"{data.MineralItem.ItemName}";

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
            undecided.SetActive(true);
        }

        if (data.ChrisCheck == MineralData.ExcelState.NotInGame)
        {
            notInGame.SetActive(true);
        }
    }


    //----------------------------------------------------------------
    public void ViewDetails()
    {
        GameObject.FindFirstObjectByType<MineralsCollectionItemViewDetails>().ViewItem(data, discovered);
    }
}