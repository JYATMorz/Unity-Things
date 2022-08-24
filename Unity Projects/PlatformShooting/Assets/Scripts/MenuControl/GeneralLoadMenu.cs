using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GeneralLoadMenu : MonoBehaviour
{
    public static GeneralLoadMenu Instance { get; private set;} = null;

    public bool IsLoadingScene {get; private set; } = false;

    [Header("Loading Panel Transition")]
    public Animator fadeTransition;
    public GameObject loadingPanel;
    [Header("Loading Scene Tips")]
    public TextMeshProUGUI completeText;
    public TextMeshProUGUI tipsText;
    [Header("Cursor Setting")]
    public Texture2D cursorTexture;

    private bool _isLoadComplete = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ChangeCursor(SceneManager.GetActiveScene().buildIndex);
    }

    void Start()
    {
        PlayThemeAudio(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartLoadScene(int sceneIndex)
    {
        IsLoadingScene = true;
        GeneralAudioControl.Instance.StopAudio(ConstantSettings.themeTag);
        GeneralAudioControl.Instance.StopAudio(ConstantSettings.inGameTag);
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
        while (!asyncLoad.isDone || !Input.anyKeyDown)
        {
            yield return null;

            if (!_isLoadComplete && asyncLoad.isDone)
            {
                _isLoadComplete = true;
                StartCoroutine(FlashingText());

                ChangeCursor(sceneIndex);
            }
        }

        fadeTransition.SetTrigger("FadeOut");
        _isLoadComplete = false;
        yield return new WaitForSeconds(1f);

        loadingPanel.SetActive(false);
        IsLoadingScene = false;
        GameMenu.IsPause = false;
        PlayThemeAudio(sceneIndex);
    }

    IEnumerator FlashingText()
    {
        yield return null;

        while (_isLoadComplete)
        {
            for (float a = 0f; a <= 1f; a += Time.deltaTime / 1.2f)
            {
                completeText.alpha = a;
                yield return null;
            }

            completeText.alpha = 1f;
            yield return null;

            for (float a = 1f; a >= 0f; a -= Time.deltaTime / 1.2f)
            {
                completeText.alpha = a;
                yield return null;
            }

            completeText.alpha = 0f;
            yield return null;
        }

        completeText.alpha = 0f;
    }

    private void PlayThemeAudio(int sceneIndex)
    {
        if (sceneIndex == 0)
            GeneralAudioControl.Instance.PlayAudio(ConstantSettings.themeTag, 0.1f);
        else
            GeneralAudioControl.Instance.PlayAudio(ConstantSettings.inGameTag, 0.1f);
    }

    private void ChangeCursor(int sceneIndex = 0)
    {
        if (sceneIndex == 0)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        } else
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
    }
}
