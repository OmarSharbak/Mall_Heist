using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using JButler;
using JayMountains;

public class EnergySystem : MMSingleton<EnergySystem>
{
    private const string ES3_EnergySystem_KeyName = "EnergySystem";
    public EnergySystemData data = new EnergySystemData();

    [BoxGroup("Test")] public bool Test = true;
    [BoxGroup("Test")] public float Period = 0.1f;
    [BoxGroup("Test")] public float Damage = 1;

    public bool CanRegen = false;
    public float RegenInterval = 0.1f;
    public float RegenAmount = 1;
    public bool FullHealth = false;

    private float nextActionTime = 0.0f;
    private Health health;

    // Store the scene that should trigger Start.
    private Scene scene;


    //----------------------------------------------------------------
    protected override void Awake()
    {
        // It is safe to remove listeners even if they didn't exist so far.
        // This makes sure it is added only once.
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Add the listener to be called when a scene is loaded.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    //----------------------------------------------------------------
    private void Update()
    {
        if (health == null) return;

        if (Test)
        {
            if (Time.time > nextActionTime)
            {
                nextActionTime = Time.time + Period;
                // execute block of code here
                health.Damage(Damage, this.gameObject, 0, 0, Vector3.zero);
                Debug.Log($"health {health.CurrentHealth}");
                data.CurrentEnergy = health.CurrentHealth;
            }
        }

        if (!Test && CanRegen)
        {
            if (Time.time > nextActionTime)
            {
                nextActionTime = Time.time + RegenInterval;
                if (health.CurrentHealth < health.MaximumHealth)
                {
                    health.ReceiveHealth(RegenAmount, this.gameObject);
                    FullHealth = false;
                }
                else
                {
                    FullHealth = true;
                }
                data.CurrentEnergy = health.CurrentHealth;
            }
        }
    }


    //----------------------------------------------------------------
    private void OnDestroy()
    {
        // Always clean up your listeners when not needed anymore.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    //----------------------------------------------------------------
    // Listener for sceneLoaded.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Re-Initializing", this);

        // Do your "Start" stuff here.
        health = null;
        StartCoroutine(WaitForEndOfFrame());
    }


    //----------------------------------------------------------------
    IEnumerator WaitForEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");

        //Debug.Log($"Re-... {components.Length}", this);
        if (player.Length > 1)
        {
            Debug.LogError("Shouldn't happen..");
        }
        if (player.Length == 1)
        {
            Character component = player[0].GetComponent<Character>();
            health = component.GetComponent<Health>();

            if (ScenesManager.Instance.GameRoomScene.SceneName == SceneManager.GetActiveScene().name)
            {
                if (data.CollectionPerksBoosted == CollectionPerksBoostManager.Instance.TotalEnergyIncreaseValue)
                {
                    health.CurrentHealth = data.CurrentEnergy;
                }
                else if (data.CollectionPerksBoosted < CollectionPerksBoostManager.Instance.TotalEnergyIncreaseValue)
                {
                    health.CurrentHealth = data.CurrentEnergy + (CollectionPerksBoostManager.Instance.TotalEnergyIncreaseValue - data.CollectionPerksBoosted);
                }
                else if (data.CollectionPerksBoosted > CollectionPerksBoostManager.Instance.TotalEnergyIncreaseValue)
                {
                    health.CurrentHealth = Mathf.Clamp(data.CurrentEnergy, 0, 100 + CollectionPerksBoostManager.Instance.TotalEnergyIncreaseValue);
                }

                health.InitialHealth = 100 + CollectionPerksBoostManager.Instance.TotalEnergyIncreaseValue;
                health.MaximumHealth = 100 + CollectionPerksBoostManager.Instance.TotalEnergyIncreaseValue;

                data.CollectionPerksBoosted = CollectionPerksBoostManager.Instance.TotalEnergyIncreaseValue;
            }
            else
            {
                health.CurrentHealth = data.CurrentEnergy;
            }

            health.InitialHealth = 100 + data.CollectionPerksBoosted;
            health.MaximumHealth = 100 + data.CollectionPerksBoosted;
            health.UpdateHealthBar(true);
        }
    }


    //----------------------------------------------------------------
    [Button]
    private void CheckCharactersInSceneTest()
    {
        Character[] components = GameObject.FindObjectsOfType<Character>();
        Debug.Log($"{components.Length}", this);
    }


    //----------------------------------------------------------------
    public void RegenToggle(bool toggle)
    {
        if (toggle == true)
        {
            Test = false;
            CanRegen = true;
        }
        else
        {
            Test = false;
            CanRegen = false;
        }
    }


    #region Local Saving & Loading + First Launch
    //----------------------------------------------------------------
    public void Save() => SaveData();


    //----------------------------------------------------------------
    public void Load() => LoadData();


    //----------------------------------------------------------------
    private void LoadFirstLaunchSettings()
    {
        JLog.Log(true, $"<b>Energy System ►</b> Loaded first launch settings.");
        data.CurrentEnergy = 100;
        SaveData();
    }


    //----------------------------------------------------------------
    private void LoadData()
    {
        if (!ES3.FileExists() && GameDataManager.Instance.FirstLaunch != 1)
        {
            JLog.Log(true, $"<b>Energy System ►</b> Error - Local save does not exist.");
            return;
        }

        ES3Settings ES3Settings = GameDataManager.Instance.ES3Settings;

        if (!ES3.KeyExists(ES3_EnergySystem_KeyName, ES3Settings))
        {
            JLog.Log(true, $"<b>Energy System ►</b> Save/Load not ran before. This is a new content/feature.");
            LoadFirstLaunchSettings();
            return;
        }

        data = ES3.Load<EnergySystemData>(ES3_EnergySystem_KeyName, ES3Settings);

        JLog.Log(true, $"<b>Energy System ►</b> Loaded from local save.");
    }


    //----------------------------------------------------------------
    private void SaveData()
    {
        if (!GameDataManager.Instance.EnableLocalSave) return;

        ES3.Save<EnergySystemData>(ES3_EnergySystem_KeyName, data);

        JLog.Log(true, $"<b>Energy System ►</b> Saved to local save.");
    }
    #endregion


    //----------------------------------------------------------------
#if UNITY_IOS || UNITY_EDITOR
    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            //EnterApplication();

        }
        else
        {
            //ExitApplication();
            Save();
        }
    }
#endif


    //----------------------------------------------------------------
#if UNITY_ANDROID || UNITY_EDITOR
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            //ExitApplication();
            Save();
        }
        else
        {
            //EnterApplication();

        }
    }
#endif
}


//----------------------------------------------------------------
//----------------------------------------------------------------
[Serializable]
public class EnergySystemData
{
    public float CurrentEnergy = 100; // Default is 100.

    // To check if the saved energy has any boosted values applied. There are 3 scenarios.
    // 1. If the boosted value is the same as this property:
    //    > No additional energy will be added.
    // 2. If the new boosted value is more than this property:
    //    > It will be added as extra (regenerated) enegry and saved so we know it has been applied.
    // 3. If the new boosted value is lesser than this property:
    //    > Don't do anything to the (regenerated) enegry. Just cap it at the new maximum and save it.
    public float CollectionPerksBoosted = 0;
}