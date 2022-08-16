using UnityEngine;
using System;
using System.Collections;

public class TemplateBullet : MonoBehaviour
{
    public GameObject bulletImpact;

    private const int _selfDestructionTime = 10;
    private const int _ammoDamage = 10;
    private const string _neutralTag = "Neutral";

    private readonly string[] _characterTag = { "Neutral", "RedTeam", "BlueTeam" };

    private string _ownerTag;

    void Start()
    {
        _ownerTag = GetComponentsInParent<Rigidbody>()[2].tag;

        StartCoroutine(SelfDestruction());
    }

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;
        if (Array.Exists(_characterTag, tag => tag == contact.tag))
        {
            if (contact.tag != _neutralTag || _ownerTag != _neutralTag)
            {
                contact.GetComponent<HealthControl>().ReceiveDamage(_ammoDamage);
            }
        }
        Instantiate(bulletImpact, transform.position, transform.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.down));

        Destroy(gameObject);
    }

    IEnumerator SelfDestruction()
    {
        yield return new WaitForSeconds(_selfDestructionTime);
        Destroy(gameObject);
    }
}
