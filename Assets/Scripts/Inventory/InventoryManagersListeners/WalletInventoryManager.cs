using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using JButler;

namespace JayMountains
{
    public class WalletInventoryManager : MMSingleton<WalletInventoryManager>
    {
        public CurrencyItem GoldCurrency; // Reference.

        private Inventory walletInventory;


        //----------------------------------------------------------------
        /// <summary>
        /// Initialize.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            walletInventory = transform.GetComponentInChildren<Inventory>();
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Convenient method to add currency to wallet.
        /// </summary>
        /// <param name="currencyItem"></param>
        /// <param name="amount"></param>
        /// <param name="saveData">If you're adding a lot of items don't set this to true. Use the WalletInventoryManager.SaveInventory once all items have been added.</param>
        [Button(ButtonStyle.CompactBox, Expanded = true), BoxGroup("add", ShowLabel = false)]
        public void AddCurrency([ValueDropdown("CurrencyItemSO", FlattenTreeView = true)] CurrencyItem currencyItem, int amount, WalletInventoryEventTypes updateType = WalletInventoryEventTypes.AddFast, bool saveData = false)
        {
            bool success = walletInventory.AddItem(currencyItem, amount);
            JLog.Log(true, $"<b>Inventory (Wallet) ►</b> Adding {amount} {currencyItem.ItemID} success: {success}");

            switch (updateType)
            {
                case WalletInventoryEventTypes.AddFast: WalletInventoryEvent.Trigger(WalletInventoryEventTypes.AddFast, currencyItem.ItemID); break;
            }

            if (saveData) SaveData();
        }


        //----------------------------------------------------------------
        /// <summary>
        /// Convenient method to remove currency from wallet.
        /// </summary>
        /// <param name="currencyItem"></param>
        /// <param name="amount"></param>
        /// <param name="saveData">If you're removing a lot of items don't set this to true. Use the WalletInventoryManager.SaveInventory once all items have been removed.</param>
        [Button(ButtonStyle.CompactBox, Expanded = true), BoxGroup("remove", ShowLabel = false)]
        public void RemoveCurrency([ValueDropdown("CurrencyItemSO", FlattenTreeView = true)] CurrencyItem currencyItem, int amount, bool saveData = false)
        {
            if (walletInventory.GetQuantity(currencyItem.ItemID) < amount)
            {
                JLog.Log(true, $"<b>Inventory (Wallet) ►</b> You don't have enough currency for this transaction.");
                return;
            }

            bool success = walletInventory.RemoveItemByID(currencyItem.ItemID, amount);
            JLog.Log(true, $"<b>Inventory (Wallet) ►</b> Removing {amount} {currencyItem.ItemID} success: {success}");

            WalletInventoryEvent.Trigger(WalletInventoryEventTypes.RemoveFast, currencyItem.ItemID);

            if (saveData) SaveData();
        }


        //----------------------------------------------------------------
        public int GetCurrency(CurrencyItem currencyItem)
        {
            return walletInventory.GetQuantity(currencyItem.ItemID);
        }


        #region Local Saving & Loading + First Launch
        //----------------------------------------------------------------
        public void SaveInventory() => SaveData();


        //----------------------------------------------------------------
        public void LoadInventory() => LoadData();


        //----------------------------------------------------------------
        private void LoadFirstLaunchInventory()
        {
            transform.GetComponent<Lootbox>().GiveLootboxItems();
            JLog.Log(true, $"<b>Inventory (Wallet) ►</b> Loaded first launch items.");

            SaveData();
        }


        //----------------------------------------------------------------
        private void LoadData()
        {
            if (!ES3.FileExists() && GameDataManager.Instance.FirstLaunch != 1)
            {
                JLog.Log(true, $"<b>Inventory (Wallet) ►</b> Error - Local save does not exist.");
                return;
            }

            ES3Settings ES3Settings = GameDataManager.Instance.ES3Settings;

            if (!ES3.KeyExists(walletInventory.gameObject.name, ES3Settings))
            {
                JLog.Log(true, $"<b>Inventory (Wallet) ►</b> Save/Load not ran before. This is a new content/feature.");
                LoadFirstLaunchInventory();
                return;
            }

            SerializedInventory serializedInventory = ES3.Load<SerializedInventory>(walletInventory.gameObject.name, ES3Settings);
            walletInventory.ExtractSerializedInventory(serializedInventory);

            JLog.Log(true, $"<b>Inventory (Wallet) ►</b> Loaded from local save.");
        }


        //----------------------------------------------------------------
        private void SaveData()
        {
            if (!GameDataManager.Instance.EnableLocalSave) return;

            SerializedInventory serializedInventory = new SerializedInventory();
            walletInventory.FillSerializedInventory(serializedInventory);
            ES3.Save<SerializedInventory>(walletInventory.gameObject.name, serializedInventory);

            JLog.Log(true, $"<b>Inventory (Wallet) ►</b> Saved to local save.");
        }
        #endregion


        #region Odin
        //----------------------------------------------------------------
        // Odin stuff.
#if UNITY_EDITOR
        private static IEnumerable CurrencyItemSO()
        {
            return UnityEditor.AssetDatabase.FindAssets("t:CurrencyItem")
                .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
                .Select(x => new ValueDropdownItem(x, UnityEditor.AssetDatabase.LoadAssetAtPath<CurrencyItem>(x)));
        }
#endif
        #endregion
    }
}