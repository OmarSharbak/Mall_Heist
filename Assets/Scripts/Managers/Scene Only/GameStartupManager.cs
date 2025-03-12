using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JButler;
using MEC;

public class GameStartupManager : Singleton<GameStartupManager>
{
    public enum StartupFailedError
    {
        NoInternet,
        PrivacyPolicyDeclined,
    }

    public string LoginFailedInfo = "";

    [SerializeField] private TextMeshProUGUI textLoading = null;
    [SerializeField] private GameObject privacyPolicyWindow = null;
    [SerializeField] private GameObject loginOptionWindow = null;
    [SerializeField] private GameObject loginGuestButton = null;
    [SerializeField] private TextMeshProUGUI textGameBuildVersion;

    private bool FreshLaunch { get => GameDataManager.Instance.FirstLaunch == 1; }
    private int privacyPolicyOutcome = -1;


    //----------------------------------------------------------------
    protected override void Awake()
    {
        base.Awake();
        textGameBuildVersion.text = $"v{Application.version} ({GameVersion.Android_BuildNumber})";
    }


    //----------------------------------------------------------------
    private void Start()
    {
        Optimize();
        Timing.RunCoroutine(Startup());
    }


    //----------------------------------------------------------------
    private void Optimize()
    {
        Application.targetFrameRate = 60;
    }


    //----------------------------------------------------------------
    private IEnumerator<float> Startup()
    {
        //------------------------------------------
        // -- INTERNET CONNECTION
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Checking Internet connectivity..."), Segment.SlowUpdate));
        if (Application.internetReachability == NetworkReachability.NotReachable) { ThrowStartupFailedError(StartupFailedError.NoInternet); yield break; }
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Internet connection: OK.", 0.25f), Segment.SlowUpdate));


        //------------------------------------------
        // -- FIRST-TIME LAUNCH : PRIVACY POLICY
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Checking First-time launch..."), Segment.SlowUpdate));
        if (FreshLaunch)
        {
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("First-time launch: YES."), Segment.SlowUpdate));

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Waiting for Privacy Policy outcome..."), Segment.SlowUpdate));
            OpenPrivacyPolicyWindow();
            while (privacyPolicyOutcome == -1) yield return Timing.WaitForOneFrame;
            if (privacyPolicyOutcome == 0) { ThrowStartupFailedError(StartupFailedError.PrivacyPolicyDeclined); yield break; }
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Privacy Policy: ACCEPTED.", 0.5f), Segment.SlowUpdate));
        }
        else
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("First-time launch: NO.", 0.5f), Segment.SlowUpdate));


        //------------------------------------------
        // -- LOGIN SELECTION
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Creating a session..."), Segment.SlowUpdate));
        if (FreshLaunch)
        {
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("No preferred login option found."), Segment.SlowUpdate));
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Waiting for selection..."), Segment.SlowUpdate));
            ToggleLoginOptionWindow(true);

            while (LootLockerPlayerAuth.Instance.SelectedLoginOption == LoginOptions.None) yield return Timing.WaitForOneFrame;
            AlertPopupsManager.Instance.BlockInput(true);
            switch (LootLockerPlayerAuth.Instance.SelectedLoginOption)
            {
                // -- LOOTLOCKER GUEST LOGIN
                case LoginOptions.Guest:
                    yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Established a session: OK.", 0.75f), Segment.SlowUpdate));
                    PlayerPrefs.SetInt("PreferredLoginOption", (int)LoginOptions.Guest);
                    break;
            }

            AlertPopupsManager.Instance.BlockInput(false);
            ToggleLoginOptionWindow(false);
        }
        else
        {
            LootLockerPlayerAuth.Instance.SelectedLoginOption = (LoginOptions)PlayerPrefs.GetInt("PreferredLoginOption", 0);
            if (LootLockerPlayerAuth.Instance.SelectedLoginOption != 0)
            {
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Preferred login option found."), Segment.SlowUpdate));

                AlertPopupsManager.Instance.BlockInput(true);
                switch (LootLockerPlayerAuth.Instance.SelectedLoginOption)
                {
                    // -- LOOTLOCKER GUEST LOGIN
                    case LoginOptions.Guest:

                        break;
                }
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Established a session: OK.", 0.75f), Segment.SlowUpdate));

                AlertPopupsManager.Instance.BlockInput(false);
            }
        }


        //------------------------------------------
        // -- GAME DATA
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Setup GameData..."), Segment.SlowUpdate));
        GameDataManager.Instance.SetupGameData();

        yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Loading GameData..."), Segment.SlowUpdate));
        GameDataManager.Instance.InitializeGameData();

        yield return Timing.WaitUntilDone(Timing.RunCoroutine(Loading("Loading: DONE.", 1f), Segment.SlowUpdate));


        //------------------------------------------
        // Finally, lets load the next scene!
        yield return Timing.WaitForSeconds(1f);
        if (FreshLaunch)
        {
            // First launch only happens once.
            PlayerPrefs.SetInt("FirstLaunch", 0);
        }
        ScenesManager.Instance.ProceedToMainMenu();
    }


    //----------------------------------------------------------------
    private void ThrowStartupFailedError(StartupFailedError error)
    {
        switch (error)
        {
            case StartupFailedError.NoInternet:
                Message("Internet connection:\n***** NO INTERNET CONNECTION *****");
                AlertPopupsManager.Instance.ToggleNetworkErrorAlert(true);
                break;

            case StartupFailedError.PrivacyPolicyDeclined:
                Message("Privacy Policy:\n***** DECLINED *****");
                break;
        }
    }


    //----------------------------------------------------------------
    /// <summary>
    /// Show the Privacy Policy popup window.
    /// </summary>
    private void OpenPrivacyPolicyWindow()
    {
        privacyPolicyWindow.SetActive(true);
    }


    //----------------------------------------------------------------
    /// <summary>
    /// Button - Privacy Policy popup window user selection options.
    ///       -1 = Waiting for user input.
    /// <br/> 0  = Declined.
    /// <br/> 1  = Accepted.
    /// </summary>
    public void PrivacyPolicyDecision(int outcome)
    {
        privacyPolicyOutcome = outcome;

        // Declined.
        if (privacyPolicyOutcome == 0)
        {
            //ES3.Save<bool>("firstLaunch", true);
            Application.Quit();
        }

        // Accepted.
        if (privacyPolicyOutcome == 1)
        {
            privacyPolicyWindow.SetActive(false);
        }
    }


    //----------------------------------------------------------------
    /// <summary>
    /// Toggle the Login Option window.
    /// </summary>
    private void ToggleLoginOptionWindow(bool toggle)
    {
        loginOptionWindow.SetActive(toggle);
    }

    public void SelectGuestLogin() => LootLockerPlayerAuth.Instance.SelectedLoginOption = LoginOptions.Guest;


    //----------------------------------------------------------------
    /// <summary>
    /// Progress of startup loading.
    /// </summary>
    /// <param name="context">Message of the current loading progress process shown both in-game and as debug log.</param>
    /// <param name="progressBarFillAmount">SceneLoading progress bar fill amount.</param>
    private IEnumerator<float> Loading(string context, float progressBarFillAmount = 0)
    {
        if (progressBarFillAmount != 0) ScenesManager.Instance.ProgressBarUpdate(progressBarFillAmount);
        Message(context);
        yield return Timing.WaitForOneFrame;
    }


    //----------------------------------------------------------------
    /// <summary>
    /// Message of the current loading progress process shown both in-game and as debug log.
    /// </summary>
    private void Message(string context)
    {
        textLoading.text = context;
        JLog.Log(true, $"<b>Startup ►</b> {context}");
    }
}