using Mirror;
using UnityEngine;

public class ObjectiveTrigger : NetworkBehaviour
{

	public Objective[] nextObjectives;

	Objective currentObjective;

	bool isTriggered = false;

	private void Start()
	{
		currentObjective = GetComponent<Objective>();
	}

	public void TriggerNextObjective()
	{
		foreach(Objective obj in nextObjectives)
			obj.setActiveGameObjects(true);
	}

	private void Update()
	{
		if(currentObjective!=null && currentObjective.isComplete && !isTriggered)
		{
			isTriggered = true;
			foreach (Objective obj in nextObjectives)				
				if (obj!=null)
					obj.setActiveGameObjects(true);
		}
	}
}
