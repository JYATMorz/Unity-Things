using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour {
    
    public static bool IsPause;

    public GameObject pauseMenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPause) ResumeGame();
            else PauseGame();
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPause = false;
    }

    public void PauseGame()
    {
        // TODO: Animation transition ?
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // FIXME: Check if fixedDeltaTime cause issues
        IsPause = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        //TODO: The same trick in MainMenu but for main menu
    }

    public void QuitGame()
    {
        Debug.Log("Pause Menu");
        Application.Quit();
    }
}