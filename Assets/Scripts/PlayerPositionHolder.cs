using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionHolder : MonoBehaviour
{
    private float startYPosition;

    private void Start()
    {
        startYPosition = transform.localPosition.y;
        Debug.Log("Start Y Position: " +  startYPosition);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, startYPosition, transform.localPosition.z);
    }

}
