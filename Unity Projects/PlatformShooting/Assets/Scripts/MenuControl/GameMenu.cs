using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
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
    public Button pauseButton;
    public GameObject pauseMenuUI;
    public Animator pauseToResume;
    [Header("Game Over UI Elements")]
    public GameObject endGameUI;
    public GameObject statsTabPanel;

    private int _redTeamNum = 0;
    private int _blueTeamNum = 0;
    private int _neutralNum = 0;
    private Dictionary<string, TotalStats> _maxStatsRecord;
    private TextMeshProUGUI _gameOverText;
    private Button[] _weaponButtons;
    private Button[] _tabButtons;

    void Awake()
    {
        _weaponButtons = weaponPanel.GetComponentsInChildren<Button>();
        for (int i = 0; i < _weaponButtons.Length; i++)
        {
            int buttonIndex = i;
            _weaponButtons[i].onClick.AddListener(() => CurrentWeaponControl.ChangeWeapon(buttonIndex));
        }

        _tabButtons = statsTabPanel.GetComponentsInChildren<Button>();
        for (int i = 0; i < _tabButtons.Length; i++)
        {
            int buttonIndex = i;
            _tabButtons[i].onClick.AddListener(() => ChangeStatsTab(buttonIndex));
        }

        // FIXME: Use Unity Inspector event assignment and remove this
        pauseButton.onClick.AddListener(() => PauseGame());

        _maxStatsRecord = new Dictionary<string, TotalStats>()
        {
            [ConstantSettings.blueTeamTag] = new TotalStats(),
            [ConstantSettings.redTeamTag] = new TotalStats(),
            [ConstantSettings.neutralTag] = new TotalStats(),
            [ConstantSettings.playerTag] = new TotalStats()
        };
    }

    void Start()
    {
        _gameOverText = endGameUI.GetComponentInChildren<TextMeshProUGUI>(true);
        _redTeamNum = GameObject.FindGameObjectsWithTag(ConstantSettings.redTeamTag).Length;
        _blueTeamNum = GameObject.FindGameObjectsWithTag(ConstantSettings.blueTeamTag).Length;
        _neutralNum = GameObject.FindGameObjectsWithTag(ConstantSettings.neutralTag).Length;

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

        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.inGameTag);
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPause = true;

        GeneralAudioControl.Instance.PauseAudio(ConstantSettings.inGameTag);
    }

    public void LoadMainAsync()
    {
        Time.timeScale = 1f;
        IsPause = true;
        GeneralLoadMenu.Instance.StartLoadScene(0);
    }

    public void QuitGame()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            LoadMainAsync();
            return;
        }

        Application.Quit();
    }

    public void RestartGame()
    {
        IsPause = true;
        GeneralLoadMenu.Instance.StartLoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TeamChanged(string tag)
    {
        switch (tag)
        {
            case ConstantSettings.redTeamTag:
                _redTeamNum ++;
                break;
            case ConstantSettings.blueTeamTag:
                _blueTeamNum ++;
                break;
            default:
                Debug.LogError("Switching Character's Tag isn't Correct: " + tag);
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
            case ConstantSettings.redTeamTag:
                _redTeamNum --;
                break;
            case ConstantSettings.blueTeamTag:
                _blueTeamNum --;
                break;
            case ConstantSettings.neutralTag:
                _neutralNum --;
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
            ConstantSettings.redColor + "Red: " + _redTeamNum.ToString().PadLeft(2, '0') + "\n" +
            ConstantSettings.blueColor + "Blue: " + _blueTeamNum.ToString().PadLeft(2, '0');
        scoreText.text = gameScore;
    }

    private void GameOver()
    {
        if (MainCamera.IsGameOver) return;

        GeneralAudioControl.Instance.StopAudio(ConstantSettings.inGameTag);
        MainCamera.GameIsOver();

        if ((_redTeamNum <= 0) && (_blueTeamNum <= 0)) _gameOverText.text = ConstantSettings.purpleColor + "A Rare Tie !";
        else if (_redTeamNum <= 0) _gameOverText.text = ConstantSettings.blueColor + "Blue Wins";
        else if (_blueTeamNum <= 0) _gameOverText.text = ConstantSettings.redColor + "Red Wins";
        else _gameOverText.text = ConstantSettings.whiteColor + "Hmm...Something wrong with the result.";

        inGameUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        endGameUI.SetActive(true);
        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.endTag, 0.5f);
    }

    public void ShowNotification(string noteType)
    {
        if (GeneralLoadMenu.Instance.IsLoadingScene) return;

        string notifyText = noteType switch {
            "DeadZone" => "Shooting Dead Zone !\n",
            (ConstantSettings.blueTeamTag + "Die") => ConstantSettings.blueColor + "One Blue Team Member Die.",
            (ConstantSettings.redTeamTag + "Die") => ConstantSettings.redColor + "One Red Team Member Die.",
            (ConstantSettings.neutralTag + "Die") => ConstantSettings.whiteColor + "One Innocent Character Die.",
            (ConstantSettings.blueTeamTag + "Born") => ConstantSettings.blueColor + "New Blue Team Member.",
            (ConstantSettings.redTeamTag + "Born") => ConstantSettings.redColor + "New Red Team Member.",
            "PlayerDie" => ConstantSettings.purpleColor + "You Are Dead. Wait For Reborn.",
            "PlayerBorn" => ConstantSettings.purpleColor + "You Are Alive. Try Again.",
            _ => null,
        };

        if (notifyText is null)
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

    public void UpdateCharacterStats(TotalStats newStat, string teamTag, bool isPlayer = false)
    {
        if (MainCamera.IsGameOver) return;

        // TODO: Design the Stats Panel
        if (isPlayer) _maxStatsRecord[ConstantSettings.playerTag] += newStat;

        _maxStatsRecord[teamTag].Max(newStat);
    }

    private void SortCharacterStats()
    {
        /*
        TODO: sort _maxStatsRecord DSC for each stats,
        eg. Highest Kill Count: Blue: 5, Red: 4, Player (Blue): 3

            StatsPanel
            --- Stats Title (TextMeshPro)
            --- Tab Buttons (Most Kill / Most Damage / Most Ammo) (Transition Animation ? Color Difference ?)
            --- StatsRank (Vertical Layout Group)
                --- PlayerPanel
                    --- Image (Character Default Image)
                    --- TextMeshPro (Amount in Number) (Transition Animation ? Flash to Black and Return)
                +-- Blue: 3
                +-- Red: 3
        */
    }

    private void ChangeStatsTab(int index)
    {
        switch (index)
        {
            case 0:
                DisplayKillStats();
                break;
            case 1:
                DisplayDamageStats();
                break;
            case 2:
                DisplayAmmoStats();
                break;
            default:
                Debug.Log("Wrong button to change stats tab !");
                break;
        }
    }

    private void DisplayKillStats()
    {
        string textPlayer = _maxStatsRecord[ConstantSettings.playerTag].TotalKill.ToString();
    }

    private void DisplayDamageStats()
    {
        string textPlayer = _maxStatsRecord[ConstantSettings.playerTag].TotalDamage.ToString();
    }

    private void DisplayAmmoStats()
    {
        string textPlayer = _maxStatsRecord[ConstantSettings.playerTag].TotalAmmo.ToString();
    }
}