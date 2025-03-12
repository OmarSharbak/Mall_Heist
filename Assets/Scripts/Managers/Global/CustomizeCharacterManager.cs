using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using JButler;

namespace JayMountains
{
    public class CustomizeCharacterManager : MMSingleton<CustomizeCharacterManager>
    {
        private bool _isMale;
        public bool IsMale { get { return _isMale; } }


        //----------------------------------------------------------------
        public void SetGender(bool isMale)
        {
            _isMale = isMale;
            SaveData();
        }


        #region Local Saving & Loading + First Launch
        //----------------------------------------------------------------
        public void Save() => SaveData();


        //----------------------------------------------------------------
        public void Load() => LoadData();


        //----------------------------------------------------------------
        private void LoadFirstLaunch()
        {
            _isMale = true;
            JLog.Log(true, $"<b>Customize Character ►</b> Loaded first launch items.");

            SaveData();
        }


        //----------------------------------------------------------------
        private void LoadData()
        {
            if (!ES3.FileExists() && GameDataManager.Instance.FirstLaunch != 1)
            {
                JLog.Log(true, $"<b>Customize Character ►</b> Error - Local save does not exist.");
                return;
            }

            ES3Settings ES3Settings = GameDataManager.Instance.ES3Settings;

            if (!ES3.KeyExists("IsMale", ES3Settings))
            {
                JLog.Log(true, $"<b>Customize Character ►</b> Save/Load not ran before. This is a new content/feature.");
                LoadFirstLaunch();
                return;
            }

            _isMale = ES3.Load<bool>("IsMale", ES3Settings);

            JLog.Log(true, $"<b>Customize Character ►</b> Loaded from local save.");
        }


        //----------------------------------------------------------------
        private void SaveData()
        {
            if (!GameDataManager.Instance.EnableLocalSave) return;

            ES3.Save<bool>("IsMale", _isMale);

            JLog.Log(true, $"<b>Customize Character ►</b> Saved to local save.");
        }
        #endregion
    }
}