using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image imgFill;

    void Start()
    {
        imgFill = GetComponent<Image>();
    }

    public void SetHealthValue(float healthPercent)
    {
        imgFill.fillAmount = healthPercent;
    }

    public void SetMaxHealth()
    {
        imgFill.fillAmount = 1f;
    }
}