using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class Grenade : MonoBehaviour
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

        foreach (Collider character in Physics.OverlapSphere(transform.position, _explosionRadius, ConstantSettings.characterLayer))
        {
            if (Physics.Linecast(transform.position, character.transform.position, ~ConstantSettings.floorLayer))
            {
                if (!character.CompareTag(ConstantSettings.deadTag) && !ConstantSettings.AreBothNeutral(character, _ownerBody))
                {
                    int damage = ConstantSettings.ExplosionDamage(_ammoDamage, transform.position, character.transform.position, _explosionRadius);
                    character.GetComponent<HealthControl>().ReceiveDamage(damage, _ownerBody);
                }
                character.attachedRigidbody.AddExplosionForce(_ammoDamage, transform.position, _explosionRadius, 0.1f * _ammoDamage, ForceMode.Impulse);
            }
            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
    }
}