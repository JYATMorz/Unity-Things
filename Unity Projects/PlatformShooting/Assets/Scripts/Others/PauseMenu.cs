using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour {
    
    public static bool IsPause;

    public gameObject pauseMenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPause) ResumeGame();
            else PauseGame();
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPause = false;
    }

    public void PauseGame()
    {
        // TODO: Animation transition ?
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // FIXME: Check if fixedDeltaTime cause issues
        isPause = true;
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