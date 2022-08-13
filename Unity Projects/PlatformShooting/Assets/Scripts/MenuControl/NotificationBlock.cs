using UnityEngine;
using System.Collections;
using TMPro;

public class NotificationBlock : MonoBehaviour
{
    private const string _stylePrefix = "<margin=2em>";
    private const float _maxHeight = 40f;

    private TextMeshProUGUI _notifyText;
    private RectTransform _rectTransform;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _notifyText = GetComponent<TextMeshProUGUI>();

        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

        _notifyText.alpha = 0f;
        _notifyText.text = _stylePrefix;
    }

    public void StartNotification(string text)
    {
        if (gameObject.activeInHierarchy) StartCoroutine(NotificationRect(text));
    }

    IEnumerator NotificationRect(string text)
    {
        yield return null;
        _notifyText.text += text;

        for (float delta = 0f; delta <= 1f; delta += Time.deltaTime * 1.5f)
        {
            _notifyText.alpha = delta;
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _maxHeight * delta);

            yield return null;
        }

        _notifyText.alpha = 1f;
        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _maxHeight);
        yield return new WaitForSeconds(3f);

        for (float delta = 1f; delta >= 0f; delta -= Time.deltaTime)
        {
            _notifyText.alpha = delta;
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _maxHeight * delta);
            yield return null;
        }

        yield return null;
        Destroy(gameObject);
    }
}
