using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour {
    // TODO: Move all fade related objects to a new canvas with dont destroy on load
    public Animator fadeTransition;
    public GameObject loadingPanel;

    public void StartGame()
    {
        StartCoroutine(FadeOutTransition());
    }

    public void QuitGame()
    {
        Debug.Log("Main Menu");
        Application.Quit();
    }

    IEnumerator FadeOutTransition()
    {
        fadeTransition.SetTrigger("FadeOut");

        yield return new WaitForSeconds(1f);

        loadingPanel.SetActive(true);
        StartCoroutine(LoadGameAsync());
    }

    IEnumerator LoadGameAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");

        // TODO: Play some fade / in fade out effect here
        // https://www.youtube.com/watch?v=YMj2qPq9CP8
        while (!asyncLoad.isDone)
        {
            Debug.Log(asyncLoad.progress);
            // Debug.Log(Mathf.Clamp01(asyncLoad.progress / 0.9f));
            yield return null;
        }
    }
}