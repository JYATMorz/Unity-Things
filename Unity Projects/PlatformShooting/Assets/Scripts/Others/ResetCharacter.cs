using UnityEngine;
using System;

public class ResetCharacter : MonoBehaviour
{
    private readonly string[] _characterTags = { "Neutral", "BlueTeam", "RedTeam" };
    private const string _bulletTag = "Bullet";
    private const float _freeHeight = 23.5f;

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;

        if (Array.Exists(_characterTags, tag => tag == contact.tag))
            contact.transform.position = new Vector3(contact.transform.position.x, _freeHeight, 0);
        else Destroy(contact);
    }
}
