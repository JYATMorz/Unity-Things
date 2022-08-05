using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameMenu : MonoBehaviour {
    
    public static bool IsPause = false;
    public GameObject PauseMenuUI;
    public GameObject EndGameUI;
    public GameObject InGameUI;

    private int _redTeamNum = 0;
    private int _blueTeamNum = 0;
    private int _neutralNum = 0;
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _notifyText;
    private TextMeshProUGUI _gameOverText;

    private const string _neutralTag = "Neutral";
    private const string _blueTeamTag = "BlueTeam";
    private const string _redTeamTag = "RedTeam";

    void Start()
    {
        _scoreText = InGameUI.GetComponentsInChildren<TextMeshProUGUI>(true)[0];
        _notifyText = InGameUI.GetComponentsInChildren<TextMeshProUGUI>(true)[1];
        _gameOverText = EndGameUI.GetComponentInChildren<TextMeshProUGUI>(true);


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

    public void RestartGame()
    {
        // TODO: Fade in Fade out effect as game scene load in
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TeamChanged(string tag)
    {
        switch (tag)
        {
            case _redTeamTag:
                _redTeamNum ++;
                break;
            case _blueTeamTag:
                _blueTeamNum ++;
                break;
            default:
                Debug.LogError("Switching Character's Tag isn't Correct !");
                break;
        }
        _neutralNum --;
        if (_neutralNum < 0) Debug.LogWarning("Why less than 0 NPC !");

        ShowNotification(tag + "Born");
        UpdateGameUI();
    }

    public void CharacterDie(string tag)
    {
        switch (tag)
        {
            case _redTeamTag:
                _redTeamNum --;
                break;
            case _blueTeamTag:
                _blueTeamNum --;
                break;
            default:
                Debug.LogError("Dead Character's Tag isn't Correct !");
                break;
        }
        ShowNotification(tag + "Die");
        UpdateGameUI();

        if (_redTeamNum <= 0 || _blueTeamNum <= 0) GameOver();
    }

    private void UpdateGameUI()
    {
        string gameScore = 
            "Red: " + _redTeamNum.ToString().PadLeft(2) + "\n" +
            "Blue: " + _redTeamNum.ToString().PadLeft(2) + "";
        _scoreText.text = gameScore;
    }

    private void GameOver()
    {
        if ((_redTeamNum <= 0) && (_blueTeamNum <= 0)) _gameOverText.text = "A Rare Tie !";
        else if (_redTeamNum <= 0) _gameOverText.text = "Blue Wins";
        else if (_blueTeamNum <= 0) _gameOverText.text = "Red Wins";
        else _gameOverText.text = "Hmm...Something wrong with the result.";

        InGameUI.SetActive(false);
        PauseMenuUI.SetActive(false);
        EndGameUI.SetActive(true);
    }

    public void ShowNotification(string noteType)
    {
        string notification = noteType switch {
            "DeadZone" => "Shooting Dead Zone !\n",
            (_blueTeamTag + "Die") => "One Blue Team Member Die.",
            (_redTeamTag + "Die") => "One Red Team Member Die.",
            (_blueTeamTag + "Born") => "New Blue Team Member.",
            (_redTeamTag + "Born") => "New Blue Team Member.",
            "PlayerDie" => "You Are Dead. Wait For Reborn.",
            "PlayerBorn" => "You Are Alive, Again.",
            _ => null,
        };

        if (notification == null)
        {
            Debug.LogWarning("Wrong Notification Input");
            return;
        }

        _notifyText.text = notification;
        
        StopAllCoroutines();
        StartCoroutine(NotificationAlpha());
    }

    IEnumerator NotificationAlpha()
    {
        for (float alpha = 0f; alpha <= 1f; alpha += 0.1f)
        {
            _notifyText.alpha = alpha;
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        for (float alpha = 1f; alpha >= 0f; alpha -= 0.1f)
        {
            _notifyText.alpha = alpha;
            yield return null;
        }

        _notifyText.alpha = 0f;
    }
}