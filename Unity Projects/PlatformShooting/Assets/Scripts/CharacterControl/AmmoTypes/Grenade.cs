using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class Grenade : MonoBehaviour
{
    // TODO: Leave explosion particle on the floor
    public GameObject explosionEffect;
    // TODO: Smoke impact effect
    public VisualEffect hitEffect;

    private const int _ammoDamage = 20;
    private const float _explosionRadius = 5f;
    private const float _lifeTime = 3f;
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
        StartCoroutine(LifeTimeOver(_lifeTime));

        GetComponent<Rigidbody>().AddTorque(Random.value, Random.value, Random.value, ForceMode.Impulse);
    }

    void OnCollisionEnter()
    {
        // TODO: Add hit (particle) effect when collides
    }

    void OnDestroy()
    {
        // TODO: Add explosion (particle) dead effect when destroy
    }

    IEnumerator LifeTimeOver(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        HugeExplosion();
        Destroy(gameObject);
    }

    private void HugeExplosion()
    {
        foreach (Collider character in Physics.OverlapSphere(transform.position, _explosionRadius, _characterLayer))
        {
            // FIXME: Nobody get hurt (LineCast)
            if (Physics.Linecast(transform.position, character.transform.position, ~_floorLayer))
            // if (true)
            {
                if (!character.CompareTag(_deadTag) && !(_ownerTag == _neutralTag && character.CompareTag(_neutralTag)))
                {
                    // FIXME: who is killer?
                    int damage = Mathf.FloorToInt(_ammoDamage * (1 - (transform.position - character.transform.position).sqrMagnitude / (_explosionRadius * _explosionRadius)));
                    // character.SendMessage("ReceiveDamage", damage);
                    character.GetComponent<CharacterControl>().ReceiveDamage(damage, _ownerBody);
                }
                character.attachedRigidbody.AddExplosionForce(_ammoDamage, transform.position, _explosionRadius, _ammoDamage, ForceMode.Impulse);
            }
            
        }

    }
}