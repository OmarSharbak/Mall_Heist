using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedLabsGames.Tools.QuickAnim;


public static class QuickAnimExtenstions
{

    /// <summary>
    /// Play a clip on QuickAnimComponent if it exists.
    /// </summary>
    /// <param name="go">Target Gameobject</param>
    /// <param name="name">Clip name to play</param>
    public static void PlayQuickAnim(this GameObject go,string name)
    {
        QuickAnimComponent component = go.GetComponent<QuickAnimComponent>();

        if (component == null)
        {
            throw new System.Exception("Can't find Quick Anim component on GameObject (" + go.name + ")");
        }

        component.Play(name);
    }


    /// <summary>
    /// CrossFade to a clip on QuickAnimComponent if it exists.
    /// </summary>
    /// <param name="go">Target GameObject</param>
    /// <param name="name">Clip Name to CrossFade</param>
    /// <param name="crossFadeTime">CrossFade Duration</param>
    public static void CrossFadeQuickAnim(this GameObject go, string name,float crossFadeTime=1)
    {
        QuickAnimComponent component = go.GetComponent<QuickAnimComponent>();

        if (component == null)
        {
            throw new System.Exception("Can't find Quick Anim component on GameObject (" + go.name + ")");
        }

        component.CrossFade(name,crossFadeTime);
    }


    /// <summary>
    /// Returns the Quick Anim component, null if it doesn't. 
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static QuickAnimComponent GetQuickAnimComponent(this GameObject go)
    {
        QuickAnimComponent component = go.GetComponent<QuickAnimComponent>();

        if (component==null)
        {
            Debug.LogError("Can't find Quick Anim component on GameObject (" + go.name + ")");
            return null;
        }

        return go.GetComponent<QuickAnimComponent>();
    }


    /// <summary>
    /// Invoke an event on Quick Anim Unity Event if it exists.
    /// </summary>
    /// <param name="go">Target Gameobject</param>
    /// <param name="eventName">Event name to invoke</param>
    public static void InvokeQuickAnimEvent(this GameObject go, string eventName)
    {
        QuickAnimUnityEvent component = go.GetComponent<QuickAnimUnityEvent>();

        if (component == null) {
            throw new System.Exception("Can't find Quick Anim Unity Event Component on GameObject (" + go.name + ")");
        }

        component.InvokeQuickAnimEvent(eventName);
    }

}
