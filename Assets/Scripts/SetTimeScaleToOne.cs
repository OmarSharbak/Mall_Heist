using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTimeScaleToOne : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.None; // This will unlock the cursor
        Cursor.visible = true; // This will make the cursor visible
    }
}
