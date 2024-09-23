using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectiveItem : MonoBehaviour
{
    [SerializeField] TMP_Text countText;

    // Start is called before the first frame update
    void Start()
    {
        if (countText != null) 
            countText.text = "1/1";
    }
}
