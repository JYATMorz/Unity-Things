using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour {
    
    public static bool IsPause = false;
    public static GameObject PauseMenuUI;
    public static GameObject EndGameUI;
    public static GameObject InGameUI;

    private static int _redTeamNum = 0;
    private static int _blueTeamNum = 0;
    private static int _neutralNum = 0;

    private const string _neutralTag = "Neutral";
    private const string _blueTeamTag = "BlueTeam";
    private const string _redTeamTag = "RedTeam";

    void Start()
    {
        _redTeamNum = GameObject.FindGameObjectsWithTag(_redTeamTag).Length;
        _blueTeamNum = GameObject.FindGameObjectsWithTag(_blueTeamTag).Length;
        _neutralNum = GameObject.FindGameObjectsWithTag(_neutralTag).Length;
    }

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
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPause = false;
    }

    public void PauseGame()
    {
        // TODO: Animation transition ?
        PauseMenuUI.SetActive(true);
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

    public static void GameOver()
    {
        Debug.Log("Game Over Menu!");
        InGameUI.SetActive(false);
        PauseMenuUI.SetActive(false);
        EndGameUI.SetActive(true);

        // TODO: Change text to show who win or a tie.
    }

    public void RestartGame()
    {
        // TODO: Fade in Fade out effect as game scene load in
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void TeamChanged(string tag)
    {
        switch (tag)
        {
            case _redTeamTag:
                _redTeamNum ++;
                break;
            case _blueTeamTag:
                _redTeamNum ++;
                break;
            default:
                Debug.LogError("Switching Character's Tag isn't Correct !");
                break;
        }
        _neutralNum --;

        Debug.Log($"Blue: {_blueTeamNum}, NPC: {_neutralNum}, Red: {_redTeamNum}");
    }

    public static void CharacterDie(string tag)
    {
        // TODO: update UI including game over panel
        switch (tag)
        {
            case _redTeamTag:
                _redTeamNum --;
                break;
            case _blueTeamTag:
                _redTeamNum --;
                break;
            case _neutralTag:
                _redTeamNum --;
                break;
            default:
                Debug.LogError("Dead Character's Tag isn't Correct !");
                break;
        }

        if (_redTeamNum <= 0 || _blueTeamNum <= 0)
        {
            if (GameObject.FindWithTag(_redTeamTag) == null)
            {
                Debug.Log("RedTEAM Lose");
            } else if (GameObject.FindWithTag(_blueTeamTag) == null)
            {
                Debug.Log("BlueTEAM Lose");
            } else Debug.LogWarning("Unexpected Winning Condition");

            GameOver();
        }
    }
}