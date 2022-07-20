using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slide;
    public Gradient gradient;
    public Image fill;

    public void SetHealthValue(int health)
    {
        slide.value = health;
        fill.color = gradient.Evaluate(slide.normalizedValue);
    }

    public void SetMaxHealth(int health)
    {
        slide.maxValue = health;
        slide.value = health;
        fill.color = gradient.Evaluate(1f);
    }
}
