using UnityEngine;
using System;

public class TemplateBullet : MonoBehaviour
{
    public GameObject bulletImpact;

    private const int _ammoDamage = 10;
    private const string _neutralTag = "Neutral";

    private readonly string[] _characterTag = { "Neutral", "RedTeam", "BlueTeam" };

    private string _ownerTag;

    void Start()
    {
        _ownerTag = GetComponentsInParent<Rigidbody>()[2].tag;
    }

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;
        if (Array.Exists(_characterTag, tag => tag == contact.tag))
        {
            if (contact.tag != _neutralTag || _ownerTag != _neutralTag)
            {
                contact.GetComponent<CharacterControl>().ReceiveDamage(_ammoDamage);
            }
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        Instantiate(bulletImpact, transform.position, transform.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.down));
    }
}
