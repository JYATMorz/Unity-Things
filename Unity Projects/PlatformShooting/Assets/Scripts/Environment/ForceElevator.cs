using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class ForceElevator : MonoBehaviour
{
    private const float _liftForce = 30f;
    private const float _initAlpha = 0.75f;

    private bool _playingEffect = false;
    private MeshRenderer _meshRender;

    void Awake()
    {
        _meshRender = GetComponent<MeshRenderer>();
    }

    void OnTriggerEnter()
    {
        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.jumpTag, transform.position);
        if (!_playingEffect) StartCoroutine(LiftEffect());
    }

    void OnTriggerStay(Collider other)
    {
        other.attachedRigidbody.AddForce(transform.up * _liftForce, ForceMode.Impulse);
    }

    IEnumerator LiftEffect()
    {
        _playingEffect = true;
        Color materialColor = _meshRender.material.color;

        for (float alpha = _initAlpha; alpha < 1f; alpha += Time.deltaTime)
        {
            materialColor.a = alpha;
            _meshRender.material.color = materialColor;
            yield return null;
        }

        for (float alpha = 1f; alpha > _initAlpha; alpha -= Time.deltaTime / 2)
        {
            materialColor.a = Mathf.Clamp(alpha, _initAlpha, 1f);
            _meshRender.material.color = materialColor;
            yield return null;
        }

        _playingEffect = false;
    }
}