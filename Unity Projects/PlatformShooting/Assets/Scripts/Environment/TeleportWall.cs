using UnityEngine;
using UnityEngine.VFX;
using System;
using System.Collections;

public class TeleportWall : MonoBehaviour {
    
    private const float _upPositionY = 14.625f;
    private const float _downPositionY = 0.75f;
    private const float _initIntensity = 10f;
    private const float _freezeTime = 0.8f;

    private VisualEffect _teleportVFX;
    private Light _teleportLight;

    void Awake()
    {
        _teleportVFX = GetComponentInChildren<VisualEffect>();
        _teleportLight = GetComponentInChildren<Light>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ConstantSettings.bulletTag)) Destroy(other.gameObject);
        else if (Array.Exists(ConstantSettings.characterTags, tag => other.CompareTag(tag)))
            StartCoroutine(TeleportCharacter(other.gameObject));
    }

    IEnumerator TeleportCharacter(GameObject character)
    {
        character.GetComponent<CharacterControl>().IsTeleported = true;
        Vector3 oldPosition = character.transform.position;

        _teleportVFX.Play();
        for (float Scalar = 0f; Scalar < _freezeTime; Scalar += Time.deltaTime)
        {
            _teleportLight.intensity += Scalar * 10f;
            yield return null;
        }

        character.transform.position = new Vector3(
            - oldPosition.x * 0.96f,
            UnityEngine.Random.value < 0.5f ? _upPositionY : _downPositionY,
            oldPosition.z
        );
        yield return new WaitForSeconds(1f - _freezeTime);

        _teleportLight.intensity = _initIntensity;
        character.GetComponent<CharacterControl>().IsTeleported = false;
        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.teleportTag, character.transform.position);
    }
}