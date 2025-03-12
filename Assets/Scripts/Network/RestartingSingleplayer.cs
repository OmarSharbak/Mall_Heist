using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartingSingleplayer : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1.0f;
        Invoke(nameof(LoadSingle), 1f);
    }

    private void LoadSingle()
    {
        Debug.Log("load Single");
        SceneManager.LoadScene("SingleplayerScene");
    }
}
