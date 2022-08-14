using UnityEngine;
using System;
using System.Collections;

public class TeleportWall : MonoBehaviour {

    private readonly string[] _characterTags = { "Neutral", "BlueTeam", "RedTeam", "Dead" };
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet")) Destroy(other.gameObject);
        else if (Array.Exists(_characterTags, tag => tag == other.tag))
            StartCoroutine(TeleportCharacter(other.gameObject));
    }

    IEnumerator TeleportCharacter(GameObject character)
    {
        // TODO: need teleport character VFX
        character.GetComponent<CharacterControl>().IsTeleported = true;
        Vector3 oldPosition = character.transform.position;

        Debug.Log("VFX Start!");
        yield return new WaitForSeconds(0.5f);

        character.transform.position = new Vector3(- oldPosition.x * 0.96f, oldPosition.y, oldPosition.z);

        yield return new WaitForSeconds(0.5f);
        character.GetComponent<CharacterControl>().IsTeleported = false;
        Debug.Log("VFX Complete!");
    }
}