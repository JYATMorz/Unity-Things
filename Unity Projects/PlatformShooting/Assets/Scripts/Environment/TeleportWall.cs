using UnityEngine;
using UnityEngine.VFX;
using System;
using System.Collections;

public class TeleportWall : MonoBehaviour {
    
    private const float _upPositionY = 14.625f;
    private const float _downPositionY = 0.75f;

    private VisualEffect _teleportVFX;

    void Awake()
    {
        _teleportVFX = GetComponentInChildren<VisualEffect>();
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
        yield return new WaitForSeconds(0.5f);

        character.transform.position = new Vector3(
            - oldPosition.x * 0.96f,
            UnityEngine.Random.value < 0.5f ? _upPositionY : _downPositionY,
            oldPosition.z
        );

        yield return new WaitForSeconds(0.5f);
        character.GetComponent<CharacterControl>().IsTeleported = false;
    }
}