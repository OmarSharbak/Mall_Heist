using UnityEngine;

public class SphereScript : MonoBehaviour {

	//a function that is fired when the sphere reaches the bottom of its cycle
	public void OnSphereReachesBottom()
	{
		Debug.Log ("Sphere - bottom");
	}

	//a function that is fired when the sphere reaches the middle of its cycle
	public void OnSphereReachesMiddle()
	{
		Debug.Log ("Sphere - middle");
	}

	//a function that is fired when the sphere reaches the top of its cycle
	public void OnSphereReachesTop()
	{
		Debug.Log ("Sphere - top");
	}
}
