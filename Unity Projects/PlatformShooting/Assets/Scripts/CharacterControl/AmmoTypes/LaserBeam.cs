using UnityEngine;
using UnityEngine.VFX;
using System;
using System.Collections;

public class LaserBeam : MonoBehaviour
{
    public VisualEffect laserImpact;

    private const int _ammoDamage = 10;
    private const float _lifeTime = 5f;
    
    private Rigidbody _laserBody;
    private Rigidbody _ownerBody;

    void Start()
    {
        _ownerBody = GetComponentsInParent<Rigidbody>()[2];
        _laserBody = GetComponent<Rigidbody>();

        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.laserTag, transform.position);

        StartCoroutine(LifeTimeOver(_lifeTime));
        StartCoroutine(TooSlowToLive());
    }

    void OnCollisionEnter(Collision other)
    {
        laserImpact.Play();

        GameObject contact = other.gameObject;
        if (Array.Exists(ConstantSettings.aliveTags, tag => tag == contact.tag))
        {
            if (!ConstantSettings.AreBothNeutral(contact, _ownerBody))
            {
                contact.GetComponent<HealthControl>().ReceiveDamage(_ammoDamage, _ownerBody);
            }
        }
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
