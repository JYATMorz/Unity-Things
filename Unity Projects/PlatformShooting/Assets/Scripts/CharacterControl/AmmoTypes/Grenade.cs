// TODO: Ammo with large size, low speed, explosion effect, big weight, medium recharge time, low bouncing, high damage

// Need to find a way to let NPC use parabola

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
    private const float _lifeTime = 4f;
    private const string _deadTag = "Dead";
    private const string _neutralTag = "Neutral";

    private int _characterLayer;
    private int _floorLayer;
    private string _ownerTag;

    void Start()
    {
        _ownerTag = GetComponentsInParent<Rigidbody>()[2].tag;
        Debug.Log(_ownerTag);

        _characterLayer = LayerMask.GetMask("Neutral", "BlueTeam", "RedTeam", "Dead");
        _floorLayer = LayerMask.GetMask("Floor", "Elevator");
        StartCoroutine(LifeTimeOver(_lifeTime));
    }

    void OnCollisionEnter()
    {
        // TODO: Add hit (particle) effect when collides
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
            if (Physics.Linecast(transform.position, character.transform.position, _floorLayer))
            {
                if (_ownerTag != _neutralTag || !character.CompareTag(_neutralTag) || !character.CompareTag(_deadTag))
                {
                    character.SendMessage("ReceiveDamage", Mathf.FloorToInt(_ammoDamage
                        * (1 - (transform.position - character.transform.position).sqrMagnitude / (_explosionRadius * _explosionRadius))));
                }
                character.attachedRigidbody.AddExplosionForce(_ammoDamage, transform.position, _explosionRadius, 0, ForceMode.Impulse);
            }
            
        }

        // TODO: Add explosion (particle) dead effect when destroy
    }
}