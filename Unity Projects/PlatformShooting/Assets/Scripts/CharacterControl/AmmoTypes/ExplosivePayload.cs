using UnityEngine;
using System.Collections;

public class ExplosivePayload : MonoBehaviour
{
    public GameObject explosionEffect;

    private const int _selfDestructionTime = 10;
    private const int _ammoDamage = 15;
    private const float _explosionRadius = 2f;
    private const string _deadTag = "Dead";
    private const string _neutralTag = "Neutral";

    private int _characterLayer;
    private int _floorLayer;
    private string _ownerTag;
    private Rigidbody _ownerBody;

    void Start()
    {
        _ownerBody = GetComponentsInParent<Rigidbody>()[2];
        _ownerTag = _ownerBody.tag;

        _characterLayer = LayerMask.GetMask("Neutral", "BlueTeam", "RedTeam", "Dead");
        _floorLayer = LayerMask.GetMask("Floor", "Elevator");

        StartCoroutine(SelfDestruction());
    }

    void OnCollisionEnter()
    {
        Explosion();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);
    }

    private void Explosion()
    {
        foreach (Collider character in Physics.OverlapSphere(transform.position, _explosionRadius, _characterLayer))
        {
            if (Physics.Linecast(transform.position, character.transform.position, ~_floorLayer))
            {
                if (!character.CompareTag(_deadTag) && !(_ownerTag == _neutralTag && character.CompareTag(_neutralTag)))
                {
                    int damage = Mathf.FloorToInt(_ammoDamage * (1 - (transform.position - character.transform.position).sqrMagnitude / (_explosionRadius * _explosionRadius)));
                    character.GetComponent<HealthControl>().ReceiveDamage(damage, _ownerBody);
                }
                character.attachedRigidbody.AddExplosionForce(_ammoDamage, transform.position, _explosionRadius, 0, ForceMode.Impulse);
            }
            
        }
    }


    IEnumerator SelfDestruction()
    {
        yield return new WaitForSeconds(_selfDestructionTime);
        Destroy(gameObject);
    }
}