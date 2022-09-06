using UnityEngine;
using System.Collections;

public class ExplosivePayload : MonoBehaviour, IDamage
{
    public GameObject explosionEffect;

    private const int _ammoDamage = 15;
    private const float _explosionRadius = 2f;

    private Rigidbody _ownerBody;

    void Start()
    {
        _ownerBody = GetComponentsInParent<Rigidbody>()[2];

        StartCoroutine(SelfDestruction());
    }

    void OnCollisionEnter()
    {
        StartCoroutine(Explosion());
    }

    void OnDestroy()
    {
        if (GeneralLoadMenu.Instance.IsLoadingScene) return;

        Instantiate(explosionEffect, transform.position, transform.rotation);
    }

    IEnumerator Explosion()
    {
        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.explodeTag, transform.position, 0.2f);

        yield return StartCoroutine(IDamage.ExplosionAttack(_ownerBody, transform.position, _explosionRadius, _ammoDamage));

        Destroy(gameObject);
    }

    IEnumerator SelfDestruction()
    {
        yield return new WaitForSeconds(ConstantSettings.ammoSelfDestruction);
        StartCoroutine(Explosion());
    }
}