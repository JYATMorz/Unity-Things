using UnityEngine;
using System;

public class TemplateBullet : MonoBehaviour
{
    public GameObject bulletImpact;

    private const int _ammoDamage = 15;

    private readonly string[] _characterTag = { "Neutral", "RedTeam", "BlueTeam" };

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;
        if (Array.Exists(_characterTag, tag => tag == contact.tag))
        {
            contact.SendMessage("ReceiveDamage", _ammoDamage);
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // TODO: Add more detailed smoke (particle) effect when collides
        Instantiate(bulletImpact, transform.position, transform.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.down));
    }
}
