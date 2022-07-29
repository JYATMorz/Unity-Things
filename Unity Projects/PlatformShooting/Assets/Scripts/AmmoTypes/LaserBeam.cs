// TODO: Ammo with limited size, extreme fast speed, low damage, zero gravity, long lifetime and charge time, medium damage, full bouncing

// TODO: possible use case: NPC can indirectly aim player / Charge Effect?

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

    private readonly string[] _characterTag = { "Neutral", "RedTeam", "BlueTeam" };

    void Start()
    {
        StartCoroutine(LifeTimeOver(_lifeTime));
    }

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;
        if (Array.Exists(_characterTag, tag => tag == contact.tag))
        {
            contact.SendMessage("ReceiveDamage", _ammoDamage);
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
