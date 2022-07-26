// TODO: low gravity, small explosion, no bouncing, hit and explode, low recharge time, low damage

using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class LaserBeam : MonoBehaviour
{
    // TODO: VFX Effect for Laser
    public VisualEffect _payloadImpact;

    private const int _ammoDamage = 10;
    private const int _explosionRadius = 2;
    private const string _characterTag = "Character";
    private const string _floorTag = "Floor";

    void OnCollisionEnter(Collision other)
    {
        GameObject contact = other.gameObject;
        if (contact.CompareTag(_characterTag))
        {
            contact.SendMessage("ReceiveDamage", _ammoDamage);
        }
        SmallExplosion();
        Destroy(gameObject);
    }

    private void SmallExplosion()
    {
        // TODO: Do damage & explosion force here

        // TODO: Add explosion (particle) dead effect when destroy
    }
}