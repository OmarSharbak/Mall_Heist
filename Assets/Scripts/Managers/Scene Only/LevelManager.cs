using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProgressionSystem.Scripts.Variables;
using MoreMountains.TopDownEngine;

public class LevelManager : MoreMountains.TopDownEngine.LevelManager
{
    [SerializeField] private IntVariable floors;
    [SerializeField] private IntVariable north;
    [SerializeField] private IntVariable south;
    [SerializeField] private IntVariable east;
    [SerializeField] private IntVariable west;

    [SerializeField] private GameObject gameoverPanel;

    protected override void Start()
    {
        base.Start();
        floors.Value = 0;
        north.Value = 0;
        south.Value = 0;
        east.Value = 0;
        west.Value = 0;
    }

    /// <summary>
    /// Button click.
    /// </summary>
    public void LeaveMines()
    {
        EnergySystem.Instance.data.CurrentEnergy = Players[0].CharacterHealth.CurrentHealth;
        ScenesManager.Instance.ProceedToMainMenu();
    }

    /// <summary>
    /// Catches TopDownEngineEvents and acts on them.
    /// </summary>
    public override void OnMMEvent(TopDownEngineEvent engineEvent)
    {
        base.OnMMEvent(engineEvent);
        switch (engineEvent.EventType)
        {
            case TopDownEngineEventTypes.PlayerDeath:
                gameoverPanel.SetActive(true);
                break;

            case TopDownEngineEventTypes.LevelComplete:

                break;
        }
    }
}