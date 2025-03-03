using UnityEngine;
using Mirror;

public class ObjectiveInteracted : NetworkBehaviour
{
	InteractableObject interactableObject;
	Register register;
	SealableDoor sealableDoor;
	Objective objective;
	bool hasInteracted = false;

	// Start is called before the first frame update
	void Start()
	{
		interactableObject = GetComponent<InteractableObject>();
		objective = GetComponent<Objective>();
		register = GetComponent<Register>();
		sealableDoor = GetComponent<SealableDoor>();
	}

	// Update is called once per frame
	void Update()
	{
		if (hasInteracted == false)
		{
			if (interactableObject != null && interactableObject.pickedUpTimes == 2)
			{
				hasInteracted = true;
				objective.CompleteObjective();
			}
			else if (register != null && register.interacted)
			{
				hasInteracted = true;
				objective.CompleteObjective();
			}
			else if (sealableDoor != null && sealableDoor.isSealed)
			{
				hasInteracted = true;
				objective.CompleteObjective();
			}
		}
	}
}
