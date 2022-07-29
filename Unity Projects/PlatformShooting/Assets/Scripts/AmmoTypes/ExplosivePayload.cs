// TODO: low gravity, small explosion, no bouncing, hit and explode, low recharge time, low damage

using UnityEngine;

public class ExplosivePayload : MonoBehaviour
{
    // TODO: Leave small explosion particle on the floor
    public GameObject explosionEffect;

    private const int _ammoDamage = 10;
    private const float _explosionRadius = 2f;
    private const string _deadTag = "Dead";

    private int _characterLayer;
    private int _floorLayer;

    void Start()
    {
        _characterLayer = LayerMask.GetMask("Neutral", "BlueTeam", "RedTeam", "Dead");
        _floorLayer = LayerMask.GetMask("Floor", "Elevator");
    }

    void OnCollisionEnter()
    {
        SmallExplosion();
        Destroy(gameObject);
    }

    private void SmallExplosion()
    {
        // TODO: Do damage & explosion force here
        foreach (Collider character in Physics.OverlapSphere(transform.position, _explosionRadius, _characterLayer))
        {
            // TODO: If explosion can hurt character
            if (Physics.Linecast(transform.position, character.transform.position, _floorLayer))
            {
                if (!character.CompareTag(_deadTag))
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