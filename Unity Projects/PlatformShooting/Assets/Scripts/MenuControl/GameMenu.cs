using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameMenu : MonoBehaviour, IMenuUI {
    
    public static bool IsPause { get; set; } = false;
    public WeaponControl CurrentWeaponControl { get; set; } = null;

    [Header("In Game UI Elements")]
    public GameObject inGameUI;
    public GameObject notifyPrefab;
    public GameObject notificationPanel;
    public TextMeshProUGUI scoreText;
    public GameObject weaponPanel;
    [Header("Pause Menu UI Elements")]
    public GameObject pauseMenuUI;
    public Animator pauseToResume;
    [Header("Game Over UI Elements")]
    public GameObject endGameUI;

    private int _redTeamNum = 0;
    private int _blueTeamNum = 0;
    private int _neutralNum = 0;
    private TextMeshProUGUI _gameOverText;
    private Button[] _weaponButtons;

    private const string _neutralTag = "Neutral";
    private const string _blueTeamTag = "BlueTeam";
    private const string _redTeamTag = "RedTeam";

    void Awake()
    {
        _weaponButtons = weaponPanel.GetComponentsInChildren<Button>();

        for (int i = 0; i < _weaponButtons.Length; i++)
        {
            int buttonIndex = i;
            _weaponButtons[i].onClick.AddListener(() => CurrentWeaponControl.ChangeWeapon(buttonIndex));
        }
    }

    void Start()
    {
        _gameOverText = endGameUI.GetComponentInChildren<TextMeshProUGUI>(true);
        _redTeamNum = GameObject.FindGameObjectsWithTag(_redTeamTag).Length;
        _blueTeamNum = GameObject.FindGameObjectsWithTag(_blueTeamTag).Length;
        _neutralNum = GameObject.FindGameObjectsWithTag(_neutralTag).Length;

        UpdateGameUI();
    }

    void Update()
    {
        if (MainCamera.IsGameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPause) ResumeGame();
            else PauseGame();
        }
    }

    public void ResumeGame()
    {
        StartCoroutine(ResumeTransition(0.15f));
    }

    IEnumerator ResumeTransition(float animeTime)
    {
        pauseToResume.SetTrigger("Resume");

        yield return new WaitForSecondsRealtime(animeTime);

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPause = false;
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPause = true;
    }

    public void LoadMainAsync()
    {
        Time.timeScale = 1f;
        GeneralLoadMenu.Instance.StartLoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        GeneralLoadMenu.Instance.StartLoadScene(SceneManager.GetActiveScene().buildIndex);
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
                Debug.Log("Character didn't die Correctly !");
                break;
        }
        ShowNotification(tag + "Die");
        UpdateGameUI();

        if (_redTeamNum <= 0 || _blueTeamNum <= 0) GameOver();
    }

    private void UpdateGameUI()
    {
        string gameScore = 
            "<#D0BDC7>Red: " + _redTeamNum.ToString().PadLeft(2, '0') + "\n" +
            "<#B4BED1>Blue: " + _blueTeamNum.ToString().PadLeft(2, '0');
        scoreText.text = gameScore;
    }

    private void GameOver()
    {
        MainCamera.GameIsOver();

        if ((_redTeamNum <= 0) && (_blueTeamNum <= 0)) _gameOverText.text = "<#C6A2D1>A Rare Tie !";
        else if (_redTeamNum <= 0) _gameOverText.text = "<#B4BED1>Blue Wins";
        else if (_blueTeamNum <= 0) _gameOverText.text = "<#D0BDC7>Red Wins";
        else _gameOverText.text = "<#FFFFFF>Hmm...Something wrong with the result.";

        inGameUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        endGameUI.SetActive(true);
    }

    public void ShowNotification(string noteType)
    {
        string notifyText = noteType switch {
            "DeadZone" => "Shooting Dead Zone !\n",
            (_blueTeamTag + "Die") => "One Blue Team Member Die.",
            (_redTeamTag + "Die") => "One Red Team Member Die.",
            (_blueTeamTag + "Born") => "New Blue Team Member.",
            (_redTeamTag + "Born") => "New Red Team Member.",
            "PlayerDie" => "You Are Dead. Wait For Reborn.",
            "PlayerBorn" => "You Are Alive, Again.",
            _ => null,
        };

        if (notifyText == null)
        {
            Debug.LogWarning("Wrong Notification Input");
            return;
        }

        if (noteType == "PlayerBorn") SwitchWeaponIcon(CurrentWeaponControl.CurrentAmmoNo());

        GameObject newNotify = Instantiate(notifyPrefab, notificationPanel.transform);
        newNotify.GetComponent<NotificationBlock>().StartNotification(notifyText);
    }

    public void SwitchWeaponIcon(int num)
    {
        for (int i = 0; i < _weaponButtons.Length; i++)
        {
            if (i == num) _weaponButtons[i].GetComponent<ButtonResize>().SizeToMax();
            else _weaponButtons[i].GetComponent<ButtonResize>().SizeToMin();
        }
    }
}