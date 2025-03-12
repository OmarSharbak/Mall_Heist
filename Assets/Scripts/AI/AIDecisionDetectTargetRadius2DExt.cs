using System;
using UnityEngine;
using UnityEngine.Events;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

public class AIDecisionDetectTargetRadius2DExt : AIDecisionDetectTargetRadius2D
{
    [Serializable]
    public class OnDetectEnterEvent : UnityEvent { }
    [SerializeField]
    public OnDetectEnterEvent onDetectEnterEvent = new OnDetectEnterEvent();

    [Serializable]
    public class OnDetectEnterExit : UnityEvent { }
    [SerializeField]
    public OnDetectEnterExit onDetectEnterExit = new OnDetectEnterExit();


    //----------------------------------------------------------------
    public override bool Decide()
    {
        bool detect = DetectTarget();

        if (detect)
        {
            if (this.GetComponent<AIBrain>().CurrentState.StateName == "Detecting")
                onDetectEnterEvent.Invoke();
            return true;
        }

        return false;
    }


    //----------------------------------------------------------------
    public override void OnExitState()
    {
        base.OnExitState();
        onDetectEnterExit.Invoke();
    }
}