// TODO: Ammo with large size, low speed, explosion effect, big weight, medium recharge time, low bouncing, high damage

// Need to find a way to let NPC use parabola

using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class Grenade : MonoBehaviour
{
    // TODO: VFX Effect for Laser
    public VisualEffect _grenadeImpact;
    public VisualEffect _grenadeDestroy;

    private const int _ammoDamage = 20;
    private const float _explosionRadius = 5f;
    private const float _lifeTime = 4f;

    private int _characterLayer;
    private int _floorLayer;

    void Start()
    {
        _characterLayer = LayerMask.GetMask("Player", "Character");
        _floorLayer = LayerMask.GetMask("Floor");
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
        // TODO: Do damage & explosion force here
        foreach (Collider character in Physics.OverlapSphere(transform.position, _explosionRadius, _characterLayer))
        {
            // TODO: If explosion can hurt character
            if (Physics.Linecast(transform.position, character.transform.position, _floorLayer))
            {
                character.SendMessage("ReceiveDamage",
                    _ammoDamage * (1 - (transform.position - character.transform.position).sqrMagnitude / (_explosionRadius * _explosionRadius)));
                character.attachedRigidbody.AddExplosionForce(_ammoDamage, transform.position, _explosionRadius, 0, ForceMode.Impulse);
            }
            
        }

        // TODO: Add explosion (particle) dead effect when destroy
    }
}