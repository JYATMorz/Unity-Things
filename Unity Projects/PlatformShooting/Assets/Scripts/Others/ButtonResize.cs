using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonResize : MonoBehaviour
{
    private const float _maxSize = 80f;
    private const float _minSize = 60f;
    private const int _resizeScalar = 100;

    private RectTransform _rectTransform;
    private Button _thisButton;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _thisButton = GetComponent<Button>();
    }

    public void SizeToMax()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeSize(_resizeScalar));
    }

    public void SizeToMin()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeSize(-_resizeScalar));
    }

    IEnumerator ChangeSize(int scalar)
    {
        yield return null;

        if (scalar < 0)
        {
            _thisButton.interactable = true;
            for (float size = _rectTransform.sizeDelta.x; size > _minSize; size += scalar * Time.deltaTime)
            {
                SetRectTransformSize(size);
                yield return null;
            }
            SetRectTransformSize(_minSize);
        } else if (scalar > 0)
        {
            _thisButton.interactable = false;
            for (float size = _rectTransform.sizeDelta.x; size < _maxSize; size += scalar * Time.deltaTime)
            {
                SetRectTransformSize(size);
                yield return null;
            }
            SetRectTransformSize(_maxSize);
        }
    }

    private void SetRectTransformSize(float length)
    {
        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length);
        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, length);
    }
}
