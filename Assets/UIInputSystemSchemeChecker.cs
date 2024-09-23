using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;

public class UIInputSystemSchemeChecker : MonoBehaviour
{
    InputSystemUIInputModule uIInputModule;
    // Start is called before the first frame update
    void Start()
    {
        uIInputModule = GetComponent<InputSystemUIInputModule>();
    }

    // Update is called once per frame
    void Update()
    {
        if(uIInputModule != null)
        {

        }   
    }
}
