using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PingHUD : MonoBehaviour
{

    public TextMeshProUGUI pingText;
	// Update is called once per frame
	void Update()
    {
		pingText.text=$"RTT: {NetworkTime.rtt * 1000:F0}ms";
	}
}
