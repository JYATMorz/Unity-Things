using UnityEngine;
using System.Collections;

public class ExplosivePayload : MonoBehaviour
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
        Instantiate(explosionEffect, transform.position, transform.rotation);
    }

    IEnumerator Explosion()
    {
        foreach (Collider character in Physics.OverlapSphere(transform.position, _explosionRadius, ConstantSettings.characterLayer))
        {
            if (Physics.Linecast(transform.position, character.transform.position, ~ConstantSettings.floorLayer))
            {
                if (!character.CompareTag(ConstantSettings.deadTag) && !ConstantSettings.AreBothNeutral(character, _ownerBody))
                {
                    int damage = ConstantSettings.ExplosionDamage(_ammoDamage, transform.position, character.transform.position, _explosionRadius);
                    character.GetComponent<HealthControl>().ReceiveDamage(damage, _ownerBody);
                }
                character.attachedRigidbody.AddExplosionForce(_ammoDamage, transform.position, _explosionRadius, 0, ForceMode.Impulse);
            }
            yield return new WaitForFixedUpdate();
        }
        Destroy(gameObject);
    }

    IEnumerator SelfDestruction()
    {
        yield return new WaitForSeconds(ConstantSettings.ammoSelfDestruction);
        StartCoroutine(Explosion());
    }
}