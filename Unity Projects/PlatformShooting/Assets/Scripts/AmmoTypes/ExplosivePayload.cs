// TODO: low gravity, small explosion, no bouncing, hit and explode, low recharge time, low damage

using UnityEngine;
using UnityEngine.VFX;

public class ExplosivePayload : MonoBehaviour
{
    // TODO: VFX Effect for Laser
    public VisualEffect _payloadImpact;

    private const int _ammoDamage = 10;
    private const float _explosionRadius = 2f;

    private int _characterLayer;
    private int _floorLayer;

    void Start()
    {
        _characterLayer = LayerMask.GetMask("Player", "Character");
        _floorLayer = LayerMask.GetMask("Floor");
    }

    void OnCollisionEnter()
    {
        SmallExplosion();
        Destroy(gameObject);
    }

    private void SmallExplosion()
    {
        // TODO: Do damage & explosion force here

        // TODO: Add explosion (particle) dead effect when destroy
    }
}