using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GeneralLoadMenu : MonoBehaviour
{
    public static GeneralLoadMenu Instance { get; private set;} = null;

    public Animator fadeTransition;
    public GameObject loadingPanel;
    public TextMeshProUGUI tipsText;

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

    void Start()
    {
        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.themeTag, 0.2f);
    }

    public void StartLoadScene(int sceneIndex)
    {
        GeneralAudioControl.Instance.StopAudio(ConstantSettings.themeTag);
        GeneralAudioControl.Instance.StopAudio(ConstantSettings.endTag);
        StartCoroutine(LoadGameAsync(sceneIndex));
    }

    IEnumerator LoadGameAsync(int sceneIndex)
    {
        loadingPanel.SetActive(true);
        tipsText.text = ConstantSettings.tipsText[Random.Range(0, ConstantSettings.tipsText.Length)];
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
        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.themeTag, 0.2f);
    }

}
