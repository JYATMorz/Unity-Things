using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour {
    
    public static bool IsPause = false;

    public GameObject pauseMenuUI;
    public GameObject endGameUI;

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
        Time.timeScale = 0f;
        IsPause = true;
    }

    public void LoadMainAsync()
    {
        Time.timeScale = 1f;
        //TODO: The same trick in MainMenu but for main menu
        ResumeGame();
    }

    public void QuitGame()
    {
        Debug.Log("Pause Menu");
        Application.Quit();
    }

    public void GameOver()
    {
        Debug.Log("Game Over Menu!");
        endGameUI.SetActive(true);
    }

    public void RestartGame()
    {
        // TODO: Fade in Fade out effect as game scene load in
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void OneCharacterDie()
    {
        // TODO: count remaining alive members and update UI including game over panel
        Debug.Log("Menu Copy");
    }
}