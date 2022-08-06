using UnityEngine;

public class ExplosivePayload : MonoBehaviour
{
    // TODO: Leave small explosion particle on the floor
    public GameObject explosionEffect;

    private const int _ammoDamage = 10;
    private const float _explosionRadius = 2f;
    private const string _deadTag = "Dead";
    private const string _neutralTag = "Neutral";

    private int _characterLayer;
    private int _floorLayer;
    private string _ownerTag;

    void Start()
    {
        _ownerTag = GetComponentsInParent<Rigidbody>()[2].tag;

        _characterLayer = LayerMask.GetMask("Neutral", "BlueTeam", "RedTeam", "Dead");
        _floorLayer = LayerMask.GetMask("Floor", "Elevator");
    }

    void OnCollisionEnter()
    {
        SmallExplosion();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // TODO: Add explosion (particle) dead effect when destroy
    }

    private void SmallExplosion()
    {
        foreach (Collider character in Physics.OverlapSphere(transform.position, _explosionRadius, _characterLayer))
        {
            if (Physics.Linecast(transform.position, character.transform.position, _floorLayer))
            {
                if (!character.CompareTag(_deadTag) && !(_ownerTag == _neutralTag && character.CompareTag(_neutralTag)))
                {
                    character.SendMessage("ReceiveDamage", Mathf.FloorToInt(_ammoDamage
                        * (1 - (transform.position - character.transform.position).sqrMagnitude / (_explosionRadius * _explosionRadius))));
                }
                character.attachedRigidbody.AddExplosionForce(_ammoDamage, transform.position, _explosionRadius, 0, ForceMode.Impulse);
            }
            
        }

    }
}