using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIColliderEnabler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //It just enables the box collider with the right layer for the health bar UI to pop because of bad design from EmeraldAI that turns of colliders they didnt create
        GetComponent<BoxCollider>().enabled = true;
    }
}
