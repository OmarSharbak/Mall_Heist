using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour {

	//a function that is fired when the cube is rightside-up
	public void OnCubeRightsideUp()
	{
		Debug.Log ("Cube - rightside up");
	}

	//a function that is fired when the cube is upside-down
	public void OnCubeUpsideDown()
	{
		Debug.Log ("Cube - upside down");
	}

}
