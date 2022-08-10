using UnityEngine;
using UnityEngine.VFX;
using System;
using System.Collections;

public class LaserBeam : MonoBehaviour
{
    // TODO: VFX Effect for Laser
    public VisualEffect laserImpact;
    public VisualEffect laserDestroy;

    private const int _ammoDamage = 10;
    private const float _lifeTime = 5f;
    private const string _neutralTag = "Neutral";

    private readonly string[] _characterTag = { "Neutral", "RedTeam", "BlueTeam" };
    
    private Rigidbody _laserBody;
    private string _ownerTag;

    void Start()
    {
        _ownerTag = GetComponentsInParent<Rigidbody>()[2].tag;
        _laserBody = GetComponent<Rigidbody>();

        StartCoroutine(LifeTimeOver(_lifeTime));
        StartCoroutine(TooSlowToLive());
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
            Destroy(gameObject);
        }
        // TODO: Add sci-fi effect when collides
    }

    void OnDestroy()
    {
        // TODO: Add sci-fi dead effect when destroy
    }

    IEnumerator LifeTimeOver(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        
        Destroy(gameObject);
    }

    IEnumerator TooSlowToLive()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (_laserBody.velocity.sqrMagnitude < 50) Destroy(gameObject);
            yield return new WaitForFixedUpdate();
        }
    }
}
