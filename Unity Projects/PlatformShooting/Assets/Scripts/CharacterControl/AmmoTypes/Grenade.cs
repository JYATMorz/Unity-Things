using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class Grenade : MonoBehaviour, IDamage
{
    public GameObject explosionEffect;
    public VisualEffect hitEffect;

    private const int _ammoDamage = 30;
    private const float _explosionRadius = 5f;
    private const float _lifeTime = 2f;

    private Rigidbody _ownerBody;

    void Start()
    {
        _ownerBody = GetComponentsInParent<Rigidbody>()[2];

        StartCoroutine(LifeTimeOver(_lifeTime));

        GetComponent<Rigidbody>().AddTorque(Random.value, Random.value, Random.value, ForceMode.Impulse);
    }

    void OnCollisionEnter()
    {
        hitEffect.Play();
    }

    void OnDestroy()
    {
        if (GeneralLoadMenu.Instance.IsLoadingScene) return;

        Instantiate(explosionEffect, transform.position, transform.rotation);
    }

    IEnumerator LifeTimeOver(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        GeneralAudioControl.Instance.PlayAudio(ConstantSettings.explodeTag, transform.position);

        yield return StartCoroutine(IDamage.ExplosionAttack(_ownerBody, transform.position, _explosionRadius, _ammoDamage, 0.1f * _ammoDamage));

        Destroy(gameObject);
    }
}