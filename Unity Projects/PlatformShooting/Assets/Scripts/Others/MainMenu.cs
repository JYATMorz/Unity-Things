using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour{
    
    public void StartGame()
    {
        StartCoroutine(LoadGameAsync());
    }

    public void QuitGame()
    {
        Debug.Log("Main Menu");
        Application.Quit();
    }

    IEnumerator LoadGameAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");

        // TODO: Play some fade / in fade out effect here
        // https://www.youtube.com/watch?v=YMj2qPq9CP8
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}