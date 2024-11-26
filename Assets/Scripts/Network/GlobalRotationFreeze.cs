using UnityEngine;

public class GlobalRotationFreeze : MonoBehaviour
{

	Quaternion initialRot;
	private void Start()
	{
		initialRot = transform.localRotation;
	}
	void Update()
	{
		// Apply the new rotation
		transform.rotation= initialRot;
	}
}
