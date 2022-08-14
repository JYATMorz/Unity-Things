using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralLoadMenu : MonoBehaviour
{
    public static GeneralLoadMenu Instance { get; private set;} = null;

    public Animator fadeTransition;
    public GameObject loadingPanel;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartLoadScene(int sceneIndex)
    {
        StartCoroutine(LoadGameAsync(sceneIndex));
    }

    IEnumerator LoadGameAsync(int sceneIndex)
    {
        loadingPanel.SetActive(true);
        fadeTransition.SetTrigger("FadeIn");

        yield return new WaitForSeconds(1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        fadeTransition.SetTrigger("FadeOut");

        yield return new WaitForSeconds(1f);

        loadingPanel.SetActive(false);
        GameMenu.IsPause = false;
    }

}
