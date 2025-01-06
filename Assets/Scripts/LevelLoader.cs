using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] float transitionTime = 1.0f;
    [SerializeField] Animator animator;

    public void LoadLevel(int lvlIndex = 1)
    {
        StartCoroutine(LoadLevelNumerator(lvlIndex));
    }
	public void LoadLobby()
	{
		SceneManager.LoadScene("LobbyScene");

	}



	IEnumerator LoadLevelNumerator(int levelIndex)
    {
        animator.SetTrigger("Start");
        
        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
