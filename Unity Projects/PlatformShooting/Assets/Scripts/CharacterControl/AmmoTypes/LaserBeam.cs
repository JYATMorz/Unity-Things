// TODO: !!! USE CCD SPECULATIVE !!!

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
    private const float _lifeTime = 10f;
    private const string _neutralTag = "Neutral";

    private readonly string[] _characterTag = { "Neutral", "RedTeam", "BlueTeam" };
    

    private string _ownerTag;

    void Start()
    {
        _ownerTag = GetComponentsInParent<Rigidbody>()[2].tag;

        StartCoroutine(LifeTimeOver(_lifeTime));
    }

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;
        if (Array.Exists(_characterTag, tag => tag == contact.tag))
        {
            if (contact.tag != _neutralTag || _ownerTag != _neutralTag)
            {
                contact.SendMessage("ReceiveDamage", _ammoDamage);
            }
        }
        // TODO: Add sci-fi effect when collides
    }

    IEnumerator LifeTimeOver(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        // TODO: Add sci-fi dead effect when destroy
        Destroy(gameObject);
    }
}
