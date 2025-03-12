using System.Collections.Generic;
using UnityEngine;
using JayMountains;
using Sirenix.OdinInspector;

public class MineralsCollectionDisplay : MonoBehaviour
{
    public RegionMineralsData RegionMineralsData;

    [SerializeField] private GameObject content;
    [SerializeField] private GameObject mineralItemPrefab;


    //----------------------------------------------------------------
    private void OnEnable()
    {
        CreateDisplayOnRuntimeOrEditor();
    }


    //----------------------------------------------------------------
    [Button]
    private void CreateDisplayOnRuntimeOrEditor()
    {
        // ----------
        // Clear.

#if UNITY_EDITOR
        while (content.transform.childCount != 0)
        {
            DestroyImmediate(content.transform.GetChild(0).gameObject);
        }
#else
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
#endif

        // ----------
        // Create.
        for (int i = 0; i < RegionMineralsData.Minerals.Count; i++)
        {
            // Instantiate the mineral items.
            GameObject item = null;
#if UNITY_EDITOR
            item = UnityEditor.PrefabUtility.InstantiatePrefab(mineralItemPrefab, content.transform) as GameObject;
#else
            item = Instantiate(mineralItemPrefab, content.transform) as GameObject;
#endif
            // Set the mineral item data.
            item.name = $"{RegionMineralsData.Minerals[i].MineralItem.ItemName}";
            MineralsCollectionItem itemData = item.transform.GetComponent<MineralsCollectionItem>();
            itemData.SetData(RegionMineralsData.Minerals[i]);
        }
    }
}