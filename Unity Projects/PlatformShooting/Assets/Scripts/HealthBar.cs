using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slide;
    public Gradient gradient;
    public Image imgFill;

    public void SetHealthValue(int healthPercent)
    {
        slide.value = healthPercent;

        imgFill.color = gradient.Evaluate(slide.normalizedValue);

        // imgFill.fillAmount = healthPercent;
    }

    public void SetMaxHealth(int health)
    {
        slide.maxValue = health;
        slide.value = health;

        imgFill.color = gradient.Evaluate(1f);

        // imgFill.fillAmount = 1f;
    }
}

/*
And also I would not use the slider component as a fill effect.
The image component has a "Image type" property (if you have a source image attached), 
you can set it to filled,
it achieves the same effect and you have one less component to worry about.

I hope you guys find this useful :)
https://www.youtube.com/watch?v=BLfNP4Sc_iA

https://blog.csdn.net/qq_42440767/article/details/96491390
*/
