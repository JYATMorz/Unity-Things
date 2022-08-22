using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour, IMenuUI {

    public static bool IsPause { get; set; } = false;
    public WeaponControl CurrentWeaponControl { get; set; } = null;

    public GameObject titleTextPanel;

    private TextMeshProUGUI[] _titleTexts;

    void Awake()
    {
        _titleTexts = titleTextPanel.GetComponentsInChildren<TextMeshProUGUI>();
    }

    public void StartGame()
    {
        GeneralLoadMenu.Instance.StartLoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Basically do nothing, change text rotation for fun
    public void CharacterDie(string tag)
    {
        RotateText();
    }

    public void TeamChanged(string tag)
    {
        RotateText();
    }

    public void ShowNotification(string noteType)
    {
        RotateText();
    }

    public void SwitchWeaponIcon(int num)
    {
        RotateText();
    }

    private void RotateText()
    {
        _titleTexts[Random.Range(0, _titleTexts.Length)].rectTransform.rotation
            *= Quaternion.AngleAxis(Random.value - 0.5f, Vector3.forward);
    }
}