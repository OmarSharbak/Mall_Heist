using UnityEngine;
using UnityEngine.Events;

namespace SweatyChair.StateManagement
{

    public static class StateManager
    {

        public static event UnityAction<State> stateChanged;

        public static State currentState { get; private set; } = State.None;
        public static State lastState { get; private set; } = State.None;

        public static void Set(State newState)
        {
            if (currentState == newState)
                return;

            //if (StateSettings.current.debugMode)
            //    Debug.LogFormat("StateManager:Set({0})", newState);

            lastState = currentState;
            currentState = newState;

            stateChanged?.Invoke(currentState);
        }

        public static void SetAsLast()
        {
            Set(lastState);
        }

        public static State Get()
        {
            return currentState;
        }

        // Returns true if the state matches current state
        public static bool Compare(State checkState)
        {
            return checkState == currentState;
        }

        public static bool Compare(params State[] checkStates)
        {
            foreach (State state in checkStates)
            {
                if (state == currentState)
                    return true;
            }
            return false;
        }

        // Returns true if the state mask includes current state
        public static bool Compare(int checkStateMask)
        {
            return (checkStateMask & (1 << (int)currentState)) != 0;
        }

        // Returns true if the state matches last state
        public static bool CompareLast(State checkState)
        {
            return checkState == lastState;
        }

        // Returns true if the state mask includes current state
        public static bool CompareLast(int checkStateMask)
        {
            return (checkStateMask & (1 << (int)lastState)) != 0;
        }

#if UNITY_EDITOR

		[UnityEditor.MenuItem("Debug/State/Print Current State", false, 100)]
		public static void PrintCurrentState()
		{
			Debug.Log(currentState);
		}

		[UnityEditor.MenuItem("Debug/State/Print Last State", false, 100)]
		public static void PrintLastState()
		{
			Debug.Log(lastState);
		}

		[UnityEditor.MenuItem("Debug/State/Reset State", false, 100)]
		public static void ResetState()
		{
			Set(State.None);
		}

#endif

    }

    // TODO: to be moved to StateSettings so no hard code is needed across different games
    public enum State
    {
        None,           // Default state
        Logo,           // Developer logos at app launch
        SplashScreen,   // Showing game splash/loading screen at app launch
        Intro,          // A story intro, normally only show once at first launch
        Tutorial,       // Playing tutorial
        Menu,           // Showing main menu
        LogIn,          // Showing login panel, for some game only
        Shop,           // Showing shop panel
        PreGame,        // State or screen before entering game, e.g. picking starting cards
        Game,           // In a game
        PauseGame,      // Pausing a game
        EndGame,        // Game end, e.g. ending score board
        Continue,       // Revive to continue
        Quitting,       // WHen quiting the app, use this to make sure no core logic is running at this stage
        Message,        // A message box shown
        Editor,         // In a editor, only for app having an editor, e.g. Model Editor in Block42
        LevelSelect,    // Selecting level, e.g. selecting a world in Block42
        LevelSave,      // Saving a level, e.g. saving a world in Block42
        Inventory,      // Inventory e.g. card collection
        InventoryDraw,  // Inventory draw e.g. card machine
        Gadget,         // Gedget e.g. tech tree
        Setting,        // Setting panel
        Leaderboards,   // In-game leaderboard
        Achievements,   // In-game achievements
        DailyTask,      // Daily tasks
        Sharing,        // Sharing panel
        HouseAd,        // House ads
    }

}