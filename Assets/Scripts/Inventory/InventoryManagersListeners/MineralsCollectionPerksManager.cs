using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using JButler;

namespace JayMountains
{
    public class MineralsCollectionPerksManager : MMSingleton<MineralsCollectionPerksManager>
    {
        public Inventory MineralsCollectionPerks_Inventory { get; private set; }


        //----------------------------------------------------------------
        /// <summary>
        /// Initialize.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            MineralsCollectionPerks_Inventory = transform.GetComponentInChildren<Inventory>();
        }


        //----------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="saveData">If you're adding a lot of items don't set this to true. Use the SaveInventory once all items have been added.</param>
        [Button(ButtonStyle.CompactBox, Expanded = true), BoxGroup("add", ShowLabel = false)]
        public void AddMineral(InventoryItem item, int quantity = 1, bool saveData = false)
        {
            bool added = MineralsCollectionPerks_Inventory.AddItemAt(item, quantity, item.TargetIndex);

            if (!added)
            {
                JLog.Log(true, $"<b>Inventory (Minerals Collection Perks) ►</b> Failed to add.");
                return;
            }

            JLog.Log(true, $"<b>Inventory (Minerals Collection Perks) ►</b> Added: x{quantity} {item.ItemID}");
            if (saveData) SaveData();
        }


        #region Local Saving & Loading + First Launch
        //----------------------------------------------------------------
        public void SaveInventory() => SaveData();


        //----------------------------------------------------------------
        public void LoadInventory() => LoadData();


        //----------------------------------------------------------------
        private void LoadFirstLaunchInventory()
        {
            JLog.Log(true, $"<b>Inventory (Minerals Collection Perks) ►</b> Loaded first launch items.");

            SaveData();
        }


        //----------------------------------------------------------------
        private void LoadData()
        {
            if (!ES3.FileExists() && GameDataManager.Instance.FirstLaunch != 1)
            {
                JLog.Log(true, $"<b>Inventory (Minerals Collection Perks) ►</b> Error - Local save does not exist.");
                return;
            }

            ES3Settings ES3Settings = GameDataManager.Instance.ES3Settings;

            if (!ES3.KeyExists(MineralsCollectionPerks_Inventory.gameObject.name, ES3Settings))
            {
                JLog.Log(true, $"<bInventory (Minerals Collection Perks) ►</b> Save/Load not ran before. This is a new content/feature.");
                LoadFirstLaunchInventory();
                return;
            }

            SerializedInventory serializedInventory;

            serializedInventory = ES3.KeyExists(MineralsCollectionPerks_Inventory.gameObject.name, ES3Settings) == true ? ES3.Load<SerializedInventory>(MineralsCollectionPerks_Inventory.gameObject.name, ES3Settings) : null;
            MineralsCollectionPerks_Inventory.ExtractSerializedInventory(serializedInventory);

            JLog.Log(true, $"<b>Inventory (Minerals Collection Perks) ►</b> Loaded from local save.");
        }


        //----------------------------------------------------------------
        private void SaveData()
        {
            if (!GameDataManager.Instance.EnableLocalSave) return;

            SerializedInventory serializedInventory = new SerializedInventory();

            MineralsCollectionPerks_Inventory.FillSerializedInventory(serializedInventory);
            ES3.Save<SerializedInventory>(MineralsCollectionPerks_Inventory.gameObject.name, serializedInventory);

            JLog.Log(true, $"<b>Inventory (Minerals Collection Perks) ►</b> Saved to local save.");
        }
        #endregion
    }
}