// TODO: Ammo with large size, low speed, explosion effect, big weight, medium recharge time, low bouncing, high damage

// Need to find a way to let NPC use parabola

using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class LaserBeam : MonoBehaviour
{
    // TODO: VFX Effect for Laser
    public VisualEffect _grenadeImpact;
    public VisualEffect _grenadeDestroy;

    private const int _ammoDamage = 20;
    private const int _explosionRadius = 5;
    private const float _lifeTime = 4f;
    private const string _characterTag = "Character";
    private const string _floorTag = "Floor";

    void Start()
    {
        StartCoroutine(LifeTimeOver(_lifeTime));
    }

    void OnCollisionEnter()
    {
        // TODO: Add explosion (particle) effect when collides
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
        /*
            if character in explosion radius (Physics.OverlapSphereNonAlloc):
                get characters rigidbody, foreach loop
                character.rigidbody.addExplosionForce((float)_ammoDamage, transform.position, (float)_explosionRadius, ForceMode.Impulse)
                character.SendMessage("ReceiveDamage", damage*Mathf.Clamp01(1-distance(character<-->explosion)/_explosionRadius))
        */
        // TODO: Add explosion (particle) dead effect when destroy
    }
}