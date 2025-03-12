using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public int sceneToLoad = 1;  // Name of the scene to load

    public LevelLoader levelLoader;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Assuming your player has a tag "Player"
        {
            levelLoader.LoadLevel(1);
        }
    }

    public void LoadLevel(int levelToLoad=1)
    {
        levelLoader.LoadLevel(1);
    }
}
