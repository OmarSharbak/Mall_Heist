using System;
using UnityEngine;
using Sirenix.OdinInspector;
using JButler;
using JayMountains;

public class GameDataManager : PersistentSingleton<GameDataManager>
{
    /// <summary>
    /// Game is launched for the first time after fresh installation.
    /// </summary>
    [ReadOnly] public int FirstLaunch = 1;

    public ES3Settings ES3Settings { get; private set; }
    public bool EnableLocalSave = true;

    protected override void Awake()
    {
        base.Awake();
        CheckFirstLaunch();
    }

    private void CheckFirstLaunch()
    {
        // Check if this is a first launch.
        FirstLaunch = PlayerPrefs.GetInt("FirstLaunch", 1);

        if (FirstLaunch == 1)
        {
            JLog.Log(true, $"<b>GameDataManager ►</b> Game newly installed launch.");
        }
        else
        {
            JLog.Log(true, $"<b>GameDataManager ►</b> Game standard launch.");
        }
    }

    private void SetupEasySaveSettings()
    {
        // Change save file name.
        switch (LootLockerPlayerAuth.Instance.SelectedLoginOption)
        {
            case LoginOptions.Guest:
                ES3Settings.defaultSettings.path = "GUE.es3";
                break;
        }

        // Apply defaults as set within the inspector Runtime Settings of EasySave3.
        ES3Settings = new ES3Settings(true);

        if (ES3.FileExists())
        {
            try
            {
                ES3.KeyExists("test", ES3Settings);
            }
            catch (Exception e1)
            {
                Debug.Log($"First catch. An Exception of '{e1}' occurred.");

                // If we can't load it with encryption, either the data isn't encrypted, the password is wrong, or the data is corrupt.
                // Try loading without encryption to see whether the data is encrypted.
                try
                {
                    ES3.KeyExists("test", new ES3Settings(ES3.EncryptionType.None));

                    // If the KeyExists check fails, the try-block ends immediately and execution moves on to the catch block.
                    // Resave data with encryption.
                    byte[] saveDataBytes = ES3.LoadRawBytes(new ES3Settings(ES3.EncryptionType.None));
                    ES3.SaveRaw(saveDataBytes, ES3Settings);
                    Debug.Log($"First catch. Fixed.");
                }
                catch (Exception e2)
                {
                    Debug.Log($"Second catch. An Exception of '{e2}' occurred.");

                    // This means that the issue is that the password was wrong, or the data is corrupted.
                    // It's impossible to know which is the case as this would make it much easier for hackers to crack the password.
                    // In this case, just throw an error to the user saying that the password is incorrect or the save file is invalid.
                    // Most likely the password is incorrect.

#if UNITY_EDITOR
                    // Maybe the file is encrypted but we are trying to open it without encryption.
                    // This should only be the case if developing within Unity.
                    try
                    {
                        ES3.KeyExists("test", new ES3Settings(ES3.EncryptionType.AES, "b0OmboOmpow"));

                        // If the KeyExists check fails, the try-block ends immediately and execution moves on to the catch block.
                        // Resave data without encryption.
                        byte[] saveDataBytes = ES3.LoadRawBytes(new ES3Settings(ES3.EncryptionType.AES, "b0OmboOmpow"));
                        ES3.SaveRaw(saveDataBytes, ES3Settings);
                        Debug.Log($"Second catch. Fixed.");
                    }
                    catch (Exception e3)
                    {
                        Debug.Log($"Third catch. An Exception of '{e3}' occurred.");
                    }
#endif
                }
            }
        }
        else
        {
            ES3.Save<string>("DateCreated", $"yyyy/MM/dd hh:mm:ss");
        }
    }

    public void SetupGameData()
    {
        SetupEasySaveSettings();
    }

    public void InitializeGameData()
    {
        BagInventoryManager.Instance.LoadInventory();
        WalletInventoryManager.Instance.LoadInventory();
        CustomizeCharacterManager.Instance.Load();
        MineralsCollectionPerksManager.Instance.LoadInventory();
        EnergySystem.Instance.Load();
    }
}