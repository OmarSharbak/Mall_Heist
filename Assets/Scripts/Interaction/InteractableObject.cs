using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPOOutline;
using Mirror;

public class InteractableObject : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPickedUpChanged))]   
    public int pickedUpTimes = 3;

    public Outlinable outlinable;

    public virtual void Interact(PlayerInteractionController playerInteractionController)
    {
        // This method is intended to be overridden by subclasses.
        Debug.Log("Interacting with " + gameObject.name);
    }

    private void OnPickedUpChanged(int _oldValue, int _newValue)
    {
        Debug.Log("CLIENT - picked up changed "+ _newValue);

		if (outlinable!=null && pickedUpTimes == 0)
		{
			outlinable.enabled = false;
		}
	}
	[Command(requiresAuthority =false)]
	public void CmdSetPickedUpTimes(int _newValue)
	{
		Debug.Log("SERVER - picked up changed " + _newValue);

		pickedUpTimes= _newValue;
	}
}
