using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour, IMenuUI {

    public static bool IsPause { get; set; } = false;
    public WeaponControl CurrentWeaponControl { get; set; } = null;

    public void StartGame()
    {
        GeneralLoadMenu.Instance.StartLoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Basically do nothing, maybe change text rotation
    public void CharacterDie(string tag)
    {
        Debug.Log(tag);
    }

    public void TeamChanged(string tag)
    {
        Debug.Log(tag);
    }

    public void ShowNotification(string noteType)
    {
        Debug.Log(noteType);
    }

    public void SwitchWeaponIcon(int num)
    {
        Debug.Log(num);
    }
}